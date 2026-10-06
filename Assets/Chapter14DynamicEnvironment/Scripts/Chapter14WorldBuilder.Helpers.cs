using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Chapter14DynamicEnvironment
{
    internal static partial class Chapter14WorldBuilder
    {
        private static void BuildRuntimeNavMesh(GameObject environmentRoot, GameObject enemy)
        {
            NavMeshSurface surface = environmentRoot.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~(1 << 2);
            surface.BuildNavMesh();

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent == null)
                return;

            if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        private static float TerrainWorldHeight(Terrain terrain, float x, float z)
        {
            Vector3 sample = new Vector3(x, 0f, z);
            return terrain.SampleHeight(sample) + terrain.transform.position.y;
        }

        private static void CreateTree(
            Transform parent,
            Vector3 position,
            Material trunkMaterial,
            Material leavesMaterial)
        {
            GameObject treeRoot = new GameObject("Tree");
            treeRoot.transform.SetParent(parent);
            treeRoot.transform.position = position;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(treeRoot.transform);
            trunk.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            trunk.transform.localScale = new Vector3(0.6f, 1.6f, 0.6f);
            trunk.GetComponent<Renderer>().material = trunkMaterial;

            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(treeRoot.transform);
            leaves.transform.localPosition = new Vector3(0f, 4.1f, 0f);
            leaves.transform.localScale = new Vector3(3.3f, 3.5f, 3.3f);

            Collider leavesCollider = leaves.GetComponent<Collider>();
            if (leavesCollider != null)
                leavesCollider.enabled = false;

            leaves.GetComponent<Renderer>().material = leavesMaterial;
        }

        private static GameObject CreateBox(
            string objectName,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().material = material;
            return box;
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader)
            {
                name = materialName
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            return material;
        }

        private static Material CreateTransparentMaterial(string materialName, Color color)
        {
            Material material = CreateMaterial(materialName, color);

            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);

            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 0f);

            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);

            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static Texture2D CreateColorTexture(Color colorA, Color colorB)
        {
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "Runtime_GrassTexture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[16];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    bool alternate = (x + y) % 2 == 0;
                    pixels[y * 4 + x] = alternate ? colorA : colorB;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateWaterTexture(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime_WaterTexture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float waveA = Mathf.Sin((x + y * 0.55f) * 0.35f);
                    float waveB = Mathf.Sin((x * 0.17f - y * 0.28f) + 1.2f);
                    float wave = (waveA + waveB) * 0.5f;
                    float brightness = Mathf.InverseLerp(-1f, 1f, wave);

                    Color dark = new Color(0.03f, 0.30f, 0.55f, 0.8f);
                    Color light = new Color(0.20f, 0.72f, 0.92f, 0.8f);
                    pixels[y * size + x] = Color.Lerp(dark, light, brightness);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
