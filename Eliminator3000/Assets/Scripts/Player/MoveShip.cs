using UnityEngine;
using UnityEngine.InputSystem;
public class MoveShip : MonoBehaviour
{
    private Vector2 movementInputVector;
    [SerializeField] private float horizontalSpeed = 7f;
    [SerializeField] private float trackWidth = 12f, maxHeight = 5f, minHeight;
    private GameStateManager gameStateManager;
    private Shoot shoot;

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        if (TryGetComponent<Shoot>(out Shoot _shoot))
            shoot = _shoot;
        else Debug.LogError("No Shoot component found on: " + gameObject.name);
    }

    private void Update()
    {
        if (gameStateManager.CurrentState != GameStateManager.GameState.Ingame) return;
        if (shoot != null && shoot.LockedShooting) return;
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

        if (transform.position.y < minHeight
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
