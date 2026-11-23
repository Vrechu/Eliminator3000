using UnityEngine;

public class DummyPickupTrigger : MonoBehaviour
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
                EventBus<PlayerDummyPickupEvent>.Publish(new PlayerDummyPickupEvent(0));
            if (other.CompareTag("Player2"))
                EventBus<PlayerDummyPickupEvent>.Publish(new PlayerDummyPickupEvent(1));

            Destroy(gameObject);
        }
    }
}
