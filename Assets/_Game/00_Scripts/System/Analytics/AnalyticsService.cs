using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Slafurry.Core.Abstract;
using Slafurry.System.Player;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AnalyticsService : GameSystem<AnalyticsService>
{
    [SerializeField]
    private AnalyticsConfig config;

    private AnalyticsBuffer _buffer;
    private readonly Dictionary<string, long> _sceneEnterTimes = new();
    private bool _isSending;

    private string _supabaseUrl;
    private string _supabaseAnonKey;

    private const string TableName = "analytics_events";
    private const string StreamingAssetsConfig = "analytics_config.json";

    public override int Priority => 999;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();

        LoadConfig();

        _buffer = new AnalyticsBuffer();
        _buffer.Initialize(
            config != null ? config.BatchSize : 50,
            config != null ? config.FlushInterval : 30f,
            FlushToSupabase
        );
    }

    private void LoadConfig()
    {
        string url = null;
        string key = null;

        string path = Path.Combine(Application.streamingAssetsPath, StreamingAssetsConfig);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<StreamingConfig>(json);
                if (data != null)
                {
                    url = data.supabaseUrl;
                    key = data.supabaseAnonKey;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Analytics] Failed to load streaming config: {e.Message}");
            }
        }

        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(key))
        {
            _supabaseUrl = url;
            _supabaseAnonKey = key;
        }
        else
        {
            _supabaseUrl = config?.SupabaseUrl ?? "";
            _supabaseAnonKey = config?.SupabaseAnonKey ?? "";
        }
    }

    public override IEnumerator Initialize()
    {
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

        yield return null;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "DontDestroyOnLoad")
            return;
        _sceneEnterTimes[scene.name] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DurationMs = duration,
            }
        );
    }

    private void FlushToSupabase(List<AnalyticsEvent> batch)
    {
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseAnonKey))
        {
            foreach (var evt in batch)
            {
                Debug.Log(
                    $"[Analytics] {evt.EventName}: obj={evt.ObjectName}, "
                        + $"parent={evt.ParentName}, scene={evt.SceneName}, "
                        + $"player={evt.PlayerName}, ts={evt.Timestamp}"
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
            sb.Append("\"duration_ms\":");
            sb.Append(e.DurationMs);
            sb.Append(',');
            sb.Append("\"timestamp\":");
            sb.Append(e.Timestamp);
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
