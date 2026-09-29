using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaveScriptableObject", menuName = "ScriptableObjects/EnemyWave")]
public class EnemyWave : ScriptableObject
{
    [TextArea(15, 20), SerializeField] 
    private string Info;

    public int SpawnPointIndex;
    public GameObject[] EnemyPrefabs;
    public float[] TimesBetweenSpawns;
    public int[] WaypointOrder;
    public int EndPointIndex;
}
