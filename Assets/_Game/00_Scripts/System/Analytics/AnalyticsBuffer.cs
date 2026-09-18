using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnalyticsBuffer
{
    [SerializeField]
    private int batchSize = 50;

    [SerializeField]
    private float flushInterval = 30f;

    private readonly Queue<AnalyticsEvent> _queue = new();
    private float _timer;
    private Action<List<AnalyticsEvent>> _onFlush;

    public int Count
    {
        get
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }
    }

    public void Initialize(int batchSize, float flushInterval, Action<List<AnalyticsEvent>> onFlush)
    {
        this.batchSize = batchSize;
        this.flushInterval = flushInterval;
        _onFlush = onFlush;
        _timer = 0f;
    }

    public void Enqueue(AnalyticsEvent analyticsEvent)
    {
        lock (_queue)
        {
            _queue.Enqueue(analyticsEvent);
        }
    }

    public bool Tick(float deltaTime)
    {
        if (_onFlush == null)
            return false;

        _timer += deltaTime;
        if (_timer < flushInterval)
            return false;

        _timer = 0f;
        Flush();
        return true;
    }

    public void Flush()
    {
        if (_onFlush == null)
            return;

        List<AnalyticsEvent> batch;
        lock (_queue)
        {
            if (_queue.Count == 0)
                return;

            batch = new List<AnalyticsEvent>();
            while (_queue.Count > 0 && batch.Count < batchSize)
            {
                batch.Add(_queue.Dequeue());
            }
        }

        _onFlush?.Invoke(batch);
    }
}
