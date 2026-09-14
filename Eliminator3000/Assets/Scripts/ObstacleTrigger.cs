using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
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
            Debug.Log("Player 1 hit an obstacle!");
            livesManager.LoseLife(1, 1);
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "Player2")
        {
            Debug.Log("Player 2 hit an obstacle!");
            livesManager.LoseLife(2, 1);
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "BackPlane")
        {
            Destroy(gameObject);
        }
    }
}
