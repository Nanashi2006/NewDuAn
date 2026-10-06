#if UNITY_EDITOR
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LakesideGameBuilder
{
    private const string ScenePath = "Assets/Scenes/LakesideVillage_Rebuilt.unity";
    private const string GeneratedFolder = "Assets/Chapter14DynamicEnvironment/Generated";
    private const string GrassTexturePath = "Assets/ALP_Assets/GrassFlowersFREE/Textures/Ground/Grass01_BigUV.png";
    private const string GrassDetailPath = "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass01.tga";
    private const string FlowerDetailPath = "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grassFlower03.tga";
    private const string DirtTexturePath = "Assets/Handpainted_Grass_and_Ground_Textures/Textures/Dirt/dirt_clay/dirt_clay_up.png";
    private const string DogPrefabPath = "Assets/DogKnight/Prefab/DogPBR.prefab";
    private const string DogPolyPrefabPath = "Assets/DogKnight/Prefab/DogPolyart.prefab";
    private const string GoblinPrefabPath = "Assets/DacingEyebrows/deb_Goblin01/Prefab/deb_Goblin01.prefab";
    private const string WaterPrefabPath = "Assets/AQUAS-Lite/Prefabs/WaterPlane.prefab";
    private const string SkyboxPath = "Assets/Free HDR Skyboxes Pack/Material/sky-2.mat";

    [MenuItem("Tools/Chapter 14/Build Lakeside Village Game")]
    public static void BuildGame()
    {
        EnsureFolder(GeneratedFolder);
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject world = new GameObject("=== REAL ASSET BUILD V2 - LAKESIDE VILLAGE ===");
        GameObject audit = Child(world, "00_REAL_ASSETS_IN_USE");
        GameObject environment = Child(world, "01_ENVIRONMENT_REAL_ASSETS");
        GameObject village = Child(environment, "Village_Detailed_Fallback_Architecture");
        GameObject vegetation = Child(environment, "Vegetation_ALP_GrassFlowers");
        GameObject showcase = Child(environment, "DogKnight_Asset_Showcase");
        GameObject gameplay = Child(world, "02_GAMEPLAY");
        GameObject actors = Child(gameplay, "Actors_REAL_PREFABS");
        GameObject systems = Child(gameplay, "Systems");
        GameObject ui = Child(world, "03_UI");

        AddAssetAudit(audit.transform, "PLAYER", DogPrefabPath);
        AddAssetAudit(audit.transform, "ENEMY", GoblinPrefabPath);
        AddAssetAudit(audit.transform, "WATER", WaterPrefabPath);
        AddAssetAudit(audit.transform, "SKYBOX", SkyboxPath);
        AddAssetAudit(audit.transform, "GROUND_TEXTURE", GrassTexturePath);
        AddAssetAudit(audit.transform, "ROAD_TEXTURE", DirtTexturePath);
        AddAssetAudit(audit.transform, "GRASS_DETAIL", GrassDetailPath);
        AddAssetAudit(audit.transform, "FLOWER_DETAIL", FlowerDetailPath);

        Material grass = CreateOrUpdateTexturedMaterial(
            GeneratedFolder + "/Grass_FROM_ALP.mat",
            GrassTexturePath,
            Color.white,
            new Vector2(8f, 8f),
            0.05f);

        Material dirt = CreateOrUpdateTexturedMaterial(
            GeneratedFolder + "/Dirt_FROM_HANDPAINTED.mat",
            DirtTexturePath,
            new Color(0.95f, 0.88f, 0.76f),
            new Vector2(4f, 12f),
            0.08f);

        Material wall = CreateOrUpdateTexturedMaterial(
            GeneratedFolder + "/CottageWall_FROM_HANDPAINTED.mat",
            DirtTexturePath,
            new Color(0.78f, 0.64f, 0.46f),
            new Vector2(2f, 2f),
            0.12f);

        Material wood = CreateOrUpdateTexturedMaterial(
            GeneratedFolder + "/WoodStyle_FROM_HANDPAINTED.mat",
            DirtTexturePath,
            new Color(0.42f, 0.25f, 0.13f),
            new Vector2(3f, 3f),
            0.14f);

        Material roof = CreateOrUpdateTexturedMaterial(
            GeneratedFolder + "/RoofStyle_FROM_HANDPAINTED.mat",
            DirtTexturePath,
            new Color(0.38f, 0.12f, 0.08f),
            new Vector2(4f, 4f),
            0.05f);

        CreateLighting(systems.transform);
        CreateAssetTerrain(environment.transform);
        CreateRoads(environment.transform, dirt);
        CreateLake(environment.transform);
        CreateVillage(village.transform, wall, wood, roof, dirt);
        CreateFencesAndBridge(village.transform, wood);
        CreateVegetationMarkers(vegetation.transform);
        CreateDogKnightShowcase(showcase.transform);
        CreateMudZone(gameplay.transform, dirt);

        GameObject player = CreatePlayer(actors.transform);
        CreateEnemies(actors.transform, player.transform);
        CreateRain(systems.transform);
        CreateMinimap(ui.transform, player.transform);
        CreateInstructions(ui.transform);

        NavMeshSurface surface = environment.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeObject = world;
        EditorGUIUtility.PingObject(world);
        Debug.Log("REAL ASSET BUILD V2 complete. OPEN AND PLAY: " + ScenePath);
    }

    private static void AddAssetAudit(Transform parent, string label, string path)
    {
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        string state = asset != null ? "OK" : "MISSING";
        GameObject marker = new GameObject(state + " - " + label + " - " + path.Replace("Assets/", string.Empty));
        marker.transform.SetParent(parent);
    }

    private static void CreateLighting(Transform parent)
    {
        GameObject sun = new GameObject("Directional Light - Sun");
        sun.transform.SetParent(parent);
        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.shadows = LightShadows.Soft;

        Material sky = AssetDatabase.LoadAssetAtPath<Material>(SkyboxPath);
        if (sky == null)
            sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Free HDR Skyboxes Pack/Material/sky-1.mat");
        if (sky == null)
            sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/DogKnight/Material/Skybox_Mat.mat");

        if (sky != null) RenderSettings.skybox = sky;
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = false;
    }

    private static void CreateAssetTerrain(Transform parent)
    {
        string dataPath = GeneratedFolder + "/Lakeside_REAL_ASSET_Terrain.asset";
        TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(dataPath);
        if (data == null)
        {
            data = new TerrainData();
            AssetDatabase.CreateAsset(data, dataPath);
        }

        data.heightmapResolution = 129;
        data.size = new Vector3(90f, 8f, 90f);
        data.SetHeights(0, 0, new float[129, 129]);
        data.alphamapResolution = 64;

        Texture2D grassTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GrassTexturePath);
        TerrainLayer grassLayer = CreateOrUpdateTerrainLayer(
            GeneratedFolder + "/TerrainLayer_ALP_Grass.terrainlayer",
            grassTexture,
            new Vector2(12f, 12f));
        data.terrainLayers = new[] { grassLayer };

        float[,,] alpha = new float[64, 64, 1];
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                alpha[y, x, 0] = 1f;
        data.SetAlphamaps(0, 0, alpha);

        ConfigureTerrainDetails(data);

        GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
        terrainObject.name = "REAL ASSET Terrain - ALP Grass + Flowers";
        terrainObject.transform.SetParent(parent);
        terrainObject.transform.position = new Vector3(-45f, 0f, -45f);
        terrainObject.isStatic = true;

        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.drawInstanced = true;
        terrain.detailObjectDistance = 55f;
        terrain.detailObjectDensity = 0.75f;
    }

    private static TerrainLayer CreateOrUpdateTerrainLayer(string path, Texture2D texture, Vector2 tileSize)
    {
        TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        if (layer == null)
        {
            layer = new TerrainLayer();
            AssetDatabase.CreateAsset(layer, path);
        }
        layer.diffuseTexture = texture;
        layer.tileSize = tileSize;
        EditorUtility.SetDirty(layer);
        return layer;
    }

    private static void ConfigureTerrainDetails(TerrainData data)
    {
        Texture2D grass = AssetDatabase.LoadAssetAtPath<Texture2D>(GrassDetailPath);
        Texture2D flowers = AssetDatabase.LoadAssetAtPath<Texture2D>(FlowerDetailPath);
        if (grass == null && flowers == null)
        {
            data.detailPrototypes = new DetailPrototype[0];
            return;
        }

        data.SetDetailResolution(256, 8);
        System.Collections.Generic.List<DetailPrototype> prototypes = new System.Collections.Generic.List<DetailPrototype>();

        if (grass != null)
        {
            DetailPrototype p = new DetailPrototype();
            p.prototypeTexture = grass;
            p.renderMode = DetailRenderMode.GrassBillboard;
            p.minWidth = 0.35f;
            p.maxWidth = 0.75f;
            p.minHeight = 0.45f;
            p.maxHeight = 1.0f;
            p.healthyColor = Color.white;
            p.dryColor = new Color(0.78f, 0.76f, 0.56f, 1f);
            p.noiseSpread = 0.15f;
            prototypes.Add(p);
        }

        if (flowers != null)
        {
            DetailPrototype p = new DetailPrototype();
            p.prototypeTexture = flowers;
            p.renderMode = DetailRenderMode.GrassBillboard;
            p.minWidth = 0.25f;
            p.maxWidth = 0.55f;
            p.minHeight = 0.35f;
            p.maxHeight = 0.75f;
            p.healthyColor = Color.white;
            p.dryColor = Color.white;
            p.noiseSpread = 0.2f;
            prototypes.Add(p);
        }

        data.detailPrototypes = prototypes.ToArray();
        System.Random random = new System.Random(1406);
        int resolution = data.detailResolution;

        for (int layer = 0; layer < prototypes.Count; layer++)
        {
            int[,] map = new int[resolution, resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float worldX = ((float)x / resolution) * 90f - 45f;
                    float worldZ = ((float)y / resolution) * 90f - 45f;
                    bool road = Mathf.Abs(worldX) < 5f || (Mathf.Abs(worldZ - 3f) < 4f && Mathf.Abs(worldX) < 24f);
                    bool lake = worldX > 6f && worldX < 34f && worldZ > -8f && worldZ < 24f;
                    if (road || lake) continue;

                    double chance = layer == 0 ? 0.32 : 0.025;
                    if (random.NextDouble() < chance) map[y, x] = 1;
                }
            }
            data.SetDetailLayer(0, 0, layer, map);
        }
    }

    private static void CreateRoads(Transform parent, Material dirt)
    {
        GameObject roads = Child(parent.gameObject, "Roads - Handpainted Dirt Texture");
        Cube("Village Road", roads.transform, new Vector3(0f, 0.035f, -5f), new Vector3(8f, 0.07f, 58f), dirt).isStatic = true;
        Cube("Cross Road", roads.transform, new Vector3(0f, 0.045f, 3f), new Vector3(42f, 0.08f, 6f), dirt).isStatic = true;
        Cube("Lakeside Path", roads.transform, new Vector3(13f, 0.05f, 18f), new Vector3(18f, 0.07f, 3f), dirt).isStatic = true;
    }

    private static void CreateLake(Transform parent)
    {
        GameObject lakeRoot = Child(parent.gameObject, "Lake - AQUAS Lite REAL PREFAB");

        GameObject blocker = new GameObject("Lake_NavMesh_Blocker_Invisible");
        blocker.transform.SetParent(lakeRoot.transform);
        blocker.transform.position = new Vector3(20f, -0.35f, 8f);
        BoxCollider blockerCollider = blocker.AddComponent<BoxCollider>();
        blockerCollider.size = new Vector3(28f, 0.7f, 31f);

        GameObject waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WaterPrefabPath);
        if (waterPrefab != null)
        {
            GameObject water = InstantiateAsset(waterPrefab, lakeRoot.transform, "AQUAS WaterPlane - REAL ASSET");
            water.transform.position = new Vector3(20f, 0.12f, 8f);
            water.transform.localScale = new Vector3(2.8f, 1f, 3.1f);
        }
        else
        {
            GameObject fallback = Cube("MISSING AQUAS - FALLBACK WATER", lakeRoot.transform, new Vector3(20f, 0.12f, 8f), new Vector3(28f, 0.05f, 31f), null);
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AQUAS-Lite/Materials/AQUAS_Lite_Water.mat");
            if (waterMat != null) fallback.GetComponent<Renderer>().sharedMaterial = waterMat;
        }
    }

    private static void CreateVillage(Transform parent, Material wall, Material wood, Material roof, Material dirt)
    {
        Vector3[] positions =
        {
            new Vector3(-17f,0f,15f), new Vector3(-8f,0f,16f), new Vector3(-17f,0f,-5f),
            new Vector3(-9f,0f,-14f), new Vector3(9f,0f,-16f), new Vector3(7f,0f,28f)
        };

        float[] rotations = { 12f, -10f, 90f, 20f, -25f, 180f };
        for (int i = 0; i < positions.Length; i++)
            CreateDetailedCottage(parent, i + 1, positions[i], rotations[i], wall, wood, roof);

        GameObject plaza = Child(parent.gameObject, "Village_Plaza_Detailed");
        Cube("Plaza Floor", plaza.transform, new Vector3(0f, 0.07f, 3f), new Vector3(15f, 0.12f, 14f), dirt);

        GameObject well = Child(plaza, "Stone_Well");
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 1.35f, 0.55f, 3f + Mathf.Sin(angle) * 1.35f);
            GameObject block = Cube("WellStone_" + i.ToString("00"), well.transform, pos, new Vector3(0.65f, 1f, 0.45f), wall);
            block.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
        }
        Cube("WellBeam_L", well.transform, new Vector3(-1.3f, 2.2f, 3f), new Vector3(0.2f, 3.2f, 0.2f), wood);
        Cube("WellBeam_R", well.transform, new Vector3(1.3f, 2.2f, 3f), new Vector3(0.2f, 3.2f, 0.2f), wood);
        Cube("WellTop", well.transform, new Vector3(0f, 3.65f, 3f), new Vector3(3.2f, 0.2f, 0.25f), wood);
    }

    private static void CreateDetailedCottage(Transform parent, int index, Vector3 position, float yaw, Material wall, Material wood, Material roof)
    {
        GameObject house = Child(parent.gameObject, "Cottage_" + index.ToString("00") + " [Detailed Fallback - No House Prefab Was Uploaded]");
        house.transform.position = position;
        house.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        Cube("Foundation", house.transform, new Vector3(0f, 0.3f, 0f), new Vector3(6.7f, 0.6f, 5.7f), wood);
        Cube("Walls", house.transform, new Vector3(0f, 2.25f, 0f), new Vector3(6.2f, 3.8f, 5.2f), wall);

        GameObject roofL = Cube("Roof_Left", house.transform, new Vector3(-1.55f, 4.65f, 0f), new Vector3(3.8f, 0.42f, 6.2f), roof);
        roofL.transform.localRotation = Quaternion.Euler(0f, 0f, 27f);
        GameObject roofR = Cube("Roof_Right", house.transform, new Vector3(1.55f, 4.65f, 0f), new Vector3(3.8f, 0.42f, 6.2f), roof);
        roofR.transform.localRotation = Quaternion.Euler(0f, 0f, -27f);

        Cube("Door", house.transform, new Vector3(0f, 1.45f, -2.64f), new Vector3(1.25f, 2.7f, 0.18f), wood);
        Cube("DoorFrame_Top", house.transform, new Vector3(0f, 2.82f, -2.77f), new Vector3(1.6f, 0.14f, 0.14f), roof);
        Cube("Window_L", house.transform, new Vector3(-2f, 2.25f, -2.64f), new Vector3(1.05f, 1.2f, 0.16f), roof);
        Cube("Window_R", house.transform, new Vector3(2f, 2.25f, -2.64f), new Vector3(1.05f, 1.2f, 0.16f), roof);
        Cube("Chimney", house.transform, new Vector3(1.8f, 5.35f, 0.9f), new Vector3(0.7f, 2.1f, 0.7f), wall);

        for (int i = -2; i <= 2; i++)
            Cube("WallBeam_" + i, house.transform, new Vector3(i * 1.3f, 2.2f, -2.73f), new Vector3(0.12f, 3.7f, 0.12f), wood);
    }

    private static void CreateFencesAndBridge(Transform parent, Material wood)
    {
        GameObject fences = Child(parent.gameObject, "Fences_Enemy_Must_Path_Around");
        for (int i = 0; i < 9; i++)
        {
            Vector3 basePos = new Vector3(-24f + i * 2.3f, 0f, 6f);
            Cube("Post_" + (i + 1).ToString("00"), fences.transform, basePos + Vector3.up * 1f, new Vector3(0.22f, 2f, 0.22f), wood);
            if (i < 8)
            {
                Cube("RailTop_" + (i + 1).ToString("00"), fences.transform, basePos + new Vector3(1.15f, 1.45f, 0f), new Vector3(2.3f, 0.15f, 0.18f), wood);
                Cube("RailLow_" + (i + 1).ToString("00"), fences.transform, basePos + new Vector3(1.15f, 0.7f, 0f), new Vector3(2.3f, 0.15f, 0.18f), wood);
            }
        }

        GameObject bridge = Child(parent.gameObject, "Bridge_Over_Lake");
        for (int i = 0; i < 11; i++)
            Cube("Bridge_Plank_" + (i + 1).ToString("00"), bridge.transform, new Vector3(7.5f + i * 1.45f, 0.5f, -2f), new Vector3(1.34f, 0.18f, 3.6f), wood);

        Cube("BridgeRail_L", bridge.transform, new Vector3(14.75f, 1.3f, -3.65f), new Vector3(15f, 0.15f, 0.15f), wood);
        Cube("BridgeRail_R", bridge.transform, new Vector3(14.75f, 1.3f, -0.35f), new Vector3(15f, 0.15f, 0.15f), wood);
    }

    private static void CreateVegetationMarkers(Transform parent)
    {
        Child(parent.gameObject, "REAL vegetation is painted by Terrain DetailPrototype from ALP textures");
        Child(parent.gameObject, "grass01.tga - billboard detail");
        Child(parent.gameObject, "grassFlower03.tga - billboard detail");
    }

    private static void CreateDogKnightShowcase(Transform parent)
    {
        GameObject stageAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DogKnight/Mesh/Stage.fbx");
        if (stageAsset != null)
        {
            GameObject stage = InstantiateAsset(stageAsset, parent, "DogKnight Stage FBX - REAL ASSET");
            stage.transform.position = new Vector3(-28f, 0.15f, -22f);
            stage.transform.localScale = Vector3.one * 2f;
        }

        GameObject swordAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DogKnight/Mesh/Sword.fbx");
        GameObject shieldAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DogKnight/Mesh/Shield.fbx");
        if (swordAsset != null)
        {
            GameObject sword = InstantiateAsset(swordAsset, parent, "Sword Display - REAL FBX");
            sword.transform.position = new Vector3(-27f, 1.2f, -20f);
            sword.transform.rotation = Quaternion.Euler(0f, 0f, 55f);
            sword.transform.localScale = Vector3.one * 1.5f;
        }
        if (shieldAsset != null)
        {
            GameObject shield = InstantiateAsset(shieldAsset, parent, "Shield Display - REAL FBX");
            shield.transform.position = new Vector3(-29f, 1.2f, -20f);
            shield.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            shield.transform.localScale = Vector3.one * 1.5f;
        }

        GameObject guardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DogPolyPrefabPath);
        if (guardPrefab != null)
        {
            GameObject guard = InstantiateAsset(guardPrefab, parent, "Village Guard DogPolyart - REAL PREFAB");
            guard.transform.position = new Vector3(-28f, 0f, -18f);
            guard.transform.rotation = Quaternion.Euler(0f, 145f, 0f);
        }
    }

    private static void CreateMudZone(Transform parent, Material dirt)
    {
        GameObject mud = Cube("Mud Slow Zone", parent, new Vector3(-4f, 0.08f, 23f), new Vector3(10f, 0.12f, 8f), dirt);
        BoxCollider collider = mud.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        mud.AddComponent<LakesideMudZone>();
    }

    private static GameObject CreatePlayer(Transform parent)
    {
        GameObject root = new GameObject("PLAYER_DogKnight_PBR_REAL_PREFAB");
        root.transform.SetParent(parent);
        root.transform.position = new Vector3(0f, 0.08f, -20f);
        root.tag = "Player";

        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 1f, 0f);

        GameObject dogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DogPrefabPath);
        GameObject body;
        if (dogPrefab != null)
        {
            body = InstantiateAsset(dogPrefab, root.transform, "DogKnight_Visual_REAL_PREFAB");
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
        }
        else
        {
            body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "MISSING DogKnight - Fallback Player";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
        }

        Transform fp = new GameObject("CameraAnchor_FirstPerson").transform;
        fp.SetParent(root.transform);
        fp.localPosition = new Vector3(0f, 1.75f, 0.15f);

        Transform tp = new GameObject("CameraAnchor_ThirdPerson").transform;
        tp.SetParent(root.transform);
        tp.localPosition = new Vector3(0f, 2.7f, -5.5f);

        Transform attack = new GameObject("AttackOrigin").transform;
        attack.SetParent(root.transform);
        attack.localPosition = new Vector3(0f, 1.15f, 0.65f);

        GameObject cameraObject = new GameObject("Main Camera - Rebuilt Scene");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetPositionAndRotation(tp.position, tp.rotation);

        LakesideGameplayRebuild gameplay = root.AddComponent<LakesideGameplayRebuild>();
        SerializedObject so = new SerializedObject(gameplay);
        so.FindProperty("controller").objectReferenceValue = controller;
        so.FindProperty("animator").objectReferenceValue = body.GetComponentInChildren<Animator>();
        so.FindProperty("playerCamera").objectReferenceValue = camera;
        so.FindProperty("attackOrigin").objectReferenceValue = attack;
        so.FindProperty("thirdPersonBody").objectReferenceValue = body;
        so.FindProperty("firstPersonAnchor").objectReferenceValue = fp;
        so.FindProperty("thirdPersonAnchor").objectReferenceValue = tp;
        so.FindProperty("enemyLayer").intValue = ~0;
        so.ApplyModifiedPropertiesWithoutUndo();

        return root;
    }

    private static void CreateEnemies(Transform parent, Transform player)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GoblinPrefabPath);
        Vector3[] positions = { new Vector3(-10f, 0.05f, 1f), new Vector3(5f, 0.05f, 10f), new Vector3(-13f, 0.05f, 25f) };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject root = new GameObject("ENEMY_Goblin_REAL_PREFAB_" + (i + 1).ToString("00"));
            root.transform.SetParent(parent);
            root.transform.position = positions[i];

            GameObject visual;
            if (prefab != null)
            {
                visual = InstantiateAsset(prefab, root.transform, "Goblin_Visual_REAL_PREFAB");
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.name = "MISSING Goblin - Fallback Visual";
                visual.transform.SetParent(root.transform);
                visual.transform.localPosition = Vector3.up;
            }

            CapsuleCollider col = root.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.5f;
            col.center = Vector3.up;

            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.9f;

            Slider slider = CreateWorldHealthBar(root.transform);
            LakesideEnemy enemy = root.AddComponent<LakesideEnemy>();
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("agent").objectReferenceValue = agent;
            so.FindProperty("animator").objectReferenceValue = visual.GetComponentInChildren<Animator>();
            so.FindProperty("healthSlider").objectReferenceValue = slider;
            so.FindProperty("healthCanvas").objectReferenceValue = slider.transform.parent.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static Slider CreateWorldHealthBar(Transform parent)
    {
        GameObject canvasObject = new GameObject("Enemy Health UI");
        canvasObject.transform.SetParent(parent);
        canvasObject.transform.localPosition = new Vector3(0f, 2.6f, 0f);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2.6f, 0.35f);
        canvasRect.localScale = Vector3.one * 0.01f;

        GameObject sliderObject = new GameObject("HP 100");
        sliderObject.transform.SetParent(canvasObject.transform, false);
        Slider slider = sliderObject.AddComponent<Slider>();
        RectTransform rect = slider.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image background = sliderObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fa = fillArea.AddComponent<RectTransform>();
        fa.anchorMin = new Vector2(0.03f, 0.15f);
        fa.anchorMax = new Vector2(0.97f, 0.85f);
        fa.offsetMin = Vector2.zero;
        fa.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fr = fill.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = Vector2.one;
        fr.offsetMin = Vector2.zero;
        fr.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.15f, 0.9f, 0.2f, 1f);
        slider.fillRect = fr;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        return slider;
    }

    private static void CreateRain(Transform parent)
    {
        GameObject rainObject = new GameObject("Weather_Rain_System [R to Toggle]");
        rainObject.transform.SetParent(parent);
        rainObject.transform.position = new Vector3(0f, 18f, 0f);

        ParticleSystem ps = rainObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.startLifetime = 1.4f;
        main.startSpeed = 18f;
        main.startSize = 0.045f;
        main.maxParticles = 7000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 2200f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(70f, 1f, 70f);

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 5f;
        renderer.velocityScale = 0.12f;

        Light sun = Object.FindAnyObjectByType<Light>();
        LakesideRainController controller = rainObject.AddComponent<LakesideRainController>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("rain").objectReferenceValue = ps;
        so.FindProperty("directionalLight").objectReferenceValue = sun;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateMinimap(Transform parent, Transform player)
    {
        GameObject cameraObject = new GameObject("Minimap Camera");
        cameraObject.transform.SetParent(parent);
        Camera cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 18f;
        cam.transform.position = player.position + Vector3.up * 30f;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cameraObject.AddComponent<LakesideMinimapFollow>().target = player;

        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(GeneratedFolder + "/MinimapRT.renderTexture");
        if (rt == null)
        {
            rt = new RenderTexture(512, 512, 16) { name = "MinimapRT" };
            AssetDatabase.CreateAsset(rt, GeneratedFolder + "/MinimapRT.renderTexture");
        }
        cam.targetTexture = rt;

        GameObject canvasObject = new GameObject("HUD Canvas - REBUILT V2");
        canvasObject.transform.SetParent(parent);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject map = new GameObject("Minimap");
        map.transform.SetParent(canvasObject.transform, false);
        RawImage image = map.AddComponent<RawImage>();
        image.texture = rt;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(230f, 230f);
    }

    private static void CreateInstructions(Transform parent)
    {
        Canvas canvas = parent.GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        GameObject titleObject = new GameObject("REAL ASSET BUILD V2 LABEL");
        titleObject.transform.SetParent(canvas.transform, false);
        Text title = titleObject.AddComponent<Text>();
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.text = "LAKESIDE VILLAGE - REAL ASSET BUILD V2";
        title.fontSize = 22;
        title.fontStyle = FontStyle.Bold;
        title.color = Color.white;
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(20f, -20f);
        titleRect.sizeDelta = new Vector2(700f, 50f);

        GameObject textObject = new GameObject("Controls Help");
        textObject.transform.SetParent(canvas.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "WASD Move  |  Shift Run  |  Space Jump  |  LMB Attack\nF5 First/Third Person  |  R Toggle Rain";
        text.fontSize = 20;
        text.alignment = TextAnchor.LowerLeft;
        text.color = Color.white;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(20f, 20f);
        rect.sizeDelta = new Vector2(780f, 80f);
    }

    private static GameObject InstantiateAsset(GameObject asset, Transform parent, string name)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        if (instance == null) instance = Object.Instantiate(asset, parent);
        instance.name = name;
        return instance;
    }

    private static GameObject Child(GameObject parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        return go;
    }

    private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = scale;
        if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static Material CreateOrUpdateTexturedMaterial(string path, string texturePath, Color tint, Vector2 tiling, float smoothness)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        material.color = tint;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", tiling);
        }
        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
            material.SetTextureScale("_MainTex", tiling);
        }
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif