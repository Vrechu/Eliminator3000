using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ouch!");
            LivesManager.Instance.LoseLife(1);

        }

        if (other.CompareTag("BackPlane"))
        {
            Destroy(gameObject);
        }
    }
}
