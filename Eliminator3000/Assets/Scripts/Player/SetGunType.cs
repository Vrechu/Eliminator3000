using System;
using Unity.VisualScripting;
using UnityEngine;

public class SetGunType : MonoBehaviour
{
    [SerializeField]
    private int player;
    [SerializeField]
    private GameObject baseModel, gunModel;

    public enum GunType
    {
        Base,
        Gun
    }
    public GunType gunType = GunType.Base;

    private void OnEnable()
    {
        if (player == 0)
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
        baseModel.SetActive(true);
    }

    private void OnGunPickup(PlayerGunPickupEvent _playerGunPickupEvent)
    {
        if (_playerGunPickupEvent.Player == player)
        {
            gunType = GunType.Gun;
            baseModel.SetActive(false);
            gunModel.SetActive(true);
        }
    }

    private void DisableModels()
    {
        baseModel.SetActive(false);
        gunModel.SetActive(false);
    }
}
