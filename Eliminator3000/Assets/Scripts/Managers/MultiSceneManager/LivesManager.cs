using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    private PlayerProfileManager playerProfileManager;


    private void OnEnable()
    {
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
    }

    private void OnDestroy()
    {
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
    }

    private void Start()
    {
        playerProfileManager = PlayerProfileManager.Instance;
    }

    private void OnPlayerHit(PlayerHitEvent _context)
    {
        LoseLife(_context.Player, _context.Damage);
    }

    /// <summary>
    /// Reduces the life of the specified player by the given amount and publishes an event to notify other systems of the change. 
    /// If the player's lives reach zero, it also publishes a separate event to indicate that the player has no remaining lives.
    /// </summary>
    /// <param name="_playerIndex">Player to lose a life</param>
    /// <param name="_amount">Amount of lives to lose</param>
    public void LoseLife(int _playerIndex, int _amount)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].Lives -= _amount;
        EventBus<PlayerLivesChangedEvent>.Publish(new PlayerLivesChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].Lives));
        CheckLives(_playerIndex);
    }

    /// <summary>
    /// Checks if the specified player has any remaining lives. 
    /// If the player's lives are zero or less, it publishes an event to notify other systems that the player has no remaining lives.
    /// </summary>
    /// <param name="_playerIndex">Player whos lives are checked</param>
    public void CheckLives(int _playerIndex)
    {
        if (playerProfileManager.PlayerProfiles[_playerIndex].Lives <= 0)
        {
            playerProfileManager.PlayerProfiles[_playerIndex].AliveInLevel = false;
            EventBus<PlayerLivesAtZeroEvent>.Publish(new (_playerIndex));

            CheckPlayersAlive();
        }
    }

    public void CheckPlayersAlive()
    {
        if (playerProfileManager.BothPlayersDead())
        {
            EventBus<BothPlayersDeadEvent>.Publish(new BothPlayersDeadEvent());
        }
    }
}
