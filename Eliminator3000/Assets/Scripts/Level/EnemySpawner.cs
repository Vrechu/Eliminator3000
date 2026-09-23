using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private float spawnInterval = 2f;
    [SerializeField]
    private GameObject[] enemyPrefabs;
    private Timer spawnTimer;
    private int nextEnemyIndex = 0;
    private bool isSpawning = false;


    private void Start()
    {
        spawnTimer = new Timer(spawnInterval, true);
    }

    private void Update()
    {
        if (isSpawning) SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (nextEnemyIndex >= enemyPrefabs.Length) Destroy(gameObject);
        if (!spawnTimer.IsFinished()) return;

        GameObject enemyPrefab = enemyPrefabs[nextEnemyIndex];
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        nextEnemyIndex += 1;
        Debug.Log($"Spawned enemy {enemyPrefab.name} at {spawnPoint.position}");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1")
            || other.CompareTag("Player2"))
        {
            isSpawning = true;
            Debug.Log("Player entered spawn area, starting enemy spawn");
        }
    }
}
