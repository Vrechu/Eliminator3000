using UnityEngine;

public class GunPickupTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private int gunIndex;
    private bool isPickedUp = false;
    private void OnTriggerEnter(Collider _other)
    {
        if (isPickedUp) return;
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            isPickedUp = true;
            if (_other.CompareTag("Player1"))
            {
                EventBus<PlayerGunPickupEvent>.Publish(new PlayerGunPickupEvent(0, gunIndex));
            }
            else if (_other.CompareTag("Player2"))
            {
                EventBus<PlayerGunPickupEvent>.Publish(new PlayerGunPickupEvent(1, gunIndex));
            }
            Destroy(gameObject);
        }
    }
}
