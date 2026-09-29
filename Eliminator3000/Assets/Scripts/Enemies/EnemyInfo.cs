using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    [TextArea(15, 20)]
    [SerializeField] private string Info;
    public int ScoreValue;
}
