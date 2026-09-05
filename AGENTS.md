# AGENTS.md — INSOS JAYA JAYA

## Project Overview
Unity 2022.3.62f3 2D game (namespace `Slafurry`). Educational/activity game with menus, dialog, and sectioned gameplay.

## Quick Start
- **Open in Unity**: Open the folder as a Unity project (2022.3 LTS required)
- **Formatter**: `dotnet csharpier .` (CSharpier 1.2.6 via dotnet-tools.json)
- **No CLI build/test**: Unity projects require the Editor for builds and play-mode tests

## Architecture

### Initialization Flow (Critical)
Every `MonoBehaviour` that needs startup logic must register with `LoadingSystem`:
1. `Singleton<T>.Awake()` → auto-registers with `LoadingSystem.Instance.Register(this)`
2. `LoadingSystem.Start()` → runs `Initialize()` on all registered objects (ordered by `Priority`)
3. After all `Initialize()` complete → runs `PostInitialize()` on each

**Rules**:
- `Initialize()`: async setup, asset loading. **NEVER touch other objects here**
- `PostInitialize()`: object wiring, cross-references safe here
- `Manager` subclasses: override `OnPostInitialize()` (not `PostInitialize()` directly) — it auto-registers with `GameManager` first

### Base Classes
| Class | Purpose | Persists? |
|-------|---------|-----------|
| `GameSystem<T>` | Global systems (audio, save, loading, scene, pause, VFX) | Yes (`DontDestroyOnLoad`) |
| `LocalSingleton<T>` | Scene-scoped singletons | No |
| `Manager` | Session gameplay coordinators (enemy, wave, bullet managers) | No — registers with `GameManager` |

### Convenience Facades
Static wrappers for common systems — prefer these over direct `Instance` access:
- `Audio.PlayMusic()`, `Audio.PlaySFX2D()` → `Slafurry.System.Audio`
- `SceneSystem.Load()` → `Slafurry.System.Scene`
- `Pause.On()` / `Pause.Off()` → `Slafurry.System.Pause`
- `Save.To()` / `Save.From()` → `Slafurry.System.Save`

### Directory Layout
```
Assets/
├── _Game/
│   ├── 00_Scripts/
│   │   ├── Core/          # Abstract bases, interfaces (Singleton, Manager, IInitializable)
│   │   ├── System/        # Global systems (Audio, Save, Loading, Scene, Pause, Input, VFX, Localization)
│   │   ├── Game/          # Gameplay (Triggers, Dialog)
│   │   ├── UI/            # UI components (Menus, HUD, Transitions)
│   │   └── Utils/         # Helpers (Pool, UI animations, GameFeel effects)
│   ├── 01_Objects/        # Prefabs
│   ├── 02_Sprites/        # Art assets
│   ├── 03_Audio/          # Audio files
│   ├── 04_Scenes/         # Menu/ and Game/ scenes
│   └── 05_Settings/       # URP, ScriptableObjects
├── Editor/                # Custom editor tools (GameAssetCreator window)
├── Resources/             # Runtime-loaded assets (Animations)
└── _Vendor/               # Third-party (NavMesh, TextMeshPro)
```

### Scene Flow (Build Order)
1. `01_StartMenu` → `02_ChooseGenderMenu` → `03_ChooseActivityMenu`
2. `Section 1/01-04` → `Section 2/` → `Section 3/` (gameplay sections)

### Key Packages
- Input System (`com.unity.inputsystem`) — not legacy Input
- URP 14.x (2D renderer)
- NavMeshPlus (AI navigation)
- Newtonsoft JSON (save system)
- TextMeshPro (UI text)

## Code Conventions
- **Namespace**: `Slafurry.{System/Subsystem}` (e.g., `Slafurry.System.Audio`, `Slafurry.Core.Abstract`)
- **Formatting**: CSharpier — run before committing
- **Singletons**: Always inherit `GameSystem<T>` or `LocalSingleton<T>`, never implement singleton pattern manually
- **Triggers**: Inherit `BaseTrigger`, use `playLimit`/`unlimited` for one-shot vs repeat
- **Editor tools**: Use `[GameAssetCreator("Category", "Name")]` attribute on ScriptableObjects to register in the custom asset creator window (`Slafurry > Game Data`)

## Git Workflow
- Branch from `main`, use `git-push-branch.sh` (interactive: prompts for branch + commit message)
- Use `git-pull-main.sh` to sync main before starting work
- Commit prefixes: `feat:`, `fix:`, `build:` (see git log)
