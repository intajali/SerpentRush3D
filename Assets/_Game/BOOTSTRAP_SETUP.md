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

## Ground

`Bootstrap` includes a `Ground` plane at y = 0, 100 by 100 units, using `Assets/_Game/Materials/Ground.mat`. The snake sits just above it, so the body reads against the floor and casts a shadow. The arena half extent on `GameConfig` is 25, and this plane covers that area with room to spare.

## Snake head

1. Let the Editor import `com.unity.inputsystem` from `Packages/manifest.json`.
2. Run **Serpent Rush > Setup > Place Snake Head**. This instances `Assets/_Game/Prefabs/SnakeHead.prefab` and, when `Game` is in the scene, adds **Snake Session Link** on `Game` with the head assigned.
3. Press Play. The session enters Playing on its own, and the head starts moving forward.
4. Steer with WASD or the arrow keys. Release the keys and it continues on its current heading. **Snake Camera Follow** on Main Camera stays behind the head, and **Heading Delay** (0.45 seconds) lets the snake turn before the camera swings around. Turn off **Start In Playing** on `GameConfig` when a menu should wait for a start action.

Prefab hierarchy:

```text
SnakeHead          Snake Controller, Snake Movement, Snake Body Controller
  Head             Capsule mesh, Capsule Collider
SnakeSegment       Capsule mesh, Snake Segment. No Rigidbody.
```

Inspector values on the prefab:

| Component | Field | Value |
| --- | --- | --- |
| Transform | Position | (0, 0.28, 0), facing +Z |
| Head mesh | Rotation | 90° on X, so the capsule lies along the heading |
| Snake Movement | Config | `Assets/_Game/Data/SnakeMovementConfig.asset` |
| Snake Movement | Lock Planar Height | on |
| Snake Controller | Steer Action | empty (WASD, arrows, and left stick) |
| Snake Controller | View Camera | empty (uses Main Camera) |
| Snake Controller | Control Enabled | on, until the session link limits it to Playing |
| Head Capsule Collider | Radius / Height | 0.5 / 2 |

`SnakeMovementConfig` defaults: forward speed 6, max turn 140 degrees per second, deadzone 0.15, camera-relative steering on. The wider turn keeps the head from curling back through the neck.

## Snake body

The body is already on `Assets/_Game/Prefabs/SnakeHead.prefab`. `SnakeController` still moves the head through `SnakeMovement` in `Update`. `SnakeBodyController` records that transform in `LateUpdate` and places each segment on the recorded path.

Segments are not parented to the head or to each other. Each one is sampled at a fixed arc length behind the head, so a turn stays in the path instead of being cut short.

1. Open `Assets/_Game/Scenes/Bootstrap.unity` and let the Editor finish importing.
2. Select the `SnakeHead` instance. **Snake Body Controller** should list `SnakeBodyConfig` and `SnakeSegment`. Those references come from the prefab. Do not add a Rigidbody to the segment.
3. Press Play. The session enters Playing, the head moves, and 50 segments trail along the starting line behind it.
4. Steer with WASD or the arrow keys. The body stays on the curve the head already traveled.
5. To grow or shrink it during Play, select `SnakeHead` and use the component context menu: **Add Body Segment** or **Remove Body Segment**.
6. A `Snake Segment Pool` object appears in the Hierarchy while playing. Inactive extras live under it. Stop Play and that object is removed.

`SnakeBodyConfig` is `Assets/_Game/Data/SnakeBodyConfig.asset`.

| Field | Default | What it changes |
| --- | --- | --- |
| Segment Spacing | 0.75 | Arc length from the head to the first segment, and from each segment to the next. Each segment is sized to that length. |
| Segment Diameter | 0.36 | Thickness. Kept below the spacing so neighbors meet end to end. |
| Initial Segment Count | 5 on the current asset | Segments spawned at the start. Restart Play after changing it. 50 is still supported. |
| Path Sample Spacing | 0.05 | How often a point is stored. Lower values follow tighter turns. |
| Pool Capacity | 80 | Inactive instances kept for reuse. Raised automatically when the start length is larger. |

The follow camera sits close to the head, so only the front of a 50-segment body is in frame. To see more of the trail, select Main Camera and raise **Follow Offset**, for example `(0, 28, -24)`.

Manual wiring, if the controller is missing from a head:

1. Create the config with **Serpent Rush > Snake Body Config**, or use the asset already in `Assets/_Game/Data`.
2. Confirm `Assets/_Game/Prefabs/SnakeSegment.prefab` has **Snake Segment** on the root, the capsule mesh, `SnakeHead` material, and no Rigidbody.
3. On the same object as **Snake Movement**, add **Snake Body Controller**.
4. Assign the config and the segment prefab.
5. Leave the segment unparented in the scene. The controller pools instances at runtime.
