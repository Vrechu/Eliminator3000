using UnityEngine;

public class EnemyProjectileHit : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            if (_other.CompareTag("Player1"))
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, 1));
            }
            if (_other.CompareTag("Player2"))
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(2, 1));
            }
            Destroy(gameObject);
        }
    }
}
