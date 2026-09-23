# Attack Combo Audit

**Project:** `D:\2D\2dd\Game2D`  
**Unity:** `6000.4.5f1`  
**Phạm vi:** scene đang bật trong Build Settings (`Assets/Scenes/SampleScene.unity`), Player trong scene, controller thực sự gắn trên Animator, Input Actions, tám script state/controller và bốn clip attack.  
**Phương pháp:** đọc file YAML/C#/.meta, đối chiếu GUID và Editor log; không sửa code/asset, không chạy Play Mode.

## 1. Executive Summary

**Kết luận hiện tại: FAIL.** Input và C# tạo được yêu cầu vào `PlayerAttackState`, nhưng chuỗi tấn công không thể được xác nhận là hoạt động. `player_sword_atk1` trong Combo dùng Blend Tree không có motion con; cả bốn clip attack không có event `TriggerAnimationEvent` ở cuối. Vì vậy `PlayerAttackState.Update()` không nhận tín hiệu kết thúc từ các clip và không có đường thoát bình thường về Idle/Run. Các transition Animator còn có state đặt tên sai chỉ số, đặt ở Base Layer Any State thay vì Combo Any State, cho phép self transition, và thiếu đường ra Combo rõ ràng.

**Điểm cần phân biệt:** với `AttackIndex` 2, 3, 4, GUID motion ở đích lần lượt là clip attack2, attack3, attack4. Tên Animator state ở các đích này sai, nhưng không được kết luận rằng hình ảnh 2–4 bị đảo clip. Attack1 thực sự không gắn clip attack1 ở bất kỳ state nào trong controller đang dùng.

**Runtime confidence: FAIL đối với mục tiêu combo; kết quả Play Mode: NOT VERIFIED.** Đây là kết luận từ dữ liệu project và flow code, không phải tuyên bố đã chạy thử game.

## 2. Current Architecture

### C# State Machine thực tế

```text
PlayerInput (Unity Events) -> PlayerController.Attack(ctx.performed)
                         -> _attackPressed = true
Idle.Update / Run.Update -> ConsumeAttackPressed()
                         -> PlayerStateMachine.ChangeState(AttackState)
                         -> old.Exit() -> AttackState.Enter()
AttackState.Enter()      -> AnimationEvent=false; StopHorizontal();
                            IsRun/IsJump/IsFall=false; IsAttack=true;
                            tăng _comboAttackIndex; reset nếu quá 4 hoặc quá 1.5 s;
                            SetInteger(AttackIndex)
AttackState.Update()     -> chỉ thoát nếu AnimationEvent=true
                         -> HasMoveInput ? RunState : IdleState
AttackState.Exit()       -> IsAttack=false; AttackIndex=0
```

`PlayerStateMachine.ChangeState()` bỏ qua `newState == _currentState` (`Assets/Scips/Player1/PlayerStateMachine.cs:11`). Thiết kế hiện tại **không cần** AttackState -> AttackState: một event hợp lệ sẽ đưa AttackState -> Idle/Run, rồi input mới đưa Idle/Run -> cùng đối tượng AttackState. Bộ đếm combo nằm trong đối tượng AttackState nên được giữ qua các lần vào. Hiện đường này đứt ở Animation Event/Animator, không phải ở điều kiện `newState == _currentState`.

### Animator thực tế

`SampleScene.unity:521` gắn `Assets/Animations/player1/PlayerAnimaiton.controller` (GUID `05a43b8cc481eca4293ceb375db73478`) trên chính GameObject Player. Base Layer mặc định là `player_idle`, có `player_run`, `player_jump`, `player_fall`, nhiều state attack cũ/trùng, và sub-state machine `Combo`. `player_idle` và `player_run` có transition vào Combo với `IsAttack=true`, **Has Exit Time=true**, duration `0.25` (`PlayerAnimaiton.controller:930-1054`). Base Layer cũng có bốn Any State transition theo `IsAttack=true` + `AttackIndex` bằng 1/2/3/4 (`:143-146`, `:241-262`, `:874-923`, `:1173-1194`). Bốn transition đó trỏ trực tiếp vào child states của Combo; danh sách Any State **bên trong** Combo rỗng (`:1131`).

Combo mặc định vào state tên `player_sword_atk1` (`:1139`), nhưng motion của state này là Blend Tree rỗng (`:1161`, `:312`); không có clip attack1. Child states có các transition tới Exit khi `IsAttack=false`, nhưng Base Layer không có transition định tuyến từ Combo Exit sang `player_idle`/`player_run` (`:149-151`).

### Input thực tế

`Assets/InputSystem_Actions.inputactions` có map `Player` và action `Attack` loại `Button` (`:6`, `:28-29`), binding gamepad West, chuột trái, touch tap, joystick trigger, XR PrimaryAction và Enter (`:258-319`). `SampleScene.unity:650-699` có `PlayerInput`, behavior `2` (Invoke Unity Events), action GUID trùng asset, và Unity Event `Player/Attack` trỏ tới `PlayerController.Attack`. PlayerController và Animator ở cùng GameObject (`SampleScene.unity:505-521`, `:829`). `m_DefaultActionMap` để rỗng (`:778`); map đầu tiên của asset là `Player`, nhưng hoạt động chọn map lúc chạy chưa được thử bằng Play Mode.

## 3. What Works

- Tên/type parameter khớp code: `IsAttack` Bool (`m_Type: 4`) và `AttackIndex` Int (`m_Type: 3`), không có lỗi chính tả `AtackIndex`/`IsAtack` trong controller đang gắn (`PlayerAnimaiton.controller:764-774`; `PlayerAttackState.cs:8-9,32,41`).
- `ctx.performed` đặt `_attackPressed`, và `ConsumeAttackPressed()` xóa cờ sau khi đọc (`PlayerController.cs:115-140`). Idle và Run đều ưu tiên kiểm tra Attack trước khi chuyển do input di chuyển (`PlayerIdleState.cs:27-30`; `PlayerRunState.cs:26-29`).
- `AttackState.Enter()` gọi `base.Enter()`, đặt `AnimationEvent=false`, dừng vận tốc ngang và đặt các bool movement về false (`PlayerAttackState.cs:22-41`; `PlayerStateBase.cs:24-27`). Không thấy điều kiện trong C# tự thoát Attack quá sớm.
- Công thức chỉ số C# tạo 1 -> 2 -> 3 -> 4 -> 1 và reset về 1 nếu lần vào Attack tiếp theo cách lần vào trước hơn 1.5 giây (`PlayerAttackState.cs:34-48`). `Exit()` đặt `IsAttack=false`, `AttackIndex=0` mà không xóa `_comboAttackIndex` (`:80-90`).
- `PlayerController.TriggerAnimationEvent()` là `public void` trên cùng GameObject có Animator, nên **có thể nhận** Animation Event nếu clip thực sự phát và event được gắn (`PlayerController.cs:193-196`; `SampleScene.unity:505-521,829`).
- Bốn clip `.anim` có binding `SpriteRenderer.m_Sprite` ở path rỗng (đúng GameObject có Animator/SpriteRenderer), sprite-sheet GUID `2bfeca0e924d0b94291b3d40e0ef7381` giải được tới `adventurer-v1.5-Sheet.png.meta`; `m_LoopTime: 0` ở cả bốn clip. Clip có keyframe: attack1 8 frame/0.6667 s, attack2 5 frame/0.4167 s, attack3 và attack4 6 frame/0.5 s, đều ở 12 fps.
- Scene có Rigidbody2D, Animator, SpriteRenderer, PlayerController, PlayerInput cùng GameObject; `groundCheck` được gán (`SampleScene.unity:505-829`). GUID script/controller/action chính khớp `.meta`.
- Unity Editor đúng bản nằm tại `C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Unity.exe`. `Library/ScriptAssemblies/Assembly-CSharp.dll` được tạo lúc 17:14:28, sau thời điểm sửa cuối của các script Player1 (17:08:17). Editor log ghi các lần compile thành công sau lỗi CS0103 lịch sử; không thấy lỗi C# mới sau lần build cuối (`Editor.log` quanh dòng 7529-7546, 7841-7858, 8856-8873).

## 4. Problems Found

### ATTACK-001 — Critical

- **File/line:** `Assets/Sprites/v2.1/Test Player/player_sword_atk1.anim:89-105`; `player_sword_atk2.anim:171`; `player_sword_atk3.anim:174`; `player_sword_atk4.anim:174`; `Assets/Scips/Player1/PlayerAttackState.cs:56-68`.
- **Current behavior:** không clip nào có event `TriggerAnimationEvent` ở gần cuối. Attack1 chỉ có `EnableEntityHitbox` tại 0.25 s và `DisableEntityHitbox` tại 0.4167 s; Attack2–4 có `m_Events: []`.
- **Expected:** mỗi clip attack đang phát gọi `TriggerAnimationEvent` đúng một lần trước khi kết thúc, để `AnimationEvent=true` và AttackState thoát.
- **Root cause:** dữ liệu `.anim` chưa gắn event kết thúc. Method C# tồn tại nhưng không được clip gọi.
- **Fix recommendation:** gắn event `TriggerAnimationEvent` vào từng clip ở thời điểm cuối phù hợp, kiểm tra thời điểm thật với exit/transition; xác nhận Console không báo receiver lỗi và state thoát đúng một lần.

### ATTACK-002 — Critical

- **File/line:** `Assets/Animations/player1/PlayerAnimaiton.controller:1113-1161`, đặc biệt `:1139`, `:1147`, `:1161`, Blend Tree `:305-313`; clip attack1 GUID `50811066693088441b0c0a4c7129595d`.
- **Current behavior:** `AttackIndex=1` trỏ vào Combo child `player_sword_atk1`, nhưng motion là Blend Tree `-4028228276883824828` với `m_Childs: []`. GUID clip `player_sword_atk1.anim` không xuất hiện trong controller đang gắn.
- **Expected:** state đích của index 1 phát đúng clip attack1, để có hình ảnh và event kết thúc.
- **Root cause:** motion của state Attack1 không được nối tới clip.
- **Fix recommendation:** gắn clip attack1 đúng GUID cho child state được chọn; tránh dùng các state trùng tên/Blend Tree rỗng còn sót làm đường chạy chính.

### ATTACK-003 — High

- **File/line:** `Assets/Animations/player1/PlayerAnimaiton.controller:143-146,241-262,874-923,1173-1194`.
- **Current behavior:** bốn transition theo index nằm ở **Base Layer Any State**, còn Combo Any State rỗng. Cả bốn transition có `m_CanTransitionToSelf: 1` và `m_HasExitTime: 0`; `IsAttack=true` giữ suốt AttackState. Điều này cho phép Animator đánh giá lại transition tới chính attack state khi điều kiện vẫn đúng, có nguy cơ restart animation liên tục hoặc chen ngang đường vào Combo/exit. Chưa Play Mode để xác nhận tần suất restart.
- **Expected:** mỗi input attack chọn đúng một state/clip và phát hết một lần; transition không tự kích lại khi giữ nguyên bool/index.
- **Root cause:** phạm vi Any State và cờ self transition không phù hợp với bool duy trì trong cả đòn.
- **Fix recommendation:** chỉnh các transition attack trong controller để chỉ kích một lần cho mỗi lần vào Attack; xác minh trong Animator Play Mode. Giữ C# State Machine hiện tại và `AttackIndex` 1–4.

### ATTACK-004 — High

- **File/line:** `Assets/Animations/player1/PlayerAnimaiton.controller:149-151,1131-1139`; các transition child tới Exit tại `:55-78`, `:1254-1272` và các block tương ứng.
- **Current behavior:** child attack states có điều kiện `IsAttack=false` để đi tới Exit (đa số Has Exit Time=true, duration 0.25), nhưng Base Layer lưu `m_StateMachineTransitions` cho Combo là `second: []`; không thấy đường Combo Exit -> `player_idle` hoặc `player_run` phụ thuộc `IsRun`. Với ATTACK-001, C# còn không bao giờ đặt `IsAttack=false` qua luồng thông thường. Vì thế đường trả về hình ảnh Idle/Run chưa được chứng minh.
- **Expected:** khi event kết thúc và C# chọn Idle/Run, Animator cũng rời Combo đúng state movement tương ứng.
- **Root cause:** thiếu định tuyến Animator rõ ràng từ Combo ra movement, cộng với event kết thúc chưa có.
- **Fix recommendation:** nối đường thoát Combo tới Idle/Run theo bool movement phù hợp; kiểm tra thời điểm Exit Time và duration sau khi event được thêm.

### ATTACK-005 — Medium

- **File/line:** `Assets/Animations/player1/PlayerAnimaiton.controller:285-299,579-593,1228-1242`; transition index tại `:241-262,874-923`.
- **Current behavior:** `AttackIndex=2` tới state tên `player_sword_atk4` nhưng motion GUID là clip attack2; `AttackIndex=3` tới state tên `player_sword_atk2` nhưng motion là clip attack3; `AttackIndex=4` tới state tên `player_sword_atk3 0` nhưng motion là clip attack4. Combo còn child `player_sword_atk3` khác không có outgoing transition (`:1339-1352`).
- **Expected:** tên state, index và clip tương ứng một một, không có state trùng gây chọn nhầm khi sửa.
- **Root cause:** tên và các bản sao state chưa được đồng bộ khi nối motion/transition.
- **Fix recommendation:** sau khi xác nhận flow mong muốn, sửa tối thiểu tên/đích state để index 1–4 trùng tên và clip; rà soát state trùng, không xóa hàng loạt asset cũ khi chưa xác định tham chiếu.

### ATTACK-006 — Medium

- **File/line:** `Assets/Sprites/v2.1/Test Player/player_sword_atk1.anim:89-105`; `Assets/Scips/Player1/PlayerController.cs:193-196`.
- **Current behavior:** Attack1 có event `EnableEntityHitbox` và `DisableEntityHitbox`, nhưng tìm trong toàn bộ C# dưới `Assets` không có method cùng tên. PlayerController chỉ có `TriggerAnimationEvent`. Khi clip attack1 được nối và phát, hai event cũ có nguy cơ báo không có receiver trên GameObject Animator.
- **Expected:** mọi event trên clip có method nhận hợp lệ cùng GameObject Animator, hoặc được chủ ý thay bằng event phục vụ hệ thống hiện tại.
- **Root cause:** clip mang event từ hệ thống attack/hitbox khác, không khớp PlayerController hiện tại.
- **Fix recommendation:** xác định có thật sự cần hitbox trong scope này; nếu có, bổ sung receiver đúng theo thiết kế hitbox; nếu không, xử lý event cũ khi nối clip. Không xóa event/asset trước khi kiểm tra mục đích gameplay.

### ATTACK-007 — Medium

- **File/line:** `Assets/Animations/player1/PlayerAnimaiton.controller:930-1054`; `Assets/Scips/Player1/PlayerAttackState.cs:32-41`.
- **Current behavior:** Idle -> Combo và Run -> Combo có `Has Exit Time=true` (lần lượt 0.25 và 0.7083334) và duration 0.25. C# đặt `IsAttack=true` ngay khi nhận input, nhưng Animator có thể chờ đến exit time của Idle/Run. Base Any State hiện có thể tranh quyền và che lỗi này.
- **Expected:** độ trễ vào đòn được xác định và phù hợp input; không có khoảng C# đã ở AttackState nhưng Animator còn ở movement kéo dài ngoài ý muốn.
- **Root cause:** timing transition Animator chưa ăn khớp với flow C# và các đường Any State song song.
- **Fix recommendation:** sau khi dọn đường chọn attack, kiểm tra bằng Animator Play Mode và điều chỉnh Has Exit Time/duration ở đúng transition nếu cần.

### ATTACK-008 — Low / design warning

- **File/line:** `Assets/Scips/Player1/PlayerController.cs:115-140`; `PlayerAttackState.cs:34-48,56-68`.
- **Current behavior:** input `Attack` khi đang ở AttackState đặt `_attackPressed=true`, nhưng AttackState không consume cờ; lần đầu Idle/Run chạy lại sẽ consume và có thể bắt đầu đòn kế tiếp từ input đã bấm trước khi đòn hiện tại kết thúc. Cờ chỉ là bool nên nhiều lần bấm trong một đòn gộp thành một. Mốc 1.5 s được tính giữa **thời điểm bắt đầu** hai đòn, không phải từ thời điểm kết thúc clip trước.
- **Expected:** quy tắc buffer và thời gian chain được định nghĩa rõ, không dùng input quá cũ; mỗi đòn kế tiếp vẫn đòi một lần `performed` tương ứng.
- **Root cause:** cờ input không có timestamp/expiry; `_lastAttackTime` ghi trong `Enter()`.
- **Fix recommendation:** xác nhận ý đồ buffer bằng test sau khi P0 được sửa; chỉ thay đổi tối thiểu nếu test chứng minh có input cũ ngoài cửa sổ mong muốn.

## 5. Animator Audit

**Controller đang dùng:** `Assets/Animations/player1/PlayerAnimaiton.controller`; các controller khác trong project không được gắn trên Player của `SampleScene` và không dùng để kết luận flow này.

**Parameters:** `IsJump` Bool, `IsRun` Bool, `IsFall` Bool, `Blend` Float, `IsAttack` Bool, `AttackIndex` Int (`:739-774`). Tên `IsAttack`/`AttackIndex` khớp C#; condition integer `m_ConditionMode: 3` là Equals, không phải Greater.

**Đường vào:** Idle -> Combo (`IsAttack=true`, Has Exit Time 0.25, duration 0.25); Run -> Combo (`IsAttack=true`, Has Exit Time 0.7083334, duration 0.25). Base Layer Any State còn bốn transition trực tiếp tới Combo child, Has Exit Time=false, duration 0, Can Transition To Self=true. Combo Any State rỗng. Combo default = `player_sword_atk1` (Blend Tree rỗng). **Đường ra:** bốn child đích có transition `IsAttack=false` tới Exit, song thiếu định tuyến từ Combo ra Idle/Run ở Base Layer.

**Mapping thật theo GUID:**

- `AttackIndex=1` -> state `player_sword_atk1` trong Combo -> Blend Tree rỗng; clip `player_sword_atk1.anim` GUID `508110...` không được tham chiếu.
- `AttackIndex=2` -> state tên `player_sword_atk4` trong Combo -> GUID `eb88b45b...` = **clip `player_sword_atk2.anim`**.
- `AttackIndex=3` -> state tên `player_sword_atk2` trong Combo -> GUID `e5a4ecb9...` = **clip `player_sword_atk3.anim`**.
- `AttackIndex=4` -> state tên `player_sword_atk3 0` trong Combo -> GUID `f71ca227...` = **clip `player_sword_atk4.anim`**.

**Clips:** attack1/2/3/4 đều one-shot (`m_LoopTime: 0`) và có keyframe `SpriteRenderer.m_Sprite` trỏ tới sprite sheet hợp lệ. `TriggerAnimationEvent`: **0/4 clip**. Attack1 có hai event hitbox không có receiver C# tìm thấy; Attack2–4 không có event. Chưa có event gần cuối để đánh giá khả năng được gọi trước exit; hiện exit theo `IsAttack=false` không được kích qua luồng thông thường.

## 6. Input Audit

- Asset `InputSystem_Actions.inputactions` GUID `2bcd2660ca9b64942af0de543d8d7100` khớp `PlayerInput.m_Actions` trong scene (`SampleScene.unity:651`). `Attack` ID `6c2ab1b8-8984-453a-af3d-a3c78ae1679a` khớp action event trong scene (`:698`).
- Action type là Button, không có interaction riêng; binding chuột trái và Enter chắc chắn có trong map Player, bên cạnh gamepad/touch/joystick/XR. `PlayerInput.m_NotificationBehavior: 2` và Unity Event gọi `PlayerController.Attack(InputAction.CallbackContext)` (`:650-699`).
- Callback chỉ đặt cờ khi `ctx.performed`; `ConsumeAttackPressed()` trả true một lần rồi xóa. Idle và Run đều gọi nó. Các điều kiện C# cho Idle -> Attack và Run -> Attack là **PASS về mặt tĩnh**. `m_DefaultActionMap` rỗng và việc thiết bị/control scheme thực tế chưa được thử trong Play Mode.

## 7. State Machine Audit

- `ChangeState()` thực hiện `old.Exit() -> current=newState -> new.Enter()` và chặn null, self, shutdown (`PlayerStateMachine.cs:9-21`). Self guard không cản combo **nếu** event đưa Attack -> Idle/Run trước input tiếp theo.
- `PlayerStateBase.Enter()` reset `AnimationEvent=false`; AttackState gọi base nên không mang event cũ vào đòn kế tiếp (`PlayerStateBase.cs:24-27`, `PlayerAttackState.cs:22-24`). Idle/Run không gọi base, nhưng cũng không đọc `AnimationEvent`, nên chưa thấy đường thoát Attack quá sớm do cờ cũ.
- `AttackState.Update()` chỉ kiểm tra `AnimationEvent`; không có timer fallback. Với dữ liệu clip hiện tại, C# sẽ duy trì AttackState, `IsAttack=true` và `StopHorizontal()` ở mỗi FixedUpdate. `Exit()` xóa `IsAttack`/`AttackIndex` chỉ chạy khi có chuyển state thành công (`PlayerAttackState.cs:52-90`).
- Sau một event hợp lệ, nhánh `HasMoveInput ? RunState : IdleState` là đúng theo yêu cầu ở mức code. Phần Animator trả về hình ảnh movement còn thiếu định tuyến như ATTACK-004.
- Jump có ưu tiên trước Attack trong Idle/Run nếu cùng khung hình và đang grounded (`PlayerIdleState.cs:19-30`; `PlayerRunState.cs:18-29`); đây là hành vi thực tế, không kết luận là lỗi attack nếu chưa có yêu cầu đổi ưu tiên.

## 8. Combo Sequence

**Ý định trong C#:** lần vào AttackState thứ nhất -> 1; lần sau trong tối đa 1.5 s tính từ `Enter()` trước -> 2, rồi 3, rồi 4; lần thứ năm -> 1. Nếu quá 1.5 s -> 1. Không có code tự gọi AttackState -> AttackState, và không tự tăng chỉ số khi animation chạy.

**Flow thực tế theo dữ liệu hiện có:**

```text
Idle/Run + Attack performed
  -> AttackState.Enter: IsAttack=true, AttackIndex=1
  -> Base Any State chọn Combo/player_sword_atk1
  -> motion Blend Tree rỗng, không phát clip attack1
  -> không có TriggerAnimationEvent
  -> AttackState.Update không chuyển về Idle/Run
  -> các input Attack sau chỉ giữ _attackPressed=true; Attack2/3/4 không tới được qua flow bình thường
```

Nếu ép `AttackIndex=2/3/4` bằng debug, đích của từng index có clip hình ảnh đúng số, nhưng cả ba clip đều thiếu event kết thúc và self transition vẫn là nguy cơ. Đây **không** phải bằng chứng combo chạy được.

## 9. Required Fixes

**P0 — bắt buộc để có flow 4 đòn:**

1. Nối clip `player_sword_atk1.anim` vào state index 1, bỏ motion rỗng khỏi đường chạy chính.
2. Gắn `TriggerAnimationEvent` vào cuối bốn clip với thời điểm được thử trong Play Mode; xử lý hai event hitbox cũ của attack1 theo mục đích gameplay.
3. Làm đường chọn attack/thoát Combo nhất quán với C# state, đảm bảo mỗi index chạy một lần và Animator trở về Idle/Run sau `IsAttack=false`; xác minh self transition và routing Combo Exit.

**P1 — nên sửa sau khi P0 được xác minh:**

1. Đồng bộ tên state với AttackIndex và GUID clip, rà soát các state trùng tên/không dùng.
2. Kiểm tra độ trễ Idle/Run -> Combo do Has Exit Time và transition duration; điều chỉnh theo cảm giác điều khiển mong muốn.
3. Quy định rõ input buffer trong AttackState và thời điểm bắt đầu cửa sổ 1.5 s.

**P2 — cải thiện sau:** thêm kiểm tra tự động cho GUID clip/Animator parameter/event và test Play Mode cho chuỗi combo để tránh tái diễn mismatch. Không cần đổi kiến trúc State Machine hay tạo bốn AttackState.

## 10. Verification Plan

1. Với Editor đang mở, sau P0 để Unity reimport và compile; xác nhận Console không còn C# error, missing receiver, missing Animator parameter, MissingReferenceException hoặc NullReferenceException liên quan Player.
2. Quan sát Animator trong Play Mode: mỗi lần `performed` từ Idle phát đúng Attack1 một lần; cuối clip gọi event đúng một lần, `IsAttack` về false, `AttackIndex` về 0, Animator vào Idle.
3. Bấm mới trong cửa sổ: lần lượt clip Attack2, Attack3, Attack4; sau Attack4 lần tiếp theo về Attack1. Không bấm thêm thì không tự chạy sang đòn sau.
4. Chờ hơn 1.5 s theo quy tắc thời gian đã chọn; input tiếp theo phải về Attack1. Thử bấm trong lúc clip đang chạy, bấm nhiều lần, bấm sát cuối, và giữ nút để xác định buffer/expiry thực tế.
5. Giữ Move từ Run -> Attack và sau event -> Run; thả Move trước cuối clip -> Idle. Xác nhận không mắc trong Combo hoặc restart vô hạn do Any State self transition.
6. Xem trực tiếp bốn clip trong Animation window để chắc chắn event ở gần cuối nhưng vẫn được gọi trước exit/crossfade; xác nhận sprite frame và `Loop Time` đúng.

### Status theo tiêu chí yêu cầu

- **State Machine:** PASS cấu trúc C#; FAIL flow end-to-end do không nhận event.
- **Input Attack:** PASS cấu hình asset/callback; NOT VERIFIED bằng thiết bị thật trong Play Mode.
- **Idle -> Attack:** PASS đường C#; FAIL hoạt động trọn đòn do Animator/event.
- **Run -> Attack:** PASS đường C#; FAIL hoạt động trọn đòn do Animator/event; WARNING Has Exit Time.
- **Attack1:** FAIL, motion rỗng và không có event kết thúc.
- **Attack2:** FAIL end-to-end; GUID clip đúng theo index 2 nhưng tên state sai và thiếu event.
- **Attack3:** FAIL end-to-end; GUID clip đúng theo index 3 nhưng tên state sai và thiếu event.
- **Attack4:** FAIL end-to-end; GUID clip đúng theo index 4 nhưng tên state sai và thiếu event.
- **Combo reset:** PASS công thức C#; FAIL xác minh end-to-end vì không thoát được Attack1.
- **Animator Parameters:** PASS tên/type `IsAttack` Bool và `AttackIndex` Int.
- **Animator Transitions:** FAIL bố trí Any State/self và thiếu đường thoát Combo rõ ràng; WARNING timing Idle/Run -> Combo.
- **Animation Events:** FAIL, 0/4 clip có `TriggerAnimationEvent`; Attack1 có hai event không tìm thấy receiver.
- **Return to Idle:** PASS nhánh C# khi có event; FAIL flow thực tế.
- **Return to Run:** PASS nhánh C# khi có event và MoveInput; FAIL flow thực tế.
- **Compile:** PASS theo lần compile Unity Editor gần nhất; batch check không chạy vì project đang mở/lock. Editor log có lỗi CS0103 cũ, nhưng các lần compile sau đã thành công và `_attackPressed` hiện tồn tại.
- **Runtime confidence:** FAIL đối với tiêu chí combo; Play Mode chưa chạy nên không chứng nhận Console sạch hoặc các transition hoạt động đúng frame.

### Giới hạn kiểm chứng

`Temp/UnityLockfile` tồn tại và có nhiều tiến trình Unity đang chạy; không mở batch Unity thứ hai trên cùng project để tránh xung đột. Báo cáo dựa trên asset serialized, C# và log compile mới nhất. Editor log còn có `NullReferenceException` ở UIElements RenderChain cũ (khoảng dòng 6631), không có bằng chứng nó xuất phát từ Player; do đó không dùng nó để quy lỗi gameplay hoặc để chứng nhận Console hiện tại sạch. Không thay đổi code, Animator, clip, Input Actions, prefab hay scene trong lượt audit này.
