using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private MonoBehaviour playerController;
    private GameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
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
        Instantiate(projectilePrefab, transform.position, Quaternion.Euler(90f,0f,0f));
    }
}
