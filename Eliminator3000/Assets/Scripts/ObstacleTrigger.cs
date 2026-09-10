using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private LivesManager livesManager;
    private void Start()
    {
        livesManager = LivesManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            Debug.Log("Player 1 hit an obstacle!");
            livesManager.LoseLife(1,1);
        }

        if (other.CompareTag("Player2"))
        {
            Debug.Log("Player 2 hit an obstacle!");
            livesManager.LoseLife(2,1);
        }

        if (other.CompareTag("BackPlane"))
        {
            Destroy(gameObject);
        }
    }
}
