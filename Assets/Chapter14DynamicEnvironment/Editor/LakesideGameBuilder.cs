#if UNITY_EDITOR
using System.IO;
using Chapter14DynamicEnvironment;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// The rebuilt village is committed as a complete scene. This menu opens it without regenerating buildings.
public static class LakesideGameBuilder
{
    public const string ScenePath = "Assets/Scenes/LakesideVillage_Rebuilt.unity";

    [MenuItem("Tools/Chapter 14/Build Lakeside Village Game")]
    public static void BuildGame()
    {
        if (Application.isPlaying) { Debug.LogWarning("Stop Play Mode before opening the village."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!File.Exists(ScenePath))
        {
            Debug.LogError("The rebuilt scene is missing. Pull the full fix branch, including Assets/Scenes/LakesideVillage_Rebuilt.unity.");
            return;
        }
        EditorSceneManager.OpenScene(ScenePath);
        Chapter14DynamicEnvironment.Editor.LakesideSceneTools.FrameVillage();
        ValidateScene();
    }

    [MenuItem("Tools/Chapter 14/Validate Lakeside Village")]
    public static void ValidateScene()
    {
        LakesideSceneRuntime scene = Object.FindAnyObjectByType<LakesideSceneRuntime>();
        if (scene == null) { Debug.LogError("Open LakesideVillage_Rebuilt before validating."); return; }
        int failures = 0;
        foreach (LakesideAnimationDriver driver in scene.GetComponentsInChildren<LakesideAnimationDriver>(true))
        {
            Animator animator = driver.animator;
            if (animator == null || animator.runtimeAnimatorController == null)
            { Debug.LogError("Missing asset Animator on " + driver.name, driver); failures++; continue; }
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller == null || controller.layers.Length == 0)
            { Debug.LogError("Missing gameplay controller on " + driver.name, driver); failures++; continue; }
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (string name in new[] { driver.idleState, driver.walkState, driver.runState, driver.attackState, driver.dieState })
            {
                bool found = false;
                foreach (ChildAnimatorState state in machine.states) if (state.state.name == name) found = true;
                if (!found) { Debug.LogError("Missing animation state " + name + " on " + driver.name, driver); failures++; }
            }
        }
        foreach (Renderer renderer in scene.GetComponentsInChildren<Renderer>(true))
            foreach (Material material in renderer.sharedMaterials)
                if (material == null || material.shader == null || !material.shader.isSupported || material.shader.name == "Hidden/InternalErrorShader")
                { Debug.LogError("Missing or unsupported material on " + renderer.name, renderer); failures++; }
        if (scene.GetComponentsInChildren<PlayerController>(true).Length != 1)
        { Debug.LogError("Expected exactly one PlayerController."); failures++; }
        if (scene.GetComponentsInChildren<LakesideAnimationDriver>(true).Length != 4)
        { Debug.LogError("Expected DogKnight plus three animated Goblins."); failures++; }
        Debug.Log(failures == 0 ? "Lakeside references checked. Test movement, F5, five attacks, water, R/1/2/3 and day/night in Play Mode." : "Lakeside validation found " + failures + " problems; check the Console.");
    }
}
#endif
