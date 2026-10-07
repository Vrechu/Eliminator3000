using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    private PlayerProfileManager playerProfileManager;


    private void OnEnable()
    {
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerInstantiated);
    }

    private void OnDestroy()
    {
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerInstantiated);
    }

    private void Start()
    {
        playerProfileManager = PlayerProfileManager.Instance;
    }

    private void OnPlayerInstantiated(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        ResetPlayer(_playerAvatarInstantiatedEvent.PlayerIndex);
    }

    private void ResetPlayer(int _playerIndex)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].Health = 100;
        playerProfileManager.PlayerProfiles[_playerIndex].Lives = 3;
        EventBus<PlayerHealthChangedEvent>.Publish(new PlayerHealthChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].Health));
        EventBus<PlayerLivesChangedEvent>.Publish(new PlayerLivesChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].Lives));
    }

    private void OnPlayerHit(PlayerHitEvent _context)
    {
        LoseHealth(_context.Player, _context.Damage);
    }

    /// <summary>
    /// Reduces the life of the specified player by the given amount and publishes an event to notify other systems of the change. 
    /// If the player's lives reach zero, it also publishes a separate event to indicate that the player has no remaining lives.
    /// </summary>
    /// <param name="_playerIndex">Player to lose a life</param>
    /// <param name="_amount">Amount of lives to lose</param>
    public void LoseHealth(int _playerIndex, int _amount)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].Health -= _amount;
        CheckHealth(_playerIndex);
        EventBus<PlayerHealthChangedEvent>.Publish(new PlayerHealthChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].Health));
    }

    /// <summary>
    /// Checks if the specified player has any remaining lives. 
    /// If the player's lives are zero or less, it publishes an event to notify other systems that the player has no remaining lives.
    /// </summary>
    /// <param name="_playerIndex">Player whos lives are checked</param>
    private void CheckHealth(int _playerIndex)
    {
        if (playerProfileManager.PlayerProfiles[_playerIndex].Health <= 0)
        {
            playerProfileManager.PlayerProfiles[_playerIndex].Health = 100;
            LoseLife(_playerIndex);
        }
    }

    private void LoseLife(int _playerIndex, int _amount = 1)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].Lives -= _amount;
        CheckLives(_playerIndex);
        EventBus<PlayerLivesChangedEvent>.Publish(new(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].Lives));
    }

    private void CheckLives(int _playerIndex)
    {
        if (playerProfileManager.PlayerProfiles[_playerIndex].Lives <= 0)
        {
            playerProfileManager.PlayerProfiles[_playerIndex].AliveInLevel = false;
            EventBus<PlayerLivesAtZeroEvent>.Publish(new(_playerIndex));

            CheckPlayersAlive();
        }
    }


    private void CheckPlayersAlive()
    {
        if (playerProfileManager.BothPlayersDead())
        {
            EventBus<BothPlayersDeadEvent>.Publish(new BothPlayersDeadEvent());
        }
    }
}
