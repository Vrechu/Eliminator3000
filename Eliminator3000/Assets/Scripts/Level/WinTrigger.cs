using UnityEngine;
using UnityEngine.Events;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            EventBus<FinishedLevelEvent>.Publish(new FinishedLevelEvent());
        }
    }    
}
