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

    [MenuItem("Tools/Chapter 14/Build Lakeside Village Game")]
    public static void BuildGame()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject world = new GameObject("=== LAKESIDE VILLAGE GAME ===");
        GameObject environment = Child(world, "01_ENVIRONMENT");
        GameObject village = Child(environment, "Village_Props");
        GameObject gameplay = Child(world, "02_GAMEPLAY");
        GameObject actors = Child(gameplay, "Actors");
        GameObject systems = Child(gameplay, "Systems");
        GameObject ui = Child(world, "03_UI");

        Material grass = CreateOrLoadMaterial("Assets/Chapter14DynamicEnvironment/Generated/Grass_Generated.mat", new Color(0.22f, 0.48f, 0.18f), 0.05f);
        Material dirt = CreateOrLoadMaterial("Assets/Chapter14DynamicEnvironment/Generated/Dirt_Generated.mat", new Color(0.35f, 0.22f, 0.12f), 0.05f);
        Material wood = CreateOrLoadMaterial("Assets/Chapter14DynamicEnvironment/Generated/Wood_Generated.mat", new Color(0.34f, 0.19f, 0.09f), 0.1f);
        Material roof = CreateOrLoadMaterial("Assets/Chapter14DynamicEnvironment/Generated/Roof_Generated.mat", new Color(0.22f, 0.08f, 0.06f), 0.05f);

        CreateLighting(systems.transform);
        CreateGround(environment.transform, grass, dirt);
        CreateLake(environment.transform);
        CreateVillage(village.transform, wood, roof, dirt);
        CreateFencesAndBridge(village.transform, wood);
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

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeObject = world;
        EditorGUIUtility.PingObject(world);
        Debug.Log("Lakeside Village rebuild complete: " + ScenePath);
    }

    private static void CreateLighting(Transform parent)
    {
        GameObject sun = new GameObject("Directional Light - Sun");
        sun.transform.SetParent(parent);
        sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;

        Material sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/DogKnight/Material/Skybox_Mat.mat");
        if (sky == null)
            sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Free HDR Skyboxes Pack/Material/sky-1.mat");
        if (sky != null) RenderSettings.skybox = sky;
        RenderSettings.ambientIntensity = 1f;
    }

    private static void CreateGround(Transform parent, Material grass, Material dirt)
    {
        GameObject terrain = Cube("Main Ground", parent, new Vector3(0f, -0.5f, 0f), new Vector3(80f, 1f, 80f), grass);
        terrain.isStatic = true;

        Cube("Village Road", parent, new Vector3(0f, 0.02f, -5f), new Vector3(8f, 0.08f, 58f), dirt).isStatic = true;
        Cube("Cross Road", parent, new Vector3(0f, 0.03f, 3f), new Vector3(42f, 0.08f, 6f), dirt).isStatic = true;
    }

    private static void CreateLake(Transform parent)
    {
        GameObject basin = Cube("Lake Basin", parent, new Vector3(19f, -0.2f, 8f), new Vector3(28f, 0.5f, 32f), null);
        basin.name = "Lake_Basin_NavMesh_Block";

        GameObject waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AQUAS-Lite/Prefabs/WaterPlane.prefab");
        if (waterPrefab != null)
        {
            GameObject water = (GameObject)PrefabUtility.InstantiatePrefab(waterPrefab, parent);
            water.name = "AQUAS Lake Water";
            water.transform.position = new Vector3(19f, 0.05f, 8f);
            water.transform.localScale = new Vector3(2.8f, 1f, 3.2f);
        }
        else
        {
            GameObject water = Cube("Lake Water", parent, new Vector3(19f, 0.05f, 8f), new Vector3(28f, 0.05f, 32f), null);
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AQUAS-Lite/Materials/AQUAS_Lite_Water.mat");
            if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;
        }
    }

    private static void CreateVillage(Transform parent, Material wood, Material roof, Material dirt)
    {
        Vector3[] positions =
        {
            new Vector3(-16f,0f,14f), new Vector3(-7f,0f,15f), new Vector3(-16f,0f,-4f),
            new Vector3(-8f,0f,-13f), new Vector3(8f,0f,-15f), new Vector3(9f,0f,18f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject house = Child(parent.gameObject, $"House_{i + 1:00}");
            house.transform.position = positions[i];
            Cube("Walls", house.transform, new Vector3(0f, 2f, 0f), new Vector3(6f, 4f, 5f), wood);
            GameObject r = Cube("Roof", house.transform, new Vector3(0f, 4.6f, 0f), new Vector3(7f, 1.1f, 6f), roof);
            r.transform.rotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 8f : -8f);
            Cube("Door", house.transform, new Vector3(0f, 1.2f, -2.55f), new Vector3(1.2f, 2.4f, 0.15f), dirt);
        }

        GameObject plaza = Child(parent.gameObject, "Village_Plaza");
        Cube("Plaza Floor", plaza.transform, new Vector3(0f, 0.06f, 3f), new Vector3(14f, 0.1f, 14f), dirt);
        GameObject well = Cube("Well", plaza.transform, new Vector3(0f, 0.7f, 3f), new Vector3(2.5f, 1.4f, 2.5f), wood);
        well.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
    }

    private static void CreateFencesAndBridge(Transform parent, Material wood)
    {
        GameObject fences = Child(parent.gameObject, "Fences_Enemy_Must_Path_Around");
        for (int i = 0; i < 7; i++)
        {
            Cube($"Fence_{i + 1:00}", fences.transform, new Vector3(-20f + i * 2f, 1f, 5f), new Vector3(0.25f, 2f, 3f), wood);
        }

        GameObject bridge = Child(parent.gameObject, "Bridge_Over_Lake");
        for (int i = 0; i < 9; i++)
        {
            Cube($"Bridge_Plank_{i + 1:00}", bridge.transform, new Vector3(10f + i * 1.5f, 0.45f, -2f), new Vector3(1.35f, 0.2f, 3.5f), wood);
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
        GameObject root = new GameObject("PLAYER_DogKnight");
        root.transform.SetParent(parent);
        root.transform.position = new Vector3(0f, 1.05f, -20f);
        root.tag = "Player";

        CharacterController controller = root.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 1f, 0f);

        GameObject dogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DogKnight/Prefab/DogPBR.prefab");
        GameObject body;
        if (dogPrefab != null)
        {
            body = (GameObject)PrefabUtility.InstantiatePrefab(dogPrefab, root.transform);
            body.name = "DogKnight_Visual";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
        }
        else
        {
            body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Fallback_Player_Visual";
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

        GameObject cameraObject = new GameObject("Main Camera");
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
        so.ApplyModifiedPropertiesWithoutUndo();

        return root;
    }

    private static void CreateEnemies(Transform parent, Transform player)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DacingEyebrows/deb_Goblin01/Prefab/deb_Goblin01.prefab");
        Vector3[] positions = { new Vector3(-9f, 0f, 2f), new Vector3(6f, 0f, 8f), new Vector3(-12f, 0f, 24f) };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject root = new GameObject($"ENEMY_Goblin_{i + 1:00}");
            root.transform.SetParent(parent);
            root.transform.position = positions[i];

            GameObject visual;
            if (prefab != null)
            {
                visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
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
            so.FindProperty("healthCanvas").objectReferenceValue = slider.transform.root.gameObject;
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
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

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
        fillImage.color = new Color(0.75f, 0.08f, 0.06f, 1f);
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
        shape.scale = new Vector3(55f, 1f, 55f);

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 5f;
        renderer.velocityScale = 0.12f;

        Light sun = Object.FindFirstObjectByType<Light>();
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

        EnsureFolder("Assets/Chapter14DynamicEnvironment/Generated");
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Chapter14DynamicEnvironment/Generated/MinimapRT.renderTexture");
        if (rt == null)
        {
            rt = new RenderTexture(512, 512, 16) { name = "MinimapRT" };
            AssetDatabase.CreateAsset(rt, "Assets/Chapter14DynamicEnvironment/Generated/MinimapRT.renderTexture");
        }
        cam.targetTexture = rt;

        GameObject canvasObject = new GameObject("HUD Canvas");
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

        GameObject textObject = new GameObject("Controls Help");
        textObject.transform.SetParent(canvas.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "WASD Move  |  Shift Run  |  Space Jump  |  LMB Attack\nF5 First/Third Person  |  R Toggle Rain";
        text.fontSize = 20;
        text.alignment = TextAnchor.LowerLeft;
        text.color = Color.white;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(20f, 20f);
        rect.sizeDelta = new Vector2(780f, 80f);
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

    private static Material CreateOrLoadMaterial(string path, Color color, float smoothness)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;

        EnsureFolder("Assets/Chapter14DynamicEnvironment/Generated");
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        material = new Material(shader) { color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(material, path);
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
