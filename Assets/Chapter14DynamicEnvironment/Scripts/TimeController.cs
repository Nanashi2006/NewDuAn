using System;
using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    public class TimeController : MonoBehaviour
    {
        [Header("Thời gian chung")]
        public float timeMultiplier = 1200f;
        [Range(0f, 24f)]
        public float startHour = 12f;

        [Header("Ánh sáng Mặt trời")]
        public Light sunLight;
        [Range(0f, 24f)]
        public float sunriseHour = 6f;
        [Range(0f, 24f)]
        public float sunsetHour = 18f;

        [Header("URP Cảnh quan")]
        public AnimationCurve lightIntensityCurve;
        public Gradient skyColor;
        public Material skyboxMaterial;

        public DateTime CurrentTime => currentTime;

        private WeatherManager weather;
        private DateTime currentTime;
        private TimeSpan sunriseTime;
        private TimeSpan sunsetTime;

        private void Start()
        {
            weather = FindAnyObjectByType<WeatherManager>();
            currentTime = DateTime.Today + TimeSpan.FromHours(startHour);
            sunriseTime = TimeSpan.FromHours(sunriseHour);
            sunsetTime = TimeSpan.FromHours(sunsetHour);

            EnsureDefaults();
            UpdateLighting();
            RotateSun();
        }

        private void Update()
        {
            currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);
            RotateSun();
            UpdateLighting();
        }

        private void EnsureDefaults()
        {
            if (lightIntensityCurve == null || lightIntensityCurve.length == 0)
            {
                lightIntensityCurve = new AnimationCurve(
                    new Keyframe(0f, 0.03f),
                    new Keyframe(0.23f, 0.03f),
                    new Keyframe(0.30f, 0.75f),
                    new Keyframe(0.50f, 1.25f),
                    new Keyframe(0.72f, 0.80f),
                    new Keyframe(0.79f, 0.03f),
                    new Keyframe(1f, 0.03f)
                );
            }

            if (skyColor == null)
            {
                skyColor = new Gradient();
            }

            GradientColorKey[] colorKeys =
            {
                new GradientColorKey(new Color(0.025f, 0.035f, 0.08f), 0f),
                new GradientColorKey(new Color(0.12f, 0.10f, 0.20f), 0.20f),
                new GradientColorKey(new Color(0.95f, 0.34f, 0.12f), 0.25f),
                new GradientColorKey(new Color(0.26f, 0.55f, 0.95f), 0.40f),
                new GradientColorKey(new Color(0.26f, 0.55f, 0.95f), 0.62f),
                new GradientColorKey(new Color(0.95f, 0.28f, 0.10f), 0.75f),
                new GradientColorKey(new Color(0.025f, 0.035f, 0.08f), 0.84f),
                new GradientColorKey(new Color(0.025f, 0.035f, 0.08f), 1f)
            };

            GradientAlphaKey[] alphaKeys =
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            skyColor.SetKeys(colorKeys, alphaKeys);
        }

        private static TimeSpan TimeDifference(TimeSpan fromTime, TimeSpan toTime)
        {
            TimeSpan difference = toTime - fromTime;
            if (difference.TotalSeconds < 0)
                difference += TimeSpan.FromHours(24);
            return difference;
        }

        private void RotateSun()
        {
            if (sunLight == null)
                return;

            float rotation;

            if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
            {
                TimeSpan riseToSet = TimeDifference(sunriseTime, sunsetTime);
                TimeSpan timeSinceSunrise = TimeDifference(sunriseTime, currentTime.TimeOfDay);
                double percent = timeSinceSunrise.TotalMinutes / riseToSet.TotalMinutes;
                rotation = Mathf.Lerp(0f, 180f, (float)percent);
            }
            else
            {
                TimeSpan setToRise = TimeDifference(sunsetTime, sunriseTime);
                TimeSpan timeSinceSunset = TimeDifference(sunsetTime, currentTime.TimeOfDay);
                double percent = timeSinceSunset.TotalMinutes / setToRise.TotalMinutes;
                rotation = Mathf.Lerp(180f, 360f, (float)percent);
            }

            sunLight.transform.rotation = Quaternion.Euler(rotation, 170f, 0f);
        }

        private void UpdateLighting()
        {
            float timePercent = (float)currentTime.TimeOfDay.TotalHours / 24f;
            Color evaluatedSky = skyColor.Evaluate(timePercent);

            if (sunLight != null)
            {
                sunLight.intensity = lightIntensityCurve.Evaluate(timePercent) * (weather != null ? weather.LightMultiplier : 1f);
                sunLight.color = Color.Lerp(
                    new Color(1f, 0.55f, 0.35f),
                    Color.white,
                    Mathf.Clamp01(sunLight.intensity)
                );
            }

            RenderSettings.ambientLight = Color.Lerp(
                evaluatedSky * 0.35f,
                evaluatedSky,
                0.7f
            );

            if (skyboxMaterial != null)
            {
                if (skyboxMaterial.HasProperty("_SkyTint"))
                    skyboxMaterial.SetColor("_SkyTint", evaluatedSky);

                if (skyboxMaterial.HasProperty("_Tint"))
                    skyboxMaterial.SetColor("_Tint", evaluatedSky);

                if (skyboxMaterial.HasProperty("_Exposure"))
                    skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(0.2f, 1.25f, lightIntensityCurve.Evaluate(timePercent)));
            }
        }

    }
}
