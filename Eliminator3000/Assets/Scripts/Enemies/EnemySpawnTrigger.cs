using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private int waveNumber;
    [SerializeField] private LayerMask targetLayers;
    private bool triggered = false;

    private void OnTriggerEnter(Collider _other)
    {
        if (triggered) return;
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {            
            triggered = true;
            EventBus<EnemySpawnTriggeredEvent>.Publish(new EnemySpawnTriggeredEvent(waveNumber));
            Debug.Log("spawning: " + waveNumber);
        }
    }
}


