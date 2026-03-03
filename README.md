# 🧩 Match-3 Puzzle Game

A feature-rich Match-3 puzzle game built with **Unity**, designed with a clean, scalable architecture using **MVP pattern**, **VContainer** dependency injection, and **data-driven ScriptableObject** configurations.

---

## 📖 Overview

A classic Match-3 puzzle game where players swap colored items on a grid to create matches of three or more, triggering chain reactions, booster activations, and obstacle destruction. The game features a level-based progression system with JSON-defined levels, multiple booster types, and polished visual effects.

---

## 🎮 Gameplay Features

### Grid Items
- **Regular Items:** Red, Blue, Green, Yellow — matched by swapping adjacent tiles
- **Boosters:** Created from special match patterns
  - 🚀 **Horizontal Rocket** — clears an entire row
  - 🚀 **Vertical Rocket** — clears an entire column
  - 💣 **Bomb** — clears a surrounding area
  - 🎯 **Fly** — targets and destroys specific items *(not supported yet)*
  - 🔮 **Orb** — advanced targeting booster *(not supported yet)*
- **Obstacles:** Destructible blockers on the grid
  - 📦 **Box** — standard breakable obstacle
  - 🏺 **Vase** — fragile obstacle type
  - 🪨 **Stone** — durable obstacle *(not supported yet)*

### Game Flow
- Level-based progression with move limits
- Goal-based objectives (collect specific items/destroy obstacles)
- Automatic shuffle detection when no valid moves remain
- Chain reaction system with sequential booster resolution
- Win/Fail conditions with dedicated end-level menus (`SuccessMenuView` / `FailMenuView`)

---

## 🏗️ Architecture

### Three-Tier Layer Structure

```
┌──────────────────────────────────────────┐
│               Game Layer                 │  Gameplay logic, grid mechanics,
│            (Game.asmdef)                 │  boosters, UI, level management
├──────────────────────────────────────────┤
│               Core Layer                 │  Reusable systems: state machine,
│            (Core.asmdef)                 │  pooling, scene loading, save system
└──────────────────────────────────────────┘
```

- **Core** — Framework-level, game-agnostic utilities and services (state machine, object pooling, scene management, animation modules, extension methods)
- **Game** — All gameplay-specific logic, separated into Common (shared across scenes) and scene-specific modules

### MVP (Model–View–Presenter)

The project follows the **MVP architectural pattern** to cleanly separate concerns:

- **Model** — Pure data and state (e.g., `GridModel`, `LevelGoalModel`, `LevelProgressionModel`)
- **View** — MonoBehaviour-based UI and visual representation (e.g., `GridView`, `HudView`, `LevelEndView`, `SuccessMenuView`, `FailMenuView`)
- **Presenter** — Orchestration logic connecting models and views (e.g., `GridPresenter`, `HudPresenter`, `LevelEndPresenter`)

All communication between layers flows through **interfaces**, ensuring loose coupling and testability.

### Dependency Injection — VContainer

Dependency injection is managed via **VContainer** with scoped `LifetimeScope` hierarchies:

| Scope | Responsibility |
|---|---|
| `AppLifetimeScope` | Singleton services: scene loading, object pooling, save system, level data |
| `GameplayLifetimeScope` | Scoped per gameplay session: grid logic, input, HUD, boosters, fill strategies |
| `MenuLifetimeScope` | Main menu UI bindings |
| `LoadLifetimeScope` | Scene transition and loading screen |

### State Machine

A custom **finite state machine** drives the core gameplay loop with the following grid states:

```
Idle → InputResolve → MatchResolve → BoosterResolve → FillResolve → (loop)
                                                              ↘
                                                           Shuffle
```

- **IdleState** — Awaiting player input
- **InputResolveState** — Processing swipe/tap and validating moves
- **MatchResolveState** — Detecting and resolving matches on the grid
- **BoosterResolveState** — Executing booster chain reactions with sequential timeline
- **FillResolveState** — Filling empty cells with new items (slide-down / fall-down)
- **ShuffleState** — Reshuffling the board when no valid moves exist

The state machine supports **transition priorities**, **exit permissions**, and **one-shot transitions**.

---

## 📂 Project Structure

```
_Project/
├── Art/
│   ├── Materials/                # Particle materials (bomb, rocket effects)
│   ├── Shaders/                  # Custom shaders
│   └── Sprites/
│       ├── Gameplay/             # Grid, items, boosters, obstacles, particles
│       └── UI/                   # HUD, backgrounds, level-end screens
├── Prefabs/
│   ├── Core/                     # Core system prefabs
│   └── Game/
│       ├── Gameplay/             # Grid items, particle effects (blast, booster)
│       └── UI/                   # UI element prefabs
├── Resources/
│   ├── Configs/                  # Runtime configuration assets
│   └── Levels/                   # JSON level definitions (level_1..3)
├── Scenes/
│   ├── LoadScene.unity           # Loading / transition scene
│   ├── MenuScene.unity           # Main menu
│   └── GameScene.unity           # Core gameplay scene
├── ScriptableObjects/
│   ├── Core/                     # Scene asset configs
│   └── Game/
│       ├── Containers/           # Config containers (app, gameplay)
│       ├── GridElement/          # Item, booster, obstacle data definitions
│       └── Pooled/               # Pool configs for grid objects, VFX, UI
└── Scripts/
    ├── Editor/                   # (4 scripts) — Unity Editor tooling
    │   ├── BoosterActionBaseDrawer.cs   # Custom inspector for booster actions
    │   ├── LevelEditorWindow.cs         # Level design window (Tools → Level Editor)
    │   ├── SceneIdGeneratorEditor.cs    # Auto-generates SceneIdLookup from build settings
    │   └── SceneSelectionOverlay.cs     # Scene quick-switch overlay in Scene view
    ├── Core/                     # (57 scripts) — Game-agnostic framework
    │   ├── Common/               # Shared data structures (EnumConfigMap, IDamagable)
    │   ├── Extensions/           # C# & Unity extension methods
    │   ├── Modules/              # Animation & FX modules
    │   │   ├── Animation/        # bounce, fade, move, size, text, panel, animator
    │   │   └── FX/               # particle, image FX
    │   ├── Services/             # Pool, Save, Scene loading services
    │   ├── Systems/              # State machine implementation
    │   └── Utils/                # Async, build, pool, UI-world-space, poly utilities
    └── Game/                     # (195 scripts) — Gameplay-specific code
        ├── Common/               # Cross-scene shared code
        │   ├── Bootstrappers/    # App & gameplay initialization
        │   ├── LifetimeScopes/   # VContainer DI scope definitions
        │   ├── Factories/        # SlotView & FXView factories
        │   ├── MVP/              # Shared models, presenters, views
        │   ├── Services/         # Input, level data, setup services
        │   └── Utils/            # Grid utilities (index, match, direction, JSON mapping, randomization)
        ├── Editor/               # Level editor window, custom drawers
        ├── Factories/            # GridObjectFactory
        ├── GridElements/         # Core grid domain
        │   ├── Animations/       # GridObjectAnimation
        │   ├── Containers/       # Config containers per grid object kind
        │   ├── Data/             # ScriptableObject data, booster actions, combos
        │   ├── Enums/            # ItemType, BoosterType, ObstacleType, etc.
        │   ├── Extensions/       # Booster, GridLayout, GridMesh extensions
        │   └── GridObjects/      # BaseGridObject, ItemObject, BoosterObject, ObstacleObject
        └── MVP/
            ├── Models/           # GridModel with typed grid building
            ├── Presenters/       # GridPresenter + Handlers, States, Strategies
            └── Views/            # GridView, HudView, LevelEndView, SuccessMenuView, FailMenuView + pooled FX views
```

---

## 🔧 Key Systems

### Object Pooling
A centralized `ObjectPoolService` with `PolymorphicPool` and `SinglePool` variants, configured via ScriptableObjects. Supports auto-release and is used extensively for grid objects, VFX particles, and UI slots.

### Grid View
`GridView` is the visual backbone of the gameplay scene, responsible for translating grid data into on-screen representation. On initialization it receives the grid dimensions and an active-cell mask, then performs three key steps: calculating the optimal cell size to fit the screen, computing the grid origin offset so the board is top-aligned, and generating a procedural mesh for the board background.

Key responsibilities:

- **Adaptive Cell Sizing** — Cell size is computed dynamically based on screen dimensions, side padding ratio, and cell spacing, then clamped to a configurable `MaxCellSize` via `GridLayoutConfigSO`
- **Coordinate Conversion** — Provides `GridToWorld`, `WorldToGrid`, `GridToScreen`, and `ScreenToGrid` methods for seamless translation between grid coordinates, world space, and screen space
- **Procedural Mesh Generation** — Builds a two-submesh board mesh at runtime (inner cell quads + pipe-style frame with rounded corners) through `GridMeshExtensions`, supporting holes for inactive cells. Frame thickness, corner smoothness, and mesh quality (Low/Medium/High segments) are configured via `GridMeshConfigSO`
- **Input Direction Mapping** — Flips vertical input directions to account for the inverted Y-axis between screen space and grid coordinate space
- **Editor Gizmos** — Optional grid gizmo drawing for visual debugging in the Scene view

### Grid Fill Strategies
Two interchangeable fill strategies, resolved at runtime via `FillStrategyResolver`:

- **SlideDownFillStrategy** — Items slide diagonally into empty spaces. Implemented as partial classes: `Simulation`, `Spawn`, `Recording`, `Workspace`, `TimelineScheduling`
- **FallDownFillStrategy** — Items fall straight down into empty spaces. Also implemented as partial classes: `Simulation`, `Workspace`, `TimelineScheduling`

Both strategies use dedicated `AnimationScheduler` classes (`SlideAnimationScheduler`, `FallAnimationScheduler`) to coordinate visual timing.

### Booster Action System
Boosters are defined through a **data-driven action hierarchy**:

- `BoosterActionBase` → base class with damage amount
  - `AreaActionBase` → area-of-effect boosters (Bomb, Rockets)
  - `TargetingActionBase` → target-specific boosters (Fly, Orb) *(not supported yet)*

Booster combos are defined via `BoosterComboDataSO` with configurable combo rules. The `BoosterTimelineBuilder` and `ImpactTimeline` system handle sequential execution of chained booster effects.

### Level System
Levels are defined as **JSON files** loaded at runtime:

```json
{
  "level_number": 1,
  "grid_width": 11,
  "grid_height": 10,
  "move_count": 30,
  "grid": ["rand", "rand", "empty", "bo", "v", ...]
}
```

Grid cell types include: `rand` (random item), `empty` (no cell), `bo` (box obstacle), `v` (vase), and specific item/booster codes. The `LevelDefinitionProvider` and `LevelDataService` handle parsing and serving level data. JSON utility logic is split between `LevelJsonEditorUtils` (editor-time) and `LevelJsonRuntimeUtils` (runtime).

### Animation Modules (Core)
Reusable, composable animation building blocks:

- `MoveAnimationModule` — Position tweening
- `BounceAnimationModule` — Scale bounce effects
- `FadeAnimationModule` — Alpha fading
- `SizeAnimationModule` — Scale animations
- `TextAnimationModule` — Numeric text counters
- `AnimatedPanelModule` — UI panel show/hide
- `AnimatorModule` — Wraps Unity's `Animator` component with play/pause/trigger/state-check APIs and an `OnComplete` event
- `ParticleFxModule` / `ImageFxModule` — VFX control

### Cheat & Debug System
`GridCheatHandler` (implements `IGridCheatHandler`, `ITickable`) provides **editor and development** keyboard shortcuts for rapid in-game testing:

| Key | Action |
|---|---|
| `S / L / A / X` | Spawn Blue / Green / Red / Yellow item at mouse position |
| `B / N` | Spawn Box / Vase obstacle at mouse position |
| `T / H / V` | Spawn Bomb / Horizontal Rocket / Vertical Rocket at mouse position |
| `R` | Remove grid object at mouse position |
| `C / O` | Clear all Boosters / Obstacles from grid |
| `F` | Force FillResolve state |
| `Space` | Load main menu |
| `D` | Log current state ID |
| `P` *(Editor only)* | Toggle editor pause |

### Scene Management
Async scene loading via `SceneLoadService` with `ProgressHandler` for loading bars, supporting both `AsyncOperation` and Addressable `AsyncOperationHandle` groups. Scene IDs are auto-generated by `SceneIdGeneratorEditor` into `SceneIdLookup.cs`, eliminating hardcoded scene name strings. The `SceneSelectionOverlay` provides quick scene switching directly from the Scene view toolbar.

---

## 🛠️ Tech Stack

| Category | Technology |
|---|---|
| **Engine** | Unity |
| **Language** | C# |
| **DI Container** | VContainer |
| **Async** | UniTask |
| **Architecture** | MVP + Custom State Machine |
| **Data Config** | ScriptableObjects + JSON |
| **Editor Tools** | NaughtyAttributes, Custom Level Editor, Scene ID Generator, Scene Selection Overlay |
| **Assembly Defs** | `Core.asmdef`, `Game.asmdef`, `Editor.asmdef` |

---

## 📊 Project Stats

| Metric | Count |
|---|---|
| Total C# Scripts | 256 |
| Core Layer Scripts | 57 |
| Game Layer Scripts | 195 |
| Editor Scripts | 4 |
| Interfaces | 48 |
| ScriptableObject Types | 16 |
| Scenes | 3 |
| Level Definitions | 3 |

---

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3.32f1
- VContainer package
- UniTask package
- NaughtyAttributes package
- Addressables package (for scene loading)

### Setup
1. Clone the repository
2. Open the project in Unity
3. Ensure all packages are resolved via Package Manager
4. Open `LoadScene` as the entry point scene
5. Press Play — the app bootstrapper initializes services and transitions to the menu

### Level Editor
A custom **Level Editor Window** (`Scripts/Editor/LevelEditorWindow.cs`) is available for designing and editing levels directly within the Unity Editor. Open it via **Tools → Level Editor**.

### Scene ID Generator
`SceneIdGeneratorEditor` auto-generates the `SceneIdLookup.cs` file from Unity's build settings, keeping all scene references type-safe and refactor-friendly. Run it whenever scenes are added or reordered.

---

## 🎯 Design Principles

- **Composition over Inheritance** — Modular animation/FX modules composed on objects rather than deep inheritance trees
- **Interface-Driven** — 48 interfaces ensuring all dependencies are abstract and swappable
- **Data-Driven Design** — Game behavior configured through ScriptableObjects, not hardcoded
- **Single Responsibility** — Handlers, helpers, and utilities each own one concern (e.g., `BlastFxHandler`, `GoalSlotHandler`, `GridMatchCalcUtil`, `GridCheatHandler`)
- **Clean Separation** — Core layer has zero knowledge of Game layer; Game layer references Core
- **Partial Classes for Complex Systems** — Fill strategies use partial class files (`.Simulation`, `.Workspace`, `.TimelineScheduling`, etc.) to keep large logic units organized and readable

---

## 📝 License

All rights reserved. This project is proprietary.
