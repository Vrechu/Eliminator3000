using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ScoreManager : MonoBehaviour
{
    private PlayerProfileManager playerProfileManager;


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
        playerProfileManager = PlayerProfileManager.Instance;
    }

    private void OnPlayerScored(PlayerScoredEvent _playerScoredEvent)
    {
        ChangePlayerScore(_playerScoredEvent.Player, _playerScoredEvent.Score);
    }

    private void OnPlayerHit(PlayerHitEvent _playerHitEvent)
    {
        ChangePlayerScore(_playerHitEvent.Player, -_playerHitEvent.ScoreLoss);
    }

    /// <summary>
    /// Changes the score of the specified player by the given amount. If the resulting score is less than 0, it will be set to 0. 
    /// After changing the score, a ScoreChangedEvent is published to notify other systems of the change.
    /// </summary>
    /// <param name="_playerIndex">The player whose score is to be changed.</param>
    /// <param name="_score">The amount by which to change the player's score.</param>
    private void ChangePlayerScore(int _playerIndex, int _score)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].Score += _score;
            if (playerProfileManager.PlayerProfiles[_playerIndex].Score < 0)
            {
            playerProfileManager.PlayerProfiles[_playerIndex].Score = 0;
            }
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].Score));
    }
}
