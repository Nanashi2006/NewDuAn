using UnityEngine;

namespace PS46218
{
    // Cau 1: lay Perlin Noise lam do cao cho tung diem tren Terrain.
    [DefaultExecutionOrder(-100)]
    public sealed class TaoDiaHinh : MonoBehaviour
    {
        public const string SeedKey = "PS46218.TerrainSeed";
        public const int Resolution = 129;

        public int seed = 46218;
        [Min(8)] public int kichThuoc = 64;
        [Min(1f)] public float doCao = 12f;
        [Min(0.1f)] public float tiLeNoise = 3.5f;
        public Material vatLieu;

        public Terrain DiaHinh { get; private set; }

        private TerrainData data;
        private TerrainLayer lopCo;
        private Texture2D mauCo;

        private void Awake()
        {
            data = new TerrainData
            {
                name = "Dia hinh Perlin Noise",
                heightmapResolution = Resolution,
                size = new Vector3(kichThuoc, doCao, kichThuoc),
                alphamapResolution = 16
            };

            // Tao mau co bang code, khong can tai texture hay model.
            mauCo = new Texture2D(2, 2);
            mauCo.SetPixels(new[]
            {
                new Color(0.32f, 0.54f, 0.25f), new Color(0.36f, 0.58f, 0.28f),
                new Color(0.36f, 0.58f, 0.28f), new Color(0.32f, 0.54f, 0.25f)
            });
            mauCo.Apply();
            lopCo = new TerrainLayer { diffuseTexture = mauCo, tileSize = new Vector2(6f, 6f) };
            data.terrainLayers = new[] { lopCo };
            var trongSo = new float[16, 16, 1];
            for (int z = 0; z < 16; z++)
                for (int x = 0; x < 16; x++)
                    trongSo[z, x, 0] = 1f;
            data.SetAlphamaps(0, 0, trongSo);

            GameObject obj = Terrain.CreateTerrainGameObject(data);
            obj.name = "Terrain duoc tao bang thuat toan";
            obj.transform.SetParent(transform, false);
            DiaHinh = obj.GetComponent<Terrain>();
            DiaHinh.materialTemplate = vatLieu;
            DiaHinh.heightmapPixelError = 3f;

            // Dung lai seed da luu de mat dat khop voi toa do da luu.
            TaoVoiSeed(PlayerPrefs.GetInt(SeedKey, Random.Range(1, 100000)));
        }

        public void TaoVoiSeed(int seedMoi)
        {
            seed = seedMoi;
            var heights = new float[Resolution, Resolution];
            float offsetX = seed % 10000;
            float offsetZ = seed / 10000f + 123f;
            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float nx = x / (Resolution - 1f) * tiLeNoise + offsetX;
                    float nz = z / (Resolution - 1f) * tiLeNoise + offsetZ;
                    heights[z, x] = Mathf.Clamp01(Mathf.PerlinNoise(nx, nz));
                }
            }
            data.SetHeights(0, 0, heights);
        }

        public float LayDoCao(Vector3 viTri)
        {
            return DiaHinh.SampleHeight(viTri) + DiaHinh.transform.position.y;
        }

        private void OnDestroy()
        {
            // Giai phong cac tai nguyen da tao khi dung Play.
            if (DiaHinh != null)
            {
                DiaHinh.terrainData = null;
                TerrainCollider collider = DiaHinh.GetComponent<TerrainCollider>();
                if (collider != null) collider.terrainData = null;
            }
            if (data != null) Destroy(data);
            if (lopCo != null) Destroy(lopCo);
            if (mauCo != null) Destroy(mauCo);
        }
    }
}
