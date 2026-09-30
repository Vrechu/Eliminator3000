using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    private ProfileManager profileManager;


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
        profileManager = ProfileManager.Instance;
    }

    private void OnPlayerHit(PlayerHitEvent _context)
    {
        LoseLife(_context.Player, _context.Damage);
    }

    /// <summary>
    /// Reduces the life of the specified player by the given amount and publishes an event to notify other systems of the change. 
    /// If the player's lives reach zero, it also publishes a separate event to indicate that the player has no remaining lives.
    /// </summary>
    /// <param name="_player">Player to lose a life</param>
    /// <param name="_amount">Amount of lives to lose</param>
    public void LoseLife(int _player, int _amount)
    {
        if (_player == 1)
        {
            profileManager.Player1.Lives -= _amount;
        }
        else if (_player == 2)
        {
            profileManager.Player2.Lives -= _amount;
        }
        EventBus<PlayerLivesChangedEvent>.Publish(new PlayerLivesChangedEvent(_player, profileManager.AllProfiles()[_player - 1].Lives));
        CheckLives(_player);
    }

    /// <summary>
    /// Checks if the specified player has any remaining lives. 
    /// If the player's lives are zero or less, it publishes an event to notify other systems that the player has no remaining lives.
    /// </summary>
    /// <param name="_player">Player whos lives are checked</param>
    public void CheckLives(int _player)
    {
        if (profileManager.AllProfiles()[_player - 1].Lives <= 0)
        {
            EventBus<PlayerLivesAtZeroEvent>.Publish(new (_player));
        }
    }
}
