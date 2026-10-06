# Chapter 14 - Sông hồ, Thời tiết, Thời gian (Unity 6 URP)

Demo này được làm cho project `NewDuAn` và bám theo nội dung Chương 14 của tài liệu bài học.

## Cách chạy

1. Merge branch/PR vào project.
2. Mở Unity bằng **Unity 6000.6.0f1**.
3. Mở scene:
   `Assets/Chapter14DynamicEnvironment/Scenes/Chapter14_Demo.unity`
4. Bấm **Play**.
5. Map sẽ được dựng tự động khi vào Play Mode để không cần tải asset/texture bên ngoài.

## Điều khiển

- `WASD`: di chuyển.
- `Shift`: chạy.
- `Space`: nhảy.
- `Chuột phải + kéo`: xoay camera góc nhìn thứ ba.
- `Lăn chuột`: zoom camera.
- `Chuột trái`: tấn công, mỗi lần gây **20 damage**.
- `1`: Sunny.
- `2`: Raining.
- `3`: Foggy.

## Các phần đã làm

### 1. Địa hình trũng + hồ
- Terrain được tạo ở runtime.
- Nền địa hình được nâng theo ý tưởng **Height = 50** rồi bù `Terrain Y = -43`.
- Lòng hồ được đào thấp xuống bằng heightmap.
- Có cầu gỗ đi ngang hồ.

### 2. Nước cuộn URP
- `WaterSurface` dùng material URP tạo runtime.
- Texture nước procedural được tạo bằng code, không cần asset ngoài.
- `WaterScroll.cs` cuộn `_BaseMap` / `_MainTex` liên tục.

### 3. Swimming Trigger
- `WaterVolume.cs` dùng Trigger.
- Khi Player xuống nước: tốc độ và trọng lực giảm.
- Khi lên bờ: tốc độ trở lại bình thường.

### 4. Day / Night
- `TimeController.cs` chạy thời gian trong game.
- Directional Light quay theo giờ.
- Cường độ ánh sáng thay đổi theo AnimationCurve.
- Procedural Skybox đổi màu theo Gradient.
- Đồng hồ `HH:mm` hiển thị trên màn hình.

### 5. Weather Manager
- `Sunny`: sương mù rất nhẹ.
- `Raining`: tạo Particle System mưa, hạt kéo dài và rơi theo Player.
- `Foggy`: tăng fog density.
- Tự đổi thời tiết ngẫu nhiên sau mỗi khoảng thời gian.
- Có phím 1/2/3 để test ngay.

### Bài làm thêm
- Mây 3D tự trôi (`CloudDrift.cs`).
- Mưa được cấu hình dạng hạt kéo dài để nhìn giống mưa hơn tuyết.

## Player + Enemy

- Player dùng `CharacterController` và **New Input System**.
- Camera góc nhìn thứ ba.
- Enemy dùng `NavMeshAgent`, có tầm nhìn + Raycast + `eyeOffset`.
- Enemy đuổi Player trong phạm vi, mất mục tiêu thì quay về vị trí ban đầu.
- Hồ dùng `NavMeshObstacle` carving, Enemy không đi xuyên lòng hồ và có thể dùng cầu.
- Enemy có 100 HP.
- Player đánh 20 HP/lần.
- Mỗi lần click chỉ trừ máu một lần cho mỗi Enemy.
- Enemy chết thì dừng AI và biến mất sau 2 giây.
- Thanh HP của Enemy hiển thị phía trên đầu bằng GUI.

## Script chính

- `Chapter14DemoBootstrap.cs`: dựng map demo.
- `PlayerController.cs`: đi/chạy/nhảy/bơi.
- `PlayerCombat.cs`: tấn công.
- `ThirdPersonCamera.cs`: camera follow/orbit.
- `EnemyController.cs`: AI NavMesh + CanSeePlayer.
- `EnemyHealth.cs`: HP/death.
- `WaterScroll.cs`: cuộn texture nước.
- `WaterVolume.cs`: vùng trigger nước.
- `TimeController.cs`: ngày đêm.
- `WeatherManager.cs`: Sunny/Raining/Foggy.
- `CloudDrift.cs`: mây trôi.
- `DemoHUD.cs`: hướng dẫn điều khiển trong game.

## Ghi chú

Map dùng primitive và texture sinh bằng code để branch nhẹ, dễ merge, không kéo theo asset có bản quyền và không phụ thuộc thư viện nặng. Package `com.unity.ai.navigation` và `com.unity.inputsystem` đã có sẵn trong project.
