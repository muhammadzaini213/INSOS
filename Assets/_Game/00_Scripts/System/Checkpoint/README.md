# Checkpoint System Documentation

## 📋 Overview

The **Checkpoint System** allows players to save and resume their progress through the game's three sections. It uses **PlayerPrefs** to store checkpoint data and automatically saves progress on every scene transition.

---

## ✅ Features

- ✅ **Auto-save on scene load** - No manual save buttons required
- ✅ **Per-section checkpoints** - Independent progress for Section 1, 2, and 3
- ✅ **Gender persistence** - Restores selected gender from checkpoint
- ✅ **Auto-resume** - Activity buttons load checkpoint scenes automatically
- ✅ **Reset functionality** - Clear progress per section or all at once
- ✅ **Scene name parsing** - Auto-detects section from scene name pattern

---

## 📦 Components

### **1. CheckpointManager** (Singleton System)
**Path**: `Assets/_Game/00_Scripts/System/Checkpoint/CheckpointManager.cs`

**Purpose**: Core singleton that manages all checkpoint operations.

**Key Methods**:
```csharp
// Save checkpoint (auto-called by SceneCheckpoint)
CheckpointManager.Instance.SaveCheckpoint(int section, string sceneName);

// Load checkpoint data (returns null if no checkpoint)
CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(int section);

// Reset section progress (delete checkpoint)
CheckpointManager.Instance.ResetSectionProgress(int section);

// Check if checkpoint exists
bool hasCheckpoint = CheckpointManager.Instance.HasCheckpoint(int section);

// Get scene to load (checkpoint or default start scene)
string scene = CheckpointManager.Instance.GetSceneToLoad(int section);

// Get last played section (1, 2, 3, or 0 if none)
int section = CheckpointManager.Instance.GetLastPlayedSection();

// Clear all checkpoints
CheckpointManager.Instance.ClearAllCheckpoints();
```

**Events**:
```csharp
CheckpointManager.Instance.OnCheckpointSaved += (section, sceneName) => { };
CheckpointManager.Instance.OnCheckpointReset += (section) => { };
```

---

### **2. CheckpointData** (Data Structure)
**Path**: `Assets/_Game/00_Scripts/System/Checkpoint/CheckpointData.cs`

**Purpose**: Serializable data class for checkpoint information.

**Fields**:
```csharp
public class CheckpointData
{
    public int section;        // 1, 2, or 3
    public string sceneName;   // "05_Section 1"
    public int gender;         // 0 = Boy, 1 = Girl
    public long timestamp;     // Unix timestamp
}
```

**Helper Methods**:
```csharp
// Get human-readable timestamp (e.g., "Last played: 2 days ago")
string timeText = checkpointData.GetTimestampText();
```

---

### **3. SceneCheckpoint** (Auto-Save Component)
**Path**: `Assets/_Game/00_Scripts/Game/Checkpoint/SceneCheckpoint.cs`

**Purpose**: Attach to GameObjects in gameplay scenes to auto-save checkpoints.

**How it works**:
1. On `Start()`, reads current scene name
2. Parses section number from name (e.g., `"05_Section 1"` → Section 1)
3. Calls `CheckpointManager.Instance.SaveCheckpoint(section, sceneName)`
4. Skips non-gameplay scenes (menu scenes, scenes without "Section X" in name)

**Supported Scene Name Patterns**:
- `"01_Section 1"` → Section 1 ✅
- `"05_Section 2"` → Section 2 ✅
- `"10_Section 3"` → Section 3 ✅
- `"02_Section 1_1"` → Section 1 (handles sub-variations) ✅
- `"StartMenu"` → Skipped (not a gameplay scene) ⏭️

---

### **4. ActivitySelectionMenu** (UI Integration)
**Path**: `Assets/_Game/00_Scripts/UI/Menu/ActivitySelectionMenu.cs`

**Purpose**: Activity selection menu with checkpoint integration.

**Features**:
- Buttons load checkpoint scenes (if exist) or default start scenes (if new)
- Shows "Continue" indicators when checkpoints exist
- Auto-updates indicators on menu open

**Integration Steps**:
1. Attach to Canvas/UI root in `03_ChooseActivityMenu` scene
2. Assign button references in Inspector
3. Wire button callbacks to `OnMorningActivityClicked()`, `OnCardGameClicked()`, `OnCleaningClicked()`
4. (Optional) Assign "Continue" indicator GameObjects

---

### **5. ContinueButton** (Main Menu Integration)
**Path**: `Assets/_Game/00_Scripts/UI/Menu/ContinueButton.cs`

**Purpose**: Main menu "Continue" button that loads last played checkpoint.

**Features**:
- Auto-disables if no checkpoints exist
- Shows last played section info (optional)
- Loads checkpoint scene for last played section

**Integration Steps**:
1. Add component to "Continue" button GameObject
2. Assign button reference
3. Wire `OnContinueClicked()` to button's OnClick event

---

### **6. ResetProgressButton** (Settings Menu)
**Path**: `Assets/_Game/00_Scripts/UI/Menu/ResetProgressButton.cs`

**Purpose**: Reset buttons for settings menu.

**Methods**:
```csharp
public void OnResetAllProgressClicked()    // Reset all sections
public void OnResetSection1Clicked()       // Reset Section 1 only
public void OnResetSection2Clicked()       // Reset Section 2 only
public void OnResetSection3Clicked()       // Reset Section 3 only
```

---

## 🚀 Setup Instructions

### **Phase 1: Test Core System (5 minutes)**

1. **Play any scene** to initialize `CheckpointManager`
   - Open any scene in the project
   - Press Play in Unity Editor
   - Check Console for: `"[CheckpointManager] Checkpoint system ready!"`

2. **Test checkpoint save** (via Editor Context Menu)
   - Create empty GameObject → Add `SceneCheckpoint` component
   - Right-click component → `Test: Save Checkpoint Now`
   - Check Console for: `"[CheckpointManager] Checkpoint saved..."`

3. **Verify PlayerPrefs** (via Unity menu)
   - Unity Editor → Edit → PlayerPrefs
   - Should see keys: `Checkpoint_Section1`, `LastPlayedSection`

---

### **Phase 2: Add Auto-Save to Scenes (30 minutes)**

**Option A: Quick Test (1 scene)**
1. Open `01_Section 1` scene
2. Create empty GameObject → Name it `"[CheckpointSystem]"`
3. Add `SceneCheckpoint` component
4. Play scene → Check console for checkpoint save log

**Option B: Full Implementation (Recommended)**

1. **Create SceneCheckpoint Prefab**:
   ```
   - Create empty GameObject
   - Name: "SceneCheckpoint"
   - Add component: SceneCheckpoint
   - Drag to Assets/_Game/02_Prefabs/System/SceneCheckpoint.prefab
   ```

2. **Add prefab to all gameplay scenes**:
   ```
   Section 1 scenes (20 scenes):
   - 01_Section 1.unity
   - 02_Section 1.unity
   - 02_Section 1_1.unity
   - 02_Section 1_2.unity
   - 02_Section 1_3.unity
   - 02_Section 1_4.unity
   - 03_Section 1.unity
   - ... (through 17_Section 1.unity)
   
   Section 2 scenes (4 scenes):
   - 01_Section 2.unity
   - 02_Section 2.unity
   - 03_Section 2.unity
   - TutorialTest.unity (optional)
   
   Section 3 scenes (10 scenes):
   - 01_Section 3.unity
   - 02_Section 3.unity
   - ... (through 10_Section 3.unity)
   ```

3. **Skip menu scenes** (no checkpoint needed):
   - `01_StartMenu.unity`
   - `02_ChooseGenderMenu.unity`
   - `03_ChooseActivityMenu.unity`

---

### **Phase 3: Activity Selection Menu (10 minutes)**

**Scene**: `03_ChooseActivityMenu.unity`

1. **Add ActivitySelectionMenu component**:
   - Select Canvas or UI root GameObject
   - Add Component → `ActivitySelectionMenu`

2. **Assign button references** in Inspector:
   - Find "Morning Activity" button → Assign to `morningActivityButton`
   - Find "Cleaning" button → Assign to `cleaningButton`
   - Find "Card Game" button → Assign to `cardGameButton`

3. **Update button OnClick events**:
   - Morning Activity button:
     - Remove old OnClick event (if any)
     - Add new: `ActivitySelectionMenu.OnMorningActivityClicked()`
   
   - Card Game button:
     - Remove old OnClick event
     - Add new: `ActivitySelectionMenu.OnCardGameClicked()`
   
   - Cleaning button:
     - Remove old OnClick event
     - Add new: `ActivitySelectionMenu.OnCleaningClicked()`

4. **(Optional) Add Continue Indicators**:
   - Create UI Text/Icon: "▶ Continue" or "Resume"
   - Place near each activity button
   - Assign to `section1ContinueIcon`, `section2ContinueIcon`, `section3ContinueIcon`
   - Icons auto-show/hide based on checkpoint existence

---

### **Phase 4: (Optional) Main Menu Continue Button (10 minutes)**

**Scene**: `01_StartMenu.unity`

1. **Create "Continue" button**:
   - Duplicate existing "Play" button
   - Rename to "Continue"
   - Position above/below "Play" button

2. **Add ContinueButton component**:
   - Select "Continue" button GameObject
   - Add Component → `ContinueButton`
   - Assign `continueButton` reference in Inspector

3. **Update OnClick event**:
   - Remove old OnClick event
   - Add new: `ContinueButton.OnContinueClicked()`

4. **(Optional) Add info text**:
   - Create TextMeshProUGUI: "Last played: Section X"
   - Assign to `infoText` in Inspector
   - Enable `showSectionInfo` checkbox

---

### **Phase 5: (Optional) Reset Progress Button (5 minutes)**

**Scene**: Settings menu scene

1. **Add ResetProgressButton component**:
   - Select settings menu root GameObject
   - Add Component → `ResetProgressButton`

2. **Create reset button**:
   - Create button: "Reset All Progress"
   - OnClick → `ResetProgressButton.OnResetAllProgressClicked()`

3. **(Recommended) Add confirmation dialog**:
   - Before reset, show confirmation popup
   - See `ResetProgressButton.cs` TODO comments

---

## 🧪 Testing Checklist

### **✅ Test 1: Basic Save/Load**
1. Start game → Select gender → Choose "Morning Activity"
2. Play through `01_Section 1` → scene transitions to `02_Section 1`
3. Quit game
4. Restart → Select gender → Choose "Morning Activity"
5. **Expected**: Loads `02_Section 1` (not `01_Section 1`) ✅

---

### **✅ Test 2: Multiple Sections**
1. Play Section 1 → Progress to `05_Section 1`
2. Return to activity menu → Choose "Cleaning" (Section 3)
3. Play Section 3 → Progress to `03_Section 3`
4. Return to activity menu → Choose "Morning Activity"
5. **Expected**: Loads `05_Section 1` (Section 1 checkpoint preserved) ✅

---

### **✅ Test 3: Gender Persistence**
1. Select "Boy" → Play Section 1 → Progress to `05_Section 1`
2. Quit game
3. Restart → Select "Girl" → Choose "Morning Activity"
4. **Expected**: Loads `05_Section 1` with Boy gender restored ✅
5. Character sprite should be Boy (checkpoint restores saved gender)

---

### **✅ Test 4: Reset Section**
1. Have checkpoint in Section 1: `05_Section 1`
2. Call `CheckpointManager.Instance.ResetSectionProgress(1)`
3. Go to activity menu → Choose "Morning Activity"
4. **Expected**: Loads `01_Section 1` (default start scene) ✅

---

### **✅ Test 5: Continue Button**
1. Have checkpoint in Section 2: `02_Section 2`
2. Go to main menu
3. **Expected**: "Continue" button is enabled ✅
4. Click "Continue"
5. **Expected**: Loads `02_Section 2` with correct gender ✅

---

### **✅ Test 6: Continue Indicators**
1. Have checkpoint in Section 1 only
2. Go to activity menu
3. **Expected**: 
   - Section 1 shows "Continue" icon ✅
   - Section 2 shows no icon ❌
   - Section 3 shows no icon ❌

---

## 📊 PlayerPrefs Data Structure

### **Keys Used**:
```
PlayerPrefs Keys:
├─ "PlayerGender" (existing)         → int (0 = Boy, 1 = Girl)
├─ "Language" (existing)             → string ("en" / "id")
├─ "MusicVolume" (existing)          → float (0-1)
├─ "SFXVolume" (existing)            → float (0-1)
├─ "Checkpoint_Section1" (NEW)       → JSON string
├─ "Checkpoint_Section2" (NEW)       → JSON string
├─ "Checkpoint_Section3" (NEW)       → JSON string
└─ "LastPlayedSection" (NEW)         → int (1, 2, or 3)
```

### **Example JSON** (Checkpoint_Section1):
```json
{
  "section": 1,
  "sceneName": "05_Section 1",
  "gender": 0,
  "timestamp": 1726568191
}
```

---

## 🛠️ Advanced Usage

### **Manual Checkpoint Save**
```csharp
// From any script
CheckpointManager.Instance.SaveCheckpoint(1, "05_Section 1");
```

### **Check Checkpoint Existence**
```csharp
if (CheckpointManager.Instance.HasCheckpoint(1))
{
    Debug.Log("Section 1 has progress!");
}
```

### **Load Checkpoint Data**
```csharp
CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(1);
if (data != null)
{
    Debug.Log($"Last scene: {data.sceneName}");
    Debug.Log($"Gender: {data.gender}");
    Debug.Log(data.GetTimestampText()); // "Last played: 2 days ago"
}
```

### **Listen to Events**
```csharp
void Start()
{
    CheckpointManager.Instance.OnCheckpointSaved += OnCheckpointSaved;
    CheckpointManager.Instance.OnCheckpointReset += OnCheckpointReset;
}

void OnCheckpointSaved(int section, string sceneName)
{
    Debug.Log($"Checkpoint saved: Section {section}, Scene {sceneName}");
}

void OnCheckpointReset(int section)
{
    Debug.Log($"Section {section} reset!");
}
```

### **Debug Helpers** (Editor Only)
```csharp
// Print all checkpoints (Context Menu or code)
CheckpointManager.Instance.DebugPrintAllCheckpoints();

// Output:
// === CHECKPOINT DEBUG INFO ===
// Section 1: 05_Section 1 | Gender: 0 | Last played: 2 hours ago
// Section 2: No checkpoint
// Section 3: 03_Section 3 | Gender: 1 | Last played: 5 days ago
// Last Played Section: 3
// ============================
```

---

## 🔧 Troubleshooting

### **Issue: Checkpoint not saving**
- **Check**: `SceneCheckpoint` component added to scene?
- **Check**: Console for `"[CheckpointManager] Checkpoint saved..."` log?
- **Check**: Scene name contains "Section 1", "Section 2", or "Section 3"?

### **Issue: Checkpoint not loading**
- **Check**: `ActivitySelectionMenu` component assigned correctly?
- **Check**: Button OnClick events wired to correct methods?
- **Check**: Console for `"[CheckpointManager] Loading checkpoint scene..."` log?

### **Issue: Gender not restored**
- **Check**: `GetSceneToLoad()` is used (not direct `SceneSystem.Load()`)?
- **Check**: Gender is set before checkpoint save?

### **Issue: "Continue" button always disabled**
- **Check**: At least one checkpoint saved?
- **Check**: `ContinueButton` component has button reference assigned?
- **Check**: Console for `"[ContinueButton] Button state updated..."` log?

### **Issue: CheckpointManager not initialized**
- **Wait**: CheckpointManager auto-initializes on first scene load
- **Check**: `LoadingSystem` exists in first scene?
- **Fix**: Add retry logic (see `SceneCheckpoint.cs` for example)

---

## 📝 Notes

- **Scene Naming Convention**: The system parses section numbers from scene names. If you rename scenes, ensure they contain "Section 1", "Section 2", or "Section 3".

- **PlayerPrefs Persistence**: PlayerPrefs data persists across game sessions. Use `PlayerPrefs.DeleteAll()` (Editor menu) to clear all data during testing.

- **Gender Override**: When loading a checkpoint, the saved gender overrides the current gender selection. This ensures character consistency.

- **Single Checkpoint Per Section**: Each section has one checkpoint slot. New checkpoints overwrite previous ones.

- **Menu Scene Detection**: Scenes without "Section X" in the name are automatically skipped (no checkpoint saved).

---

## 🎯 Quick Reference

### **Save Checkpoint**
```csharp
CheckpointManager.Instance.SaveCheckpoint(section, sceneName);
```

### **Load Checkpoint**
```csharp
CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(section);
```

### **Reset Checkpoint**
```csharp
CheckpointManager.Instance.ResetSectionProgress(section);
```

### **Get Scene to Load**
```csharp
string scene = CheckpointManager.Instance.GetSceneToLoad(section);
SceneSystem.Load(scene);
```

---

## 📚 Related Files

- **Core System**: `Assets/_Game/00_Scripts/System/Checkpoint/`
  - `CheckpointManager.cs` - Main singleton
  - `CheckpointData.cs` - Data structure

- **Auto-Save**: `Assets/_Game/00_Scripts/Game/Checkpoint/`
  - `SceneCheckpoint.cs` - Auto-save component

- **UI Integration**: `Assets/_Game/00_Scripts/UI/Menu/`
  - `ActivitySelectionMenu.cs` - Activity menu
  - `ContinueButton.cs` - Continue button
  - `ResetProgressButton.cs` - Reset button

- **Existing Systems** (not modified):
  - `SaveSystem.cs` - JSON save/load system
  - `PlayerData.cs` - Gender management
  - `SceneLoader.cs` - Scene loading

---

**Created**: 2026-09-17  
**Version**: 1.0  
**Author**: Checkpoint System for INSOS JAYA JAYA
