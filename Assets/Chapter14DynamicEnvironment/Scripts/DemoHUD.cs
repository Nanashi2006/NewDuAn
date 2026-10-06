using UnityEngine;
using UnityEngine.InputSystem;

namespace Chapter14DynamicEnvironment
{
    public class DemoHUD : MonoBehaviour
    {
        private PlayerController player;
        private TimeController clock;
        private WeatherManager weather;
        private bool showHelp;
        private GUIStyle title, small, value;
        private void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            clock = FindAnyObjectByType<TimeController>();
            weather = FindAnyObjectByType<WeatherManager>();
        }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) showHelp = !showHelp;
        }
        private static void Panel(Rect r)
        {
            Color old = GUI.color;
            GUI.color = new Color(0.055f, 0.095f, 0.11f, 0.88f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = new Color(0.85f, 0.69f, 0.39f);
            GUI.DrawTexture(new Rect(r.x, r.y, 3, r.height), Texture2D.whiteTexture);
            GUI.color = old;
        }
        private void OnGUI()
        {
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                title.normal.textColor = new Color(0.95f,0.88f,0.7f);
                small = new GUIStyle(GUI.skin.label) { fontSize = 13 };
                small.normal.textColor = new Color(0.75f,0.83f,0.82f);
                value = new GUIStyle(title) { fontSize = 17, alignment = TextAnchor.MiddleRight };
            }
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * Mathf.Min(Screen.width / 1280f, Screen.height / 720f));
            float w = Screen.width / GUI.matrix.m00, h = Screen.height / GUI.matrix.m11;
            Panel(new Rect(24,24,292,77));
            GUI.Label(new Rect(40,34,265,30), "LÀNG VEN HỒ", title);
            GUI.Label(new Rect(40,65,265,25), "Sông hồ · Thời tiết · Thời gian", small);
            Panel(new Rect(w-235,24,211,77));
            GUI.Label(new Rect(w-220,33,178,28), clock == null ? "09:00" : clock.CurrentTime.ToString("HH:mm"), value);
            string label = weather == null ? "Nắng" : weather.currentWeather == WeatherState.Sunny ? "Trời nắng" : weather.currentWeather == WeatherState.Raining ? "Mưa" : "Sương mù";
            GUI.Label(new Rect(w-220,66,178,24), label, small);
            Panel(new Rect(24,h-66,395,42));
            GUI.Label(new Rect(39,h-57,370,24), player != null && player.IsSwimming ? "Đang bơi · Di chuyển chậm trong nước" : "Khám phá làng · H: hướng dẫn điều khiển", small);
            if (showHelp)
            {
                Panel(new Rect(24,115,400,180));
                GUI.Label(new Rect(40,127,365,160), "WASD  Di chuyển       Shift  Chạy\nSpace  Nhảy / bơi       Chuột trái  Chém\nChuột phải + kéo  Xoay camera\nF5  Góc nhìn 1 / 3 · Con lăn  Thu phóng\n1  Nắng · 2 / R  Mưa · 3  Sương mù\nMỗi cú chém gây 20 HP · Enemy 100 HP", small);
            }
            GUI.matrix = old;
        }
    }
}
