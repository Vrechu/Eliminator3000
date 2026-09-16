using Unity.VisualScripting;
using UnityEngine;

public class ScorePickupTrigger : MonoBehaviour
{
    [SerializeField]
    private int scoreValue = 10;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Player1"))
        {
            EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(1, scoreValue));
            Destroy(gameObject);
        }
        else if (_other.CompareTag("Player2"))
        {
            EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(2, scoreValue));
            Destroy(gameObject);
        }
        else if (_other.CompareTag("BackPlane"))
        {
            Destroy(gameObject);
        }
    }
}
