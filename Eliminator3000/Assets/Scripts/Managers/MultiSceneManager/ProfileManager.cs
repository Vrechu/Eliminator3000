using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;
/// <summary>
/// DEPRECATED: This class is no longer used. Use PlayerProfileManager instead.
/// </summary>
public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    public PlayerProfile Player1, Player2;
    public bool player1Alive { get; private set; }
    public bool player2Alive { get; private set; }

    [SerializeField]
    private GameObject player1Prefab, player2Prefab;

    private bool wasdJoined = false, arrowsJoined = false;

    private GameStateManager gameStateManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
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
        EventBus<PlayerLivesAtZeroEvent>.Subscribe(OnPlayerLivesAtZero);
        EventBus<PlayerSpawnEvent>.Subscribe(OnPlayerSpawn);
        EventBus<LevelEnteredEvent>.Subscribe(OnLevelEntered);
        EventBus<LevelExitEvent>.Subscribe(OnGameEnd);
    }

    private void OnDestroy()
    {
        EventBus<PlayerLivesAtZeroEvent>.UnSubscribe(OnPlayerLivesAtZero);
        EventBus<PlayerSpawnEvent>.UnSubscribe(OnPlayerSpawn);
        EventBus<LevelEnteredEvent>.UnSubscribe(OnLevelEntered);
        EventBus<LevelExitEvent>.UnSubscribe(OnGameEnd);
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        PlayerJoin();
    }

    /// <summary>
    /// Waits for players to join the game using the keyboard. Player 1 can join by pressing the space key, and Player 2 can join by pressing the right control key. 
    /// This method checks if the game is in a state that allows joining (Pregame, Ingame, or Paused) and ensures that each player can only join once.
    /// </summary>
    private void PlayerJoin()
    {
        if (Keyboard.current == null) return;
        if (gameStateManager.CurrentState == GameStateManager.GameState.Pregame
            || gameStateManager.CurrentState == GameStateManager.GameState.Ingame
            || gameStateManager.CurrentState == GameStateManager.GameState.Paused)
        {
            if (!wasdJoined
                && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlayerInput player = PlayerInput.Instantiate(
                    player1Prefab,
                    controlScheme: "WASD",
                    pairWithDevice: Keyboard.current);

                NewProfile(1, player1Prefab, player, player.gameObject);
                EventBus<PlayerJoinedEvent>.Publish(new PlayerJoinedEvent(1));

                wasdJoined = true;
            }

            if (!arrowsJoined
                && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
            {
                PlayerInput player = PlayerInput.Instantiate(
                    player2Prefab,
                    controlScheme: "Arrows",
                    pairWithDevice: Keyboard.current);

                NewProfile(2, player2Prefab, player, player.gameObject);
                EventBus<PlayerJoinedEvent>.Publish(new PlayerJoinedEvent(2));
                arrowsJoined = true;
            }
        }
    }

    /// <summary>
    /// Returns an array containing all player profiles.
    /// </summary>
    /// <returns>An array of PlayerProfile objects.</returns>
    public PlayerProfile[] AllProfiles()
    {
        return new PlayerProfile[] { Player1, Player2 };
    }

    private void NewProfile(int _player,
        GameObject _playerPrefab,
        PlayerInput _playerInput,
        GameObject _ingameAvatar)
    {
        if (_player == 1)
        {
            Player1 = new PlayerProfile(_playerPrefab, _playerInput, _ingameAvatar);
            player1Alive = true;
        }
        else if (_player == 2)
        {
            Player2 = new PlayerProfile(_playerPrefab, _playerInput, _ingameAvatar);
            player2Alive = true;
        }
        else
        {
            Debug.LogWarning("Invalid player number. 1 or 2 expected.");
        }
    }

    /// <summary>
    /// Destroys the ingame avatar and player input of the specified player and updates their alive status. 
    /// After destroying the player, it checks if both players are dead and publishes an AllPlayersDeadEvent if they are.
    /// </summary>
    /// <param name="_player">The player to be destroyed.</param>
    private void DestroyPlayer(int _player)
    {
        if (_player == 1)
        {
            Destroy(Player1.IngameAvatar);
            Destroy(Player1.PlayerInput);
            player1Alive = false;
        }
        else if (_player == 2)
        {
            Destroy(Player2.IngameAvatar);
            Destroy(Player2.PlayerInput);
            player2Alive = false;
        }
        CheckPlayersAlive();
    }

    private void OnPlayerLivesAtZero(PlayerLivesAtZeroEvent _playerLivesAtZeroEvent)
    {
        DestroyPlayer(_playerLivesAtZeroEvent.Player);
    }

    /// <summary>
    /// Destroys both players when a LevelExitEvent is received, indicating the end of the game. 
    /// It also resets the joined status for both players.
    /// </summary>
    /// <param name="_levelExitEvent">Event info</param>
    private void OnGameEnd(LevelExitEvent _levelExitEvent)
    {
        DestroyPlayer(1);
        wasdJoined = false;
        DestroyPlayer(2);
        arrowsJoined = false;
    }

    /// <summary>
    /// Checks if both players are dead and publishes an AllPlayersDeadEvent if they are.
    /// </summary>
    private void CheckPlayersAlive()
    {
        if (player1Alive || player2Alive) return;
        EventBus<BothPlayersDeadEvent>.Publish(new BothPlayersDeadEvent());
    }

    /// <summary>
    /// Moves the ingame avatar of the player to the specified spawn point when a PlayerSpawnEvent is received.
    /// </summary>
    /// <param name="_playerSpawnEvent">The event containing information about the player and the spawn point.</param>
    private void OnPlayerSpawn(PlayerSpawnEvent _playerSpawnEvent)
    {
        if (_playerSpawnEvent.Player == 1)
        {
            Player1.IngameAvatar.transform.position = _playerSpawnEvent.SpawnPoint.position;
        }
        else if (_playerSpawnEvent.Player == 2)
        {
            Player2.IngameAvatar.transform.position = _playerSpawnEvent.SpawnPoint.position;
        }
    }


    private void OnLevelEntered(LevelEnteredEvent _levelEnteredEvent)
    {
        player1Alive = false;
        player2Alive = false;
    }
}