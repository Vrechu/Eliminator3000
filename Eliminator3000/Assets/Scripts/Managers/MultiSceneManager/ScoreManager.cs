using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ScoreManager : MonoBehaviour
{
    private ProfileManager profileManager;   


    private void OnEnable()
    {
        EventBus<PlayerScoredEvent>.Subscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
    }

    private void OnDestroy()
    {
        EventBus<PlayerScoredEvent>.UnSubscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
    }

    private void Start()
    {
        profileManager = ProfileManager.Instance;
    }

    private void OnPlayerScored(PlayerScoredEvent _playerScoredEvent)
    {
        ChangePlayerScore(_playerScoredEvent.Player, _playerScoredEvent.Score);
    }

    private void OnPlayerHit(PlayerHitEvent _playerHitEvent)
    {
        ChangePlayerScore(_playerHitEvent.Player, -_playerHitEvent.ScoreLoss);
    }

    private void ChangePlayerScore(int _player, int _score)
    {
        if (_player == 1)
        {
            profileManager.Player1.Score += _score;
            if (profileManager.Player1.Score < 0)
            {
                profileManager.Player1.Score = 0;
            }
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(1, profileManager.Player1.Score));
        }
        else if (_player == 2)
        {
            profileManager.Player2.Score += _score;
            if (profileManager.Player2.Score < 0)
            {
                profileManager.Player2.Score = 0;
            }
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(2, profileManager.Player2.Score));
        }
    }
}
