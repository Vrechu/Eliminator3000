using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    [SerializeField]
    private GameObject projectilePrefab;
    private bool inputpressedThisFrame;
    [SerializeField]
    private MonoBehaviour playerController;

    private void OnEnable()
    {
        inputActions.FindActionMap("Gameplay").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Gameplay").Disable();
    }

    public void GetShootInput(InputAction.CallbackContext context)
    {
        if (GameStateManager.Instance.CurrentState 
            != GameStateManager.GameState.Ingame) return;
        if (!context.performed) return;
        inputpressedThisFrame = false;
        inputpressedThisFrame = true;
        ShootProjectile();
    }


    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(90f,0f,0f));
    }

}
