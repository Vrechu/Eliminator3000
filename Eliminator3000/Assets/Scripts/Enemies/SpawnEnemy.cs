using UnityEngine;
using System.Collections.Generic;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private EnemyWaveInfo[] enemyWaveInfos;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform[] endPoints;

    private List<EnemyWave> activeWaves = new List<EnemyWave>();


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
    }
    
    private void Update()
    {
        if (gameStateManager.CurrentState != GameStateManager.GameState.Ingame) return;
        SpawnEnemies();
    }

    /// <summary>
    /// When an EnemySpawnTriggeredEvent is received, 
    /// this method checks if the wave number is valid and if the wave is viable for spawning.
    /// </summary>
    /// <param name="_enemySpawnEvent">The event containing information about the enemy spawn.</param>
    private void OnEnemySpawnTriggered(EnemySpawnTriggeredEvent _enemySpawnEvent)
    {
        if (_enemySpawnEvent.WaveNumber > enemyWaveInfos.Length
            || _enemySpawnEvent.WaveNumber < 0)
        {
            Debug.LogError($"Wave number {_enemySpawnEvent.WaveNumber} is out of bounds.");
        }
        if (!CheckWaveViability(enemyWaveInfos[_enemySpawnEvent.WaveNumber]))
        {
            Debug.LogError($"Wave {_enemySpawnEvent.WaveNumber} is not viable for spawning.");
            return;
        }

        activeWaves.Add(new EnemyWave(enemyWaveInfos[_enemySpawnEvent.WaveNumber]));
    }

    /// <summary>
    /// Spawns enemies from the active waves based on their spawn timers.
    /// </summary>
    private void SpawnEnemies()
    {
        if (activeWaves.Count == 0) return;
        for (int i = 0; i < activeWaves.Count; i++)
        {
            if (!activeWaves[i].spawnTimer.IsFinished()) continue;
            GameObject enemy = activeWaves[i].NextEnemy();
            if (enemy == null) activeWaves.RemoveAt(i);
            else {
                SetEnemyMovement(enemy, activeWaves[i].waveInfo);
                SpawnNextEnemy(enemy, activeWaves[i].waveInfo);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_enemy"></param>
    /// <param name="_waveInfo"></param>
    private void SpawnNextEnemy(GameObject _enemy, EnemyWaveInfo _waveInfo)
    {
        if (_enemy == null) return;         
        Instantiate(_enemy, spawnPoints[_waveInfo.SpawnPointIndex].position, Quaternion.identity);
    }

    /// <summary>
    /// Sets the movement parameters for the enemy based on the wave information. 
    /// It assigns waypoints, waypoint order, end point, 
    /// and speed to the MoveEnemy component of the enemy prefab.
    /// </summary>
    /// <param name="_enemy">The enemy GameObject to set movement parameters for.</param>
    /// <param name="_waveInfo">The wave information containing movement parameters.</param>
    private void SetEnemyMovement(GameObject _enemy, EnemyWaveInfo _waveInfo)
    {
        if (_enemy.TryGetComponent(out MoveEnemy moveEnemy))
        {
            moveEnemy.Waypoints = waypoints;
            moveEnemy.WaypointOrder = _waveInfo.WaypointOrder;
            moveEnemy.WaitTimes = _waveInfo.WaitTimes;
            moveEnemy.EndPoint = endPoints[_waveInfo.EndPointIndex];
            moveEnemy.SetSpeed(_waveInfo.EnemySpeed);
        }
        else
        {
            Debug.LogError("Enemy prefab does not have a MoveEnemy component.");
        }
    }

    /// <summary>
    /// Checks if the wave information is viable for spawning enemies.
    /// </summary>
    /// <param name="_waveInfo">The wave information to check for viability.</param>
    /// <returns>True if the wave information is viable, false otherwise.</returns>
    private bool CheckWaveViability(EnemyWaveInfo _waveInfo)
    {
        if (_waveInfo.EnemyPrefabs.Length == 0)
        {
            Debug.LogError($"Wave has no enemy prefabs defined.");
            return false;
        }
        if (_waveInfo.EnemyPrefabs.Length != _waveInfo.TimesBetweenSpawns.Length)
        {
            Debug.LogError($"Wave has mismatched lengths for EnemyPrefabs and TimesBetweenSpawns.");
            return false;
        }
        for (int i = 0; i < _waveInfo.WaypointOrder.Length; i++)
        {
            if (_waveInfo.WaypointOrder[i] < 0
                || _waveInfo.WaypointOrder[i] >= waypoints.Length)
            {
                Debug.LogError($"Wave has an invalid waypoint index: {i}.");
                return false;
            }
        }
        if (_waveInfo.WaypointOrder.Length != _waveInfo.WaitTimes.Length)
        {
            Debug.LogError($"Wave has mismatched lengths for WaypointOrder and WaitTimes.");
            return false;
        }

        if (_waveInfo.SpawnPointIndex < 0
            || _waveInfo.SpawnPointIndex >= spawnPoints.Length)
        {
            Debug.LogError($"Wave has an invalid spawn point index: {_waveInfo.SpawnPointIndex}.");
            return false;
        }
        if (_waveInfo.EndPointIndex < 0
            || _waveInfo.EndPointIndex >= endPoints.Length)
        {
            Debug.LogError($"Wave has an invalid end point index: {_waveInfo.EndPointIndex}.");
            return false;
        }
        return true;
    }
}
