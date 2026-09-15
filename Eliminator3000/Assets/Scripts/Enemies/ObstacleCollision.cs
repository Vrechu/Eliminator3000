using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    private LivesManager livesManager;
    private void Start()
    {
        livesManager = LivesManager.Instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player1")
        {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, 1));
            Destroy(gameObject);
        }

            if (collision.gameObject.tag == "Player2")
            {
                EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(2, 1));
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "BackPlane")
        {
            Destroy(gameObject);
        }
    }
}
