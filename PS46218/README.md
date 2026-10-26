# PS46218 Bài làm Game 3D nâng cao

Project Unity mới, độc lập với project NewDuAn ở thư mục cha. Dùng Terrain và Cube cơ bản, không dùng lại model hay script của các bài lab.

## Cách mở

1. Trong GitHub Desktop, chuyển sang nhánh `feat/ps46218-simple-3d-exam` và Fetch/Pull bản mới.
2. Mở **Unity Hub → Add project from disk → chọn thư mục `NewDuAn/PS46218`**. Chọn đúng thư mục `PS46218`, không chọn thư mục cha `NewDuAn`.
3. Mở bằng **Unity 6000.6.0f1**, chờ Package Manager cài URP và Input System.
4. Lần đầu mở, project tự cấu hình URP và mở scene `Assets/Scenes/PS46218.unity`. Nếu chưa thấy scene, chọn menu **PS46218 → Mo bai lam**.
5. Nhấn **Play**, bấm vào cửa sổ Game rồi dùng WASD hoặc phím mũi tên để di chuyển Cube màu cam.

## Ba chức năng theo đề

| Câu | Chức năng | File chính | Cách thử |
| --- | --- | --- | --- |
| 1 | Địa hình ngẫu nhiên bằng Perlin Noise | `Assets/Scripts/TaoDiaHinh.cs` | Nhấn **R** hoặc nút **Tao dia hinh moi**; các đồi và seed thay đổi. |
| 2 | Một hiệu ứng Post-processing: Vignette | `Assets/Scripts/HauKy.cs` | Nhấn **P**; viền tối của màn hình bật/tắt để so sánh. |
| 3 | Lưu và tải tọa độ Cube bằng PlayerPrefs | `Assets/Scripts/LuuViTri.cs` | Di chuyển, dừng Play rồi Play lại; Cube trở về vị trí cuối cùng. |

`GiaoDienDemo.cs` chỉ hiển thị hướng dẫn, tọa độ và các nút thử bài. `Assets/Editor/CaiDatProject.cs` chỉ cấu hình URP cho project mới; không cần sửa file này để hiểu ba câu chính.

## Phím điều khiển

| Phím | Tác dụng |
| --- | --- |
| WASD hoặc các phím mũi tên | Di chuyển Cube trên địa hình. |
| R | Tạo một địa hình khác bằng seed mới. |
| P | Bật hoặc tắt Vignette. |
| F5 | Lưu ngay tọa độ và seed của địa hình. |
| F9 | Tải lại tọa độ và đúng địa hình đã lưu. |

Cube tự lưu khi thoát game bình thường hoặc dừng Play trong Unity. F5 giúp lưu ngay khi đang thử; F9 chỉ đọc bản đã lưu. Khi bấm R rồi F9, địa hình cũng trở lại đúng seed đã lưu để tọa độ Y vẫn khớp mặt đất. Không có bản lưu thì Cube xuất hiện giữa địa hình.

## Cách kiểm tra để trình bày

1. **Câu 1:** nhấn R hai lần, quan sát đồi khác nhau. Giải thích: “Em duyệt các điểm của heightmap, lấy `Mathf.PerlinNoise` làm độ cao rồi truyền vào `TerrainData.SetHeights`.”
2. **Câu 2:** nhấn P hai lần. Giải thích: “Em dùng Global Volume có Vignette và bật Post-processing trên Camera. Hiệu ứng làm tối viền màn hình.”
3. **Câu 3:** đi đến một vị trí, ghi lại XYZ trên màn hình, nhấn Stop, nhấn Play lại và đối chiếu. Giải thích: “Khi thoát em lưu X, Y, Z bằng `PlayerPrefs.SetFloat` và `Save`. Lúc chạy lại em đọc bằng `GetFloat` rồi gán vào `transform.position`.”
4. Có thể thử thêm F5 → đi chỗ khác → F9 để thấy Cube trở lại ngay. Bản build và Unity Editor có kho PlayerPrefs riêng; kiểm tra lưu/tải trong cùng một môi trường.

Trong đề, chữ “PlayerPrefabs” được hiểu là API **PlayerPrefs** của Unity để lưu dữ liệu; Prefab là một khái niệm khác.

## Build để chạy riêng

Vào **File → Build Profiles**, chọn nền tảng đang có module hỗ trợ, thêm scene `Assets/Scenes/PS46218.unity` nếu cần rồi Build. Script cấu hình đã thêm scene vào danh sách build. Thoát bằng nút đóng cửa sổ để thử lưu vị trí, sau đó mở lại đúng bản build đó.

## Tài liệu API

- [Unity Mathf.PerlinNoise](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mathf.PerlinNoise.html)
- [Unity PlayerPrefs](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.html)
- [Unity URP](https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/universal-render-pipeline)
