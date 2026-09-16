using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision _collision)
    {
        if (_collision.gameObject.tag == "Player1")
        {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, 1));
            Destroy(gameObject);
        }

            if (_collision.gameObject.tag == "Player2")
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(2, 1));
            Destroy(gameObject);
        }

        if (_collision.gameObject.tag == "BackPlane")
        {
            Destroy(gameObject);
        }
    }
}
