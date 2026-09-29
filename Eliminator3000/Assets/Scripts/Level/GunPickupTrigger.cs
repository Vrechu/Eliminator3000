using UnityEngine;

public class GunPickupTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            if (_other.CompareTag("Player1"))
            {
                EventBus<PlayerGunPickupEvent>.Publish(new PlayerGunPickupEvent(1));
            }
            else if (_other.CompareTag("Player2"))
            {
                EventBus<PlayerGunPickupEvent>.Publish(new PlayerGunPickupEvent(2));
            }
            Destroy(gameObject);
        }
    }
}
