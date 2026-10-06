# Làng ven hồ — Chương 14

Scene: `Assets/Chapter14DynamicEnvironment/Scenes/Chapter14_Demo.unity`

Mở scene hoặc chọn **Chapter 14 → Open Lakeside Village**. Cảnh và các nhân vật đã được lưu vào scene: mở Unity là thấy ngay trong Scene View / Hierarchy, không cần Play để dựng map. Nếu góc Scene View chưa phù hợp, chọn **Chapter 14 → Frame Village in Scene View**.

## Cảnh quan

Phong cách low poly với một làng ven hồ: quán trọ, nhà ngư dân, xưởng, nhà lính gác, nhà kho; cửa, cửa sổ, chớp, khung gỗ, ngói, ống khói, biển hiệu; đường đá và giếng; cầu cong với ván, lan can và đèn; bến thuyền, thuyền câu, mái chèo, thùng gỗ, sọt hàng, ghế, hàng rào, quầy chợ, vườn rau, lều lính và bia tập. Rừng hỗn hợp, đá bờ hồ, lau sậy, cỏ, hoa và núi ở xa tạo chiều sâu.

Player là một hiệp sĩ có giáp, mũ, áo choàng, kiếm và khiên. Ba enemy là goblin có mô hình riêng. Tay/chân có chuyển động theo tốc độ; tay cầm kiếm vung khi chém. Đây là animation bằng script trên các khớp, không phải clip FBX/Animator. Các mesh/material được lưu trong `Art/` và có thể thay đổi trong Inspector.

## Hierarchy

- `Lakeside Village`
  - `01 Environment`: địa hình/lòng hồ, mặt nước/vùng bơi, nhà, đường đá, cầu/bến, cây/cỏ, đá/núi, đồ trang trí, vườn/trại lính.
  - `02 Characters`: player và ba enemy; mở `Visual` để xem bộ phận nhân vật.
  - `03 Time and Weather`: mặt trời, ngày/đêm, thời tiết và mây.
  - `04 Camera and Interface`: camera, Volume ánh sáng/màu và HUD.

Đối tượng có sẵn và có thể chọn, di chuyển, đổi material hoặc thêm chi tiết trong Edit Mode. Script `LakesideSceneRuntime` chỉ chuẩn bị NavMesh, bản sao sky material và static batching khi Play; không sinh lại cảnh. Những script dựng map cũ vẫn được giữ để tham khảo, nhưng scene mới không dùng bootstrap đó.

## Chức năng và điều khiển

| Điều khiển | Chức năng |
| --- | --- |
| WASD / Shift | Đi / chạy |
| Space | Nhảy, nhảy thấp hơn khi bơi |
| Chuột trái | Chém 20 HP/lần; giữ chuột không tự chém liên tục |
| Chuột phải + kéo | Xoay camera |
| Con lăn | Thu/phóng |
| 1 / 2 / 3 | Nắng / mưa / sương |
| H | Hiện/ẩn hướng dẫn |

- Nước có shader sóng, gợn, viền sáng và hiệu ứng góc nhìn; `WaterScroll` điều khiển dòng chảy.
- Trigger dưới mặt nước giảm tốc và trọng lực của player; trên cầu không bị xem là bơi.
- Enemy dùng NavMesh, nhìn thấy/đuổi player và quay về; vùng lòng hồ được đánh dấu Not Walkable, cầu nằm phía trên vùng loại trừ.
- Mỗi enemy có 100 HP, thanh máu, chết sau 5 cú chém và biến mất sau 2 giây. Mỗi cú chém chỉ gây damage một lần cho mỗi enemy.
- Ngày/đêm chạy nhanh hơn thời gian thật 360 lần, bắt đầu lúc 09:00. Đèn đường sáng vào ban đêm.
- Thời tiết ngẫu nhiên mỗi 45 giây, hoặc chọn bằng 1/2/3; mưa theo player và mây di chuyển.

## Terrain và NavMesh trong Editor

Cảnh mặc định dùng **mesh địa hình đã lưu**, có lòng hồ và đồi. Khi cần nộp/chỉnh bằng công cụ Terrain của Unity, chọn **Chapter 14 → Convert Landscape to Editable Terrain**, rồi lưu scene (Ctrl+S). Lệnh tạo TerrainData/TerrainLayer/texture/material trong `Terrain/`, dùng cùng độ cao và lòng hồ, tắt renderer/collider của mesh cũ, giữ nguyên các đối tượng khác. Chiều cao nền tương ứng 50m trên Terrain đặt Y=-50, đã đào hồ. Không chạy lệnh này trong Play Mode. Có thể Undo việc đổi đối tượng.

NavMesh được xây khi Play nếu chưa có dữ liệu đã bake. Để lưu nó vào project và xem component trong Edit Mode, chọn **Chapter 14 → Bake and Save Navigation**, rồi Ctrl+S. Sau khi đổi địa hình hoặc vật cản cần bake lại. NavMesh asset nằm trong `Navigation/`. Agent để disabled trong scene để tránh lỗi khi tải scene chưa có NavMesh; runtime bật agent sau khi mặt lưới đã sẵn sàng.

## Kiểm tra

Project dùng Unity `6000.6.0f1`, URP `17.6.0`, AI Navigation `2.0.14` và Input System `1.20.0` theo manifest có sẵn. Không thêm dependency.

Cấu trúc YAML, tham chiếu GUID/fileID, mesh buffers, quan hệ cha/con, nhân vật và vị trí cầu đã được kiểm tra bằng `python Tools/validate_lakeside_scene.py`. Không có Unity Editor trong môi trường viết code, nên chưa xác nhận import shader/C# hay Play Mode thực tế. Cần mở scene, kiểm tra Console, thử đi qua cầu, xuống hồ, đánh enemy và đổi thời tiết trước khi nộp bài.

`Tools/build_lakeside_scene.py` là công cụ tạo lại tài nguyên đã lưu. Chỉ dùng khi chủ động muốn dựng lại phiên bản mặc định: chạy nó sẽ ghi đè scene và material/mesh, vì vậy hãy commit/sao lưu thay đổi thủ công trước. Không cần Python để mở/chơi scene trong Unity.
