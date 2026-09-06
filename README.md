<h1 align="center">INSOS JAYA JAYA</h1>
<h3 align="center">Petualangan Harianku</h3>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2022.3-blue?logo=unity" alt="Unity">
  <img src="https://img.shields.io/badge/Platform-Android%20%7C%20Web-green" alt="Platform">
  <img src="https://img.shields.io/badge/License-MIT-yellow" alt="License">
  <img src="https://img.shields.io/badge/Status-Active-brightgreen" alt="Status">
</p>

<p align="center">
  Interactive educational game for kids — learn daily routines through play.
</p>

<p align="center">
  <a href="https://project-setara.itch.io/insos">
    <img src="https://img.shields.io/badge/Play-itch.io-EA4B71?logo=itch.io" alt="Play on itch.io">
  </a>
</p>

---

## Features

- 3 gameplay sections (morning routine, cleaning, object placement)
- Interactive dialog with typing effect
- 2 character support (boy & girl)
- Localization (Indonesian & English)
- Animated UI and visual effects

## Requirements

| Component | Version |
|-----------|---------|
| Unity | 2022.3 LTS (`2022.3.62f3`) |
| .NET SDK | For CSharpier formatter |

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/muhammadzaini213/INSOS.git

# 2. Open the project in Unity Hub

# 3. Format code before committing
dotnet csharpier .
```

## Project Structure

```
Assets/
├── _Game/
│   ├── 00_Scripts/       # Source code (Slafurry namespace)
│   ├── 01_Objects/       # Prefabs
│   ├── 02_Sprites/       # Art assets
│   ├── 03_Audio/         # Audio files
│   ├── 04_Scenes/        # Scenes (Menu + Gameplay)
│   └── 05_Settings/      # URP, ScriptableObjects
├── Editor/               # Custom editor tools
└── _Vendor/              # Third-party (NavMesh, TextMeshPro)
```

## Build

### Manual

Open the project in Unity → `File` → `Build Settings` → select platform → `Build`.

### CI/CD

GitHub Actions automatically builds and deploys to itch.io via Butler.

| Trigger | Platform | Deploy |
|---------|----------|--------|
| Push to `main` | Android + WebGL | itch.io |
| Tag `v*` | Android + WebGL | itch.io + GitHub Release |
| Manual (workflow_dispatch) | Select platform | itch.io |

```bash
# Release with tag
git tag v1.0.0
git push --tags
```

### Required Secrets

| Secret | Description |
|--------|-------------|
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |
| `BUTLER_API_KEY` | itch.io API key (Account → API Keys) |

## Team

Built by [Setara Project](https://project-setara.itch.io).

## License

[MIT](LICENSE) — 2026 Setara Project
