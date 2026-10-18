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
- `SerpentRush.Gameplay` references Core, the Input System, and Cinemachine.
- `SerpentRush.Managers` references Core and Gameplay. It is the only assembly that owns the flow.
- `SerpentRush.UI` references Core, the Input System, and uGUI for the on-screen stick.
- `Arena`, `AI`, and `Progression` reference Core only. Add `SerpentRush.Gameplay` when that module must publish flow events or read `GameConfig`. Do not reference `SerpentRush.Managers` from those modules.
- `GameBootstrapper` is the composition root. It constructs `EventBus`, `StateMachine`, `GameFlowRules`, `UnityTimeControl`, `UnityRuntimeSettings`, and `GameStateFactory`, then passes those interfaces into `GameManager`.
- UI buttons that cannot publish a struct can call the `Request` methods on the scene `Game` object. Other components on that object can publish through `GameManager.Events`.

Subscribe to `GameStateChanged` from `OnEnable` or `Start` on the bus stored in `GameManager.Events`. Read `GameManager.CurrentStateId` when you need the state that is already active. `GameManager` enters Bootstrap on its first update, which is after other scripts' `Start`.

## Ground

`Bootstrap` includes a `Ground` plane at y = 0, 100 by 100 units, using `Assets/_Game/Materials/Ground.mat`. The snake sits just above it, so the body reads against the floor and casts a shadow. The arena half extent on `GameConfig` is 25, and this plane covers that area with room to spare.

## Snake head

1. Let the Editor import `com.unity.inputsystem` from `Packages/manifest.json`.
2. Run **Serpent Rush > Setup > Place Snake Head**. This instances `Assets/_Game/Prefabs/SnakeHead.prefab` and, when `Game` is in the scene, adds **Snake Session Link** on `Game` with the head assigned.
3. Press Play. The session enters Playing on its own, and the head starts moving forward.
4. Steer with WASD, the arrow keys, or the bottom-left stick. Release the input and the snake continues on its current heading. **Snake Camera Rig** drives a Cinemachine third-person camera. Turn off **Start In Playing** on `GameConfig` when a menu should wait for a start action.

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
| Snake Controller | Controls | `Assets/_Game/Input/SnakeControls.inputactions` |
| Snake Controller | Steer Action | empty. Assign only to replace Controls |
| Snake Controller | View Camera | empty (uses Main Camera) |
| Snake Controller | Control Enabled | on, until the session link limits it to Playing |
| Head Capsule Collider | Radius / Height | 0.5 / 2 |

`SnakeMovementConfig` defaults: forward speed 6, max turn 140 degrees per second, deadzone 0.15, camera-relative steering on. The wider turn keeps the head from curling back through the neck.

## Camera

`Bootstrap` has **Snake Camera Rig**. On Play it adds a Cinemachine Brain to Main Camera and a **Snake Third Person** virtual camera. **Snake Camera Follow** on Main Camera stays disabled so the two cameras do not fight.

The brain updates in LateUpdate. The head moves in Update, so a Smart Update brain would sample the kinematic Rigidbody in FixedUpdate and jitter.

The rig copies the head position onto **Camera Yaw** and eases that proxy's rotation. Third Person Follow orbits the proxy, which is what makes the turn smooth. **Yaw Sharpness** is that ease. Lower values let the head lead the turn.

| Field | Default | What it changes |
| --- | --- | --- |
| Target | Snake head | Follow and look target |
| Output Camera | Main Camera | Receives the brain |
| Camera Distance | 9 | Distance behind the head |
| Camera Height | 7 | Height above the head |
| Max Camera Distance | 16 | Distance at full zoom-out |
| Max Camera Height | 12 | Height at full zoom-out |
| Segments For Full Zoom | 40 | Body length that reaches the maximums |
| Zoom Smooth Time | 0.6 | How long a length change takes to settle |
| Yaw Sharpness | 5 | How fast the camera swings behind a turn |
| Position Damping | (0.05, 0.1, 0.05) | Catch-up time in seconds. Keep this small |
| Aim Damping | (0.25, 0.2) | How fast the look direction settles |
| Look Ahead | 6 | Metres in front of the head that stay in frame |
| Screen Y | 0.12 | Tips the view down so the ground ahead stays visible |

Change distance, height, and zoom on **Snake Camera Rig**. The rig writes those onto the generated Third Person Follow every frame.

Main Camera field of view stays 68. The virtual camera copies it.

## Mobile controls

`Assets/_Game/Input/SnakeControls.inputactions` is the steer asset. **Snake Controller** on the head uses **Player / Steer**.

Open the asset and confirm this setup. Do not add a Player Input component. Without one, every binding stays live, so the keyboard and the stick work at the same time. The action uses whichever binding has the larger magnitude. A centered stick does not cancel WASD.

Action map **Player**:

| Action | Type | Control Type | Initial State Check |
| --- | --- | --- | --- |
| Steer | Value | Vector2 | on |

Bindings on **Steer**:

| Name | Path | Composite | Group | Processors |
| --- | --- | --- | --- | --- |
| WASD | 2DVector | composite | Keyboard&Mouse | |
| up | `<Keyboard>/w` | part of WASD | Keyboard&Mouse | |
| down | `<Keyboard>/s` | part of WASD | Keyboard&Mouse | |
| left | `<Keyboard>/a` | part of WASD | Keyboard&Mouse | |
| right | `<Keyboard>/d` | part of WASD | Keyboard&Mouse | |
| Arrows | 2DVector | composite | Keyboard&Mouse | |
| up | `<Keyboard>/upArrow` | part of Arrows | Keyboard&Mouse | |
| down | `<Keyboard>/downArrow` | part of Arrows | Keyboard&Mouse | |
| left | `<Keyboard>/leftArrow` | part of Arrows | Keyboard&Mouse | |
| right | `<Keyboard>/rightArrow` | part of Arrows | Keyboard&Mouse | |
| | `<Gamepad>/leftStick` | binding | Gamepad | `stickDeadzone(min=0.15,max=0.925)` |

Control schemes, for the Input Actions window only:

| Scheme | Binding Group | Device |
| --- | --- | --- |
| Keyboard&Mouse | Keyboard&Mouse | Keyboard, Mouse optional |
| Gamepad | Gamepad | Gamepad |
| Touch | Touch | Touchscreen |

The on-screen stick does not have its own action. It feeds `<Gamepad>/leftStick`, which is the binding above. Android and iOS touches are read by that stick. Project Settings **Active Input Handling** stays **Input System Package**.

**Mobile Controls** builds the joystick on Play:

1. Select **Mobile Controls**.
2. **Control Path** is `<Gamepad>/leftStick`.
3. **Movement Range** is 80. **Stick Size** is 180. **Screen Margin** is 56.
4. **Canvas Sorting Order** is 5.
5. Press Play. The Hierarchy gains **Mobile Controls Canvas**, **EventSystem**, and **Input System UI Input Module**. The module fills its default UI actions on enable.
6. Drag the stick with the mouse in the Editor, or with a finger on a device. WASD and the arrows still steer when the stick is centered.

The canvas has no full-screen image. Only the stick graphic receives pointers, so a button outside that corner still clicks. Put gameplay buttons on a Canvas with **Sort Order** above 5, and keep them out of the bottom-left stick. If a button must overlap the stick, the higher canvas wins and the stick ignores that touch.

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

The camera starts close to the head and moves out as the body grows. Select **Snake Camera Rig** and raise **Max Camera Distance** and **Max Camera Height** to see more of a long trail. **Segments For Full Zoom** is the length that reaches those maximums.

Manual wiring, if the controller is missing from a head:

1. Create the config with **Serpent Rush > Snake Body Config**, or use the asset already in `Assets/_Game/Data`.
2. Confirm `Assets/_Game/Prefabs/SnakeSegment.prefab` has **Snake Segment** on the root, the capsule mesh, `SnakeHead` material, and no Rigidbody.
3. On the same object as **Snake Movement**, add **Snake Body Controller**.
4. Assign the config and the segment prefab.
5. Leave the segment unparented in the scene. The controller pools instances at runtime.

## Energy

Orbs are pooled and grow the snake during the current run. Nothing is saved.

`Bootstrap` contains `Energy Spawner`. It uses `Assets/_Game/Data/EnergyConfig.asset` and `Assets/_Game/Prefabs/EnergyOrb.prefab`. `SnakeHead` has **Snake Growth Controller** and a kinematic Rigidbody so the head can trip the orb triggers. Body segments still have no Rigidbody.

1. Let the Editor import, then open `Bootstrap`.
2. Press Play. Yellow, blue, and purple orbs appear on the ground. They bob in place.
3. Steer into an orb. It flashes white, pops, and shrinks, then returns to the pool. A new orb appears elsewhere.
4. Energy is kept for this run. Every 5 points, **Snake Growth Controller** calls `SnakeBodyController` and adds one segment. A rare orb is 5 points, so it grows the snake immediately. An epic orb is 10 points, so it adds two segments.
5. Pause and resume keeps the energy. Starting a new run from the menu or game over clears it.

| Energy | Default points | Look |
| --- | --- | --- |
| Common | 1 | Small yellow orb |
| Rare | 5 | Medium blue orb |
| Epic | 10 | Large purple orb |

Change the points, growth threshold, colors, sizes, and spawn weights on `EnergyConfig`. **Active Count** is how many orbs stay in the arena.

UI can subscribe to `EnergyChanged` on `GameManager.Events`. The payload has `TotalEnergy`, `AmountGained`, `GrowthProgress`, `GrowthThreshold`, `SegmentsGained`, and `Source`. `SnakeGrowthController.Changed` raises the same payload directly.
