using UnityEngine;
using UnityEngine.InputSystem;
public class MoveShip : MonoBehaviour
{
    private Vector2 movementInputVector;

    [Header("Track boundaries"), SerializeField] private float trackWidth = 12f;
    [SerializeField] private float maxHeight = 5f, minHeight;
    [SerializeField, Range(0, 1)] private float edgeBounceResidualSpeed = 0.3f;

    [Header("Movement speed settings"),SerializeField] private bool accelerationEnabled = true;
    [SerializeField] private float
        horizontalMaxSpeed = 7f, verticalMaxSpeed = 7f,
        horizontalAcceleration = 10f, verticalAcceleration = 10f,
        horizontalDeceleration = 10f, verticalDeceleration = 10f;
    private float horizontalCurrentSpeed, verticalCurrentSpeed;

    [Header("Banking settings"), SerializeField] private bool bankEnabled = true;
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
        
        Bank();
        Accelerate();
        Move();
    }

    public void GetHorizontalImputs(InputAction.CallbackContext _context)
    {
        movementInputVector = _context.ReadValue<Vector2>();
    }

    private void Move()
{
    Vector3 position = transform.localPosition;
    position.x += horizontalCurrentSpeed * Time.deltaTime;
    position.y += verticalCurrentSpeed * Time.deltaTime;

    float halfWidth = trackWidth * 0.5f;

    // Horizontal edges
    if (position.x > halfWidth)
    {
        position.x = halfWidth;
        if (horizontalCurrentSpeed > 0)
            horizontalCurrentSpeed = -horizontalCurrentSpeed * edgeBounceResidualSpeed;
    }
    else if (position.x < -halfWidth)
    {
        position.x = -halfWidth;
        if (horizontalCurrentSpeed < 0)
            horizontalCurrentSpeed = -horizontalCurrentSpeed * edgeBounceResidualSpeed;
    }

    // Vertical edges
    if (position.y > maxHeight)
    {
        position.y = maxHeight;
        if (verticalCurrentSpeed > 0)
            verticalCurrentSpeed = -verticalCurrentSpeed * edgeBounceResidualSpeed;
    }
    else if (position.y < minHeight)
    {
        position.y = minHeight;
        if (verticalCurrentSpeed < 0)
            verticalCurrentSpeed = -verticalCurrentSpeed * edgeBounceResidualSpeed;
    }

    transform.localPosition = position;
}

    private void Accelerate()
    {
        if (!accelerationEnabled)
        {
            horizontalCurrentSpeed = horizontalMaxSpeed * movementInputVector.x;
            verticalCurrentSpeed = verticalMaxSpeed * movementInputVector.y;
            return;
        }

        if (movementInputVector.x == 0 || shoot != null && shoot.LockedShooting)
            horizontalCurrentSpeed = Mathf.MoveTowards(horizontalCurrentSpeed, 0, horizontalDeceleration * Time.deltaTime);
        else horizontalCurrentSpeed = Mathf.MoveTowards(horizontalCurrentSpeed, movementInputVector.x * horizontalMaxSpeed, horizontalAcceleration * Time.deltaTime);

        if (movementInputVector.y == 0 || shoot != null && shoot.LockedShooting)
            verticalCurrentSpeed = Mathf.MoveTowards(verticalCurrentSpeed, 0, verticalDeceleration * Time.deltaTime);
        else verticalCurrentSpeed = Mathf.MoveTowards(verticalCurrentSpeed, movementInputVector.y * verticalMaxSpeed, verticalAcceleration * Time.deltaTime);
    }

    private void Bank()
    {
        if (!bankEnabled) return;


        if (movementInputVector.x == 0 || shoot != null && shoot.LockedShooting)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), horizontalIdleBankSpeed * Time.deltaTime);
        else if (movementInputVector.x > 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, -horixontalBankAngle), horizontalInputBankSpeed * Time.deltaTime);
        else if (movementInputVector.x < 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, horixontalBankAngle), horizontalInputBankSpeed * Time.deltaTime);

        if (movementInputVector.y == 0 || shoot != null && shoot.LockedShooting)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), verticalIdleBankSpeed * Time.deltaTime);
        else if (movementInputVector.y > 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-verticalBankAngle, 0, transform.rotation.eulerAngles.z), verticalInputBankSpeed * Time.deltaTime);
        else if (movementInputVector.y < 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(verticalBankAngle, 0, transform.rotation.eulerAngles.z), verticalInputBankSpeed * Time.deltaTime);
    }
}