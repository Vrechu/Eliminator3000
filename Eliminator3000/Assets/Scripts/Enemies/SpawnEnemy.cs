using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform[] endPoints;
    [SerializeField] private EnemyWave[] enemyWaves;

    private int currentWaveIndex = 0;
    private int currentEnemyIndex = 0;

    private Timer spawnTimer;
    private bool isSpawning = false;
    private GameStateManager gameStateManager;

    private void OnEnable()
    {
        EventBus<EnemySpawnTriggeredEvent>.Subscribe(OnEnemySpawnTriggered);
    }

    private void OnDestroy()
    {
        EventBus<EnemySpawnTriggeredEvent>.UnSubscribe(OnEnemySpawnTriggered);
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        spawnTimer = new Timer(1f, true);
    }

    private void Update()
    {
        if (gameStateManager.CurrentState != GameStateManager.GameState.Ingame || !isSpawning) return;
        if (spawnTimer.IsFinished())
        {
            SpawnNextEnemy();
            currentEnemyIndex++;
            if (currentEnemyIndex < CurrentWave().TimesBetweenSpawns.Length)
            {
                spawnTimer.Reset(CurrentWave().TimesBetweenSpawns[currentEnemyIndex]);
            }
            if (currentEnemyIndex >= CurrentWave().EnemyPrefabs.Length)
            {
                isSpawning = false;
            }
        }
    }

    private void OnEnemySpawnTriggered(EnemySpawnTriggeredEvent _enemySpawnEvent)
    {
        if (!CheckWaveViability())
        {
            Debug.LogError($"Wave {currentWaveIndex} is not viable for spawning.");
            return;
        }
        currentWaveIndex = _enemySpawnEvent.WaveNumber;
        isSpawning = true;
        currentEnemyIndex = 0;
        spawnTimer.Reset(CurrentWave().TimesBetweenSpawns[currentEnemyIndex]);
    }

    private EnemyWave CurrentWave()
    {
        if (currentWaveIndex < enemyWaves.Length)
        {
            return enemyWaves[currentWaveIndex];
        }
        else
        {
            Debug.LogError("Current wave index is out of bounds.");
            return null;
        }
    }

    private void SpawnNextEnemy()
    {
        GameObject enemy = CurrentWave().EnemyPrefabs[currentEnemyIndex];
        if (enemy.TryGetComponent(out MoveEnemy moveEnemy))
        {
            moveEnemy.Waypoints = waypoints;
            moveEnemy.WaypointOrder = CurrentWave().WaypointOrder;
            moveEnemy.EndPoint = endPoints[CurrentWave().EndPointIndex];
            Instantiate(enemy, spawnPoints[CurrentWave().SpawnPointIndex].position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Enemy prefab does not have a MoveEnemy component.");
        }
    }

    private bool CheckWaveViability()
    {
        if (CurrentWave().EnemyPrefabs.Length == 0)
        {
            Debug.LogError($"Wave {currentWaveIndex} has no enemy prefabs defined.");
            return false;
        }
        if (CurrentWave().EnemyPrefabs.Length != CurrentWave().TimesBetweenSpawns.Length)
        {
            Debug.LogError($"Wave {currentWaveIndex} has mismatched lengths for EnemyPrefabs and TimesBetweenSpawns.");
            return false;
        }
        return true;
    }
}
