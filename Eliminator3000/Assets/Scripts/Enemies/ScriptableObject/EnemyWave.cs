using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaveScriptableObject", menuName = "ScriptableObjects/EnemyWave")]
public class EnemyWave : ScriptableObject
{
    [SerializeField] public GameObject[] EnemyPrefabs;
    [SerializeField] public int[] SpawnPointsIndices;
    [SerializeField] public float[] TimesBetweenSpawns;
    [SerializeField] public int[] WaypointOrder;
}
