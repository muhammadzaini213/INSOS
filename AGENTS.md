# AGENTS.md — INSOS JAYA JAYA

## Project Overview
Unity 2022.3.62f3 2D game (namespace `Slafurry`). Educational/activity game — "Petualangan Harianku" (My Daily Adventure) by Setara Project. Menus, dialog, sectioned gameplay.

## Quick Start
- **Open in Unity**: Open the folder as a Unity project (2022.3 LTS required)
- **Formatter**: `dotnet csharpier .` (CSharpier 1.2.6 via `dotnet-tools.json`)
- **No CLI build/test**: Unity projects require the Editor for builds and play-mode tests
- **No tests exist**: `com.unity.test-framework` is in manifest but zero test scripts/scenes

## Architecture

### Initialization Flow (Critical)
Every `MonoBehaviour` that needs startup logic must register with `LoadingSystem`:
1. `Singleton<T>.Start()` → calls `LoadingSystem.Instance.Register(this)` (NOT Awake — Awake only sets Instance)
2. `LoadingSystem.Start()` → runs `Initialize()` on all registered objects (ordered by `Priority`)
3. After all `Initialize()` complete → runs `PostInitialize()` on each

**Important**: Objects registered AFTER the initial boot (e.g., a Player prefab spawned in a new scene) are collected into a `_pendingLate` batch and processed one frame later. Initialize-before-PostInitialize ordering is preserved within each batch.

**Rules**:
- `Initialize()`: async setup, asset loading. **NEVER touch other objects here**
- `PostInitialize()`: object wiring, cross-references safe here
- `Manager` subclasses: override `OnPostInitialize()` (not `PostInitialize()` directly — it's sealed). Manager registers in `Awake()` via `LoadingSystem.Instance.Register(this)`, different from Singleton which registers in `Start()`

### Base Classes
| Class | Purpose | Persists? |
|-------|---------|-----------|
| `GameSystem<T>` | Global systems (audio, save, loading, scene, pause) | Yes (`DontDestroyOnLoad`) |
| `LocalSingleton<T>` | Scene-scoped singletons | No |
| `Manager` | Session gameplay coordinators (enemy, wave, bullet managers) | No |

**Note**: `Manager` defines abstract `RegisterToGameManager()`/`UnregisterFromGameManager()` methods, but no `GameManager` class exists yet — these are stubs.

### Convenience Facades
Static wrappers for common systems — prefer these over direct `Instance` access:
- `Audio.PlayMusic()`, `Audio.PlaySFX2D()` → `Slafurry.System.Audio`
- `SceneSystem.Load()` → `Slafurry.System.Scene`
- `Pause.On()` / `Pause.Off()` → `Slafurry.System.Pause` (key defaults to `"Global"`)
- `Save.To()` / `Save.From()` → `Slafurry.System.Save`
- `Controls` → `Slafurry.System.InputHub` (input events: `OnJumpPressed`, `OnMoveChanged`)
- `Localize` → `Slafurry.System.Localization` (`Text()`, `SetLanguage()` — supports "en" and "id")

### Directory Layout
```
Assets/
├── _Game/
│   ├── 00_Scripts/
│   │   ├── Core/          # Abstract bases, interfaces (Singleton, Manager, IInitializable, IResettable)
│   │   ├── System/        # Global systems (Audio, Save, Loading, Scene, Pause, InputHub, Localization, PlayerData)
│   │   ├── Game/          # Gameplay (Triggers, Dialog, Character)
│   │   │   └── _Old/      # Deprecated scripts (legacy OnMouseDown/OnMouseDrag)
│   │   ├── UI/            # UI components (Menus, HUD, Transitions)
│   │   └── Utils/         # Helpers (UI animations, GameFeel effects)
│   ├── 01_Objects/        # Prefabs
│   ├── 02_Sprites/        # Art assets
│   ├── 03_Audio/          # Audio files
│   ├── 04_Scenes/         # See Scene Flow below
│   └── 05_Settings/       # URP, ScriptableObjects
├── Editor/                # Custom editor tools (GameAssetCreator, PlayerDataEditor, CharacterSpriteEditor)
├── Resources/             # Runtime-loaded assets (Congratulations animation frames)
└── _Vendor/               # Third-party (NavMeshComponents, TextMeshPro)
```

### Scene Flow
**Build order**: `01_StartMenu` → `02_ChooseGenderMenu` → `03_ChooseActivityMenu` → Section 1

**Section 1** has 17 scenes (cutscenes, gameplay rooms, mirror, toothbrushing, kitchen, sandwich, etc.). Sections 2 and 3 directories exist but are **empty**.

**Menu scenes**: `01_StartMenu`, `02_ChooseGenderMenu`, `03_ChooseActivityMenu`, `MainMenu`, `SettingsMenu`, `AboutMenu`

**Prototype scenes**: `CLICK`, `ROTATE`, `DRAG` (in Menu/ directory)

### Key Packages
- Input System (`com.unity.inputsystem`) — not legacy Input
- URP 14.x (2D renderer)
- `com.unity.ai.navigation` (1.1.7) + vendored NavMeshComponents in `_Vendor/`
- Newtonsoft JSON (save system)
- TextMeshPro (UI text)
- Cinemachine (2.10.6) — camera
- Timeline (1.7.7) — cutscenes
- Recorder (4.0.3) — video/image recording

## Code Conventions
- **Namespace**: `Slafurry.{System/Subsystem}` (e.g., `Slafurry.System.Audio`, `Slafurry.Core.Abstract`). **Warning**: Many files (especially triggers, dialog, menus, old scripts) use global namespace — this is inconsistent
- **Formatting**: CSharpier — run before committing
- **Singletons**: Always inherit `GameSystem<T>` or `LocalSingleton<T>`, never implement singleton pattern manually
- **Triggers**: Inherit `BaseTrigger`, use `playLimit`/`unlimited` for one-shot vs repeat
- **Editor tools**: Use `[GameAssetCreator("Category", "Name")]` attribute on ScriptableObjects to register in the custom asset creator window (`Slafurry > Game Data`)
- **PlayerData**: Static class in `Slafurry.System.Player` — stores gender selection (Boy/Girl) in PlayerPrefs, fires `OnGenderChanged`
- **Dialog**: `Dialog` struct + `DialogBucket` ScriptableObject (global namespace), displayed by `DialogHUD` with typing effect

## Git Workflow
- Branch from `main`
- Commit prefixes observed: `feat:`, `fix:`, `build:`, `Setup/`
- No CI/CD configured
- `.csproj`/`.sln` files are gitignored but exist locally (committed before gitignore rule)
