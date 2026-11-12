using UnityEngine;
using UnityEngine.InputSystem;
public class MoveShip : MonoBehaviour
{
    private Vector2 movementInputVector;
    [SerializeField] private float horizontalSpeed = 7f;
    [SerializeField] private float trackWidth = 12f, maxHeight = 5f, minHeight;

    [Header("Banking"), SerializeField] private bool bankEnabled = true;
    [SerializeField] private float 
        horizontalInputBankSpeed = 5f, verticalInputBankSpeed = 5f, 
        horizontalIdleBankSpeed = 2f, verticalIdleBankSpeed = 2f;
    [SerializeField] private float horixontalBankAngle = 30f, verticalBankAngle = 30f;
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
        Bank();
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

        transform.localPosition += horizontalSpeed * Time.deltaTime
           * new Vector3(movementInputVector.x, movementInputVector.y, 0).normalized;
    }

    private void Bank()
    {
        if (movementInputVector.x > 0)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -horixontalBankAngle), horizontalInputBankSpeed * Time.deltaTime);
        else if (movementInputVector.x < 0)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, horixontalBankAngle), horizontalInputBankSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), horizontalIdleBankSpeed * Time.deltaTime);

        if (movementInputVector.y > 0)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(-verticalBankAngle, 0, transform.rotation.eulerAngles.z), verticalInputBankSpeed * Time.deltaTime);
        else if (movementInputVector.y < 0)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(verticalBankAngle, 0, transform.rotation.eulerAngles.z), verticalInputBankSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z), verticalIdleBankSpeed * Time.deltaTime);
    }
}