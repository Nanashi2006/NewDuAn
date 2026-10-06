# Bản làng ven hồ dùng asset – giữ nguyên bố cục

Mở thẳng **Assets/Scenes/LakesideVillage_Rebuilt.unity** rồi Play. Scene này đã được lưu đầy đủ; không cần dựng lại nhà/cầu bằng builder.

Menu cũ **Tools → Chapter 14 → Build Lakeside Village Game** giờ mở scene đã lưu. Nó hỏi lưu thay đổi đang mở trước khi chuyển scene, không ghi đè làng. **Validate Lakeside Village** kiểm tra material và controller trong Unity.

## Những phần giữ lại

Vị trí nhà, hồ/lòng hồ, cầu cong, bến, đường đá, giếng, hàng rào, ghế, đèn, cây, đá và vườn đều lấy từ scene Chapter14_Demo trong ảnh. Hierarchy vẫn chia Environment, Characters, Time and Weather, Camera and Interface; thêm nhóm Assets và cỏ/hoa thật. Scene gốc vẫn có trong project.

## Asset đã gắn

| Asset | Cách dùng |
| --- | --- |
| DogKnight/DogPBR | Model, bộ xương, kiếm/khiên và clip Idle/Walk/Run/Attack |
| deb_Goblin01 | Ba Goblin với clip Idle/Move/Die, HP 100 |
| ALP GrassFlowersFREE | Texture nền cỏ và 752 tấm cỏ/hoa, tránh đường và hồ |
| Handpainted Ground | Texture bờ đất và vùng bùn |
| AQUAS-Lite | Normal sóng và texture foam trên mặt hồ cũ với shader URP |
| Free HDR Skyboxes | sky-2, kết hợp hệ thống ngày/đêm |
| Rainy VFX | Bản sao prefab particle được gắn material URP và cấu hình mưa theo player |

Material DogKnight/Goblin/Stage đã chuyển sang shader có texture/normal/shadow/fog dùng URP. AQUAS giữ tài nguyên sóng/foam, thay shader Built-in và chặn reflection Camera.Render khi chạy SRP. Không chạy GrabPass hoặc nested reflection camera trong URP.

Controller gameplay sao chép state/clip từ asset, bỏ các transition demo tự chạy. AnimationDriver gọi state trực tiếp, không yêu cầu Speed/Run/Jump/Die trong controller không có tham số. Clip Attack/Die được bỏ Loop Time. Pack DogKnight không có clip nhảy; Space vẫn nhảy bằng CharacterController.

## Điều khiển

WASD đi, Shift chạy, Space nhảy/bơi, chuột trái chém 20 HP mỗi lần bấm. Giữ chuột không chém liên tục; mỗi đòn chỉ trừ một lần cho mỗi enemy. Enemy chết sau 5 đòn, chạy clip Die và biến mất sau 2 giây.

Chuột phải kéo xoay camera, con lăn zoom, **F5** đổi góc nhìn; góc nhìn thứ nhất ẩn renderer của player và khóa chuột, **Esc** mở khóa.

**1** nắng, **2 / R** mưa, **3** sương, **H** hướng dẫn. Ngày/đêm, đèn đêm, bơi, mây, minimap và vùng bùn vẫn hoạt động. Enemy đuổi trong 5 m rồi quay về; vùng bùn giảm tốc còn 1.5 m/s. Agent không tự đi qua NavMesh Link.

Scene mới đứng đầu và là scene được bật trong Build Settings.

## Kiểm tra

- `python3 Tools/validate_asset_rebuild.py`: kiểm tra GUID, tham chiếu cục bộ, cha/con, animator/bone/controller, clip không loop và 2.201 transform cảnh quan giữ nguyên.
- `python3 Tools/validate_lakeside_scene.py`: kiểm tra scene gốc và mesh.
- `python3 Tools/rebuild_lakeside_assets.py`: chỉ dùng khi chủ động muốn tạo lại bản sao từ scene gốc. Lệnh này ghi lại scene mới và các material đã sửa; hãy commit thay đổi thủ công trước khi dùng.

Chưa chạy import shader/C# hoặc Play Mode bằng Unity Editor trong môi trường sửa code. Cần mở scene mới, kiểm tra Console và thử đi/chạy, F5, mưa, xuống hồ, qua cầu, đi vào bùn và đánh Goblin trước khi nộp.
