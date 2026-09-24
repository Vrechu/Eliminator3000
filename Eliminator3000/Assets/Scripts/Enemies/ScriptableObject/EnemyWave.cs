using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaveScriptableObject", menuName = "ScriptableObjects/EnemyWave")]
public class Wave : ScriptableObject
{
    public float SpawnRate = 1;
    public Vector2Int[] WaveComposition;
    public bool ShuffleEnemies;

    public int[] GetWave()
    {
        if (!ShuffleEnemies) return WaveInOrder();
        return ShuffledWave(); ;
    }

    private int[] WaveInOrder()
    {
        int enemyNumber = 0;
        int[] waveInOrder = new int[WaveSize()];
        for (int i = 0; i < WaveComposition.Length; i++)
        {
            for (int j = 0; j < WaveComposition[i][1]; j++)
            {
                waveInOrder[enemyNumber] = WaveComposition[i][0];
                enemyNumber++;
            }
        }
        return waveInOrder;
    }

    public int WaveSize()
    {
        int size = 0;
        for (int i = 0; i < WaveComposition.Length; i++)
        {
            size += WaveComposition[i][1];
        }
        return size;
    }

    private int[] ShuffledWave()
    {
        int[] wave = WaveInOrder();
        for (int i = 0; i < wave.Length; i++)
        {
            int random = Random.Range(0, wave.Length);
            int temp = wave[i];
            wave[i] = wave[random];
            wave[random] = temp;
        }
        return wave;
    }
}
