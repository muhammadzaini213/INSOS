using System;

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
}
