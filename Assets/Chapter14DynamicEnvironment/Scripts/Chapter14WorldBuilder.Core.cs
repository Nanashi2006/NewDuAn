using UnityEngine;
using UnityEngine.AI;

namespace Chapter14DynamicEnvironment
{
    internal static partial class Chapter14WorldBuilder
    {
        private const float TerrainSize = 120f;
        private const float TerrainHeight = 60f;
        private const float TerrainYOffset = -43f;
        private static readonly Vector3 LakeCenter = new Vector3(18f, 0f, 8f);
        private const float WaterY = 4.2f;
        private static int randomSeed;

        public static void Build(int seed)
        {
            randomSeed = seed;
            Random.InitState(seed);

            GameObject worldRoot = new GameObject("Chapter14_World");
            GameObject environmentRoot = new GameObject("Environment_Static");
            environmentRoot.transform.SetParent(worldRoot.transform);

            Terrain terrain = CreateTerrain(environmentRoot.transform);
            CreateWater(environmentRoot.transform);
            CreateBridge(environmentRoot.transform);
            CreateEnvironmentProps(environmentRoot.transform, terrain);

            GameObject player = CreatePlayer(worldRoot.transform, terrain);
            GameObject enemy = CreateEnemy(worldRoot.transform, terrain, player.transform);
            CreateCamera(worldRoot.transform, player.transform);

            Light sun = CreateSun(worldRoot.transform);
            Material skybox = CreateProceduralSkybox();
            CreateTimeController(worldRoot.transform, sun, skybox);
            CreateWeatherManager(worldRoot.transform);
            CreateClouds(worldRoot.transform);
            worldRoot.AddComponent<DemoHUD>();

            BuildRuntimeNavMesh(environmentRoot, enemy);

            Debug.Log(
                "<color=cyan>[Chapter 14]</color> Demo map đã được tạo: Terrain + hồ + nước cuộn + bơi + ngày/đêm + mưa/sương + mây + Player + Enemy."
            );
        }

        private static Terrain CreateTerrain(Transform parent)
        {
            TerrainData terrainData = new TerrainData
            {
                heightmapResolution = 129,
                size = new Vector3(TerrainSize, TerrainHeight, TerrainSize),
                baseMapResolution = 512
            };

            int resolution = terrainData.heightmapResolution;
            float[,] heights = new float[resolution, resolution];

            const float raisedGroundMeters = 50f;
            const float lakeBottomMeters = 44f;

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = x / (float)(resolution - 1);
                    float nz = z / (float)(resolution - 1);
                    float worldX = (nx - 0.5f) * TerrainSize;
                    float worldZ = (nz - 0.5f) * TerrainSize;

                    float noise = Mathf.PerlinNoise(
                        nx * 3.2f + randomSeed * 0.11f,
                        nz * 3.2f + randomSeed * 0.07f
                    );

                    float groundMeters = raisedGroundMeters + (noise - 0.5f) * 2.2f;

                    float dx = (worldX - LakeCenter.x) / 18f;
                    float dz = (worldZ - LakeCenter.z) / 14f;
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);

                    float lakeMask = Mathf.Clamp01(1f - distance);
                    lakeMask = lakeMask * lakeMask * (3f - 2f * lakeMask);

                    float finalMeters = Mathf.Lerp(groundMeters, lakeBottomMeters, lakeMask);
                    heights[z, x] = Mathf.Clamp01(finalMeters / TerrainHeight);
                }
            }

            terrainData.SetHeights(0, 0, heights);

            TerrainLayer grassLayer = new TerrainLayer();
            grassLayer.name = "Runtime_Grass";
            grassLayer.diffuseTexture = CreateColorTexture(
                new Color(0.17f, 0.42f, 0.12f, 1f),
                new Color(0.24f, 0.50f, 0.16f, 1f)
            );
            grassLayer.tileSize = new Vector2(10f, 10f);
            grassLayer.smoothness = 0.05f;
            terrainData.terrainLayers = new[] { grassLayer };

            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = "Terrain_Raised50_DugLake";
            terrainObject.transform.SetParent(parent);
            terrainObject.transform.position = new Vector3(
                -TerrainSize * 0.5f,
                TerrainYOffset,
                -TerrainSize * 0.5f
            );

            Terrain terrain = terrainObject.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 5f;
            terrain.basemapDistance = 120f;
            return terrain;
        }

        private static void CreateWater(Transform parent)
        {
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "WaterSurface";
            water.layer = 2;
            water.transform.SetParent(parent);
            water.transform.position = new Vector3(LakeCenter.x, WaterY, LakeCenter.z);
            water.transform.localScale = new Vector3(3f, 1f, 2.15f);

            Collider waterSurfaceCollider = water.GetComponent<Collider>();
            if (waterSurfaceCollider != null)
                waterSurfaceCollider.enabled = false;

            Renderer waterRenderer = water.GetComponent<Renderer>();
            Material waterMaterial = CreateTransparentMaterial(
                "M_Water_Runtime",
                new Color(0.08f, 0.48f, 0.72f, 0.62f)
            );

            Texture2D waterTexture = CreateWaterTexture(64);
            if (waterMaterial.HasProperty("_BaseMap"))
            {
                waterMaterial.SetTexture("_BaseMap", waterTexture);
                waterMaterial.SetTextureScale("_BaseMap", new Vector2(5f, 5f));
            }
            else if (waterMaterial.HasProperty("_MainTex"))
            {
                waterMaterial.SetTexture("_MainTex", waterTexture);
                waterMaterial.SetTextureScale("_MainTex", new Vector2(5f, 5f));
            }

            waterRenderer.material = waterMaterial;
            water.AddComponent<WaterScroll>();

            GameObject triggerObject = new GameObject("WaterVolume_Trigger");
            triggerObject.layer = 2;
            triggerObject.transform.SetParent(parent);
            triggerObject.transform.position = new Vector3(LakeCenter.x, 3.05f, LakeCenter.z);

            BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(30f, 2.3f, 21.5f);
            triggerObject.AddComponent<WaterVolume>();

            NavMeshObstacle waterObstacle = triggerObject.AddComponent<NavMeshObstacle>();
            waterObstacle.shape = NavMeshObstacleShape.Box;
            waterObstacle.size = new Vector3(30f, 1.8f, 21.5f);
            waterObstacle.center = new Vector3(0f, -0.15f, 0f);
            waterObstacle.carving = true;
        }

        private static void CreateBridge(Transform parent)
        {
            Material wood = CreateMaterial(
                "M_Bridge_Runtime",
                new Color(0.37f, 0.20f, 0.09f, 1f)
            );

            CreateBox(
                "Bridge_Deck",
                parent,
                new Vector3(LakeCenter.x, 6.65f, LakeCenter.z),
                new Vector3(4.2f, 0.45f, 27f),
                wood
            );

            for (int i = -5; i <= 5; i++)
            {
                float z = LakeCenter.z + i * 2.4f;

                CreateBox(
                    "Bridge_Rail_L",
                    parent,
                    new Vector3(LakeCenter.x - 2.05f, 7.35f, z),
                    new Vector3(0.18f, 1.6f, 0.18f),
                    wood
                );

                CreateBox(
                    "Bridge_Rail_R",
                    parent,
                    new Vector3(LakeCenter.x + 2.05f, 7.35f, z),
                    new Vector3(0.18f, 1.6f, 0.18f),
                    wood
                );
            }

            CreateBox(
                "Bridge_RailTop_L",
                parent,
                new Vector3(LakeCenter.x - 2.05f, 8.05f, LakeCenter.z),
                new Vector3(0.18f, 0.18f, 27f),
                wood
            );

            CreateBox(
                "Bridge_RailTop_R",
                parent,
                new Vector3(LakeCenter.x + 2.05f, 8.05f, LakeCenter.z),
                new Vector3(0.18f, 0.18f, 27f),
                wood
            );
        }
    }
}
