using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilePrefab, upgradeProjectilePrefab;
    private GameStateManager gameStateManager;
    private SetGunType setGunType;


    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        setGunType = GetComponent<SetGunType>();
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
        Instantiate(projectilePrefab, transform.position, Quaternion.Euler(90f,0f,0f));
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
}
