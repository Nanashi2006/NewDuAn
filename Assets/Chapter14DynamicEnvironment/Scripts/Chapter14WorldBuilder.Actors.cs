using UnityEngine;
using UnityEngine.AI;

namespace Chapter14DynamicEnvironment
{
    internal static partial class Chapter14WorldBuilder
    {
        private static void CreateEnvironmentProps(Transform parent, Terrain terrain)
        {
            Material trunkMaterial = CreateMaterial(
                "M_Trunk_Runtime",
                new Color(0.28f, 0.14f, 0.06f, 1f)
            );

            Material leavesMaterial = CreateMaterial(
                "M_Leaves_Runtime",
                new Color(0.08f, 0.34f, 0.09f, 1f)
            );

            Vector2[] treePositions =
            {
                new Vector2(-42f, -36f),
                new Vector2(-28f, 30f),
                new Vector2(-8f, 38f),
                new Vector2(28f, 42f),
                new Vector2(45f, 28f),
                new Vector2(47f, -8f),
                new Vector2(34f, -40f),
                new Vector2(-18f, -42f),
                new Vector2(-48f, 8f),
                new Vector2(-36f, 22f),
                new Vector2(4f, -34f),
                new Vector2(52f, 46f)
            };

            foreach (Vector2 treePosition in treePositions)
            {
                float y = TerrainWorldHeight(terrain, treePosition.x, treePosition.y);
                CreateTree(
                    parent,
                    new Vector3(treePosition.x, y, treePosition.y),
                    trunkMaterial,
                    leavesMaterial
                );
            }

            Material rockMaterial = CreateMaterial(
                "M_Rock_Runtime",
                new Color(0.31f, 0.33f, 0.35f, 1f)
            );

            Vector2[] rockPositions =
            {
                new Vector2(-12f, 4f),
                new Vector2(4f, 28f),
                new Vector2(38f, 5f),
                new Vector2(25f, -26f),
                new Vector2(-32f, -8f),
                new Vector2(-4f, -20f)
            };

            foreach (Vector2 rockPosition in rockPositions)
            {
                float y = TerrainWorldHeight(terrain, rockPosition.x, rockPosition.y);
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Rock";
                rock.transform.SetParent(parent);
                rock.transform.position = new Vector3(rockPosition.x, y + 0.6f, rockPosition.y);
                rock.transform.localScale = new Vector3(
                    Random.Range(1.4f, 2.5f),
                    Random.Range(0.8f, 1.5f),
                    Random.Range(1.2f, 2.3f)
                );
                rock.GetComponent<Renderer>().material = rockMaterial;
            }
        }

        private static GameObject CreatePlayer(Transform parent, Terrain terrain)
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(parent);

            float groundY = TerrainWorldHeight(terrain, -28f, -20f);
            player.transform.position = new Vector3(-28f, groundY + 0.05f, -20f);
            player.tag = "Player";

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.48f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50f;

            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerCombat>();

            Material playerMaterial = CreateMaterial(
                "M_Player_Runtime",
                new Color(0.08f, 0.35f, 0.95f, 1f)
            );

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localRotation = Quaternion.identity;

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                visualCollider.enabled = false;

            visual.GetComponent<Renderer>().material = playerMaterial;

            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "Sword";
            sword.transform.SetParent(player.transform);
            sword.transform.localPosition = new Vector3(0.58f, 1.05f, 0.65f);
            sword.transform.localRotation = Quaternion.Euler(12f, 0f, -8f);
            sword.transform.localScale = new Vector3(0.10f, 0.10f, 1.15f);

            Collider swordCollider = sword.GetComponent<Collider>();
            if (swordCollider != null)
                swordCollider.enabled = false;

            sword.GetComponent<Renderer>().material = CreateMaterial(
                "M_Sword_Runtime",
                new Color(0.75f, 0.78f, 0.82f, 1f)
            );

            return player;
        }

        private static GameObject CreateEnemy(Transform parent, Terrain terrain, Transform player)
        {
            GameObject enemy = new GameObject("Enemy");
            enemy.transform.SetParent(parent);

            float groundY = TerrainWorldHeight(terrain, 35f, 23f);
            enemy.transform.position = new Vector3(35f, groundY + 0.05f, 23f);
            enemy.transform.rotation = Quaternion.Euler(0f, 210f, 0f);

            CapsuleCollider enemyCollider = enemy.AddComponent<CapsuleCollider>();
            enemyCollider.height = 2f;
            enemyCollider.radius = 0.5f;
            enemyCollider.center = new Vector3(0f, 1f, 0f);

            NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.acceleration = 12f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 1.35f;
            agent.autoBraking = true;

            EnemyHealth health = enemy.AddComponent<EnemyHealth>();
            health.maxHealth = 100f;
            health.currentHealth = 100f;

            EnemyController enemyController = enemy.AddComponent<EnemyController>();
            enemyController.playerTransform = player;
            enemyController.chaseRange = 18f;
            enemyController.viewDistance = 22f;
            enemyController.viewAngle = 170f;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(enemy.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                visualCollider.enabled = false;

            visual.GetComponent<Renderer>().material = CreateMaterial(
                "M_Enemy_Runtime",
                new Color(0.78f, 0.08f, 0.08f, 1f)
            );

            Material eyeMaterial = CreateMaterial(
                "M_EnemyEye_Runtime",
                new Color(1f, 0.85f, 0.05f, 1f)
            );

            CreateEye(visual.transform, "Eye_L", new Vector3(-0.18f, 0.35f, 0.43f), eyeMaterial);
            CreateEye(visual.transform, "Eye_R", new Vector3(0.18f, 0.35f, 0.43f), eyeMaterial);
            return enemy;
        }

        private static void CreateEye(
            Transform parent,
            string eyeName,
            Vector3 localPosition,
            Material material)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = eyeName;
            eye.transform.SetParent(parent);
            eye.transform.localPosition = localPosition;
            eye.transform.localScale = Vector3.one * 0.12f;

            Collider collider = eye.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            eye.GetComponent<Renderer>().material = material;
        }

        private static void CreateCamera(Transform parent, Transform player)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 350f;
            camera.allowHDR = true;

            cameraObject.AddComponent<AudioListener>();

            ThirdPersonCamera cameraFollow = cameraObject.AddComponent<ThirdPersonCamera>();
            cameraFollow.target = player;
            cameraFollow.yaw = 35f;
            cameraFollow.pitch = 22f;
            cameraFollow.distance = 7f;

            cameraObject.transform.position = player.position + new Vector3(-5f, 4f, -5f);
            cameraObject.transform.LookAt(player.position + Vector3.up * 1.4f);
        }

        private static Light CreateSun(Transform parent)
        {
            GameObject sunObject = new GameObject("Directional Light - Sun");
            sunObject.transform.SetParent(parent);
            sunObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.intensity = 1.15f;
            sun.color = Color.white;

            RenderSettings.sun = sun;
            return sun;
        }

        private static Material CreateProceduralSkybox()
        {
            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader == null)
                return null;

            Material skybox = new Material(skyShader)
            {
                name = "M_ProceduralSky_Runtime"
            };

            if (skybox.HasProperty("_SunSize"))
                skybox.SetFloat("_SunSize", 0.035f);

            if (skybox.HasProperty("_AtmosphereThickness"))
                skybox.SetFloat("_AtmosphereThickness", 1f);

            if (skybox.HasProperty("_Exposure"))
                skybox.SetFloat("_Exposure", 1.1f);

            RenderSettings.skybox = skybox;
            return skybox;
        }

        private static void CreateTimeController(Transform parent, Light sun, Material skybox)
        {
            GameObject timeObject = new GameObject("TimeController");
            timeObject.transform.SetParent(parent);

            TimeController timeController = timeObject.AddComponent<TimeController>();
            timeController.sunLight = sun;
            timeController.skyboxMaterial = skybox;
            timeController.startHour = 12f;
            timeController.timeMultiplier = 1200f;
        }

        private static void CreateWeatherManager(Transform parent)
        {
            GameObject weatherObject = new GameObject("WeatherManager");
            weatherObject.transform.SetParent(parent);

            WeatherManager weatherManager = weatherObject.AddComponent<WeatherManager>();
            weatherManager.currentWeather = WeatherState.Sunny;
            weatherManager.enableRandomWeather = true;
            weatherManager.weatherChangeInterval = 30f;
        }

        private static void CreateClouds(Transform parent)
        {
            GameObject cloudRoot = new GameObject("Clouds_ExtraExercise");
            cloudRoot.transform.SetParent(parent);

            Material cloudMaterial = CreateMaterial(
                "M_Cloud_Runtime",
                new Color(0.95f, 0.97f, 1f, 1f)
            );

            for (int i = 0; i < 7; i++)
            {
                GameObject cloud = new GameObject("Cloud_" + (i + 1));
                cloud.transform.SetParent(cloudRoot.transform);
                cloud.transform.position = new Vector3(
                    Random.Range(-65f, 65f),
                    Random.Range(23f, 29f),
                    Random.Range(-55f, 55f)
                );

                CloudDrift drift = cloud.AddComponent<CloudDrift>();
                drift.direction = new Vector3(1f, 0f, 0.2f + Random.Range(-0.15f, 0.15f));
                drift.speed = Random.Range(1.2f, 2.1f);
                drift.wrapExtent = 75f;

                for (int part = 0; part < 4; part++)
                {
                    GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    puff.name = "Puff_" + part;
                    puff.transform.SetParent(cloud.transform);
                    puff.transform.localPosition = new Vector3(
                        (part - 1.5f) * 2.1f + Random.Range(-0.5f, 0.5f),
                        Random.Range(-0.3f, 0.6f),
                        Random.Range(-0.7f, 0.7f)
                    );
                    puff.transform.localScale = new Vector3(
                        Random.Range(3.2f, 5.0f),
                        Random.Range(1.5f, 2.3f),
                        Random.Range(2.2f, 3.8f)
                    );

                    Collider puffCollider = puff.GetComponent<Collider>();
                    if (puffCollider != null)
                        puffCollider.enabled = false;

                    puff.GetComponent<Renderer>().material = cloudMaterial;
                }
            }
        }
    }
}
