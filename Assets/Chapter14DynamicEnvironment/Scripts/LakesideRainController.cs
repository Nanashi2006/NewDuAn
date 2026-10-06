using UnityEngine;

public class LakesideRainController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rain;
    [SerializeField] private AudioSource rainAudio;
    [SerializeField] private Light directionalLight;
    [SerializeField] private float clearIntensity = 1.1f;
    [SerializeField] private float rainyIntensity = 0.45f;
    [SerializeField] private KeyCode toggleKey = KeyCode.R;

    private bool raining;

    private void Start()
    {
        SetRain(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetRain(!raining);
    }

    public void SetRain(bool enabled)
    {
        raining = enabled;

        if (rain != null)
        {
            if (enabled && !rain.isPlaying) rain.Play();
            if (!enabled && rain.isPlaying) rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (rainAudio != null)
        {
            if (enabled && !rainAudio.isPlaying) rainAudio.Play();
            if (!enabled && rainAudio.isPlaying) rainAudio.Stop();
        }

        if (directionalLight != null)
            directionalLight.intensity = enabled ? rainyIntensity : clearIntensity;

        RenderSettings.fog = enabled;
        if (enabled)
        {
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.ambientIntensity = 0.65f;
        }
        else
        {
            RenderSettings.fogDensity = 0.002f;
            RenderSettings.ambientIntensity = 1f;
        }
    }
}
