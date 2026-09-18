using UnityEngine;

[CreateAssetMenu(fileName = "AnalyticsConfig", menuName = "Slafurry/Analytics Config")]
public class AnalyticsConfig : ScriptableObject
{
    [Header("Supabase")]
    [SerializeField]
    private string supabaseUrl = "";

    [SerializeField]
    private string supabaseAnonKey = "";

    [Header("Buffer")]
    [SerializeField]
    private int batchSize = 50;

    [SerializeField]
    private float flushInterval = 30f;

    [Header("Toggle")]
    [SerializeField]
    private bool enabled = true;

    public string SupabaseUrl => supabaseUrl;
    public string SupabaseAnonKey => supabaseAnonKey;
    public int BatchSize => batchSize;
    public float FlushInterval => flushInterval;
    public bool IsEnabled => enabled;
}
