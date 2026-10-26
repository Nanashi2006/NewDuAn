using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PS46218
{
    // Cau 2: Vignette lam toi vien man hinh, de so sanh khi bat/tat.
    [DefaultExecutionOrder(-50)]
    public sealed class HauKy : MonoBehaviour
    {
        public Camera cameraChinh;
        private Volume volume;
        private VolumeProfile profile;

        public bool DangBat => volume != null && volume.weight > 0f;

        private void Awake()
        {
            volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.42f);
            vignette.smoothness.Override(0.55f);
            volume.sharedProfile = profile;

            var cameraData = cameraChinh.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.volumeLayerMask = 1; // Global Volume nam tren layer Default.
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        }

        public void BatTat()
        {
            volume.weight = DangBat ? 0f : 1f;
        }

        private void OnDestroy()
        {
            if (profile == null) return;
            foreach (VolumeComponent effect in profile.components)
                if (effect != null) Destroy(effect);
            Destroy(profile);
        }
    }
}
