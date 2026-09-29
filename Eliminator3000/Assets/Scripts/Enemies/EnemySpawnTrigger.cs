using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private int waveNumber;
    [SerializeField] private LayerMask targetLayers;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            EventBus<EnemySpawnTriggeredEvent>.Publish(new EnemySpawnTriggeredEvent(waveNumber));
        }
    }
}


