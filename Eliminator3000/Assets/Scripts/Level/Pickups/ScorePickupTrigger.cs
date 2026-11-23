using UnityEngine;

public class ScorePickupTrigger : MonoBehaviour
{
    [SerializeField]
    private int scoreValue = 10;
    [SerializeField] private LayerMask targetLayers;
    private bool isPickedUp = false;

    private void OnTriggerEnter(Collider _other)
    {
        if (isPickedUp) return;
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            isPickedUp = true;
            if (_other.CompareTag("Player1"))
            {
                EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(0, scoreValue));
            }
            else if (_other.CompareTag("Player2"))
            {
                EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(1, scoreValue));
            }
            Destroy(gameObject);
        }
    }
}
