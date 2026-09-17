# Checkpoint System - Integration Checklist

## ✅ Files Created (8 files)

### **Core System** (3 files)
- [x] `Assets/_Game/00_Scripts/System/Checkpoint/CheckpointManager.cs` - Main singleton system
- [x] `Assets/_Game/00_Scripts/System/Checkpoint/CheckpointData.cs` - Data structure
- [x] `Assets/_Game/00_Scripts/System/Checkpoint/README.md` - Full documentation

### **Auto-Save Component** (2 files)
- [x] `Assets/_Game/00_Scripts/Game/Checkpoint/SceneCheckpoint.cs` - Auto-save on scene load
- [x] `Assets/_Game/00_Scripts/Game/Checkpoint/CheckpointDebugPanel.cs` - Debug testing panel

### **UI Integration** (3 files)
- [x] `Assets/_Game/00_Scripts/UI/Menu/ActivitySelectionMenu.cs` - Activity menu with checkpoints
- [x] `Assets/_Game/00_Scripts/UI/Menu/ContinueButton.cs` - Main menu continue button
- [x] `Assets/_Game/00_Scripts/UI/Menu/ResetProgressButton.cs` - Reset progress button

---

## 🚀 Integration Steps (TODO)

### **Phase 1: Quick Test (5 minutes)**

- [ ] **Open Unity Editor**
  - Open project in Unity 2022.3.62f3
  - Wait for compilation to finish
  - Check Console for errors

- [ ] **Test CheckpointManager Initialization**
  - Play any scene
  - Look for console log: `"[CheckpointManager] Checkpoint system ready!"`
  - If you see this, the system is working! ✅

- [ ] **Test Checkpoint Save/Load (Debug Panel)**
  - Create new scene (or use existing test scene)
  - Create Canvas (`GameObject > UI > Canvas`)
  - Add component to Canvas: `CheckpointDebugPanel`
  - Press Play
  - Press `1` key → Should see: `"✓ Saved checkpoint: Section 1..."`
  - Press `P` key → Should see checkpoint debug info
  - Press `Q` key → Should see: `"✓ Section 1 checkpoint: 05_Section 1..."`

---

### **Phase 2: Add SceneCheckpoint to Scenes (30-60 minutes)**

**Option A: Quick Test (Test 1 scene first)**

- [ ] **Test with one scene**
  - Open scene: `Assets/_Game/04_Scenes/Section 1/01_Section 1.unity`
  - Create empty GameObject: Name it `"[CheckpointSystem]"`
  - Add component: `SceneCheckpoint`
  - Press Play
  - Check Console: Should see `"[SceneCheckpoint] ✓ Checkpoint saved: Section 1, Scene '01_Section 1'"`
  - Open another scene in Section 1 (e.g., `02_Section 1.unity`)
  - Repeat the same steps
  - Test scene transition: Should save checkpoint on each scene load

- [ ] **Verify PlayerPrefs (Windows Registry or Unity Editor)**
  - Windows: Open Registry Editor → `Computer\HKEY_CURRENT_USER\SOFTWARE\[Company Name]\[Product Name]`
  - Or use Unity Editor: `Edit > Clear All PlayerPrefs` (to reset)
  - Should see keys: `Checkpoint_Section1`, `LastPlayedSection`

**Option B: Full Implementation (After testing works)**

- [ ] **Create SceneCheckpoint Prefab**
  - Create empty GameObject
  - Name: `"SceneCheckpoint"`
  - Add component: `SceneCheckpoint`
  - Drag to: `Assets/_Game/01_Objects/Prefabs/System/` (create folder if needed)
  - Save as: `SceneCheckpoint.prefab`

- [ ] **Add prefab to Section 1 scenes (20 scenes)**
  ```
  Assets/_Game/04_Scenes/Section 1/
  ├─ 01_Section 1.unity ← Add prefab
  ├─ 02_Section 1.unity ← Add prefab
  ├─ 02_Section 1_1.unity ← Add prefab
  ├─ 02_Section 1_2.unity ← Add prefab
  ├─ 02_Section 1_3.unity ← Add prefab
  ├─ 02_Section 1_4.unity ← Add prefab
  ├─ 03_Section 1.unity ← Add prefab
  └─ ... (through 17_Section 1.unity)
  ```

- [ ] **Add prefab to Section 2 scenes (4 scenes)**
  ```
  Assets/_Game/04_Scenes/Section 2/
  ├─ 01_Section 2.unity ← Add prefab
  ├─ 02_Section 2.unity ← Add prefab
  ├─ 03_Section 2.unity ← Add prefab
  └─ TutorialTest.unity ← Add prefab (optional)
  ```

- [ ] **Add prefab to Section 3 scenes (10 scenes)**
  ```
  Assets/_Game/04_Scenes/Section 3/
  ├─ 01_Section 3.unity ← Add prefab
  ├─ 02_Section 3.unity ← Add prefab
  └─ ... (through 10_Section 3.unity)
  ```

- [ ] **SKIP menu scenes (no checkpoint needed)**
  ```
  ✗ 01_StartMenu.unity (skip)
  ✗ 02_ChooseGenderMenu.unity (skip)
  ✗ 03_ChooseActivityMenu.unity (skip)
  ```

**Tip**: Use multi-scene editing or batch operations to speed up prefab placement.

---

### **Phase 3: Activity Selection Menu Integration (10 minutes)**

- [ ] **Open Activity Selection Scene**
  - Scene: `Assets/_Game/04_Scenes/Menu/03_ChooseActivityMenu.unity`

- [ ] **Add ActivitySelectionMenu Component**
  - Find Canvas or UI root GameObject
  - Add Component → `ActivitySelectionMenu`
  - Check Inspector for component

- [ ] **Assign Button References**
  - In Inspector, find these fields:
    - `morningActivityButton` → Assign "Morning Activity" button
    - `cleaningButton` → Assign "Cleaning" button
    - `cardGameButton` → Assign "Card Game" button
  - Leave "Continue Indicators" empty for now (optional feature)

- [ ] **Update Button OnClick Events**
  - **Morning Activity Button**:
    - Select button in Hierarchy
    - Find `Button` component in Inspector
    - Scroll to `On Click ()` event
    - Click `+` to add new event
    - Drag Canvas (with ActivitySelectionMenu) to object field
    - Function dropdown: `ActivitySelectionMenu.OnMorningActivityClicked()`
    - Remove old OnClick event if any
  
  - **Card Game Button**:
    - Repeat above steps
    - Function: `ActivitySelectionMenu.OnCardGameClicked()`
  
  - **Cleaning Button**:
    - Repeat above steps
    - Function: `ActivitySelectionMenu.OnCleaningClicked()`

- [ ] **Test Activity Menu**
  - Save scene
  - Enter Play Mode
  - Click "Morning Activity" button
  - Check Console: Should see `"[ActivitySelectionMenu] Morning Activity clicked → Loading: ..."`
  - If checkpoint exists, should load checkpoint scene
  - If no checkpoint, should load `01_Section 1`

---

### **Phase 4: (Optional) Continue Button in Main Menu (10 minutes)**

- [ ] **Open Main Menu Scene**
  - Scene: `Assets/_Game/04_Scenes/Menu/01_StartMenu.unity`

- [ ] **Create Continue Button** (or modify existing button)
  - Option A: Duplicate "Play" button
  - Option B: Find existing "Continue" button
  - Rename to: "ContinueButton"
  - Position appropriately

- [ ] **Add ContinueButton Component**
  - Select "ContinueButton" GameObject
  - Add Component → `ContinueButton`
  - In Inspector:
    - Assign `continueButton` → Reference to Button component
    - (Optional) Assign `infoText` → TextMeshProUGUI for "Last played: Section X"
    - Check `showSectionInfo` if you want section info displayed

- [ ] **Wire OnClick Event**
  - Select button
  - Find `Button` component
  - `On Click ()` event:
    - Add new event
    - Drag button GameObject to object field
    - Function: `ContinueButton.OnContinueClicked()`

- [ ] **Test Continue Button**
  - Create checkpoint first (play any section scene)
  - Return to main menu
  - "Continue" button should be enabled
  - Click button → Should load checkpoint scene

---

### **Phase 5: (Optional) Reset Progress Button (5 minutes)**

- [ ] **Add Reset Button to Settings Menu**
  - Open settings/pause menu scene (if exists)
  - Create button: "Reset All Progress"
  - Add Component → `ResetProgressButton`
  - Wire OnClick: `ResetProgressButton.OnResetAllProgressClicked()`

- [ ] **Test Reset**
  - Create checkpoints
  - Click "Reset All Progress"
  - Check Console: `"[ResetProgressButton] ✓ All progress reset!"`
  - Verify PlayerPrefs cleared

---

## 🧪 Testing Protocol

### **Test 1: Basic Save & Load**
- [ ] Start game → Select gender → Choose "Morning Activity"
- [ ] Play scene `01_Section 1`
- [ ] Scene auto-transitions to `02_Section 1`
- [ ] Check Console: Checkpoint saved for `02_Section 1`
- [ ] Quit game (stop Play Mode)
- [ ] Restart game → Select gender → Choose "Morning Activity"
- [ ] **EXPECTED**: Should load `02_Section 1` (NOT `01_Section 1`) ✅

---

### **Test 2: Multiple Sections**
- [ ] Play Section 1 → Progress to `05_Section 1`
- [ ] Return to activity menu
- [ ] Choose "Cleaning" (Section 3)
- [ ] Play Section 3 → Progress to `03_Section 3`
- [ ] Return to activity menu
- [ ] Choose "Morning Activity" again
- [ ] **EXPECTED**: Should load `05_Section 1` (Section 1 checkpoint preserved) ✅

---

### **Test 3: Gender Persistence**
- [ ] Select "Boy" → Play Section 1 → Progress to `05_Section 1`
- [ ] Quit game
- [ ] Restart → Select "Girl" → Choose "Morning Activity"
- [ ] **EXPECTED**: 
  - Should load `05_Section 1` ✅
  - Character sprite should be Boy (checkpoint restores gender) ✅
  - Console: `"Gender restored: Boy"` ✅

---

### **Test 4: Reset Progress**
- [ ] Have checkpoint in Section 1: `05_Section 1`
- [ ] Call `CheckpointManager.Instance.ResetSectionProgress(1)` (via Debug Panel or Reset button)
- [ ] Go to activity menu → Choose "Morning Activity"
- [ ] **EXPECTED**: Should load `01_Section 1` (default start) ✅

---

### **Test 5: Continue Button**
- [ ] Create checkpoint in any section
- [ ] Go to main menu
- [ ] **EXPECTED**: "Continue" button is enabled ✅
- [ ] Click "Continue"
- [ ] **EXPECTED**: Loads last played checkpoint scene ✅

---

### **Test 6: No Checkpoint Scenario**
- [ ] Clear all PlayerPrefs (`Edit > Clear All PlayerPrefs`)
- [ ] Start game → Select gender → Choose any activity
- [ ] **EXPECTED**: Loads default start scene (e.g., `01_Section 1`) ✅
- [ ] "Continue" button should be disabled (if implemented) ✅

---

## 🐛 Common Issues & Fixes

### **Issue: "CheckpointManager not initialized"**
- **Cause**: CheckpointManager loads on first scene Start()
- **Fix**: 
  - Ensure `LoadingSystem` exists in scene
  - Wait one frame before accessing CheckpointManager
  - Use retry logic (see `SceneCheckpoint.cs` example)

### **Issue: Checkpoint not saving**
- **Cause**: Scene name doesn't contain "Section 1/2/3"
- **Fix**: 
  - Check scene name in Build Settings
  - Ensure name follows pattern: `"XX_Section Y"`
  - Check Console for `"[SceneCheckpoint] Scene 'X' is not a gameplay scene"`

### **Issue: Checkpoint loads wrong scene**
- **Cause**: Multiple checkpoints or incorrect section parsing
- **Fix**: 
  - Use Debug Panel: Press `P` to print all checkpoints
  - Verify correct section saved
  - Check `LastPlayedSection` key in PlayerPrefs

### **Issue: Gender not restored**
- **Cause**: Not using `GetSceneToLoad()` method
- **Fix**: 
  - Always use: `CheckpointManager.Instance.GetSceneToLoad(section)`
  - Never use: Direct `SceneSystem.Load("01_Section 1")`
  - `GetSceneToLoad()` automatically restores gender from checkpoint

### **Issue: Continue button always disabled**
- **Cause**: No checkpoints saved yet
- **Fix**: 
  - Play any section scene to create checkpoint
  - Use Debug Panel to create test checkpoints
  - Check Console: `"[ContinueButton] Button state updated..."`

---

## 📊 PlayerPrefs Keys Reference

```
Checkpoint System Keys:
├─ "Checkpoint_Section1" → JSON: {"section":1,"sceneName":"05_Section 1","gender":0,"timestamp":1726568191}
├─ "Checkpoint_Section2" → JSON: {"section":2,"sceneName":"02_Section 2","gender":1,"timestamp":1726568250}
├─ "Checkpoint_Section3" → JSON: {"section":3,"sceneName":"03_Section 3","gender":0,"timestamp":1726568300}
└─ "LastPlayedSection" → int: 2

Existing Keys (not modified):
├─ "PlayerGender" → int: 0 (Boy) or 1 (Girl)
├─ "Language" → string: "en" or "id"
├─ "MusicVolume" → float: 0-1
└─ "SFXVolume" → float: 0-1
```

---

## 📝 Quick Commands (Debug Console)

### **In Unity Console (C# Interactive) or via Script**
```csharp
// Save test checkpoint
CheckpointManager.Instance.SaveCheckpoint(1, "05_Section 1");

// Load checkpoint
CheckpointData data = CheckpointManager.Instance.LoadCheckpointProgress(1);

// Print all checkpoints
CheckpointManager.Instance.DebugPrintAllCheckpoints();

// Reset section
CheckpointManager.Instance.ResetSectionProgress(1);

// Clear all
CheckpointManager.Instance.ClearAllCheckpoints();

// Clear all PlayerPrefs (Unity Editor menu)
// Edit > Clear All PlayerPrefs
```

### **In Play Mode (with Debug Panel)**
```
Keyboard Shortcuts:
1 / 2 / 3   → Save checkpoint for Section 1 / 2 / 3
Q / W / E   → Load checkpoint for Section 1 / 2 / 3
R           → Reset all checkpoints
P           → Print debug info
```

---

## ✅ Final Checklist

- [ ] All 8 files created successfully
- [ ] Unity Editor compiles without errors
- [ ] CheckpointManager initializes on Play
- [ ] Debug Panel test successful (keyboard shortcuts work)
- [ ] SceneCheckpoint added to at least one test scene
- [ ] Checkpoint saves correctly on scene load
- [ ] ActivitySelectionMenu integrated
- [ ] Activity buttons load checkpoints
- [ ] (Optional) Continue button works
- [ ] (Optional) Reset button works
- [ ] Full gameplay test completed
- [ ] Gender persistence verified
- [ ] Multi-section test passed

---

## 🎯 Next Steps After Integration

1. **Batch Add SceneCheckpoint**: Use Unity Editor script or manual prefab placement
2. **Create Continue Button UI**: Design UI for main menu continue button
3. **Add Confirmation Dialogs**: Add "Are you sure?" dialog for reset actions
4. **Visual Indicators**: Add "Continue" icons/badges to activity buttons
5. **Analytics**: Track checkpoint saves for telemetry
6. **Cloud Save**: Export checkpoints to backend (future enhancement)

---

## 📚 Documentation

- **Full Documentation**: `Assets/_Game/00_Scripts/System/Checkpoint/README.md`
- **Code Examples**: See `CheckpointDebugPanel.cs` for usage examples
- **Architecture**: See AGENTS.md for initialization flow explanation

---

**Created**: 2026-09-17  
**Status**: ✅ Implementation Complete - Ready for Integration  
**Next**: Follow Phase 1 (Quick Test) to verify system works
