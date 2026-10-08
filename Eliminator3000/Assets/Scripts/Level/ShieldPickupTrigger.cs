using UnityEngine;

public class ShieldPickupTrigger : MonoBehaviour
{
    [SerializeField] private int shieldLayers = 1;
    [SerializeField] private LayerMask targetLayers;


    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            if (other.CompareTag("Player1"))
                EventBus<PlayerShieldPickupEvent>.Publish(new PlayerShieldPickupEvent(0, shieldLayers));
            if (other.CompareTag("Player2"))
                EventBus<PlayerShieldPickupEvent>.Publish(new PlayerShieldPickupEvent(1, shieldLayers));

            Destroy(gameObject);
        }
    }
}
