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
        if (playerID < 0 || playerID > 1)
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
    
    /// <summary>
    /// Sets the current gun to the specified index and updates the gun model.
    /// </summary>
    /// <param name="_gunIndex">The index of the gun to set.</param>
    private void SetGun(int _gunIndex)
    {
        DisableModels();
        currentGunIndex = _gunIndex;
        gunModels[_gunIndex].SetActive(true);
        shootTimer.Reset(0);
        EventBus<PlayerGunSwapEvent>.Publish(new PlayerGunSwapEvent(playerID, _gunIndex));
    }

    /// <summary>
    /// Swaps to the next unlocked gun when the input action is performed, if the game state is Ingame.
    /// </summary>
    /// <param name="_context">The input action callback context.</param>
    public void GetSwapInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState
            != GameStateManager.GameState.Ingame) return;
        if (!_context.performed) return;
        SwapGun();
    }

    /// <summary>
    /// Sets the gun to the specified index when a PlayerGunPickupEvent is received, if the event is for this player.
    /// Also unlocks the gun in the guns array.
    /// </summary>
    /// <param name="_playerGunPickupEvent">The PlayerGunPickupEvent containing the player ID and gun index.</param>
    private void OnGunPickup(PlayerGunPickupEvent _playerGunPickupEvent)
    {
        if (_playerGunPickupEvent.Player == playerID)
        {
            guns[_playerGunPickupEvent.GunIndex].Unlocked = true;
            SetGun(_playerGunPickupEvent.GunIndex);
        }
    }

    /// <summary>
    /// Disables all gun models by setting them inactive. 
    /// This is used to ensure that only the currently selected gun model is active at any given time.
    /// </summary>
    private void DisableModels()
    {
        for (int i = 0; i < gunModels.Length; i++)
        {
            gunModels[i].SetActive(false);
        }
    }

    /// <summary>
    /// Checks if the gun models and gun profiles arrays are viable for use.
    /// </summary>
    /// <returns>true if the arrays are viable, false otherwise.</returns>
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

    /// <summary>
    /// Returns the current gun profile based on the current gun index. 
    /// If the index is out of bounds, it logs an error and returns null.
    /// </summary>
    /// <returns>The current GunProfile or null if the index is out of bounds.</returns>
    public GunProfile GetCurrentGunProfile()
    {
        if (currentGunIndex < 0 || currentGunIndex >= guns.Length)
        {
            Debug.LogError("Current gun index is out of bounds.");
            return null;
        }
        return guns[currentGunIndex];
    }

    /// <summary>
    /// Sets the PlayerID for each gun's projectile prefab that has a PlayerProjectileHit component.
    /// </summary>
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
        shootTimer.Reset(guns[currentGunIndex].FireRate);
    }

    private void SwapGun()
    {
        SetGun(NextUnlockedGunIndex());
    }

    /// <summary>
    /// Loops through the guns array starting from the current gun index + 1, and wraps around to the beginning if necessary. 
    /// If no other unlocked gun is found, it returns the current gun index.
    /// </summary>
    /// <returns>The index of the next unlocked gun.</returns>
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

    /// <summary>
    /// Sets all guns except the first one to be locked (unavailable). 
    /// This is used to reset the gun unlocks at the start of the game or when needed.
    /// </summary>
    private void ResetGunUnlocks()
    {
        for (int i = 1; i < guns.Length; i++)
        {
            guns[i].Unlocked = false;
        }
    }
}
