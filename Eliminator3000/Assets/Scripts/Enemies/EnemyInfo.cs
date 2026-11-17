using UnityEngine;
using TMPro;

/// <summary>
/// Information about an enemy, including its score value and descriptive text.
/// </summary>
public class EnemyInfo : MonoBehaviour
{
    [TextArea(15, 20), SerializeField]
    private string Info;

    [SerializeField] private int ScoreValue;
    [SerializeField] private int health = 2;
    [SerializeField] private int scoreMultiplierGain = 1;
    private int lastPlayerHitID = -1;

    [SerializeField] TextMeshProUGUI healthText;

    private void Start()
    {
        UpdateText();
    }

    public void TakeDamage(int _value, int _playerID)
    {
        health -= _value;
        lastPlayerHitID = _playerID;
        CheckHealth();
    }

    private void CheckHealth()
    {
        UpdateText();
        if (health <= 0)
        {
            EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(lastPlayerHitID, ScoreValue));
            EventBus<PlayerScoreMultiplierGainedEvent>.Publish(new PlayerScoreMultiplierGainedEvent(lastPlayerHitID, scoreMultiplierGain));
            Destroy(gameObject);
        }
    }

    private void UpdateText()
    {
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }
    }
}
