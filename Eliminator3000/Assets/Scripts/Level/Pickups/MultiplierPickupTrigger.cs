using UnityEngine;

public class MultiplierPickupTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private int multiplierValue = 2;
    [SerializeField] private float duration = 10f;
    private bool isPickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            isPickedUp = true;
            if (other.CompareTag("Player1"))
                EventBus<PlayerScoreMultiplierPickupEvent>.Publish(new PlayerScoreMultiplierPickupEvent(0, multiplierValue, duration));
            if (other.CompareTag("Player2"))
                EventBus<PlayerScoreMultiplierPickupEvent>.Publish(new PlayerScoreMultiplierPickupEvent(1, multiplierValue, duration));

            Destroy(gameObject);
        }
    }
}
