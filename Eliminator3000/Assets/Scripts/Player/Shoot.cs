using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Shoot : MonoBehaviour
{
    private GameStateManager gameStateManager;
    private SetGunType setGunType;


    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        setGunType = GetComponent<SetGunType>();
    }

    /// <summary>
    /// Gets the shoot input from the player and shoots a projectile if the conditions are met.
    /// </summary>
    /// <param name="_context">The input action callback context.</param>
    public void GetShootInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState 
            != GameStateManager.GameState.Ingame) return;
        if (!_context.performed) return;
        if (!setGunType.canShoot) return;
        ShootProjectile();
    }    
    
    /// <summary>
    /// Instantiates the projectile prefab at the player's position and resets the shoot timer.
    /// </summary>
    private void ShootProjectile()
    { 
        Instantiate(setGunType.GetCurrentGunProfile().ProjectilePrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
        setGunType.ResetShootTimer();
    }
}
