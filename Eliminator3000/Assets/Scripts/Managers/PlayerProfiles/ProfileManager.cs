using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    public PlayerProfile Player1, Player2;
    public bool player1Active = false, player2Active = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of ProfileManager detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        else 
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        EventBus<PlayerScoredEvent>.Subscribe(ChangePlayerScore);
        EventBus<PlayerLivesAtZeroEvent>.Subscribe(DestroyPlayer);
    }

    private void OnDestroy()
    {
        EventBus<PlayerScoredEvent>.UnSubscribe(ChangePlayerScore);
        EventBus<PlayerLivesAtZeroEvent>.UnSubscribe(DestroyPlayer);
    }

    public PlayerProfile[] AllProfiles()
    {
               return new PlayerProfile[] { Player1, Player2 };
    }

    public void NewProfile(int _player, 
        GameObject _playerPrefab, 
        PlayerInput _playerInput,
        GameObject _ingameAvatar)
    {
        if (_player == 1)
        {
            Player1 = new PlayerProfile(_playerPrefab, _playerInput, _ingameAvatar);
            player1Active = true;
        }
        else if (_player == 2)
        {
            Player2 = new PlayerProfile(_playerPrefab, _playerInput, _ingameAvatar);
            player2Active = true;
        }
        else
        {
            Debug.LogWarning("Invalid player number. 1 or 2 expected.");
        }
        EventBus<PlayerJoinedEvent>.Publish(new PlayerJoinedEvent(_player));
    }

    private void DestroyPlayer(PlayerLivesAtZeroEvent cPlayerLivesAtZeroEvent)
    {
        Debug.Log(4);
        if (cPlayerLivesAtZeroEvent.Player == 1)
        {
            Destroy(Player1.IngameAvatar);
            player1Active = false;
        }
        else if (cPlayerLivesAtZeroEvent.Player == 2)
        {
            Destroy(Player2.IngameAvatar);
            player2Active = false;
        }
        CheckPlayersAlive();
    }

    private void CheckPlayersAlive()
    {
        if (!player1Active && !player2Active)
        {
            EventBus<AllPlayersDeadEvent>.Publish(new AllPlayersDeadEvent());
        }
    }

    private void ChangePlayerScore(PlayerScoredEvent pPlayerScoredEvent)
    {
        if (pPlayerScoredEvent.Player == 1)
        {
            Player1.Score += pPlayerScoredEvent.Score;
            Debug.Log($"Player 1 Score: {Player1.Score}");
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(1, Player1.Score));
        }
        else if (pPlayerScoredEvent.Player == 2)
        {
            Player2.Score += pPlayerScoredEvent.Score;
            Debug.Log($"Player 2 Score: {Player2.Score}");
            EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(2, Player2.Score));
        }
    }
}
