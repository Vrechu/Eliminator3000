using Unity.VisualScripting;
using UnityEngine;

public class ScorePickupTrigger : MonoBehaviour
{
    [SerializeField]
    private int scoreValue = 10;
    [SerializeField] private LayerMask targetLayers;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            if (_other.CompareTag("Player1"))
            {
                EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(1, scoreValue));
            }
            else if (_other.CompareTag("Player2"))
            {
                EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(2, scoreValue));
            }
            Destroy(gameObject);
        }
    }
}
