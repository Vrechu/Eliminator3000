using UnityEngine;

public class EnemyProjectileHit : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private int damage = 1;
    [SerializeField] private int scoreLoss = 20;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            if (_other.CompareTag("Player1"))
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(0, damage, scoreLoss));
            }
            if (_other.CompareTag("Player2"))
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, damage, scoreLoss));
            }
            Destroy(gameObject);
        }
    }
}
