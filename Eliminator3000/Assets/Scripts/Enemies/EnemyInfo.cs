using UnityEngine;

/// <summary>
/// Information about an enemy, including its score value and descriptive text.
/// </summary>
public class EnemyInfo : MonoBehaviour
{
    [TextArea(15, 20), SerializeField]
    private string Info;

    public int ScoreValue;
}
