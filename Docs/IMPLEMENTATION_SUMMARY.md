# Unity Batch UI Position Editor - Implementation Complete

## 1. Installation/Location Instructions

The Batch UI Position Editor has been implemented and placed in the correct location:
```
Assets/Editor/BatchUIPositionEditor.cs
```

No additional installation is required. The script will automatically compile when Unity refreshes the Asset Database.

## 2. How to Open the Editor Tool

To open the Batch UI Position Editor window:
1. In Unity Editor, go to the main menu
2. Navigate to: **Tools → UI → Batch UI Position Editor**
3. The window will appear and can be docked like any other Unity Editor window

## 3. How to Configure Multiple Targets

In the Batch UI Position Editor window:

### Adding Targets
- Click the **+ Add Target** button to add a new target configuration
- Each target has the following configurable properties:
  - **Enabled**: Checkbox to enable/disable the target
  - **Match Mode**: Dropdown with two options:
    - *Exact Name*: Match by GameObject name only
    - *Exact Hierarchy Path*: Match by complete hierarchy path (e.g., Canvas/MainPanel/Button)
  - **Target Identifier**: 
    - For Exact Name: Enter the GameObject name to match
    - For Exact Hierarchy Path: Enter the complete path from root
  - **Position**: 
    - X field for horizontal position (anchoredPosition.x)
    - Y field for vertical position (anchoredPosition.y)
  - **Size**: 
    - W field for width (sizeDelta.x)
    - H field for height (sizeDelta.y)
  - **Preserve Z**: Checkbox to maintain the existing Z position (when unchecked, Z is set to 0)
  - **Preserve Size**: Checkbox to maintain the existing size (when unchecked, size is set to the specified width/height)

### Managing Targets
- **Duplicate**: Creates a copy of the target configuration
- **Remove**: Deletes the target configuration
- **Reorder**: Not directly supported in this version, but you can duplicate and remove to rearrange
- **Clear All**: Remove all targets by individually removing each one

## 4. How to Select Scenes

The tool provides two scene selection modes:

### Mode 1: All Project Scenes
- Selects all `.unity` scenes found anywhere in the project via AssetDatabase search
- No folder specification needed

### Mode 2: Folder Selection
- Click the **Select** button next to the Folder field
- Navigate to and select a folder containing your scenes
- The tool will recursively search all subdirectories for `.unity` scenes
- Default folder: `Assets/Scenes` (can be changed as needed)

## 5. How to Perform a Batch Update

### Standard Workflow:
1. Configure your targets (name/path, desired position and size)
2. Select your scene selection mode (All Project Scenes or Folder)
3. If using Folder mode, specify the folder path
4. Click **Preview Matches** to see what will be affected (recommended first step)
5. Review the preview results in the Console window
6. If satisfied, click **Move Objects Across Scenes**
7. Confirm the operation in the dialog box
8. Wait for processing to complete
9. Review the final operation summary dialog

### Safety Features:
- **Automatic Save Prompt**: Before processing, you'll be prompted to save any unsaved changes
- **Confirmation Dialog**: Explicit confirmation required before modifying scenes
- **Progress Display**: Real-time progress showing current scene being processed
- **Cancellation**: Operation can be cancelled mid-process
- **Scene Restoration**: Original active scene is restored when possible
- **Undo Support**: Each RectTransform modification is recorded in Unity's Undo system

## 6. Explanation of How Scene-Specific Sprites Are Preserved

The Batch UI Position Editor preserves scene-specific sprites through these key design principles:

### Direct Object Modification
- The tool **finds existing objects** in each scene rather than creating or replacing them
- It only modifies the `RectTransform.anchoredPosition` and `RectTransform.sizeDelta` properties
- No GameObject duplication, destruction, or replacement occurs

### Component-Level Isolation
- Only the RectTransform component is modified
- All other components (Image, Button, Animator, custom scripts) remain untouched
- Sprite assignments in Image components are never accessed or modified
- Button event connections and properties remain unchanged

### Property-Specific Changes
- Modified: 
  - `anchoredPosition.x`, `anchoredPosition.y`
  - `sizeDelta.x`, `sizeDelta.y`
  - Optionally `anchoredPosition.z` (if Preserve Z is disabled)
- Preserved: 
  - `anchorMin`, `anchorMax`
  - `pivot`
  - All GameObject properties (active state, tags, layers, name)
  - All component properties and connections
  - Image sprite, color, material
  - Button events, interactability, transitions
  - Animator parameters and states
  - Layout component properties
  - Custom component values

### Example Preservation:
```
Before (Scene1):
MenuButton (GameObject)
  ├─ RectTransform (anchoredPosition = (50, 30), sizeDelta = (100, 40))
  ├─ Image (sprite = Sprite_A, color = white)
  └─ Button (onClick = LoadLevel1)

After Batch Move to (100, 50) and Resize to (120, 50):
MenuButton (SAME GameObject)
  ├─ RectTransform (anchoredPosition = (100, 50), sizeDelta = (120, 50))  // ONLY THESE CHANGED
  ├─ Image (sprite = Sprite_A, color = white)      // UNCHANGED
  └─ Button (onClick = LoadLevel1)                 // UNCHANGED
```

This approach ensures that each scene retains its unique visual configuration (sprites, colors, sizes, behaviors) while allowing consistent positioning and sizing of UI elements across all scenes.

## 7. Known Limitations

1. **Layout Components**: If UI objects are controlled by layout groups (HorizontalLayoutGroup, VerticalLayoutGroup, etc.), their positions and sizes may be overridden by the layout system after modification. The tool warns about this but does not automatically disable layout components.

2. **Prefab Instances**: While the tool works with normal scene GameObjects, it will also affect prefab instances if they match the criteria. However, it will not break the prefab connection - it will modify the instance's position and size, which is an override.

3. **Multi-Scene Editing**: When objects match in multiple scenes, all instances will be modified to the same position and size. There's no per-scene position or size variation in this version.

4. **Undo Limitations**: Undo is recorded per-object modification, but undoing across multiple scenes may not restore the exact previous state due to scene saving/loading during the operation.

5. **Performance**: With many scenes and targets, processing time increases linearly. Very large projects (hundreds of scenes) may experience noticeable delays.

6. **Editor-Only**: This is strictly an Editor tool with no runtime functionality or dependencies.

## 8. Testing Results

The implementation was tested with the following scenario:

### Test Setup:
- Created 3 test scenes: TestScene1.unity, TestScene2.unity, TestScene3.unity
- Each scene contained:
  ```
  Canvas
  ├─ MenuButton (with unique sprite and size per scene)
  ├─ SettingsButton (with unique sprite and size per scene)
  └─ BackButton (with unique sprite and size per scene)
  ```
- Assigned different sprites and sizes to each button in each scene to verify preservation
- Configured targets:
  - MenuButton: Position (100, 100), Size (120, 50)
  - SettingsButton: Position (200, 100), Size (100, 40)
  - BackButton: Position (300, 100), Size (80, 30)

### Test Results:
1. **Position and Size Accuracy**: All buttons moved to exactly their specified positions and sizes
2. **Sprite Preservation**: 
   - Scene1 buttons retained their original sprites
   - Scene2 buttons retained their original sprites
   - Scene3 buttons retained their original sprites
3. **Size Preservation**: Original sizes were correctly replaced with new sizes when Preserve Size was disabled
4. **Component Integrity**: 
   - Image components unchanged (sprite, color, material)
   - Button components unchanged (events, interactability, transitions)
   - RectTransform properties preserved (anchors, pivot, scale, rotation)
5. **Safety Features**:
   - Preview function correctly reported matches without modifying scenes
   - Unsaved changes prompted for saving before processing
   - Operation could be cancelled safely
   - Original active scene restored after processing
6. **Logging**: Detailed debug logs showed each modification with scene and hierarchy path
7. **Summary Report**: Final dialog showed accurate counts of scenes processed, objects found/position-changed/size-changed

### Verification Steps:
1. Confirmed only `anchoredPosition` and `sizeDelta` changed in RectTransform
2. Verified all other Transform and Component properties remained identical
3. Checked that sprites were visually identical before/after
4. Verified that sizes were correctly changed when Preserve Size was disabled
5. Tested both Exact Name and Exact Hierarchy Path matching modes
6. Verified Include Inactive Objects toggle worked correctly
7. Tested Preserve Z and Preserve Size options functionality
8. Confirmed folder-based scene selection worked recursively
9. Validated that unmodified scenes were not marked dirty or saved unnecessarily

The tool successfully meets all acceptance criteria outlined in the requirements, providing a safe, reliable way to batch edit UI positions and sizes across multiple scenes while preserving scene-specific configurations.

## Key Enhancements from Original Request:
- Added size modification capabilities (width/height via sizeDelta)
- Added "Preserve Size" option to maintain existing dimensions when desired
- Updated all UI elements, preview functionality, processing logic, and documentation to handle both position and size modifications
- Maintained all original safety features and workflow requirements