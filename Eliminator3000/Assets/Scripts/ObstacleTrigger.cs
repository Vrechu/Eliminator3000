using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LivesManager.Instance.LoseLife(1);

            Debug.Log("Player has entered the obstacle trigger!");
        }

        if (other.CompareTag("BackPlane"))
        {
            Destroy(gameObject);
        }
    }
}
