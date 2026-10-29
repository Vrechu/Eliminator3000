using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class MoveEnemy : MonoBehaviour
{
    public Transform[] Waypoints;
    public int[] WaypointOrder;
    public float[] WaitTimes;
    public Transform EndPoint;
    [SerializeField] private float speed = 2f;

    private int currentWaypointIndex = 0;
    private Timer waitTimer;
    private bool finalWaypointReached = false;
    private GameStateManager gameStatemanager;
    private BlackHoleManager blackHoleManager;

    private void Start()
    {
        gameStatemanager = GameStateManager.Instance;
        blackHoleManager = BlackHoleManager.Instance;
        waitTimer = new Timer(WaitTimes[currentWaypointIndex]);
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

        if (!BlackHoleActive() && direction.magnitude < 0.1f)
        {
            if (finalWaypointReached) Destroy(gameObject);
            if (waitTimer.IsFinished())
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= WaitTimes.Length) return;
                waitTimer.Reset(WaitTimes[currentWaypointIndex]);
            }
        }
        else transform.position += direction.normalized * GetSpeed() * Time.deltaTime;
    }

    /// <summary>
    /// Returns the position of the next waypoint based on the currentWaypointIndex and WaypointOrder. 
    /// If all waypoints have been reached, it returns the position of the EndPoint and sets finalWaypointReached to true.
    /// </summary>
    /// <returns>The position of the next waypoint or the EndPoint if all waypoints have been reached.</returns>
    private Vector3 NextWaypointPosition()
    {        
        if (BlackHoleActive()) return blackHoleManager.BlackHolePosition;
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

    private float GetSpeed()
    {
        if (BlackHoleActive()) return blackHoleManager.BlackHoleSuckSpeed;
        return speed;
    }

    private bool BlackHoleActive()
    {
        for (int i = 0; i < blackHoleManager.BlackHolesUp.Length; i++)
        {
            if (blackHoleManager.BlackHolesUp[i]) return true;
        }
        return false;
    }
}
