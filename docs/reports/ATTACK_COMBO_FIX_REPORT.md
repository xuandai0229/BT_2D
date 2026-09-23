# Attack Combo Fix Report

## Root Cause

The `Player` in `Assets/Scenes/SampleScene.unity` uses `Assets/Animations/player1/PlayerAnimaiton.controller` (controller GUID `05a43b8cc481eca4293ceb375db73478`). The root Base Layer contained an unintended second set of Any State transitions directly to the four Attack states nested under `Combo`.

The Base Layer transition `Any State -> player_sword_atk2` had these serialized settings before the fix:

- `IsAttack = true`
- `AttackIndex Equals 2`
- Has Exit Time: off
- Transition Duration: 0
- Can Transition To Self: **on**

Unity Inspector showed that transition selected with `Can Transition To Self` checked. While Attack2 runs, the C# state intentionally keeps `IsAttack=true` and `AttackIndex=2` until its animation event is received. The duplicated Base Layer Any State route therefore re-entered Attack2 while its condition remained true, preventing the clip from reaching `TriggerAnimationEvent()`. That left the C# state waiting and the parameters stuck at Attack2.

The correct `Any State -> player_sword_atk2` transition inside `Combo` already had `Can Transition To Self` off. The Attack2 clip also already had one valid `TriggerAnimationEvent` at 0.33333334 seconds in a 0.4166667-second clip; it is one 12-fps frame before the clip end, matching Attack1's one-frame-before-end placement. The Attack2 motion GUID matched its `.anim` asset. The event and C# receiver were not the root cause.

A second Animator wiring defect was also present: the Base Layer had no usable `Combo -> Idle/Run` state-machine exit mappings. Those routes were added so a completed attack can return to the state selected by C#.

## Files Changed

- `Assets/Animations/player1/PlayerAnimaiton.controller`
  - Removed the four unintended Base Layer Any State routes to nested Attack states. Base Layer Any State is now empty; the four valid Any State routes remain inside Combo.
  - Added the missing Base Layer `Combo -> player_idle` and `Combo -> player_run` mappings with the required conditions.
  - Kept the existing C# state-machine architecture and existing Attack state transitions.
- `_codex_backup/attack_combo_fix/PlayerAnimaiton.controller.pre-fix`
  - Exact pre-fix serialized controller backup, created before editing.
- `docs/reports/attack_combo_backup_manifest.txt`
  - Pre-edit manifest with the reason and pre-fix state.
- `docs/reports/ATTACK_COMBO_FIX_REPORT.md`
  - This report.

No C# or `.anim` files were changed.

## Animator Before

- Base Layer Any State had duplicate transitions to Attack1–Attack4.
- The duplicate Attack2 route had `Can Transition To Self` enabled with `IsAttack=true` and `AttackIndex=2`.
- The valid Any State routes inside Combo had self-transition disabled.
- Combo exit transitions were present on the graph, but the parent state machine had no valid mappings for Combo to return to Idle or Run.

## Animator After

- Base Layer Any State has no attack transitions.
- Combo contains the four attack states and four Any State routes:
  - Attack1: `IsAttack=true`, `AttackIndex Equals 1`
  - Attack2: `IsAttack=true`, `AttackIndex Equals 2`
  - Attack3: `IsAttack=true`, `AttackIndex Equals 3`
  - Attack4: `IsAttack=true`, `AttackIndex Equals 4`
- All four Combo routes have Has Exit Time off, Transition Duration 0, and Can Transition To Self off.
- Each Attack state has a transition to Exit conditioned on `IsAttack=false`.
- Idle and Run enter Combo on `IsAttack=true`; both transitions have Has Exit Time off and Duration 0.
- Combo returns to Idle on `IsAttack=false` and `IsRun=false`, or to Run on `IsAttack=false` and `IsRun=true`.
- Each attack state Motion GUID matches its named `.anim` clip. The attack path uses direct clips, not the empty Blend Tree asset.
- Unity reimported the edited controller successfully.

## Animation Event Verification

Serialized `.anim` inspection:

- Attack1: PASS — one `TriggerAnimationEvent` at 0.5833333 s; clip stop time 0.6666666 s.
- Attack2: PASS — one `TriggerAnimationEvent` at 0.33333334 s; clip stop time 0.4166667 s.
- Attack3: PASS — one `TriggerAnimationEvent` at 0.41666666 s; clip stop time 0.5 s.
- Attack4: PASS — one `TriggerAnimationEvent` at 0.41666666 s; clip stop time 0.5 s.

Each event is one sample (1/12 second at 12 fps) before its clip stop time. `PlayerController.TriggerAnimationEvent()` is public, parameterless, and sets the `AnimationEvent` property. In SampleScene the Player Animator and PlayerController are components on the same Player GameObject.

## State Machine Verification

Source inspection after the buffered-combo follow-up: PASS (static verification).

- `PlayerStateBase.Enter()` clears `AnimationEvent` when a state is entered.
- The shared `PlayerAttackState` remains active across chained strikes. Each pressed Attack input is consumed and queued; only the current clip's `TriggerAnimationEvent` advances the index.
- The event flag is cleared after each strike while remaining in AttackState, so the next clip waits for its own event.
- When no next strike is queued (or Attack4 completes), the state clears `IsAttack` and `AttackIndex`, then selects Fall if airborne, otherwise Run with movement input or Idle without it.
- `PlayerAttackState.Exit()` also clears the attack parameters and internal queue/index.
- No per-attack C# states or Animator-only architecture were added. Idle/Run still enter the shared `PlayerAttackState`.
- This supplied continuous-buffered behavior intentionally starts each new combo at Attack1; the former 1.5-second cross-state index logic is no longer used by `PlayerAttackState`.

Runtime transitions Idle→Attack, Run→Attack, Attack→Idle, and Attack→Run: NOT VERIFIED in Play Mode.

## Combo Verification

Static source/controller verification:

- Attack1 route and motion: PASS (serialized).
- Attack2 route and motion: PASS (serialized; duplicate self-restarting parent route removed).
- Attack3 route and motion: PASS (serialized).
- Attack4 route and motion: PASS (serialized).
- Buffered advancement 1→2→3→4 on successive input presses and animation events: PASS (source inspection only).
- After Attack4, additional presses are ignored until the combo exits; the next AttackState entry starts at Attack1: PASS (source inspection only).
- Timeout reset after 1.5 seconds: NOT APPLICABLE to the new supplied continuous-buffered implementation; each completed combo starts the next entry at Attack1.

Runtime order, event delivery, wrap, and reset behavior: NOT VERIFIED.

## Console / Compile

- Controller import: PASS; Unity Editor log records the controller import after its edit.
- Buffered-combo C# script: Unity script compilation was requested after a script asset change; `Library/ScriptAssemblies/Assembly-CSharp.dll` was updated after the C# edit, and the inspected Editor log contains no C# compiler error. Compile: PASS based on Unity Editor compilation evidence.
- An external `dotnet build Assembly-CSharp.csproj --no-restore` was attempted but the generated legacy Unity project returned `Build FAILED` with 0 warnings and 0 errors, so it was not useful as a standalone compiler check.
- Play Mode Console verification was not completed; runtime errors/warnings are NOT VERIFIED.

## Runtime Verification

NOT VERIFIED. Play Mode tests could not be completed because the desktop-control call was interrupted at the point of starting Play Mode. No runtime pass is claimed.

- TEST 1 Idle → Attack1 → event → Idle: NOT VERIFIED.
- TEST 2 buffered Attack1 → Attack2 → event → Idle: NOT VERIFIED.
- TEST 3 Attack3: NOT VERIFIED.
- TEST 4 Attack4: NOT VERIFIED.
- TEST 5 Attack4 then next combo starts at Attack1: NOT VERIFIED.
- TEST 6 wait more than 1.5 seconds then Attack1: NOT APPLICABLE to the new supplied implementation; runtime reset-to-Attack1 has not been tested.
- TEST 7 Run → Attack and return to Run while moving: NOT VERIFIED.
- TEST 8 no restart loop, stuck parameters, missing receiver/parameter, null reference, or stuck Combo: NOT VERIFIED at runtime.

## Buffered Combo Follow-up

- Changed `Assets/Scips/Player1/PlayerAttackState.cs` to stay in the existing AttackState while chaining. It buffers one Attack press at a time and advances AttackIndex only when the active clip signals its AnimationEvent.
- Preserved the existing `PlayerController.ConsumeAttackPressed()`, `AnimationEvent` property, `TriggerAnimationEvent()` receiver, and `PlayerStateBase.Enter()` reset behavior; no other C# file or Animator asset was changed in this follow-up.
- Pre-edit backup: `_codex_backup/attack_combo_fix/PlayerAttackState.cs.pre-buffered-combo`.
- The manifest records the pre-edit file state and SHA256: `docs/reports/attack_combo_backup_manifest.txt`.
