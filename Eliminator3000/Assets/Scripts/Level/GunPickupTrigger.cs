using UnityEngine;

public class GunPickupTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Player1"))
        {
            EventBus<PlayerGunPickupEvent>.Publish(new PlayerGunPickupEvent(1));
            Destroy(gameObject);
        }
        else if (_other.CompareTag("Player2"))
        {
            EventBus<PlayerGunPickupEvent>.Publish(new PlayerGunPickupEvent(2));
            Destroy(gameObject);
        }
        else if (_other.CompareTag("BackPlane"))
        {
            Destroy(gameObject);
        }
    }
}
