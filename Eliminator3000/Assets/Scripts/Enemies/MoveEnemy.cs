using UnityEngine;

public class MoveEnemy : MonoBehaviour
{
    public Transform[] Waypoints;
    public int[] WaypointOrder;
    public float[] WaypointSpeeds;
    public float[] WaitTimes;
    public Transform EndPoint;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool kamikaze = false;
    [SerializeField] private float kamikazeOvershootDistance = 0.5f;
    private bool kamikazeReached = false;
    private Vector3 playerTrackedPosition;
    private float baseSpeed = 2f;

    private int currentWaypointIndex = 0;
    private Timer waitTimer;
    private bool finalWaypointReached = false;
    private GameStateManager gameStatemanager;
    private BlackHoleManager blackHoleManager;
    private PlayerProfileManager playerProfileManager = PlayerProfileManager.Instance;

    private void Start()
    {
        gameStatemanager = GameStateManager.Instance;
        blackHoleManager = BlackHoleManager.Instance;
        waitTimer = new Timer(WaitTimes[currentWaypointIndex]);
        SetWaypointSpeed();
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
        if (direction.magnitude < 0.1f)
        {
            if (!BlackHoleActive())
            {
                if (finalWaypointReached) Destroy(gameObject);
                if (kamikaze && kamikazeReached) finalWaypointReached = true;
                if (waitTimer.IsFinished())
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex < WaitTimes.Length)
                    {
                        waitTimer.Reset(WaitTimes[currentWaypointIndex]);
                    }
                    SetWaypointSpeed();
                }
            }
        }
        else
        {
            if (direction.magnitude < GetSpeed() * Time.deltaTime) transform.position += direction;
            else transform.position += direction.normalized * GetSpeed() * Time.deltaTime;
        }
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
            if (kamikaze && !finalWaypointReached)
            {
                if (!kamikazeReached)
                {
                    playerTrackedPosition = GetExtendedPlayerPosition();
                    kamikazeReached = true;
                }
                return playerTrackedPosition;
            }
            finalWaypointReached = true;
            return EndPoint.position; // Move to the end point if no more waypoints
        }
    }

    public void SetBaseSpeed(float _newSpeed)
    {
        baseSpeed = _newSpeed;
    }

    private void SetWaypointSpeed()
    {
        if (currentWaypointIndex < WaypointSpeeds.Length)
        {
            speed = WaypointSpeeds[currentWaypointIndex];
            if (speed <= 0) speed = baseSpeed; // Reset to base speed if the speed is zero or negative
        }
        else speed = baseSpeed; // Reset to base speed if there are no more waypoints
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

    private Vector3 GetClosestPlayerPosition()
    {
        float closestTargetDistance = 100f;
        Vector3 targetPosition = Vector3.zero;
        for (int i = 0; i < playerProfileManager.PlayerProfiles.Length; i++)
        {
            if (!playerProfileManager.PlayersJoined[i]) continue;
            if (!playerProfileManager.PlayerProfiles[i].AliveInLevel) continue;
            Vector3 currentEnemyPositiion = playerProfileManager.PlayerProfiles[i].IngameAvatar.transform.position;
            float currentEnemyDistance = (currentEnemyPositiion - transform.position).magnitude;
            if (currentEnemyDistance < closestTargetDistance)
            {
                closestTargetDistance = currentEnemyDistance;
                targetPosition = currentEnemyPositiion;
            }
        }
        if (closestTargetDistance == 100f) Debug.LogWarning("Enemy too far away: " + gameObject.name);
        return targetPosition;
    }

    private Vector3 GetExtendedPlayerPosition()
    {
               Vector3 closestPlayerPosition = GetClosestPlayerPosition();
        Vector3 directionToPlayer = (closestPlayerPosition - transform.position).normalized;
        float extensionDistance = 1f + kamikazeOvershootDistance;
        return closestPlayerPosition + directionToPlayer * extensionDistance;
    }
}
