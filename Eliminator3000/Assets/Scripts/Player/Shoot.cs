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
        if (!setGunType.GetCurrentGunProfile().Cluster) ShootSingle();
        else ShootCluster();
        setGunType.ResetShootTimer();
    }

    private void ShootSingle()
    {
        Instantiate(setGunType.GetCurrentGunProfile().ProjectilePrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
    }

    private void ShootCluster()
    {
        float height = setGunType.GetCurrentGunProfile().Angle * setGunType.GetCurrentGunProfile().Rows;
        float width = setGunType.GetCurrentGunProfile().Angle * setGunType.GetCurrentGunProfile().Columns;

        for (int i = 0; i < setGunType.GetCurrentGunProfile().Rows; i++)
        {
            for (int j = 0; j < setGunType.GetCurrentGunProfile().Columns; j++)
            {

                Instantiate(setGunType.GetCurrentGunProfile().ProjectilePrefab, transform.position,
                            Quaternion.Euler(
                                90f - height / 2 + setGunType.GetCurrentGunProfile().Angle * i,
                                0f,
                               0f - width / 2 + setGunType.GetCurrentGunProfile().Angle * j));
            }
        }
    }
}
