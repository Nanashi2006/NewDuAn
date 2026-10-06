using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    public class NightLantern : MonoBehaviour
    {
        public Light lamp;
        public TimeController clock;
        public float brightness = 2.2f;
        private void Update()
        {
            if (lamp == null || clock == null) return;
            float hour = (float)clock.CurrentTime.TimeOfDay.TotalHours;
            float night = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Min(hour - 5.5f, 18.5f - hour));
            lamp.intensity = brightness * night;
        }
    }
}
