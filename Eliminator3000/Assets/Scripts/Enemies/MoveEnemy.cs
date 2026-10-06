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

    /// <summary>
    /// Moves the enemy towards the next waypoint in the WaypointOrder. If the enemy reaches the final waypoint, it will be destroyed. 
    /// The movement is based on the speed variable and is frame-rate independent.
    /// </summary>
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

    /// <summary>
    /// Returns the position of the next waypoint based on the currentWaypointIndex and WaypointOrder. 
    /// If all waypoints have been reached, it returns the position of the EndPoint and sets finalWaypointReached to true.
    /// </summary>
    /// <returns>The position of the next waypoint or the EndPoint if all waypoints have been reached.</returns>
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

    public void SetSpeed(float _newSpeed)
    {
        speed = _newSpeed;
    }
}
