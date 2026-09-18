using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Slafurry.Core.Abstract;
using Slafurry.System.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AnalyticsService : GameSystem<AnalyticsService>
{
    [SerializeField]
    private AnalyticsConfig config;

    private AnalyticsBuffer _buffer;
    private readonly Dictionary<string, long> _sceneEnterTimes = new();
    private readonly HashSet<int> _scannedSceneRoots = new();
    private bool _isSending;

    private string _supabaseUrl;
    private string _supabaseAnonKey;

    private const string TableName = "analytics_events";
    private const string StreamingAssetsConfig = "analytics_config.json";

    public override int Priority => 999;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();

        _buffer = new AnalyticsBuffer();
        _buffer.Initialize(
            config != null ? config.BatchSize : 50,
            config != null ? config.FlushInterval : 30f,
            FlushToSupabase
        );
    }

    public override IEnumerator Initialize()
    {
        yield return StartCoroutine(LoadConfig());

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseAnonKey))
        {
            Debug.LogWarning(
                "[Analytics] Supabase URL or key not configured. Events buffered only."
            );
        }
        else
        {
            Debug.Log($"[Analytics] Supabase configured: {_supabaseUrl}");
        }

        ScanCurrentScenes();
    }

    private IEnumerator LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, StreamingAssetsConfig);
        Debug.Log($"[Analytics] Loading config from: {path}");

        using var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[Analytics] Config loaded: {request.downloadHandler.text}");
            try
            {
                var data = JsonUtility.FromJson<StreamingConfig>(request.downloadHandler.text);
                if (data != null && !string.IsNullOrEmpty(data.supabaseUrl))
                {
                    _supabaseUrl = data.supabaseUrl;
                    _supabaseAnonKey = data.supabaseAnonKey;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Analytics] Failed to parse config: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[Analytics] Failed to load config: {request.error}");
        }

        if (string.IsNullOrEmpty(_supabaseUrl))
        {
            _supabaseUrl = config?.SupabaseUrl ?? "";
            _supabaseAnonKey = config?.SupabaseAnonKey ?? "";
        }
    }

    public override void PostInitialize() { }

    public void Enqueue(AnalyticsEvent analyticsEvent)
    {
        if (config != null && !config.IsEnabled)
            return;
        _buffer.Enqueue(analyticsEvent);
    }

    private void Update()
    {
        if (config != null && !config.IsEnabled)
            return;
        _buffer.Tick(Time.unscaledDeltaTime);
    }

    // ── Scene duration tracking ──────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "DontDestroyOnLoad")
            return;

        _sceneEnterTimes[scene.name] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ScanScene(scene);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (!_sceneEnterTimes.TryGetValue(scene.name, out long enterTime))
            return;

        long duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - enterTime;
        _sceneEnterTimes.Remove(scene.name);

        Enqueue(
            new AnalyticsEvent
            {
                EventName = "scene_duration",
                ObjectName = scene.name,
                ParentName = "",
                SceneName = scene.name,
                PlayerName = PlayerData.PlayerName,
                DeviceId = SystemInfo.deviceUniqueIdentifier,
                DurationMs = duration,
            }
        );
    }

    // ── Auto-scan UnityEvents via reflection ─────────────────────

    private void ScanCurrentScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != "DontDestroyOnLoad")
                ScanScene(scene);
        }
    }

    private void ScanScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            int id = root.GetInstanceID();
            if (!_scannedSceneRoots.Add(id))
                continue;

            ScanGameObject(root);
        }
    }

    private void ScanGameObject(GameObject go)
    {
        foreach (MonoBehaviour mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null || mb == this)
                continue;

            ScanBehaviour(mb);
        }
    }

    private void ScanBehaviour(MonoBehaviour mb)
    {
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (FieldInfo field in mb.GetType().GetFields(bindingFlags))
        {
            if (!typeof(UnityEvent).IsAssignableFrom(field.FieldType))
                continue;

            UnityEvent evt;
            try
            {
                evt = (UnityEvent)field.GetValue(mb);
            }
            catch
            {
                continue;
            }

            if (evt == null)
                continue;

            string eventName = field.Name;
            string objectName = mb.gameObject.name;
            string parentName =
                mb.transform.parent != null ? mb.transform.parent.gameObject.name : "root";
            string sceneName = mb.gameObject.scene.name;

            evt.AddListener(() =>
                OnTrackedEventFired(eventName, objectName, parentName, sceneName)
            );
        }
    }

    private void OnTrackedEventFired(
        string eventName,
        string objectName,
        string parentName,
        string sceneName
    )
    {
        Enqueue(
            new AnalyticsEvent
            {
                EventName = eventName,
                ObjectName = objectName,
                ParentName = parentName,
                SceneName = sceneName,
                PlayerName = PlayerData.PlayerName,
                DeviceId = SystemInfo.deviceUniqueIdentifier,
            }
        );
    }

    // ── Supabase HTTP ────────────────────────────────────────────

    private void FlushToSupabase(List<AnalyticsEvent> batch)
    {
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseAnonKey))
        {
            foreach (var evt in batch)
            {
                Debug.Log(
                    $"[Analytics] {evt.EventName}: obj={evt.ObjectName}, "
                        + $"parent={evt.ParentName}, scene={evt.SceneName}, "
                        + $"player={evt.PlayerName}, device={evt.DeviceId}"
                        + (evt.DurationMs > 0 ? $", dur={evt.DurationMs}ms" : "")
                );
            }
            return;
        }

        StartCoroutine(PostEventsCoroutine(batch));
    }

    private IEnumerator PostEventsCoroutine(List<AnalyticsEvent> batch)
    {
        if (_isSending)
        {
            foreach (var evt in batch)
                _buffer.Enqueue(evt);
            yield break;
        }

        _isSending = true;

        string json = BuildJsonArray(batch);
        string url = $"{_supabaseUrl}/rest/v1/{TableName}";

        using var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("apikey", _supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {_supabaseAnonKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning(
                $"[Analytics] Supabase insert failed ({request.responseCode}): {request.error}"
            );
            foreach (var evt in batch)
                _buffer.Enqueue(evt);
        }
        else
        {
            Debug.Log($"[Analytics] Sent {batch.Count} events to Supabase.");
        }

        _isSending = false;
    }

    private string BuildJsonArray(List<AnalyticsEvent> batch)
    {
        var sb = new StringBuilder();
        sb.Append('[');

        for (int i = 0; i < batch.Count; i++)
        {
            if (i > 0)
                sb.Append(',');

            var e = batch[i];
            sb.Append('{');
            AppendField(sb, "event_name", e.EventName);
            sb.Append(',');
            AppendField(sb, "object_name", e.ObjectName);
            sb.Append(',');
            AppendField(sb, "parent_name", e.ParentName);
            sb.Append(',');
            AppendField(sb, "scene_name", e.SceneName);
            sb.Append(',');
            AppendField(sb, "player_name", e.PlayerName);
            sb.Append(',');
            AppendField(sb, "device_id", e.DeviceId);
            sb.Append(',');
            sb.Append("\"duration_ms\":");
            sb.Append(e.DurationMs);
            sb.Append('}');
        }

        sb.Append(']');
        return sb.ToString();
    }

    private void AppendField(StringBuilder sb, string key, string value)
    {
        sb.Append('"');
        sb.Append(key);
        sb.Append("\":\"");
        sb.Append(value?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "");
        sb.Append('"');
    }

    protected override void OnSingletonDestroyed()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        _buffer.Flush();
        base.OnSingletonDestroyed();
    }
}

[System.Serializable]
public class StreamingConfig
{
    public string supabaseUrl;
    public string supabaseAnonKey;
}
