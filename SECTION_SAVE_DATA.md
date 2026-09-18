# Separate Save Data Per Section - Implementation Guide

## ✅ What Was Added

### **Enhanced Data Structure**

Each section now has **two independent save systems**:

1. **Checkpoint** - Quick resume (scene position + gender)
2. **Section Save Data** - Full gameplay state (items, tasks, progress, stats)

---

## 📦 New Features

### **SectionSaveData Structure**
```csharp
public class SectionSaveData
{
    // Core
    public int section;                          // 1, 2, or 3
    public bool isCompleted;                     // Section completed?
    public float progressPercentage;             // 0-100%
    public long lastPlayedTimestamp;             // Last played time
    
    // Gameplay State
    public List<string> completedScenes;         // Scenes completed
    public List<string> collectedItems;          // Items collected
    public List<string> completedTasks;          // Tasks completed
    public Dictionary<string, int> taskProgress; // Task counters
    public Dictionary<string, bool> flags;       // Boolean flags
    public Dictionary<string, string> customData;// Custom data
    
    // Statistics
    public int timesPlayed;                      // Play count
    public int deathCount;                       // Failures
    public int hintsUsed;                        // Hints used
}
```

---

## 🎯 New API Methods

### **CheckpointManager - Section Save Data**

#### Get Section Data
```csharp
// Load or create section save data (cached in memory)
SectionSaveData data = CheckpointManager.Instance.GetSectionData(1);
```

#### Save Section Data
```csharp
// Save section data to PlayerPrefs
CheckpointManager.Instance.SaveSectionData(1);
```

#### Reset Section Data
```csharp
// Clear all section data (also clears checkpoint)
CheckpointManager.Instance.ResetSectionData(1);
```

#### Check If Data Exists
```csharp
// Check if section has save data
bool hasSaveData = CheckpointManager.Instance.HasSectionData(1);
```

---

## 🛠️ SectionSaveDataHelper Component

New helper component for easy interaction with section save data.

### **Usage**

```csharp
// Attach to GameObject in scene
public class MyGameplayScript : MonoBehaviour
{
    [SerializeField] private SectionSaveDataHelper saveHelper;
    
    void Start()
    {
        // Mark scene as completed
        saveHelper.MarkCurrentSceneCompleted();
        
        // Collect item
        saveHelper.CollectItem("key_1");
        
        // Complete task
        saveHelper.CompleteTask("brush_teeth");
        
        // Increment task progress
        saveHelper.IncrementTaskProgress("stars_collected", 1);
        
        // Set flag
        saveHelper.SetFlag("tutorial_seen", true);
        
        // Set progress
        saveHelper.SetProgress(45.5f); // 45.5%
        
        // Data auto-saves on scene unload (OnDestroy)
    }
    
    void CheckProgress()
    {
        // Check if item collected
        if (saveHelper.HasItem("key_1"))
        {
            Debug.Log("Player has key!");
        }
        
        // Check task completion
        if (saveHelper.IsTaskCompleted("brush_teeth"))
        {
            Debug.Log("Teeth brushed!");
        }
        
        // Get task progress
        int stars = saveHelper.GetTaskProgress("stars_collected");
        Debug.Log($"Stars collected: {stars}");
        
        // Get flag
        bool tutorialSeen = saveHelper.GetFlag("tutorial_seen");
    }
}
```

### **Methods**

#### Scene Management
```csharp
saveHelper.MarkCurrentSceneCompleted()
saveHelper.MarkSceneCompleted("05_Section 1")
bool isCompleted = saveHelper.IsSceneCompleted("05_Section 1")
```

#### Item Management
```csharp
saveHelper.CollectItem("key")
bool hasKey = saveHelper.HasItem("key")
int itemCount = saveHelper.GetCollectedItemCount()
```

#### Task Management
```csharp
saveHelper.CompleteTask("brush_teeth")
bool isDone = saveHelper.IsTaskCompleted("brush_teeth")
saveHelper.IncrementTaskProgress("stars_collected", 1)
int progress = saveHelper.GetTaskProgress("stars_collected")
```

#### Flag Management
```csharp
saveHelper.SetFlag("tutorial_seen", true)
bool flag = saveHelper.GetFlag("tutorial_seen", false)
```

#### Custom Data
```csharp
saveHelper.SetCustomData("player_name", "John")
string name = saveHelper.GetCustomData("player_name", "Unknown")
```

#### Statistics
```csharp
saveHelper.IncrementDeathCount()
saveHelper.IncrementHintsUsed()
saveHelper.SetSectionCompleted(true)
saveHelper.SetProgress(75.5f)
```

#### Getters
```csharp
int scenes = saveHelper.GetCompletedSceneCount()
int items = saveHelper.GetCollectedItemCount()
int tasks = saveHelper.GetCompletedTaskCount()
float progress = saveHelper.GetProgress()
bool completed = saveHelper.IsSectionCompleted()
```

---

## 🎮 Usage Examples

### **Example 1: Track Item Collection**

```csharp
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private string itemId = "key_1";
    [SerializeField] private SectionSaveDataHelper saveHelper;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if already collected
            if (saveHelper.HasItem(itemId))
            {
                Debug.Log("Already collected!");
                return;
            }
            
            // Collect item
            saveHelper.CollectItem(itemId);
            saveHelper.SaveSectionData(); // Save immediately
            
            Destroy(gameObject);
        }
    }
}
```

### **Example 2: Track Task Progress**

```csharp
public class ToothbrushingTask : MonoBehaviour
{
    [SerializeField] private SectionSaveDataHelper saveHelper;
    
    void OnBrushingComplete()
    {
        // Increment brushing progress
        saveHelper.IncrementTaskProgress("teeth_brushed", 1);
        
        int timesBrushed = saveHelper.GetTaskProgress("teeth_brushed");
        
        if (timesBrushed >= 3)
        {
            // Task completed!
            saveHelper.CompleteTask("brush_teeth_3_times");
            saveHelper.SetProgress(33.33f); // 1/3 of section complete
        }
        
        saveHelper.SaveSectionData();
    }
}
```

### **Example 3: Section Completion**

```csharp
public class SectionEndTrigger : MonoBehaviour
{
    [SerializeField] private int section = 1;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Get section data
            SectionSaveData data = CheckpointManager.Instance.GetSectionData(section);
            
            // Mark as completed
            data.isCompleted = true;
            data.progressPercentage = 100f;
            
            // Save
            CheckpointManager.Instance.SaveSectionData(section);
            
            Debug.Log($"Section {section} completed!");
        }
    }
}
```

### **Example 4: Check Progress on Load**

```csharp
public class SectionIntro : MonoBehaviour
{
    [SerializeField] private SectionSaveDataHelper saveHelper;
    
    void Start()
    {
        // Check if returning player
        if (saveHelper.GetCompletedSceneCount() > 0)
        {
            // Skip intro for returning players
            SkipIntro();
        }
        
        // Check specific flag
        if (!saveHelper.GetFlag("tutorial_seen"))
        {
            ShowTutorial();
            saveHelper.SetFlag("tutorial_seen", true);
            saveHelper.SaveSectionData();
        }
    }
}
```

---

## 📊 PlayerPrefs Keys

### **New Keys** (Separate per section)
```
SectionSaveData_1  → JSON: Section 1 full save data
SectionSaveData_2  → JSON: Section 2 full save data
SectionSaveData_3  → JSON: Section 3 full save data
```

### **Existing Keys** (Unchanged)
```
Checkpoint_Section1  → JSON: Section 1 checkpoint
Checkpoint_Section2  → JSON: Section 2 checkpoint
Checkpoint_Section3  → JSON: Section 3 checkpoint
LastPlayedSection    → int: Last played section
PlayerGender         → int: Gender
Language             → string: Language
MusicVolume          → float: Music volume
SFXVolume            → float: SFX volume
```

---

## 🔄 Data Flow

### **On Scene Load**
```
1. SceneCheckpoint saves checkpoint (scene name + gender)
2. SectionSaveDataHelper loads section save data (if attached)
3. Game checks flags/progress and adjusts gameplay
```

### **During Gameplay**
```
1. Player collects item → CollectItem()
2. Player completes task → CompleteTask()
3. Progress tracked in memory (cached)
```

### **On Scene Unload**
```
1. SectionSaveDataHelper.OnDestroy() auto-saves data
2. OR manually call SaveSectionData()
3. Data persisted to PlayerPrefs
```

---

## 🧪 Testing

### **Test 1: Item Persistence**
```
1. Section 1 scene 1: Collect item "key"
2. Progress to scene 2
3. Check: HasItem("key") should return true
```

### **Test 2: Task Progress**
```
1. Increment task "stars": 5 times
2. Quit and restart section
3. GetTaskProgress("stars") should return 5
```

### **Test 3: Independent Sections**
```
1. Section 1: Collect "key_1"
2. Section 2: Collect "key_2"
3. Both should persist independently
4. Resetting Section 1 shouldn't affect Section 2
```

### **Test 4: Reset**
```
1. Collect items, complete tasks in Section 1
2. Call ResetSectionData(1)
3. All Section 1 data cleared
4. Section 2/3 data unchanged
```

---

## 📝 Migration from Old System

If you have existing gameplay code, here's how to migrate:

### **Before** (Manual PlayerPrefs)
```csharp
// Old way
PlayerPrefs.SetInt("Section1_ItemCollected", 1);
PlayerPrefs.SetString("Section1_Tasks", "task1,task2");
```

### **After** (Section Save Data)
```csharp
// New way
SectionSaveData data = CheckpointManager.Instance.GetSectionData(1);
data.collectedItems.Add("item1");
data.completedTasks.Add("task1");
CheckpointManager.Instance.SaveSectionData(1);

// Or use helper
saveHelper.CollectItem("item1");
saveHelper.CompleteTask("task1");
saveHelper.SaveSectionData(); // Auto-saves on destroy by default
```

---

## 🎯 Benefits

1. **Separate Persistence** - Each section has independent save data
2. **Structured Data** - No more scattered PlayerPrefs keys
3. **Memory Cached** - Fast access, only save when needed
4. **Easy to Use** - Helper component for common operations
5. **Flexible** - Store items, tasks, flags, custom data
6. **Statistics** - Track play count, deaths, hints
7. **Auto-Save** - Optional auto-save on scene unload
8. **Debugging** - Enhanced debug output shows save data

---

## 📚 Files Modified/Created

### **Modified**
- `CheckpointData.cs` - Added `SectionSaveData` class
- `CheckpointManager.cs` - Added save data API methods

### **Created**
- `SectionSaveDataHelper.cs` - Helper component

---

**All section data is now saved separately per section!** 🎉

Each section maintains its own:
- Completed scenes
- Collected items
- Completed tasks
- Task progress counters
- Boolean flags
- Custom data
- Statistics

Use `CheckpointManager.Instance.GetSectionData(section)` or the `SectionSaveDataHelper` component to interact with section-specific save data.
