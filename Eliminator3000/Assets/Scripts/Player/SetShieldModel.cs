using UnityEngine;

public class SetShieldModel : MonoBehaviour
{
    [SerializeField]GameObject shieldModel;
    private int playerID;

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
        SetPlayerID();
        shieldModel.SetActive(false);
    }
    private void SetPlayerID()
    {
        if (TryGetComponent<InGameAvatarInfo>(out InGameAvatarInfo _info))
        {
            playerID = _info.PlayerID;
            if (playerID < 0 || playerID > 1) Debug.LogError("Invalid PLayerID set on: " + gameObject.name);
        }
        else Debug.LogError("No InGameAvatarInfo component found. Every Playerprefab should have one!");
    }


    private void OnShieldPickup(PlayerShieldPickupEvent _playerShieldPickup)
    {
        if (_playerShieldPickup.Player == playerID)
        {
            EnableShield();
        }
    }

    private void OnShieldDrop(PlayerShieldDropEvent _playerShieldDrop)
    {
        if (_playerShieldDrop.Player == playerID)
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
