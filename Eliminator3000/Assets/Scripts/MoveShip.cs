using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
public class MoveShip : MonoBehaviour
{
    public Transform shipTransform;
    public InputActionAsset InputActions;
    private InputAction moveHorizontally;
    private Vector2 horizontalVector;
    private float horizontalSpeed = 5f;


    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void Awake()
    {
        moveHorizontally = InputActions.FindAction("Move");
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();        
    }   

    private void Update()
    {
        SetInputs();
        Move();
    }

    private void SetInputs()
    {
        horizontalVector = moveHorizontally.ReadValue<Vector2>();
        
    }

    private void Move()
    {
        shipTransform.Translate(new Vector3( horizontalVector.x,0,horizontalVector.y) * horizontalSpeed * Time.deltaTime);
    }
}
