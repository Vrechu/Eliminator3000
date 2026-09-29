using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    private void OnCollisionEnter(Collision _collision)
    {
        if (((1 << _collision.gameObject.layer) & targetLayers) != 0)
        {
            if (_collision.gameObject.tag == "Player1")
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, 1));
            }

            if (_collision.gameObject.tag == "Player2")
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(2, 1));
            }
                Destroy(gameObject);
            
        }        
    }
}
