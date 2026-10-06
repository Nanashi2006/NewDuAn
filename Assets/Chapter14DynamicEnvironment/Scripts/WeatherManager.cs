using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Chapter14DynamicEnvironment
{
    public enum WeatherState
    {
        Sunny,
        Raining,
        Foggy
    }

    public class WeatherManager : MonoBehaviour
    {
        [Header("Cấu hình thời tiết")]
        public WeatherState currentWeather = WeatherState.Sunny;
        public bool enableRandomWeather = true;

        [Header("Hiệu ứng")]
        public GameObject rainParticlePrefab;

        [Header("Thời gian")]
        public float weatherChangeInterval = 30f;

        private GameObject activeRain;
        private Transform playerTransform;

        private void Start()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;

            ApplyWeather(currentWeather);

            if (enableRandomWeather)
                StartCoroutine(WeatherRoutine());
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }

            if (activeRain != null && playerTransform != null)
            {
                activeRain.transform.position = new Vector3(
                    playerTransform.position.x,
                    playerTransform.position.y + 12f,
                    playerTransform.position.z
                );
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.digit1Key.wasPressedThisFrame)
                SetWeather(WeatherState.Sunny);
            else if (keyboard.digit2Key.wasPressedThisFrame)
                SetWeather(WeatherState.Raining);
            else if (keyboard.digit3Key.wasPressedThisFrame)
                SetWeather(WeatherState.Foggy);
        }

        private IEnumerator WeatherRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(weatherChangeInterval);

                if (enableRandomWeather)
                    ChangeWeatherRandomly();
            }
        }

        private void ChangeWeatherRandomly()
        {
            int randomState = Random.Range(0, 3);
            SetWeather((WeatherState)randomState);
        }

        public void SetWeather(WeatherState state)
        {
            currentWeather = state;
            ApplyWeather(state);
        }

        private void ApplyWeather(WeatherState state)
        {
            if (activeRain != null)
            {
                Destroy(activeRain);
                activeRain = null;
            }

            switch (state)
            {
                case WeatherState.Sunny:
                    RenderSettings.fogDensity = 0.002f;
                    RenderSettings.fogColor = new Color(0.65f, 0.78f, 0.92f);
                    break;

                case WeatherState.Raining:
                    RenderSettings.fogDensity = 0.01f;
                    RenderSettings.fogColor = new Color(0.42f, 0.49f, 0.56f);
                    SpawnRain();
                    break;

                case WeatherState.Foggy:
                    RenderSettings.fogDensity = 0.03f;
                    RenderSettings.fogColor = new Color(0.68f, 0.71f, 0.72f);
                    break;
            }
        }

        private void SpawnRain()
        {
            if (playerTransform == null)
                return;

            Vector3 spawnPosition = playerTransform.position + Vector3.up * 12f;

            if (rainParticlePrefab != null)
            {
                activeRain = Instantiate(rainParticlePrefab, spawnPosition, Quaternion.identity);
                return;
            }

            activeRain = CreateRuntimeRain(spawnPosition);
        }

        private static GameObject CreateRuntimeRain(Vector3 position)
        {
            GameObject rain = new GameObject("Rain_VFX_Runtime");
            rain.transform.position = position;

            ParticleSystem particleSystem = rain.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.055f);
            main.startColor = new Color(0.72f, 0.85f, 1f, 0.8f);
            main.maxParticles = 5000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 1000f;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(28f, 1f, 28f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(-22f);

            ParticleSystemRenderer particleRenderer = rain.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            particleRenderer.lengthScale = 7f;
            particleRenderer.velocityScale = 0.35f;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                Material rainMaterial = new Material(shader);
                Color rainColor = new Color(0.72f, 0.85f, 1f, 0.65f);

                if (rainMaterial.HasProperty("_BaseColor"))
                    rainMaterial.SetColor("_BaseColor", rainColor);

                if (rainMaterial.HasProperty("_Surface"))
                    rainMaterial.SetFloat("_Surface", 1f);

                if (rainMaterial.HasProperty("_SrcBlend"))
                    rainMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);

                if (rainMaterial.HasProperty("_DstBlend"))
                    rainMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

                if (rainMaterial.HasProperty("_ZWrite"))
                    rainMaterial.SetFloat("_ZWrite", 0f);

                rainMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                rainMaterial.renderQueue = (int)RenderQueue.Transparent;
                particleRenderer.material = rainMaterial;
            }

            particleSystem.Play();
            return rain;
        }

        private void OnGUI()
        {
            GUI.Box(
                new Rect(Screen.width - 180f, 12f, 168f, 30f),
                "Weather: " + currentWeather
            );
        }
    }
}
