using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    private bool inputpressedThisFrame;

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
        inputpressedThisFrame = false;
        if (!context.performed) return;
        inputpressedThisFrame = true;
        Debug.Log("Shoot input pressed");
    }
}
