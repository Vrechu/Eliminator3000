using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilePrefab, upgradeProjectilePrefab;
    private GameStateManager gameStateManager;
    private SetGunType setGunType;
    [SerializeField]
    private int Player = 1;


    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        setGunType = GetComponent<SetGunType>();
        MarkProjectiles();
    }

    public void GetShootInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState 
            != GameStateManager.GameState.Ingame) return;
        if (!_context.performed) return;
        CheckWeaponType();
    }

    private void ShootRegularProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(90f,0f,0f));
        /*if (TryGetComponent<PlayerProjectileHit>(out PlayerProjectileHit playerProjectileHit))
        {
            playerProjectileHit.PlayerID = Player;
        }
        else
        {
            Debug.LogWarning("Shoot: PlayerProjectileHit component not found on projectilePrefab.");
        }*/

    }
    private void ShootUpgradeProjectile()
    {
        Instantiate(upgradeProjectilePrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
    }

    private void CheckWeaponType()
    {
        switch (setGunType.gunType)
        {
            case SetGunType.GunType.Base:
                ShootRegularProjectile();
                break;
            case SetGunType.GunType.Gun:
                ShootUpgradeProjectile();
                break;
            default:
                Debug.LogWarning("Unknown gun type.");
                break;
        }
    }

    private void MarkProjectiles()
    { 
        if (projectilePrefab.TryGetComponent<PlayerProjectileHit>(out PlayerProjectileHit playerProjectileHit))
        {
            playerProjectileHit.PlayerID = Player;
        }
        else 
        {
            Debug.LogWarning("Projectile prefab does not have a PlayerProjectileHit component.");
        }
        if (upgradeProjectilePrefab.TryGetComponent<PlayerProjectileHit>(out PlayerProjectileHit upgradePlayerProjectileHit))
        {
            upgradePlayerProjectileHit.PlayerID = Player;
        }
        else 
        {
            Debug.LogWarning("Upgrade projectile prefab does not have a PlayerProjectileHit component.");
        }
    }
}
