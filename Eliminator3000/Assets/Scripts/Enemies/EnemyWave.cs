using UnityEngine;

public class EnemyWave
{
    public EnemyWaveInfo waveInfo;
    public Timer spawnTimer;
    private int currentEnemyIndex = 0;

    public EnemyWave(EnemyWaveInfo cWaveInfo)
    {
        waveInfo = cWaveInfo;
        spawnTimer = new Timer(waveInfo.TimesBetweenSpawns[0]);
    }

    /// <summary>
    /// Returns the next enemy prefab to spawn based on the currentEnemyIndex. 
    /// If there are no more enemies to spawn, it returns null. 
    /// It also resets the spawn timer for the next enemy if applicable.
    /// </summary>
    /// <returns>The next enemy prefab to spawn, or null if there are no more enemies.</returns>
    public GameObject NextEnemy()
    {
        if (currentEnemyIndex < waveInfo.EnemyPrefabs.Length)
        {
            GameObject enemyPrefab = waveInfo.EnemyPrefabs[currentEnemyIndex];
            currentEnemyIndex++;            
            if (currentEnemyIndex < waveInfo.TimesBetweenSpawns.Length)
            {
                spawnTimer.Reset(waveInfo.TimesBetweenSpawns[currentEnemyIndex]);
            }
            return enemyPrefab;
        }
        return null;
    }
}
