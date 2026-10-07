using UnityEngine;

public class HealthPickupTrigger : MonoBehaviour
{
    [SerializeField] int healthAmount = 50;
    [SerializeField] LayerMask targetLayers;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            if (_other.CompareTag("Player1"))
            {
                EventBus<HealthPickupEvent>.Publish(new HealthPickupEvent(0, healthAmount));
            }
            else if (_other.CompareTag("Player2"))
            {
                EventBus<HealthPickupEvent>.Publish(new HealthPickupEvent(1, healthAmount));
            }
            Destroy(gameObject);
        }
    }

}
