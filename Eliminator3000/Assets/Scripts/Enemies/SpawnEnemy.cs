using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private EnemyWave[] enemyWaves;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform[] endPoints;

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

    /// <summary>
    /// Spawns enemies based on the current wave configuration. It checks if the game is in the "Ingame" state and if spawning is active. 
    /// If the spawn timer has finished, it spawns the next enemy and updates the spawn timer for the next enemy in the wave. 
    /// If all enemies in the current wave have been spawned, it stops spawning.
    /// </summary>
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

    /// <summary>
    /// Handles the event when an enemy spawn is triggered. 
    /// It checks the viability of the current wave and starts spawning enemies if the wave is valid.
    /// </summary>
    /// <param name="_enemySpawnEvent">The event data containing information about the enemy spawn.</param>
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

    /// <summary>
    /// Retrieves the current enemy wave based on the current wave index. 
    /// If the index is out of bounds, it logs an error and returns null.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Spawns the next enemy in the current wave. 
    /// It retrieves the enemy prefab, sets its waypoints and endpoint, and instantiates it at the designated spawn point. 
    /// If the enemy prefab does not have a MoveEnemy component, it logs an error.
    /// </summary>
    private void SpawnNextEnemy()
    {
        GameObject enemy = CurrentWave().EnemyPrefabs[currentEnemyIndex];
        if (enemy.TryGetComponent(out MoveEnemy moveEnemy))
        {
            moveEnemy.Waypoints = waypoints;
            moveEnemy.WaypointOrder = CurrentWave().WaypointOrder;
            moveEnemy.EndPoint = endPoints[CurrentWave().EndPointIndex];
            moveEnemy.SetSpeed(CurrentWave().EnemySpeed);
            Instantiate(enemy, spawnPoints[CurrentWave().SpawnPointIndex].position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Enemy prefab does not have a MoveEnemy component.");
        }
    }

    /// <summary>
    /// Checks the viability of the current wave by ensuring that it has defined enemy prefabs 
    /// and that the lengths of the EnemyPrefabs and TimesBetweenSpawns arrays match.
    /// </summary>
    /// <returns>True if the wave is viable, false otherwise.</returns>
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
