using UnityEngine;

public class SetShieldModel : MonoBehaviour
{
    [SerializeField]GameObject shieldModel;
    [SerializeField] private int playerIndex;

    private void OnEnable()
    {
        EventBus<PlayerShieldPickupEvent>.Subscribe(OnShieldPickup);
        EventBus<PlayerShieldDropEvent>.Subscribe(OnShieldDrop);
    }

    private void OnDestroy()
    {
        EventBus<PlayerShieldPickupEvent>.UnSubscribe(OnShieldPickup);
        EventBus<PlayerShieldDropEvent>.UnSubscribe(OnShieldDrop);
    }

    private void Start()
    {
        shieldModel.SetActive(false);
    }


    private void OnShieldPickup(PlayerShieldPickupEvent _playerShieldPickup)
    {
        if (_playerShieldPickup.Player == playerIndex)
        {
            EnableShield();
        }
    }

    private void OnShieldDrop(PlayerShieldDropEvent _playerShieldDrop)
    {
        if (_playerShieldDrop.Player == playerIndex)
        {
            DisableShield();
        }
    }

    private void EnableShield()
    {
        shieldModel.SetActive(true);
    }

    private void DisableShield()
    {
        shieldModel.SetActive(false);
    }
}
