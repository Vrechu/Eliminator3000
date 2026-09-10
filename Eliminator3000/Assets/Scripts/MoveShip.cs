using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
public class MoveShip : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    private InputAction moveHorizontally;
    private Vector2 horizontalVector;
    [SerializeField]
    private float horizontalSpeed = 7f;

    [SerializeField]
    private float trackwidth = 12f;
    private GameStateManager gameStateManager;



    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void Awake()
    {
        moveHorizontally = inputActions.FindAction("Move");
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
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

    public void GetHorizontalImputs(InputAction.CallbackContext context)
    {
        horizontalVector = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        if (transform.position.x < -trackwidth * 0.5f
            && horizontalVector.x < 0)
            horizontalVector.x = 0;

        if (transform.position.x > trackwidth * 0.5f
            && horizontalVector.x > 0)
            horizontalVector.x = 0;

        transform.Translate(new Vector3(horizontalVector.x, 0, 0) * horizontalSpeed * Time.deltaTime);
    }
}
