using UnityEngine;
using UnityEngine.InputSystem;

namespace PS46218
{
    // Cau 3: Cube di chuyen va luu/tai dung ba toa do x, y, z.
    public sealed class LuuViTri : MonoBehaviour
    {
        private const string XKey = "PS46218.Position.X";
        private const string YKey = "PS46218.Position.Y";
        private const string ZKey = "PS46218.Position.Z";
        private const string SaveKey = "PS46218.HasSavedPosition";

        public TaoDiaHinh diaHinh;
        public HauKy hauKy;
        public Camera cameraChinh;
        public float tocDo = 7f;
        public Vector3 cameraOffset = new Vector3(0f, 10f, -13f);

        public string ThongBao { get; private set; } = "Di chuyen Cube de thu luu vi tri.";
        private bool daKhoiTao;

        private void Start()
        {
            if (!TaiToaDo())
            {
                transform.position = new Vector3(diaHinh.kichThuoc / 2f, 0f, diaHinh.kichThuoc / 2f);
                DatTrenMatDat();
            }
            cameraChinh.transform.position = transform.position + cameraOffset;
            cameraChinh.transform.LookAt(transform.position);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            daKhoiTao = true;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            Vector3 huong = Vector3.zero;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) huong.z += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) huong.z -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) huong.x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) huong.x -= 1f;
            if (huong.sqrMagnitude > 0f)
            {
                Vector3 p = transform.position + huong.normalized * tocDo * Time.deltaTime;
                p.x = Mathf.Clamp(p.x, 1f, diaHinh.kichThuoc - 1f);
                p.z = Mathf.Clamp(p.z, 1f, diaHinh.kichThuoc - 1f);
                transform.position = p;
                DatTrenMatDat();
            }

            if (kb.rKey.wasPressedThisFrame) TaoDiaHinhMoi();
            if (kb.pKey.wasPressedThisFrame) hauKy.BatTat();
            if (kb.f5Key.wasPressedThisFrame) LuuToaDo();
            if (kb.f9Key.wasPressedThisFrame) TaiToaDo();
        }

        private void LateUpdate()
        {
            Vector3 viTriCamera = transform.position + cameraOffset;
            cameraChinh.transform.position = Vector3.Lerp(
                cameraChinh.transform.position, viTriCamera, 8f * Time.deltaTime);
            cameraChinh.transform.LookAt(transform.position);
        }

        private void DatTrenMatDat()
        {
            Vector3 p = transform.position;
            p.y = diaHinh.LayDoCao(p) + transform.localScale.y / 2f;
            transform.position = p;
        }

        public void TaoDiaHinhMoi()
        {
            int seedMoi;
            do { seedMoi = Random.Range(1, 100000); } while (seedMoi == diaHinh.seed);
            diaHinh.TaoVoiSeed(seedMoi);
            DatTrenMatDat();
            ThongBao = "Da tao dia hinh moi. Bam Luu neu muon giu ban nay.";
        }

        public void LuuToaDo()
        {
            if (!daKhoiTao) return;
            Vector3 p = transform.position;
            PlayerPrefs.SetFloat(XKey, p.x);
            PlayerPrefs.SetFloat(YKey, p.y);
            PlayerPrefs.SetFloat(ZKey, p.z);
            PlayerPrefs.SetInt(TaoDiaHinh.SeedKey, diaHinh.seed);
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
            ThongBao = "Da luu XYZ. Dung Play roi chay lai de kiem tra.";
        }

        public bool TaiToaDo()
        {
            if (PlayerPrefs.GetInt(SaveKey, 0) != 1 ||
                !PlayerPrefs.HasKey(XKey) || !PlayerPrefs.HasKey(YKey) || !PlayerPrefs.HasKey(ZKey))
            {
                ThongBao = "Chua co vi tri da luu.";
                return false;
            }
            Vector3 p = new Vector3(PlayerPrefs.GetFloat(XKey),
                PlayerPrefs.GetFloat(YKey), PlayerPrefs.GetFloat(ZKey));
            if (!HopLe(p.x) || !HopLe(p.y) || !HopLe(p.z))
            {
                ThongBao = "Vi tri da luu khong hop le. Dung vi tri ban dau.";
                return false;
            }

            int savedSeed = PlayerPrefs.GetInt(TaoDiaHinh.SeedKey, diaHinh.seed);
            if (savedSeed != diaHinh.seed) diaHinh.TaoVoiSeed(savedSeed);
            transform.position = p;
            ThongBao = "Da tai lai dung toa do XYZ va dia hinh da luu.";
            return true;
        }

        private static bool HopLe(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void OnApplicationQuit() => LuuToaDo();

        private void OnApplicationPause(bool pause)
        {
            if (pause) LuuToaDo();
        }
    }
}
