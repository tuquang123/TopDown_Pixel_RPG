# Hướng dẫn nhanh cho người mới – TopDown_Pixel_RPG

## 1) Dự án này là gì?
Đây là game RPG top-down làm bằng Unity, có các phần chính: gameplay single-player (đánh quái, lên level, quest), inventory/equipment/skill, save-load, và có thêm module multiplayer thử nghiệm (Photon PUN + NGO).

## 2) Cấu trúc tổng thể (mental model)

### Scene flow
- Build Settings đang bật 3 scene chính: `HomeScene -> LoadingScene -> GameScene`.

### Các module gameplay chính
- **Player** (`Assets/Scripts/Player`): di chuyển, auto-attack, stat, level.
- **Enemy** (`Assets/Scripts/Enemy`): AI, patrol/aggro, damage, drop.
- **Level** (`Assets/Scripts/Level`): chuyển màn, spawn quái, lưu vị trí theo level.
- **Quest** (`Assets/Scripts/Quest`): nhận nhiệm vụ, theo dõi objective, trả thưởng.
- **Inventory/Equipment/Skill** (`Assets/Scripts/Player/Inventory` + các file liên quan): dữ liệu item, đeo đồ, skill.
- **UI/Currency** (`Assets/Scripts/Ui`): UI tổng, vàng/ngọc, popup.
- **SaveLoad** (`Assets/Scripts/SaveLoad`): lưu file JSON cho dữ liệu người chơi.
- **Pattern** (`Assets/Scripts/Pattern`): Singleton, object pool, event support.

## 3) Những file “xương sống” nên đọc trước

1. **`CommonReferent`**
   - Hoạt động như “service locator” chứa reference đến object cốt lõi: player, inventory, item DB, skill, level DB...
   - Nhiều hệ thống gọi `CommonReferent.Instance` để lấy dependency.

2. **`GameManager`**
   - Khởi tạo save system (`SaveManager.Initialize()`), gọi load đầu game, bật autosave định kỳ.
   - Đây là “điểm vào” cho vòng đời save/load runtime.

3. **`PlayerController` + `PlayerStats`**
   - `PlayerController`: input, di chuyển, auto-target, auto-attack theo loại vũ khí, gọi animation trigger.
   - `PlayerStats`: quản lý stat thực tế, nhận sát thương, chết/hồi sinh, trigger UI event.

4. **`EnemyAI`**
   - Vòng lặp AI cơ bản: patrol -> phát hiện mục tiêu -> tiến tới -> tấn công.
   - Có hệ aggro, pressure từ ranged hit, drop logic, đăng ký với `EnemyTracker`.

5. **`LevelManager`**
   - Load level prefab theo `LevelDatabase`, đặt lại vị trí player + allies.
   - Gọi spawn quái từ `SpawnPoint`, clear pool/enemy khi đổi màn.

6. **`SaveManager`**
   - Save JSON theo slot, có backup file.
   - Ghi/lấy inventory, equipment, skill, level và quest progress.

7. **`QuestManager` + `CurrencyManager`**
   - `QuestManager` điều phối trạng thái quest (`NotAccepted`, `InProgress`, `Completed`, `Rewarded`).
   - `CurrencyManager` quản lý vàng/ngọc, có event cập nhật UI và liên kết quest progress (NV3 Gold).

## 4) Điều quan trọng cần biết khi bắt đầu sửa code

- **Codebase phụ thuộc Singleton khá nhiều**
  - Ưu: gọi nhanh.
  - Nhược: dễ phụ thuộc chéo, khó test unit.

- **Có 2 nhánh save khác nhau**
  - `GameManager -> SaveManager` (JSON file trong persistent data).
  - `LevelManager` lại lưu level index + position bằng `PlayerPrefs` key `SAVE_DATA`.
  - Cần thống nhất hoặc hiểu rõ để tránh lỗi “load lệch trạng thái”.

- **Có nhiều hệ event/UI runtime đan xen**
  - Ví dụ damage/heal gọi floating text, audio, camera shake, quest/currency update.
  - Khi sửa logic combat nên kiểm tra side-effect UI.

- **Multiplayer đang là module riêng**
  - Có script `Pun2/*` và `NGO_online/*`.
  - Nếu mục tiêu là single-player, có thể khoanh vùng để tránh đụng code network.

## 5) Lộ trình học tiếp theo (đề xuất cho người mới)

1. **Unity lifecycle + architecture cơ bản**
   - Nắm chắc `Awake/Start/Update/FixedUpdate`, Prefab, ScriptableObject, Scene management.

2. **Data-driven design trong game**
   - Hiểu `ItemData`, `ItemDatabase`, `QuestDatabase`, `LevelDatabase` để thêm content mà ít sửa logic.

3. **Combat pipeline**
   - Theo trace: Input -> PlayerController -> Animation Event -> ApplyDamage -> Enemy/Destructible phản hồi.

4. **Save/Load robust**
   - Học versioning migration, transactional save, rollback an toàn hơn.

5. **Refactor dần để dễ maintain**
   - Tách interface/service thay vì gọi singleton trực tiếp ở mọi nơi.
   - Thêm play mode tests cho quest/save/combat regression.

## 6) Nếu bạn mới vào dự án, hãy làm theo thứ tự này

1. Chạy scene `GameScene`, quan sát loop gameplay.
2. Đọc `CommonReferent -> GameManager -> PlayerController -> PlayerStats`.
3. Đọc `EnemyAI` để hiểu combat từ phía quái.
4. Đọc `QuestManager` và `SaveManager` để hiểu progression.
5. Sau đó mới mở các module nâng cao (Shop/Gacha/Multiplayer).

---
Nếu muốn, bước tiếp theo mình có thể viết thêm **“bản đồ dependency”** (script nào gọi script nào) để bạn onboard nhanh hơn trong 1-2 giờ đầu.
