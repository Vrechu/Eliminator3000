using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    private bool triggered = false;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)) && !triggered)
        {
            triggered = true;
            EventBus<LevelFinishedEvent>.Publish(new LevelFinishedEvent());
        }
    }    
}
