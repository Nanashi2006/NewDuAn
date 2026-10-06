using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    public class DemoHUD : MonoBehaviour
    {
        private PlayerController player;

        private void Start()
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        private void OnGUI()
        {
            if (player == null)
                player = FindFirstObjectByType<PlayerController>();

            string movementState = player != null && player.IsSwimming
                ? "TRẠNG THÁI: ĐANG Ở DƯỚI NƯỚC - GIẢM TỐC"
                : "TRẠNG THÁI: TRÊN CẠN";

            GUI.Box(new Rect(12f, 12f, 315f, 155f), "CHAPTER 14 - DYNAMIC ENVIRONMENT");
            GUI.Label(new Rect(24f, 42f, 290f, 20f), "WASD: di chuyển | Shift: chạy | Space: nhảy");
            GUI.Label(new Rect(24f, 64f, 290f, 20f), "Chuột phải + kéo: xoay camera | Wheel: zoom");
            GUI.Label(new Rect(24f, 86f, 290f, 20f), "Chuột trái: đánh Enemy (20 HP / lần)");
            GUI.Label(new Rect(24f, 108f, 290f, 20f), "1: Sunny | 2: Raining | 3: Foggy");
            GUI.Label(new Rect(24f, 132f, 290f, 20f), movementState);
        }
    }
}
