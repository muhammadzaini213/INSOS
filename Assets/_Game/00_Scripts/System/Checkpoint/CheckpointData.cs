using System;
using System.Collections.Generic;

namespace Slafurry.System.Checkpoint
{
    /// <summary>
    /// Data structure for checkpoint information.
    /// Stored in PlayerPrefs as JSON string.
    /// </summary>
    [Serializable]
    public class CheckpointData
    {
        public int section;              // 1, 2, or 3
        public string sceneName;         // "05_Section 1"
        public int gender;               // 0 = Boy, 1 = Girl (from PlayerData)
        public long timestamp;           // Unix timestamp (for "Last played: X days ago")
        
        public CheckpointData() { }
        
        public CheckpointData(int section, string sceneName, int gender)
        {
            this.section = section;
            this.sceneName = sceneName;
            this.gender = gender;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        /// <summary>
        /// Get human-readable timestamp (for UI display).
        /// Example: "Last played: 2 days ago"
        /// </summary>
        public string GetTimestampText()
        {
            DateTime savedTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            TimeSpan elapsed = DateTime.UtcNow - savedTime;
            
            if (elapsed.TotalMinutes < 1)
                return "Last played: Just now";
            if (elapsed.TotalHours < 1)
                return $"Last played: {(int)elapsed.TotalMinutes} minutes ago";
            if (elapsed.TotalDays < 1)
                return $"Last played: {(int)elapsed.TotalHours} hours ago";
            if (elapsed.TotalDays < 7)
                return $"Last played: {(int)elapsed.TotalDays} days ago";
            
            return $"Last played: {savedTime:yyyy-MM-dd}";
        }
    }
    
    /// <summary>
    /// Section-specific save data structure.
    /// Each section has its own save data independent from checkpoints.
    /// Use this to store gameplay state, collected items, completed tasks, etc.
    /// </summary>
    [Serializable]
    public class SectionSaveData
    {
        public int section;                           // Section number (1, 2, or 3)
        public bool isCompleted;                      // Has section been completed?
        public float progressPercentage;              // 0-100% progress through section
        public long lastPlayedTimestamp;              // Last time this section was played
        
        // Gameplay state
        public List<string> completedScenes;          // Scenes completed in this section
        public List<string> collectedItems;           // Items collected (e.g., "key", "coin")
        public List<string> completedTasks;           // Tasks/objectives completed
        public Dictionary<string, int> taskProgress;  // Task progress counters (e.g., "teeth_brushed": 3)
        public Dictionary<string, bool> flags;        // Boolean flags (e.g., "tutorial_seen": true)
        public Dictionary<string, string> customData; // Custom string data
        
        // Statistics
        public int timesPlayed;                       // How many times section was started
        public int deathCount;                        // Number of failures/retries
        public int hintsUsed;                         // Tutorial hints used
        
        public SectionSaveData()
        {
            completedScenes = new List<string>();
            collectedItems = new List<string>();
            completedTasks = new List<string>();
            taskProgress = new Dictionary<string, int>();
            flags = new Dictionary<string, bool>();
            customData = new Dictionary<string, string>();
        }
        
        public SectionSaveData(int section) : this()
        {
            this.section = section;
            this.isCompleted = false;
            this.progressPercentage = 0f;
            this.lastPlayedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.timesPlayed = 1;
        }
    }
}
