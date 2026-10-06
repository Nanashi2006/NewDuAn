using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    [DefaultExecutionOrder(-100)]
    public class Chapter14DemoBootstrap : MonoBehaviour
    {
        [Header("Tự tạo map khi bấm Play")]
        public bool buildOnAwake = true;
        public int randomSeed = 14;

        private void Awake()
        {
            if (!Application.isPlaying || !buildOnAwake)
                return;

            BuildWorld();
        }

        [ContextMenu("Build World (Play Mode)")]
        public void BuildWorld()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Chapter14 demo chỉ dựng map ở Play Mode.");
                return;
            }

            if (GameObject.Find("Chapter14_World") != null)
                return;

            Chapter14WorldBuilder.Build(randomSeed);
        }
    }
}
