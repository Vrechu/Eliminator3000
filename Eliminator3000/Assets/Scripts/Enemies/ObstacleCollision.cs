using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private int damage = 1;
    [SerializeField] private int scoreLoss = 30;
    private bool hasHit = false;

    private void OnCollisionEnter(Collision _collision)
    {
        if (hasHit) return;
        if (((1 << _collision.gameObject.layer) & targetLayers) != 0)
        {
            hasHit = true;
            if (_collision.gameObject.tag == "Player1")
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(0, damage, scoreLoss));
            }

            if (_collision.gameObject.tag == "Player2")
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, damage, scoreLoss));
            }
            Destroy(gameObject);
            
        }        
    }
}
