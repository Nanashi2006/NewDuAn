using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    [RequireComponent(typeof(Renderer))]
    public class WaterScroll : MonoBehaviour
    {
        [Header("Tốc độ dòng chảy")]
        public float scrollSpeedX = 0.08f;
        public float scrollSpeedY = 0.035f;

        private Renderer cachedRenderer;
        private Vector2 offset;

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (cachedRenderer == null)
                return;

            offset.x = Mathf.Repeat(offset.x + Time.deltaTime * scrollSpeedX, 1f);
            offset.y = Mathf.Repeat(offset.y + Time.deltaTime * scrollSpeedY, 1f);

            Material material = cachedRenderer.material;

            if (material.HasProperty("_BaseMap"))
                material.SetTextureOffset("_BaseMap", offset);

            if (material.HasProperty("_MainTex"))
                material.SetTextureOffset("_MainTex", offset);

            if (material.HasProperty("_BumpMap"))
                material.SetTextureOffset("_BumpMap", offset);
        }
    }
}
