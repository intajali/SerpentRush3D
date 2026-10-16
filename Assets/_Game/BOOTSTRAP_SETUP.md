# Bootstrap scene setup

Serpent Rush starts from one scene. `GameBootstrapper` initializes `GameManager`, and the first play-mode update walks `None -> Bootstrap -> MainMenu`. Snake movement is not part of this setup.

## One-time Editor setup

1. Open the project in Unity 6 and wait until the script compile finishes with no Console errors.
2. Confirm `Assets/_Game/Data/GameConfig.asset` imported. If it is missing, use **Serpent Rush > Setup > Create Default Game Config**.
3. Run **Serpent Rush > Setup > Create Bootstrap Scene**.
4. This creates `Assets/_Game/Scenes/Bootstrap.unity` with a Camera, a Directional Light, and a root `Game` object.
5. `Game` has `Game Manager` and `Game Bootstrapper`. The menu assigns `GameConfig` on the bootstrapper.
6. The menu inserts that scene at index 0 of **File > Build Profiles** (Build Settings). Leave `SampleScene` out of a player build when you no longer need the template scene.
7. Save the project.

## Manual setup

Use this if you prefer to wire the scene yourself.

1. **File > New Scene > Basic (Built-in)** or the URP equivalent that includes a camera and a light. Save it as `Assets/_Game/Scenes/Bootstrap.unity`.
2. Create an empty GameObject named `Game` at the root of the hierarchy.
3. Add **Serpent Rush > Game Manager**.
4. Add **Serpent Rush > Game Bootstrapper**. Both components must be on `Game`.
5. Drag `Assets/_Game/Data/GameConfig.asset` into the **Config** field on `Game Bootstrapper`. Do not assign it anywhere else.
6. **File > Build Profiles**, add `Bootstrap`, and move it above every other scene.
7. Open `Bootstrap` and press Play.

## Verify the flow

With **Log State Transitions** enabled on `GameConfig`, Play prints:

```text
[SerpentRush] None -> Bootstrap
[SerpentRush] Bootstrap -> MainMenu
```

Then use the `Game` object's context menu (the component's ⋮ menu) while playing:

| Action | Result |
| --- | --- |
| Request Play | `MainMenu -> Playing`, `timeScale` is 1 |
| Request Pause | `Playing -> Paused`, `timeScale` is 0 |
| Request Resume | `Paused -> Playing`, `timeScale` is 1 |
| Request Game Over | `Playing -> GameOver`, `timeScale` is 0 |
| Request Main Menu | returns to `MainMenu` from Playing, Paused, or GameOver |
| Request Pause from Main Menu | warning, state stays `MainMenu` |

Stopping Play restores `timeScale` to 1.

## Dependency rules

- `SerpentRush.Core` references nothing in this project.
- `SerpentRush.Gameplay` references Core only.
- `SerpentRush.Managers` references Core and Gameplay. It is the only assembly that owns the flow.
- `Arena`, `AI`, `UI`, and `Progression` reference Core only. Add `SerpentRush.Gameplay` when that module must publish flow events or read `GameConfig`. Do not reference `SerpentRush.Managers` from those modules.
- `GameBootstrapper` is the composition root. It constructs `EventBus`, `StateMachine`, `GameFlowRules`, `UnityTimeControl`, `UnityRuntimeSettings`, and `GameStateFactory`, then passes those interfaces into `GameManager`.
- UI buttons that cannot publish a struct can call the `Request` methods on the scene `Game` object. Other components on that object can publish through `GameManager.Events`.

Subscribe to `GameStateChanged` from `OnEnable` or `Start` on the bus stored in `GameManager.Events`. Read `GameManager.CurrentStateId` when you need the state that is already active. `GameManager` enters Bootstrap on its first update, which is after other scripts' `Start`.
