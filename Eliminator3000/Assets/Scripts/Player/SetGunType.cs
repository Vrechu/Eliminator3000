using System;
using Unity.VisualScripting;
using UnityEngine;

public class SetGunType : MonoBehaviour
{
    [SerializeField]
    private int playerID;
    [SerializeField]
    private GameObject[] gunModels;
    [SerializeField]
    private GunProfile[] guns;
    private int currentGunIndex = 0;


    private void OnEnable()
    {
        if (playerID == 0)
        {
            Debug.LogError("Player is not assigned in SetGunType script.");
            return;
        }

        EventBus<PlayerGunPickupEvent>.Subscribe(OnGunPickup);
    }

    private void OnDisable()
    {
        EventBus<PlayerGunPickupEvent>.UnSubscribe(OnGunPickup);
    }

    private void Start()
    {
        DisableModels();
        gunModels[0].SetActive(true); // Activate the base gun model by default
        GunsViable();
        MarkProjectiles();
    }

    private void OnGunPickup(PlayerGunPickupEvent _playerGunPickupEvent)
    {
        if (_playerGunPickupEvent.Player == playerID)
        {
            currentGunIndex = _playerGunPickupEvent.GunIndex;
            DisableModels();
            gunModels[_playerGunPickupEvent.GunIndex].SetActive(true);
        }
    }

    private void DisableModels()
    {
        for (int i = 0; i < gunModels.Length; i++)
        {
            gunModels[i].SetActive(false);
        }
    }

    private bool GunsViable()
    {
        if (gunModels.Length != guns.Length)
        {
            Debug.LogError("Gun models and gun profiles arrays must have the same length.");
            return false;
        }
        if (gunModels.Length == 0 || guns.Length == 0)
        {
            Debug.LogError("Gun models and gun profiles arrays cannot be empty.");
            return false;
        }
        return true;
    }

    public GunProfile GetCurrentGunProfile()
    {
        if (currentGunIndex < 0 || currentGunIndex >= guns.Length)
        {
            Debug.LogError("Current gun index is out of bounds.");
            return null;
        }
        return guns[currentGunIndex];
    }

    private void MarkProjectiles()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i].ProjectilePrefab.TryGetComponent<PlayerProjectileHit>(out PlayerProjectileHit playerProjectileHit))
            {
                playerProjectileHit.PlayerID = playerID;
            }
            else
            {
                Debug.LogWarning($"Gun at index {i} does not have a PlayerProjectileHit component.");
            }
        }
    } 
}
