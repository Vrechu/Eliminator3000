using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    public PlayerProfile Player1, Player2;
    public bool player1Active = false, player2Active = false;

    [SerializeField]
    private GameObject player1Prefab, player2Prefab;
    private Transform p1SpawnPoint, p2SpawnPoint;

    private bool wasdJoined = false, arrowsJoined = false;

    private GameStateManager gameStateManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //Debug.LogWarning("Multiple instances of ProfileManager detected. Destroying duplicate.");
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
        EventBus<PlayerLivesAtZeroEvent>.Subscribe(DestroyPlayer);
        EventBus<PlayerSpawnEvent>.Subscribe(OnPlayerSpawn);
        EventBus<LevelEnteredEvent>.Subscribe(OnLevelEntered);
    }

    private void OnDestroy()
    {
        EventBus<PlayerLivesAtZeroEvent>.UnSubscribe(DestroyPlayer);
        EventBus<PlayerSpawnEvent>.UnSubscribe(OnPlayerSpawn);
        EventBus<LevelEnteredEvent>.UnSubscribe(OnLevelEntered);
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
    /// creates a new player profile when a player joins the game, and publishes a PlayerJoinedEvent to notify other systems of the new player.
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
    }

    /// <summary>
    /// Destroys the ingame avatar of the player whose lives have reached zero, and checks if both players are dead to publish an AllPlayersDeadEvent.
    /// </summary>
    /// <param name="_playerLivesAtZeroEvent">The event containing information about the player whose lives have reached zero.</param>
    private void DestroyPlayer(PlayerLivesAtZeroEvent _playerLivesAtZeroEvent)
    {
        if (_playerLivesAtZeroEvent.Player == 1)
        {
            Destroy(Player1.IngameAvatar);
            wasdJoined = false;
            Destroy(Player1.PlayerInput);
            player1Active = false;
        }
        else if (_playerLivesAtZeroEvent.Player == 2)
        {
            Destroy(Player2.IngameAvatar);
            arrowsJoined = false;
            Destroy(Player2.PlayerInput);
            player2Active = false;
        }
        CheckPlayersAlive();
    }

    /// <summary>
    /// Checks if both players are dead and publishes an AllPlayersDeadEvent if they are.
    /// </summary>
    private void CheckPlayersAlive()
    {
        if (!player1Active && !player2Active)
        {
            EventBus<AllPlayersDeadEvent>.Publish(new AllPlayersDeadEvent());
        }
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
        player1Active = false;
        player2Active = false;
    }
}