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

    private void ChangePlayerScore(PlayerScoredEvent pPlayerScoredEvent)
    {
        if (pPlayerScoredEvent.Player == 1)
        {
            profileManager.Player1.Score += pPlayerScoredEvent.Score;
            Debug.Log($"Player 1 Score: {profileManager.Player1.Score}");
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(1, profileManager.Player1.Score));
        }
        else if (pPlayerScoredEvent.Player == 2)
        {
            profileManager.Player2.Score += pPlayerScoredEvent.Score;
            Debug.Log($"Player 2 Score: {profileManager.Player2.Score}");
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(2, profileManager.Player2.Score));
        }
    }
}
