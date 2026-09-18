using System;
using System.Collections.Generic;
using System.Reflection;
using Slafurry.System.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnityEventTracker : MonoBehaviour
{
    private readonly List<(
        UnityEvent evt,
        string eventName,
        string objectName,
        string parentName,
        string sceneName
    )> _tracked = new();

    private void Start()
    {
        ScanScene();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ScanScene();
    }

    private void ScanScene()
    {
        Scene scene = gameObject.scene;
        if (scene.name == "DontDestroyOnLoad")
            return;

        foreach (var root in scene.GetRootGameObjects())
        {
            ScanGameObject(root);
        }
    }

    private void ScanGameObject(GameObject go)
    {
        var monoBehaviours = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours)
        {
            if (mb == null)
                continue;
            if (mb is AnalyticsService or UnityEventTracker)
                continue;

            ScanBehaviour(mb);
        }
    }

    private void ScanBehaviour(MonoBehaviour mb)
    {
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var fields = mb.GetType().GetFields(bindingFlags);

        foreach (var field in fields)
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

            _tracked.Add((evt, eventName, objectName, parentName, sceneName));

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
        if (AnalyticsService.Instance == null)
            return;

        AnalyticsService.Instance.Enqueue(
            new AnalyticsEvent
            {
                EventName = eventName,
                ObjectName = objectName,
                ParentName = parentName,
                SceneName = sceneName,
                PlayerName = PlayerData.PlayerName,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            }
        );
    }
}
