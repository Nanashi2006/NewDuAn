using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Chapter14DynamicEnvironment
{
    // The scenery is serialized in the scene. Only navigation and rendering setup run here.
    [DefaultExecutionOrder(-200)]
    public class LakesideSceneRuntime : MonoBehaviour
    {
        public GameObject environment;
        public Camera mainCamera;
        public Light sun;
        public TimeController clock;
        public Material skyMaterial;
        public GameObject lakeVolume;

        private Material runtimeSky;

        private void Awake()
        {
            if (skyMaterial != null)
            {
                runtimeSky = new Material(skyMaterial);
                RenderSettings.skybox = runtimeSky;
                if (clock != null) clock.skyboxMaterial = runtimeSky;
            }
            RenderSettings.sun = sun;
            if (mainCamera != null)
                mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            if (environment == null) return;
            NavMeshAgent[] agents = GetComponentsInChildren<NavMeshAgent>();
            foreach (NavMeshAgent agent in agents) agent.enabled = false;
            NavMeshSurface surface = environment.GetComponent<NavMeshSurface>();
            if (surface == null) surface = environment.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~(1 << 2);
            ConfigureLakeExclusion();
            if (surface.navMeshData == null) surface.BuildNavMesh();
            foreach (NavMeshAgent agent in agents)
            {
                if (NavMesh.SamplePosition(agent.transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                {
                    agent.transform.position = hit.position;
                    agent.enabled = true;
                    agent.Warp(hit.position);
                }
                else Debug.LogWarning($"No walkable NavMesh at {agent.name}. Move it onto the ground.", agent);
            }
            StaticBatchingUtility.Combine(environment);
        }

        public void ConfigureLakeExclusion()
        {
            if (lakeVolume == null) return;
            NavMeshModifierVolume volume = lakeVolume.GetComponent<NavMeshModifierVolume>();
            if (volume == null) volume = lakeVolume.AddComponent<NavMeshModifierVolume>();
            volume.area = 1; // Built-in Not Walkable area.
            volume.center = new Vector3(0f, -0.1f, 0f);
            volume.size = new Vector3(30f, 3f, 23.2f);
        }

        private void OnDestroy()
        {
            if (runtimeSky == null) return;
            if (RenderSettings.skybox == runtimeSky) RenderSettings.skybox = skyMaterial;
            Destroy(runtimeSky);
        }
    }
}
