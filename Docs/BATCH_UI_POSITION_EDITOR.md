# Unity Batch UI Position Editor

## 1. Objective

Create a Unity Editor tool that allows the developer to move and resize multiple existing UI objects across multiple Unity scenes in a single batch operation.

The tool is intended for UI objects that already exist independently in each scene and do **not** currently use prefabs.

The main purpose is to allow common UI elements to be repositioned and resized consistently across many scenes while preserving each scene's individual configuration.

Example:

```text
Scene1
└── Canvas
    └── MenuButton
        └── Image
            └── Sprite_A

Scene2
└── Canvas
    └── MenuButton
        └── Image
            └── Sprite_B

Scene3
└── Canvas
    └── MenuButton
        └── Image
            └── Sprite_C
```

The developer should be able to move and resize all three `MenuButton` objects to a new position and size without converting them into prefabs and without manually editing each scene.

The same operation must support multiple different target objects.

---

# 2. Target Environment

The project uses:

```text
Unity 2022.3.62f3
```

Implementation should target the Unity 2022.3 LTS Editor API.

Use modern Unity Editor APIs where appropriate.

The tool must be Editor-only.

Recommended location:

```text
Assets/
└── Editor/
    └── BatchUIPositionEditor.cs
```

Do not create a runtime dependency.

The tool should not require a MonoBehaviour to be placed in any scene.

---

# 3. Core Requirement

The tool must support a list of multiple target UI objects.

For example:

```text
Target 1:
    Name: MenuButton
    Position: X=100, Y=50
    Size: Width=120, Height=40

Target 2:
    Name: SettingsButton
    Position: X=200, Y=100
    Size: Width=100, Height=50

Target 3:
    Name: BackButton
    Position: X=50, Y=-50
    Size: Width=80, Height=30
```

When the batch operation runs, every enabled target is searched for in every selected scene.

Each matching object receives its own configured position and size.

---

# 4. Important: Preserve Scene-Specific Properties

The most important requirement is that the tool must **not turn the objects into prefabs**.

The objects already exist independently in each scene.

Example:

```text
Scene1:
    MenuButton → Sprite_A

Scene2:
    MenuButton → Sprite_B

Scene3:
    MenuButton → Sprite_C
```

After the batch operation:

```text
Scene1:
    MenuButton → Sprite_A
    Position → New Position
    Size → New Size

Scene2:
    MenuButton → Sprite_B
    Position → New Position
    Size → New Size

Scene3:
    MenuButton → Sprite_C
    Position → New Position
    Size → New Size
```

The sprites must remain exactly as they were.

Do not:
- Create prefabs.
- Replace sprites.
- Copy Image components.
- Copy Button components.
- Copy GameObjects between scenes.
- Copy component values from one scene to another.
- Recreate the UI objects.
- Modify unrelated properties.

The tool should operate directly on the existing GameObjects inside each scene.

---

# 5. What Properties Should Be Modified?

For UI objects, the properties to modify are:

```csharp
RectTransform.anchoredPosition
RectTransform.sizeDelta
```

Example:

```csharp
RectTransform rectTransform = target.GetComponent<RectTransform>();

rectTransform.anchoredPosition = new Vector2(x, y);
rectTransform.sizeDelta = new Vector2(width, height);
```

Only the anchored position and size delta should be changed.

Do NOT modify:

```text
anchorMin
anchorMax
pivot
offsetMin
offsetMax
localScale
localRotation
rotation
position
```

unless a future feature explicitly requests those properties.

The default operation must only change:

```text
anchoredPosition.x
anchoredPosition.y
sizeDelta.x
sizeDelta.y
```

---

# 6. Z Position

Provide an option:

```text
Preserve Z Position
```

When enabled:

- Preserve the existing `anchoredPosition3D.z`.

When disabled:

- Set Z to 0.

Preferred implementation:

```csharp
Vector3 position = rectTransform.anchoredPosition3D;

position.x = targetX;
position.y = targetY;

if (!preserveZ)
    position.z = 0f;

rectTransform.anchoredPosition3D = position;
```

---

# 7. Size Preservation

Provide an option:

```text
Preserve Size
```

When enabled:

- Preserve the existing `sizeDelta`.

When disabled:

- Set size to the specified width and height.

Preferred implementation:

```csharp
if (!preserveSize)
{
    rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
}
```

---

# 8. Target Configuration

Each target should contain at least:

```text
Enabled
Target Name
Matching Mode
Hierarchy Path
X
Y
Width
Height
Preserve Z
Preserve Size
```

Conceptually:

```text
TargetConfiguration
{
    bool enabled;
    string targetName;
    MatchMode matchMode;
    string hierarchyPath;
    Vector2 anchoredPosition;
    bool preserveZ;
    Vector2 sizeDelta;
    bool preserveSize;
}
```

The exact class structure is up to the implementation.

---

# 9. Matching Modes

Support at least two matching modes.

## 9.1 Exact GameObject Name

Example:

```text
Match Mode: Exact Name
Name: MenuButton
```

Match:

```csharp
gameObject.name == targetName
```

Do not use partial matching by default.

For example:

```text
MenuButton
```

should NOT automatically match:

```text
MenuButton(Clone)
MainMenuButton
MenuButton2
```

---

## 9.2 Exact Hierarchy Path

Allow a target to specify an exact hierarchy path.

Example:

```text
Canvas/MainPanel/MenuButton
```

The tool should construct the target object's hierarchy path and compare it against the configured path.

Example:

```text
Canvas
└── MainPanel
    └── MenuButton
```

Path:

```text
Canvas/MainPanel/MenuButton
```

Hierarchy matching should use the complete path, not only the GameObject name.

---

# 10. Optional Component Matching

If practical, the implementation may provide additional matching modes such as:

```text
Component Type
Tag
```

However, these are optional.

Do not make the implementation unnecessarily complicated if exact name and hierarchy path provide sufficient functionality.

The core implementation must support:

- Exact Name
- Exact Hierarchy Path

---

# 11. Multiple Targets

The EditorWindow must provide a dynamic target list.

Example:

```text
Targets
─────────────────────────────────────

[✓] MenuButton
    Match: Exact Name
    Position: X 100   Y 50
    Size: W 120   H 40
    Preserve Z: ✓
    Preserve Size: ✓

[✓] SettingsButton
    Match: Exact Name
    Position: X 200   Y 100
    Size: W 100   H 50
    Preserve Z: ✓
    Preserve Size: ✓

[✓] BackButton
    Match: Exact Hierarchy Path
    Path: Canvas/Panel/BackButton
    Position: X 50   Y -50
    Size: W 80   H 30
    Preserve Z: ✓
    Preserve Size: ✓

─────────────────────────────────────

[ + Add Target ]
```

The user should be able to:

- Add target.
- Remove target.
- Duplicate target.
- Enable/disable target.
- Edit target values.
- Reorder targets if practical.
- Clear targets.

Disabled targets must not be processed.

---

# 12. Target List Usability

The tool should remain usable when there are many targets.

It should be possible to configure dozens of UI objects without the EditorWindow becoming unusable.

A scrollable target list is recommended.

Use a Unity 2022-compatible UI implementation.

Possible approaches:

- Custom EditorGUI list.
- `ReorderableList`.
- UI Toolkit.
- IMGUI.

Choose the simplest robust implementation appropriate for the project.

Do not introduce unnecessary dependencies.

---

# 13. Scene Selection

The tool must allow the user to determine which scenes should be processed.

At minimum support:

```text
Selected Scenes Folder
```

Example:

```text
Assets/Scenes/
```

All `.unity` scenes inside the selected folder should be discovered.

Preferably also support:

```text
Process All Project Scenes
```

Example UI:

```text
Scene Selection

Mode:
( ) All Project Scenes
(•) Scenes In Folder

Folder:
[ Assets/Scenes ]
```

If folder selection is implemented, use Unity's asset/folder selection mechanisms rather than requiring the user to type a path manually.

---

# 14. Recursive Scene Search

When using a folder:

Prefer searching subdirectories recursively.

Example:

```text
Assets/Scenes/
├── Scene1.unity
├── Scene2.unity
└── Chapters/
    ├── Scene3.unity
    └── Scene4.unity
```

All valid scenes under the selected folder should be discoverable.

If recursive searching is not desired, clearly document the behavior.

---

# 15. Scene Processing

When the user clicks:

```text
Move Objects Across Scenes
```

the tool should:

1. Validate configuration.
2. Find selected scenes.
3. Display confirmation.
4. Safely handle currently open scenes and unsaved changes.
5. Open each target scene.
6. Find matching objects for every enabled target.
7. Modify only the configured RectTransform position and size.
8. Record Undo information where appropriate.
9. Mark modified scenes dirty.
10. Save modified scenes.
11. Restore the previously active scene when practical.
12. Display a detailed operation summary.

---

# 16. Finding Objects

For each scene:

```text
Scene
    ↓
Find matching objects
    ↓
For each enabled target
    ↓
Apply target-specific position and size
```

The implementation can use:

```csharp
Scene.GetRootGameObjects()
```

and recursively inspect their children.

For example:

```csharp
foreach (GameObject root in scene.GetRootGameObjects())
{
    Transform[] transforms =
        root.GetComponentsInChildren<Transform>(true);
}
```

Use `true` if inactive objects should be included.

---

# 17. Include Inactive Objects

Provide an option:

```text
Include Inactive Objects
```

Default:

```text
Enabled
```

When enabled, inactive GameObjects should also be searched.

When disabled, inactive objects should be ignored.

Do not activate inactive objects merely to inspect them.

---

# 18. RectTransform Validation

A target can match a GameObject that does not have a RectTransform.

If that happens:

```text
Target found
        ↓
Has RectTransform?
        ↓
No
        ↓
Skip + warning
```

Do not automatically add a RectTransform.

Log a useful warning:

```text
Target "ExampleButton" was found in Scene1,
but it does not have a RectTransform and was skipped.
```

---

# 19. Button Component Validation

The tool is intended primarily for UI buttons, but it should not necessarily require a `Button` component.

The core requirement is:

```text
GameObject
+
RectTransform
```

A UI object can therefore be moved and resized even if it is not technically a Unity `Button`.

Optionally provide a validation warning if the object does not have:

```csharp
UnityEngine.UI.Button
```

but do not require it unless explicitly configured.

---

# 20. Layout System Warning

Some UI objects are controlled by layout components.

Examples:

```text
HorizontalLayoutGroup
VerticalLayoutGroup
GridLayoutGroup
ContentSizeFitter
LayoutElement
```

If a target is controlled by a layout system, manually changing its anchored position or size may be overridden.

The tool should not automatically disable or modify layout components.

If practical, detect likely layout-controlled objects and warn the user.

Example:

```text
Warning:
"MenuButton" appears to be controlled by a UI Layout Group.
Its position and size may be overridden by the layout system.
```

Do not modify layout settings automatically.

---

# 21. Sprite Preservation

The implementation must guarantee that changing the position and size does not modify the Image component.

For example:

```text
Scene1:
MenuButton
    RectTransform
    Button
    Image → Sprite_A

Scene2:
MenuButton
    RectTransform
    Button
    Image → Sprite_B
```

The tool must only perform something equivalent to:

```csharp
rectTransform.anchoredPosition = targetPosition;
rectTransform.sizeDelta = targetSize;
```

It must NOT perform operations such as:

```csharp
image.sprite = ...
```

or:

```csharp
PrefabUtility...
```

or recreate the GameObject.

---

# 22. Other Properties Must Remain Unchanged

The tool must preserve:

```text
GameObject active state
Transform hierarchy
RectTransform anchors
RectTransform pivot
RectTransform scale
RectTransform rotation
Image sprite
Image color
Image material
Button events
Button interactability
Animator
Layout settings
Custom components
Tags
Layers
Names
```

Only the requested position and size should change.

---

# 23. Undo Support

Use Unity's Undo system.

Before changing each RectTransform:

```csharp
Undo.RecordObject(
    rectTransform,
    "Batch Move and Resize UI Object"
);
```

The implementation should integrate with Unity's Editor Undo system as far as practical.

Because scenes may be opened and saved during the operation, document any limitations of Undo across scene boundaries.

Do not rely on Undo as the only data-safety mechanism.

---

# 24. Scene Saving

After modifying a scene:

```text
Mark Scene Dirty
        ↓
Save Scene
```

Use Unity 2022's:

```csharp
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
```

Do not save scenes that were not modified.

If a scene contains no matching targets, it should preferably not be marked dirty or rewritten.

---

# 25. Unsaved Changes

Before starting the batch operation, detect unsaved modifications in currently open scenes.

Do not silently discard user changes.

Use Unity's scene-saving APIs to safely handle the current state.

If user changes would be lost by opening another scene, ask for confirmation or provide a safe save/cancel workflow.

The tool should prioritize preventing accidental data loss.

---

# 26. Active Scene Restoration

Before batch processing, store the currently active scene.

After processing, attempt to restore it.

Example concept:

```text
Current:
Scene2

Batch:
Scene1
Scene2
Scene3

After:
Scene2
```

If restoration fails, report it rather than silently ignoring the issue.

---

# 27. Progress Display

Batch processing can involve many scenes and targets.

Provide progress feedback using Unity's Editor progress APIs where appropriate.

Example:

```text
Batch Moving and Resizing UI Objects

Scene 7 / 25

Processing:
Assets/Scenes/Chapter2/Level03.unity

Progress: ███████░░░ 70%
```

Allow cancellation if practical.

If the user cancels:
- Stop safely.
- Save already completed scene changes if appropriate.
- Restore the previous scene if possible.
- Report what was processed.

---

# 28. Confirmation Dialog

Before modifying scenes, show a confirmation dialog.

Example:

```text
Batch UI Position and Size Update

Scenes:
25

Targets:
8

This operation will modify and save matching RectTransform positions and sizes in the selected scenes.

Sprites and other object properties will not be changed.

Continue?

[Cancel] [Apply Changes]
```

Do not execute the batch operation without user confirmation.

---

# 29. Preview / Dry Run

A preview mode is strongly recommended.

Example:

```text
[Preview Matches]
```

The preview should scan the selected scenes without modifying them.

Example output:

```text
Scene1
    MenuButton → Found
    SettingsButton → Found
    BackButton → Not Found

Scene2
    MenuButton → Found
    SettingsButton → Found
    BackButton → Found
```

This helps prevent accidental edits.

Preview must not save or modify scenes.

If implementing a preview is too large for the initial version, keep the architecture compatible with adding it later.

---

# 30. Operation Summary

After processing, display a summary.

Example:

```text
Batch Operation Complete

Scenes processed: 12
Scenes modified: 10

Objects found: 27
Objects position changed: 27
Objects size changed: 25

Missing targets: 4
Invalid targets: 1
Skipped objects: 2

Failed scenes: 0
```

Provide per-target information where useful.

Example:

```text
MenuButton
    Found: 12
    Position changed: 12
    Size changed: 10
    Missing: 0

SettingsButton
    Found: 11
    Position changed: 11
    Size changed: 9
    Missing: 1
```

---

# 31. Logging

Use `Debug.Log`, `Debug.LogWarning`, and `Debug.LogError` appropriately.

Logs should identify:

- Scene.
- Target.
- Hierarchy path.
- Action taken.
- Failure reason when applicable.

Example:

```text
[Batch UI] Moved and resized:
Scene1 / Canvas/MainPanel/MenuButton
Position: (100, 50) → (250, 100)
Size: (120, 40) → (200, 60)
```

Avoid excessive logging for extremely large projects unless verbose logging is enabled.

---

# 32. Duplicate Matches

A target may match multiple objects.

Example:

```text
Scene1
├── CanvasA/MenuButton
└── CanvasB/MenuButton
```

If matching by exact name:

```text
MenuButton
```

both objects technically match.

The tool must not silently hide this situation.

Provide a clear count:

```text
MenuButton:
2 matches found in Scene1
```

Recommended behavior:

- Move and resize all exact-name matches.
- Clearly report the number of matches.
- Offer hierarchy-path matching for users who want a single object.

Optionally provide a strict mode:

```text
Warn on Multiple Matches
```

Do not invent arbitrary behavior for duplicates.

---

# 33. Target Conflict Detection

The tool should detect when two target configurations could modify the same object.

Example:

```text
Target 1:
Name = MenuButton

Target 2:
Path = Canvas/MenuButton
```

If both identify the same object, the tool should warn the user before execution.

Possible behavior:

```text
Warning:

The following object matches multiple targets:

Scene1/Canvas/MenuButton

Target 1 → Position (100, 50), Size (120, 40)
Target 2 → Position (200, 100), Size (100, 50)

The final position and size would be ambiguous.

[Cancel] [Review Targets]
```

Prefer preventing ambiguous modifications rather than relying on target ordering.

---

# 34. Target Configuration Persistence

The target list should preferably persist between EditorWindow sessions.

For example, closing and reopening:

```text
Tools → UI → Batch UI Position Editor
```

should not necessarily erase the configured target list.

Possible implementation:

- `EditorPrefs`
- Serialized EditorWindow state
- ScriptableObject configuration asset

Choose the approach appropriate for the tool.

Do not create unnecessary project assets unless useful.

---

# 35. Optional Configuration Assets

A future-friendly design may support saving target configurations as assets.

Example:

```text
Assets/
└── Editor/
    └── BatchUI/
        └── UIPositionProfile.asset
```

A profile could contain:

```text
Profile: Main Menu UI

Targets:
    MenuButton
    SettingsButton
    BackButton
    InventoryButton
```

This is optional for the initial implementation.

Do not make profile assets mandatory.

---

# 36. Editor Window Layout

Suggested interface:

```text
┌──────────────────────────────────────────────────────┐
│ Batch UI Position Editor                             │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Scene Selection                                      │
│                                                      │
│ Mode: (•) Folder   ( ) All Project Scenes            │
│ Folder: [ Assets/Scenes                         ]    │
│                                                      │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Targets                                              │
│                                                      │
│ ┌──────────────────────────────────────────────────┐ │
│ │ [✓] MenuButton                         [Duplicate]│ │
│ │                                                  │ │
│ │ Match: [Exact Name ▼]                            │ │
│ │ Name:  [MenuButton                         ]      │ │
│ │                                                  │ │
│ │ Position: X [100]   Y [50]                       │ │
│ │ Size: W [120]   H [40]                           │ │
│ │ Preserve Z: [✓]                                  │ │
│ │ Preserve Size: [✓]                               │ │
│ │                                                  │ │
│ │                                      [Remove]     │ │
│ └──────────────────────────────────────────────────┘ │
│                                                      │
│ ┌──────────────────────────────────────────────────┐ │
│ │ [✓] SettingsButton                    [Duplicate]│ │
│ │                                                  │ │
│ │ Match: [Exact Name ▼]                            │ │
│ │ Name:  [SettingsButton                      ]    │ │
│ │                                                  │ │
│ │ Position: X [200]   Y [100]                      │ │
│ │ Size: W [100]   H [50]                           │ │
│ │ Preserve Z: [✓]                                  │ │
│ │ Preserve Size: [✓]                               │ │
│ │                                                  │ │
│ │                                      [Remove]     │ │
│ └──────────────────────────────────────────────────┘ │
│                                                      │
│ [+ Add Target]                                       │
│                                                      │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Options                                              │
│ [✓] Include Inactive Objects                         │
│ [✓] Warn on Multiple Matches                         │
│ [✓] Preserve Z Position                             │
│ [✓] Preserve Size                                   │
│                                                      │
├──────────────────────────────────────────────────────┤
│                                                      │
│ [Preview Matches]      [Move Objects Across Scenes] │
│                                                      │
└──────────────────────────────────────────────────────┘
```

The exact visual design can be improved by the agent.

---

# 37. Per-Target Position and Size

Each target must have its own independent position and size.

Example:

```text
MenuButton:
    Position: X = 100, Y = 50
    Size: Width = 120, Height = 40

SettingsButton:
    Position: X = 250, Y = 50
    Size: Width = 100, Height = 50

BackButton:
    Position: X = -100, Y = 50
    Size: Width = 80, Height = 30
```

Do not use one global position or size for all targets.

---

# 38. Optional Relative Movement and Resizing

The initial implementation should primarily support absolute positioning and sizing:

```text
Set position to:
X = 100
Y = 50

Set size to:
Width = 120
Height = 40
```

A future enhancement may support relative movement and resizing:

```text
Move by:
X = +20
Y = -10

Resize by:
Width = +10
Height = -5
```

Do not make relative movement and resizing mandatory for the first implementation.

If implemented, clearly distinguish:

```text
Absolute Position and Size
```

from:

```text
Relative Offset and Delta
```

to avoid accidental movement and resizing.

---

# 39. Scene-Specific Position and Size Override

Do not assume the objects currently have identical positions or sizes.

Example:

```text
Scene1:
MenuButton = (100, 50), size = (120, 40)

Scene2:
MenuButton = (150, 60), size = (100, 50)

Scene3:
MenuButton = (90, 45), size = (140, 35)
```

The batch tool should still be able to set all of them to:

```text
(200, 100), size = (150, 60)
```

It should not require their existing positions or sizes to match.

---

# 40. No Prefab Requirement

The tool must work with:

```text
Normal scene GameObjects
```

It must not require:

```text
Prefab instances
```

The user specifically wants to avoid manually assigning scene-specific sprites again.

Therefore, prefab conversion is not part of the workflow.

---

# 41. Example Complete Workflow

Suppose the project contains:

```text
Assets/Scenes/
├── MainMenu.unity
├── Level1.unity
├── Level2.unity
└── Level3.unity
```

Every scene contains:

```text
Canvas
├── MenuButton
├── SettingsButton
└── BackButton
```

But sprites and sizes differ:

```text
MainMenu:
    MenuButton → MainMenuSprite, size = (100, 40)
    SettingsButton → MainSettingsSprite, size = (80, 30)
    BackButton → MainBackSprite, size = (60, 30)

Level1:
    MenuButton → Level1Sprite, size = (120, 50)
    SettingsButton → Level1SettingsSprite, size = (90, 40)
    BackButton → Level1BackSprite, size = (70, 35)

Level2:
    MenuButton → Level2Sprite, size = (110, 45)
    SettingsButton → Level2SettingsSprite, size = (85, 35)
    BackButton → Level2BackSprite, size = (65, 32)
```

Configure:

```text
Target 1:
    MenuButton
    Position: (100, 50)
    Size: Width=120, Height=40

Target 2:
    SettingsButton
    Position: (250, 50)
    Size: Width=100, Height=50

Target 3:
    BackButton
    Position: (-100, 50)
    Size: Width=80, Height=30
```

Run:

```text
Move Objects Across Scenes
```

Result:

```text
Every MenuButton:
    anchoredPosition = (100, 50)
    sizeDelta = (120, 40)

Every SettingsButton:
    anchoredPosition = (250, 50)
    sizeDelta = (100, 50)

Every BackButton:
    anchoredPosition = (-100, 50)
    sizeDelta = (80, 30)
```

All sprites remain exactly as they were.

---

# 42. Important Technical Constraint

Do not implement this by copying a source GameObject into other scenes.

The operation should conceptually be:

```text
Scene1:
    Find existing MenuButton
    Modify its RectTransform position and size

Scene2:
    Find existing MenuButton
    Modify its RectTransform position and size

Scene3:
    Find existing MenuButton
    Modify its RectTransform position and size
```

NOT:

```text
Scene1:
    Copy MenuButton

Scene2:
    Delete existing MenuButton
    Paste Scene1 version

Scene3:
    Delete existing MenuButton
    Paste Scene1 version
```

The latter would risk destroying scene-specific configuration and sprites.

---

# 43. Do Not Modify Image Components

The implementation should not need to reference:

```csharp
UnityEngine.UI.Image
```

for the actual modification.

The core operation only needs:

```csharp
RectTransform
```

This naturally protects sprite assignments.

If Image validation is added, it must be read-only.

---

# 44. Scene Asset Discovery

Use Unity's AssetDatabase to locate scenes.

For example:

```csharp
AssetDatabase.FindAssets("t:Scene");
```

For folder filtering, pass the selected folder as the search scope where appropriate.

Convert GUIDs into scene paths using:

```csharp
AssetDatabase.GUIDToAssetPath(...)
```

Only process assets ending in:

```text
.unity
```

---

# 45. Scene APIs

Use Unity 2022.3 scene APIs such as:

```csharp
UnityEditor.SceneManagement.EditorSceneManager
```

Relevant methods may include:

```csharp
EditorSceneManager.OpenScene(...)
EditorSceneManager.SaveScene(...)
EditorSceneManager.MarkSceneDirty(...)
EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
```

Use the APIs appropriately rather than relying on obsolete Unity 5 APIs.

---

# 46. Compatibility

Target:

```text
Unity 2022.3.62f3
```

Do not optimize for Unity 5 compatibility.

Modern Unity 2022.3 Editor APIs are allowed.

Do not use APIs introduced after Unity 2022.3 unless there is a strong reason.

---

# 47. File Organization

Recommended:

```text
Assets/
└── Editor/
    └── BatchUIPositionEditor.cs
```

A single `.cs` file is also acceptable if the implementation is small enough.

Do not over-engineer the architecture.

The final implementation should be easy to modify later.

---

# 48. Code Quality

The implementation should:

- Use clear names.
- Avoid unnecessary allocations where practical.
- Avoid modifying unrelated assets.
- Handle exceptions safely.
- Avoid leaving scenes open unexpectedly.
- Clear progress bars when finished or cancelled.
- Provide useful warnings.
- Avoid silent failures.
- Keep Editor-only functionality inside `Assets/Editor/`.

---

# 49. Error Handling

Handle at least:

```text
No target configured
No scenes found
Invalid scene folder
Scene failed to open
Scene failed to save
Target not found
Target has no RectTransform
Duplicate target configuration
Multiple object matches
User cancellation
Unsaved scene changes
```

The tool should never silently fail.

---

# 50. Acceptance Criteria

The implementation is considered complete when:

- [ ] Works in Unity 2022.3.62f3.
- [ ] EditorWindow exists.
- [ ] Tool accessible through `Tools → UI`.
- [ ] Multiple targets can be configured.
- [ ] Each target has its own position and size.
- [ ] Targets can be enabled/disabled.
- [ ] Exact GameObject name matching works.
- [ ] Exact hierarchy path matching works.
- [ ] Multiple matching objects are reported.
- [ ] Inactive object option works.
- [ ] Scenes can be selected by folder.
- [ ] All project scenes can optionally be processed.
- [ ] Existing scene objects are modified directly.
- [ ] No prefab conversion is required.
- [ ] Scene-specific sprites remain unchanged.
- [ ] Image components remain unchanged.
- [ ] Button components remain unchanged.
- [ ] Button events remain unchanged.
- [ ] Anchors remain unchanged.
- [ ] Pivot remains unchanged.
- [ ] Scale remains unchanged.
- [ ] Rotation remains unchanged.
- [ ] Only RectTransform position and size change.
- [ ] Modified scenes are saved.
- [ ] Unmodified scenes are not unnecessarily rewritten.
- [ ] Undo is supported where practical.
- [ ] Unsaved scene changes are handled safely.
- [ ] Progress is displayed.
- [ ] Cancellation is handled safely.
- [ ] Final operation summary is displayed.
- [ ] Useful logs and warnings are generated.
- [ ] Active scene is restored where practical.

---

# 51. Testing Scenario

The agent should test the implementation with at least three scenes.

Create or use:

```text
Scene1
Scene2
Scene3
```

Each scene should contain:

```text
Canvas
├── MenuButton
├── SettingsButton
└── BackButton
```

Give each button a different sprite and size in each scene.

Example:

```text
Scene1:
MenuButton → Sprite_A, size = (100, 40)
SettingsButton → Sprite_D, size = (80, 30)
BackButton → Sprite_G, size = (60, 30)

Scene2:
MenuButton → Sprite_B, size = (120, 50)
SettingsButton → Sprite_E, size = (90, 40)
BackButton → Sprite_H, size = (70, 35)

Scene3:
MenuButton → Sprite_C, size = (110, 45)
SettingsButton → Sprite_F, size = (85, 35)
BackButton → Sprite_I, size = (65, 32)
```

Configure:

```text
MenuButton:
    Position: (100, 50)
    Size: Width=120, Height=40

SettingsButton:
    Position: (250, 50)
    Size: Width=100, Height=50

BackButton:
    Position: (-100, 50)
    Size: Width=80, Height=30
```

Run the batch operation.

Verify:

```text
All MenuButtons:
    Position = (100, 50)
    Size = (120, 40)

All SettingsButtons:
    Position = (250, 50)
    Size = (100, 50)

All BackButtons:
    Position = (-100, 50)
    Size = (80, 30)
```

Then verify:

```text
Scene1 sprites and sizes remain:
A / D / G
sizes: (100,40) / (80,30) / (60,30)

Scene2 sprites and sizes remain:
B / E / H
sizes: (120,50) / (90,40) / (70,35)

Scene3 sprites and sizes remain:
C / F / I
sizes: (110,45) / (85,35) / (65,32)
```

Also verify that:

```text
Anchors
Pivot
Scale
Rotation
Button events
Image properties
```

remain unchanged.

---

# 52. Agent Deliverables

The agent must provide:

1. Complete implementation.
2. All required C# files.
3. Installation/location instructions.
4. How to open the Editor tool.
5. How to configure multiple targets.
6. How to select scenes.
7. How to perform a batch update.
8. Explanation of how scene-specific sprites are preserved.
9. Known limitations.
10. Testing results.

---

# 53. Final Implementation Principle

The fundamental behavior should be:

```text
                 ┌────────────────────┐
                 │ Target Configuration│
                 └─────────┬──────────┘
                           │
                           ▼
                 ┌────────────────────┐
                 │ Selected Scenes    │
                 └─────────┬──────────┘
                           │
             ┌─────────────┼─────────────┐
             ▼             ▼             ▼
          Scene 1        Scene 2       Scene 3
             │             │             │
             ▼             ▼             ▼
       Find existing   Find existing  Find existing
       GameObjects     GameObjects    GameObjects
             │             │             │
             ▼             ▼             ▼
  Modify position   Modify position  Modify position
  and size         and size         and size
             │             │             │
             ▼             ▼             ▼
  Preserve all    Preserve all    Preserve all
  other data      other data      other data
```

The key rule is:

> **Find the existing object in each scene and change ONLY its RectTransform position and size. Never replace the object.**

This allows every scene to keep its own sprite and configuration while still allowing common UI positioning and sizing to be changed in bulk.