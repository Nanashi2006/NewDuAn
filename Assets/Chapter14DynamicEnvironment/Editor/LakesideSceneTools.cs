using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Chapter14DynamicEnvironment.Editor
{
    public static class LakesideSceneTools
    {
        private const string AssetRoot = "Assets/Chapter14DynamicEnvironment";

        [MenuItem("Chapter 14/Open Lakeside Village")]
        public static void OpenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(AssetRoot + "/Scenes/Chapter14_Demo.unity");
            FrameVillage();
        }

        [MenuItem("Chapter 14/Frame Village in Scene View")]
        public static void FrameVillage()
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) view.LookAt(new Vector3(0f, 0f, 4f), Quaternion.Euler(32f, 35f, 0f), 65f);
        }

        [MenuItem("Chapter 14/Bake and Save Navigation")]
        public static void BakeNavigation()
        {
            if (Application.isPlaying) return;
            LakesideSceneRuntime scene = Object.FindAnyObjectByType<LakesideSceneRuntime>();
            if (scene == null || scene.environment == null) return;
            scene.ConfigureLakeExclusion();
            NavMeshSurface surface = scene.environment.GetComponent<NavMeshSurface>();
            if (surface == null) surface = Undo.AddComponent<NavMeshSurface>(scene.environment);
            Undo.RecordObject(surface, "Bake village navigation");
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~(1 << 2);
            surface.BuildNavMesh();
            if (surface.navMeshData == null) return;
            EnsureFolder(AssetRoot + "/Navigation");
            const string path = AssetRoot + "/Navigation/LakesideNavMesh.asset";
            NavMeshData existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if (existing == null) AssetDatabase.CreateAsset(surface.navMeshData, path);
            else
            {
                EditorUtility.CopySerialized(surface.navMeshData, existing);
                surface.navMeshData = existing;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene.gameObject.scene);
            Debug.Log("Navigation baked. Save the scene to keep the NavMesh asset reference.", scene);
        }

        // Optional native Terrain replaces the authored landscape mesh with an editable heightmap.
        // Houses, lake, bridge, vegetation, and actor transforms remain independently editable.
        [MenuItem("Chapter 14/Convert Landscape to Editable Terrain")]
        public static void CreateEditableTerrain()
        {
            if (Application.isPlaying) return;
            LakesideSceneRuntime scene = Object.FindAnyObjectByType<LakesideSceneRuntime>();
            if (scene == null || scene.environment == null) return;
            Transform landscape = scene.environment.transform.Find("01 Landscape and Lake Basin");
            if (landscape == null || landscape.GetComponentInChildren<Terrain>() != null) return;
            const string folder = AssetRoot + "/Terrain";
            EnsureFolder(folder);
            TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(folder + "/LakesideTerrain.asset");
            if (data == null)
            {
                data = new TerrainData { heightmapResolution = 257, alphamapResolution = 128, size = new Vector3(120f, 60f, 120f) };
                float[,] heights = new float[257, 257];
                for (int z = 0; z < 257; z++)
                    for (int x = 0; x < 257; x++)
                        heights[z, x] = (Height(-60f + x * 120f / 256f, -60f + z * 120f / 256f) + 50f) / 60f;
                data.SetHeights(0, 0, heights);
                TerrainLayer grass = CreateLayer(folder, "Grass", new Color(0.32f, 0.44f, 0.22f));
                TerrainLayer bank = CreateLayer(folder, "Bank", new Color(0.58f, 0.48f, 0.31f));
                data.terrainLayers = new[] { grass, bank };
                float[,,] splats = new float[128, 128, 2];
                for (int z = 0; z < 128; z++)
                    for (int x = 0; x < 128; x++)
                    {
                        float wx = -60f + x * 120f / 127f, wz = -60f + z * 120f / 127f;
                        float r = new Vector2((wx - 12f) / 17f, (wz - 10f) / 13f).magnitude;
                        float weight = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.88f, 1.17f, r));
                        splats[z, x, 0] = 1f - weight;
                        splats[z, x, 1] = weight;
                    }
                data.SetAlphamaps(0, 0, splats);
                AssetDatabase.CreateAsset(data, folder + "/LakesideTerrain.asset");
            }
            Undo.RegisterFullObjectHierarchyUndo(landscape.gameObject, "Convert landscape to Terrain");
            MeshRenderer renderer = landscape.GetComponent<MeshRenderer>();
            MeshCollider collider = landscape.GetComponent<MeshCollider>();
            if (renderer != null) renderer.enabled = false;
            if (collider != null) collider.enabled = false;
            GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
            Undo.RegisterCreatedObjectUndo(terrainObject, "Create editable Terrain");
            terrainObject.name = "Editable Terrain - Raised 50m and Lake Basin";
            terrainObject.transform.SetParent(landscape);
            terrainObject.transform.localPosition = new Vector3(-60f, -50f, -60f);
            Terrain terrain = terrainObject.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 5f;
            Shader shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
            if (shader != null)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(folder + "/TerrainMaterial.mat");
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, folder + "/TerrainMaterial.mat");
                }
                terrain.materialTemplate = material;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(landscape.gameObject.scene);
            Selection.activeGameObject = terrainObject;
            Debug.Log("Editable Terrain created. Save the scene, then bake navigation if using a saved NavMesh.", terrainObject);
        }

        private static float Height(float x, float z)
        {
            float radius = new Vector2((x - 12f) / 17f, (z - 10f) / 13f).magnitude;
            float lake = radius < 1f ? -3.2f * (1f - radius * radius) : 0f;
            float edge = Mathf.Max(0f, (Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) - 39f) / 21f);
            return lake + edge * edge * (7f + 3f * Mathf.Sin(x * 0.13f) * Mathf.Cos(z * 0.1f));
        }

        private static TerrainLayer CreateLayer(string folder, string name, Color color)
        {
            TerrainLayer layer = new TerrainLayer { name = name, tileSize = new Vector2(5f, 5f), smoothness = 0.05f };
            Texture2D texture = new Texture2D(32, 32) { name = name + "Texture", wrapMode = TextureWrapMode.Repeat };
            Color[] pixels = new Color[1024];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    pixels[y * 32 + x] = color * Mathf.Lerp(0.9f, 1.1f, Mathf.PerlinNoise(x * 0.2f, y * 0.2f));
            texture.SetPixels(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, folder + "/" + name + "Texture.asset");
            layer.diffuseTexture = texture;
            AssetDatabase.CreateAsset(layer, folder + "/" + name + ".terrainlayer");
            return layer;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
