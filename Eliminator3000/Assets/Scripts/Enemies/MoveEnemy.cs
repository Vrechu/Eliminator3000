using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class MoveEnemy : MonoBehaviour
{


    public Transform[] Waypoints;
    public int[] WaypointOrder;
    public Transform EndPoint;
    [SerializeField] private float speed = 2f;

    private int currentWaypointIndex = 0;
    private GameStateManager gameStatemanager;
    private bool finalWaypointReached = false;

    private void Start()
    {
        gameStatemanager = GameStateManager.Instance;
    }

    private void Update()
    {
        if (gameStatemanager.CurrentState != GameStateManager.GameState.Ingame) return;
        Move();
    }

    private void Move()
    {
        Vector3 direction = NextWaypointPosition() - transform.position;
                transform.position += direction.normalized * speed * Time.deltaTime;
        if (direction.magnitude < 0.1f)
        {
            if (finalWaypointReached) Destroy(gameObject);
            currentWaypointIndex++;
        }
    }

    private Vector3 NextWaypointPosition()
    {
        if (currentWaypointIndex < WaypointOrder.Length)
        {
            return Waypoints[WaypointOrder[currentWaypointIndex]].position;
        }
        else
        {
            finalWaypointReached = true;
            return EndPoint.position; // Move to the end point if no more waypoints
        }
    }
}
