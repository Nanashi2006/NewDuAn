using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PS46218.Editor
{
    // Tu dong cau hinh URP cho project moi; khong lay du lieu tu project cu.
    public sealed class CaiDatProject : IPreprocessBuildWithReport
    {
        public const string ScenePath = "Assets/Scenes/PS46218.unity";
        private const string PipelinePath = "Assets/Settings/PS46218_URP.asset";
        public int callbackOrder => 0;

        [InitializeOnLoadMethod]
        private static void KhiMoUnity()
        {
            EditorApplication.delayCall += TuDongCaiDat;
        }

        private static void TuDongCaiDat()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TuDongCaiDat;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(PipelinePath)) return;
            CauHinh();
            if (!Application.isBatchMode && !SceneManager.GetActiveScene().isDirty)
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("PS46218/Mo bai lam")]
        public static void MoBaiLam()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CauHinh();
            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void CauHinh()
        {
            Directory.CreateDirectory("Assets/Settings");
            AssetDatabase.Refresh();
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
                // API nay tao renderer kem PostProcessData va nap cac shader cua URP.
                pipeline.LoadBuiltinRendererData();
                AssetDatabase.MoveAsset("Assets/UniversalRenderer.asset", "Assets/Settings/PS46218_Renderer.asset");
                pipeline.msaaSampleCount = 2;
                pipeline.shadowDistance = 80f;
                pipeline.supportsHDR = true;
                EditorUtility.SetDirty(pipeline);
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.companyName = "PS46218";
            PlayerSettings.productName = "PS46218";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        public void OnPreprocessBuild(BuildReport report) => CauHinh();
    }
}
