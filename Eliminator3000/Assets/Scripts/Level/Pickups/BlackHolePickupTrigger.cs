using UnityEngine;

public class BlackHolePickupTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    private bool isPickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            isPickedUp = true;
            if (other.CompareTag("Player1"))
                EventBus<PlayerBlackHolePickupEvent>.Publish(new PlayerBlackHolePickupEvent(0));
            if (other.CompareTag("Player2"))
                EventBus<PlayerBlackHolePickupEvent>.Publish(new PlayerBlackHolePickupEvent(1));

            Destroy(gameObject);
        }
    }
}
