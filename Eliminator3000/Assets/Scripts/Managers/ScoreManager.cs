using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ScoreManager : MonoBehaviour
{
    private ProfileManager profileManager;   


    private void OnEnable()
    {
        EventBus<PlayerScoredEvent>.Subscribe(ChangePlayerScore);
    }

    private void OnDestroy()
    {
        EventBus<PlayerScoredEvent>.UnSubscribe(ChangePlayerScore);
    }

    private void Start()
    {
        profileManager = ProfileManager.Instance;
    }

    private void ChangePlayerScore(PlayerScoredEvent _playerScoredEvent)
    {
        if (_playerScoredEvent.Player == 1)
        {
            profileManager.Player1.Score += _playerScoredEvent.Score;
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(1, profileManager.Player1.Score));
        }
        else if (_playerScoredEvent.Player == 2)
        {
            profileManager.Player2.Score += _playerScoredEvent.Score;
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(2, profileManager.Player2.Score));
        }
    }
}
