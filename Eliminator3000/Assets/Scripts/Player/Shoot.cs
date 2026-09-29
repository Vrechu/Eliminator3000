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

    public void GetShootInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState 
            != GameStateManager.GameState.Ingame) return;
        if (!_context.performed) return;

        ShootProjectile();
    }    

    private void ShootProjectile()
    { 
        Instantiate(setGunType.GetCurrentGunProfile().ProjectilePrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
    }
}
