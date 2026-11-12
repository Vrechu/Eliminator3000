using UnityEngine;

public class FogTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float fogIncreaseSpeed = 0.01f;
    [SerializeField] private float fogDecreaseSpeed = 0.01f;
    [SerializeField] private float fogDuration = 5f;
    private float startFogDensity;
    [SerializeField, Range(0f, 1f)] private float triggeredFogDensity = 0.15f;
    private bool triggered = false;
    private bool timerFinished= false;
    private Timer fogTimer;

    private void Start()
    {
        if (!IsFogOn()) return;
        startFogDensity = RenderSettings.fogDensity;
        fogTimer = new Timer(fogDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsFogOn()) return;
        if (targetLayers == (targetLayers | (1 << other.gameObject.layer)))
        {
            triggered = true;
        }
    }

    private void Update()
    {
        if (!triggered) return;

        timerFinished = fogTimer.IsFinished();

        if (!timerFinished)
        {
            if (RenderSettings.fogDensity < triggeredFogDensity) return;

            RenderSettings.fogDensity += fogIncreaseSpeed * Time.deltaTime;
            if (RenderSettings.fogDensity >= triggeredFogDensity)
            {
                RenderSettings.fogDensity = triggeredFogDensity;
            }
        }

        else
        {
            if (RenderSettings.fogDensity > startFogDensity) return;

            RenderSettings.fogDensity -= fogDecreaseSpeed * Time.deltaTime;
            if (RenderSettings.fogDensity <= startFogDensity)
            {
                RenderSettings.fogDensity = startFogDensity;
            }
        }
    }

    private bool IsFogOn()
    {
        if (!RenderSettings.fog) return false;
        if (RenderSettings.fogMode != FogMode.Exponential) return false;
        return true;
    }
}
