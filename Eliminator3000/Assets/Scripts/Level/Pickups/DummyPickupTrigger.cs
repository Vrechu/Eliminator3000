using UnityEngine;

public class DummyPickupTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;


    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            if (other.CompareTag("Player1"))
                EventBus<PlayerDummyPickupEvent>.Publish(new PlayerDummyPickupEvent(0));
            if (other.CompareTag("Player2"))
                EventBus<PlayerDummyPickupEvent>.Publish(new PlayerDummyPickupEvent(1));

            Destroy(gameObject);
        }
    }
}
