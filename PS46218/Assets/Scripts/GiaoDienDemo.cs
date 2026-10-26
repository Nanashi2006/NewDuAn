using UnityEngine;

namespace PS46218
{
    // Chi la giao dien thu bai, ba chuc nang chinh nam trong ba script rieng.
    public sealed class GiaoDienDemo : MonoBehaviour
    {
        public LuuViTri cube;
        public TaoDiaHinh diaHinh;
        public HauKy hauKy;
        private GUIStyle tieuDe;
        private GUIStyle noiDung;
        private GUIStyle nut;

        private void OnGUI()
        {
            if (tieuDe == null)
            {
                tieuDe = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold };
                noiDung = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
                nut = new GUIStyle(GUI.skin.button) { fontSize = 14, fixedHeight = 34f };
            }
            Matrix4x4 oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 640f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);

            GUILayout.BeginArea(new Rect(16f, 16f, 350f, 384f), GUI.skin.box);
            GUILayout.Label("PS46218 | GAME 3D", tieuDe);
            GUILayout.Label("WASD / phim mui ten: di chuyen Cube", noiDung);
            GUILayout.Space(8f);
            GUILayout.Label($"Cau 1: Perlin Noise | Seed {diaHinh.seed}", noiDung);
            if (GUILayout.Button("Tao dia hinh moi (R)", nut)) cube.TaoDiaHinhMoi();
            GUILayout.Space(6f);
            GUILayout.Label("Cau 2: Hau ky Vignette", noiDung);
            if (GUILayout.Button(hauKy.DangBat ? "Tat hieu ung (P)" : "Bat hieu ung (P)", nut)) hauKy.BatTat();
            GUILayout.Space(6f);
            Vector3 p = cube.transform.position;
            GUILayout.Label($"Cau 3: X {p.x:F2} | Y {p.y:F2} | Z {p.z:F2}", noiDung);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Luu (F5)", nut)) cube.LuuToaDo();
            if (GUILayout.Button("Tai (F9)", nut)) cube.TaiToaDo();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
            GUILayout.Label(cube.ThongBao, noiDung);
            GUILayout.Label("Tu dong luu khi thoat game / dung Play.", noiDung);
            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
