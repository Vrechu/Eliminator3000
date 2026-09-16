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

    public void CheckLives(int _player)
    {
        if (profileManager.AllProfiles()[_player - 1].Lives <= 0)
        {
            EventBus<PlayerLivesAtZeroEvent>.Publish(new (_player));
        }
    }
}
