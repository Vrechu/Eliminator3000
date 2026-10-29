using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaveScriptableObject", menuName = "ScriptableObjects/EnemyWave")]

public class EnemyWaveInfo : ScriptableObject
{
    [TextArea(15, 20), SerializeField] 
    private string Info;

    public int SpawnPointIndex;
    public GameObject[] EnemyPrefabs;
    public float[] TimesBetweenSpawns;
    public float EnemySpeed;
    public int[] WaypointOrder;
    public float[] WaitTimes;
    public int EndPointIndex;
}
