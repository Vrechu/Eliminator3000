using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    private PlayerProfileManager playerProfileManager;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int maxHealth = 100;


    private void OnEnable()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerInstantiated);
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
        EventBus<HealthPickupEvent>.Subscribe(OnHealthPickup);
    }

    private void OnDestroy()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerInstantiated);
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
        EventBus<HealthPickupEvent>.UnSubscribe(OnHealthPickup);
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
        playerProfileManager.PlayerProfiles[_playerIndex].Lives = maxLives;
        playerProfileManager.PlayerProfiles[_playerIndex].Health = maxHealth;
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

    private void OnHealthPickup(HealthPickupEvent _healthPickupEvent)
    {
        GainHealth(_healthPickupEvent.Player, _healthPickupEvent.HealthAmount);
    }

    private void GainHealth(int _playerIndex, int _amount)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].Health += _amount;
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
            playerProfileManager.PlayerProfiles[_playerIndex].Health = maxHealth;
            LoseLife(_playerIndex);
        }
        if (playerProfileManager.PlayerProfiles[_playerIndex].Health > maxHealth)
        {
            playerProfileManager.PlayerProfiles[_playerIndex].Health = maxHealth;
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
