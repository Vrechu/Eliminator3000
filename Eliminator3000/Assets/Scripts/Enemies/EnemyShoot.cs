using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private bool aimAtPlayer = false;
    [SerializeField] private float shootInterval = 6f;

    private Timer timer;

    [SerializeField] private bool multiShot = false;
    [SerializeField] private GameObject[] shotOrigins;

    private PlayerProfileManager playerProfileManager;
    private DummyManager dummyManager;

    private void Start()
    {
        timer = new Timer(shootInterval, true);
        playerProfileManager = PlayerProfileManager.Instance;
        dummyManager = DummyManager.Instance;
    }

    /// <summary>
    /// Shoots a projectile from the enemy at regular intervals defined by shootInterval. 
    /// If multiShot is enabled, it shoots multiple projectiles from specified shot origins.
    /// </summary>
    private void Update()
    {
        if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.Ingame)
            return;
        if (timer.IsFinished())
        {
            if (multiShot)
            {
                ShootMultiProjectile();
            }
            else
            {
                ShootProjectile();
            }
        }
    }

    private void ShootProjectile()
    {
        Instantiate(projectilePrefab, transform.position, GetShotDirection());
    }

    private void ShootMultiProjectile()
    {
        for (int i = 0; i < shotOrigins.Length; i++)
        {
            Instantiate(projectilePrefab, shotOrigins[i].transform.position, GetShotDirection());
        }
    }

    private Quaternion GetShotDirection()
    {
        if (!aimAtPlayer) return Quaternion.Euler(0, 180f, 0f);
        for (int i = 0; i < dummyManager.DummiesUp.Length; i++)
        {
            if (dummyManager.DummiesUp[i]) return Quaternion.LookRotation(dummyManager.DummyPosition - transform.position, Vector3.up);
        }
        return Quaternion.LookRotation(GetClosestPlayerPosition() - transform.position, Vector3.up);
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
}
