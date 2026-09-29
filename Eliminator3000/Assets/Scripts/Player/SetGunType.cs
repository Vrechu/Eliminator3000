using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetGunType : MonoBehaviour
{
    [SerializeField] private int playerID;
    [SerializeField] private GameObject[] gunModels;
    [SerializeField] private GunProfile[] guns;
    private int currentGunIndex = 0;

    private Timer shootTimer = new Timer(0, false);
    public bool canShoot { get; private set; } = true;
    private GameStateManager gameStateManager;


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
        gameStateManager = GameStateManager.Instance;
        DisableModels();
        gunModels[0].SetActive(true); // Activate the base gun model by default
        GunsViable();
        MarkProjectiles();
        ResetGunUnlocks();
    }

    private void Update()
    {
        canShoot = shootTimer.IsFinished();
    }

    private void SetGun(int _gunIndex)
    {
        DisableModels();
        currentGunIndex = _gunIndex;
        gunModels[_gunIndex].SetActive(true);
        shootTimer.Reset(0);
        EventBus<PlayerGunSwapEvent>.Publish(new PlayerGunSwapEvent(playerID, _gunIndex));
    }

    public void GetSwapInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState
            != GameStateManager.GameState.Ingame) return;
        if (!_context.performed) return;
        Debug.Log($"Player {playerID} pressed swap gun button.");
        SwapGun();
    }

    private void OnGunPickup(PlayerGunPickupEvent _playerGunPickupEvent)
    {
        if (_playerGunPickupEvent.Player == playerID)
        {
            guns[_playerGunPickupEvent.GunIndex].Unlocked = true;
            SetGun(_playerGunPickupEvent.GunIndex);
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

    public void ResetShootTimer()
    {
        shootTimer.Reset(guns[currentGunIndex].fireRate);
    }

    private void SwapGun()
    {
        SetGun(NextUnlockedGunIndex());
    }

    private int NextUnlockedGunIndex()
    {
        for (int i = currentGunIndex + 1; i < guns.Length; i++)
        {
            if (guns[i].Unlocked)
            {
                return i;
            }
        }
        for (int i = 0; i < currentGunIndex; i++)
        {
            if (guns[i].Unlocked)
            {
                return i;
            }
        }
        return currentGunIndex; // Return current index if no other unlocked gun is found
    }
    private void ResetGunUnlocks()
    {
        for (int i = 1; i < guns.Length; i++)
        {
            guns[i].Unlocked = false;
        }
    }
}
