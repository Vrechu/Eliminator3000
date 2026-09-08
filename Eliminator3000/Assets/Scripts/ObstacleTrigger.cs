using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the obstacle trigger!");
        }

        if (other.CompareTag("BackPlane"))
        {
            Destroy(gameObject);
        }
    }
}
