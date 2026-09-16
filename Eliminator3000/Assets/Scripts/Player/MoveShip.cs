using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
public class MoveShip : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    private Vector2 movementInputVector;
    [SerializeField]
    private float horizontalSpeed = 7f;

    [SerializeField]
    private float trackWidth = 12f, maxHeight = 5f;
    private GameStateManager gameStateManager;



    private void OnEnable()
    {
        inputActions.FindActionMap("Gameplay").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Gameplay").Disable();
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        if (gameStateManager.CurrentState == GameStateManager.GameState.Ingame)
            Move();
    }

    public void GetHorizontalImputs(InputAction.CallbackContext _context)
    {
        movementInputVector = _context.ReadValue<Vector2>();
    }

    private void Move()
    {
        if (transform.position.x < -trackWidth * 0.5f
            && movementInputVector.x < 0)
            movementInputVector.x = 0;

        if (transform.position.x > trackWidth * 0.5f
            && movementInputVector.x > 0)
            movementInputVector.x = 0;

        if (transform.position.y < 0.5f
            && movementInputVector.y < 0)
            movementInputVector.y = 0;

        if (transform.position.y > maxHeight
            && movementInputVector.y > 0)
            movementInputVector.y = 0;

        transform.Translate(new Vector3(
            movementInputVector.x, movementInputVector.y, 0).normalized 
            * horizontalSpeed * Time.deltaTime);
    }
}
