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

    private bool wasdJoined = false;
    private bool arrowsJoined = false;

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
    }

    private void OnDestroy()
    {
        EventBus<PlayerLivesAtZeroEvent>.UnSubscribe(DestroyPlayer);
        EventBus<PlayerSpawnEvent>.UnSubscribe(OnPlayerSpawn);
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        PlayerJoin();
    }

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

    private void DestroyPlayer(PlayerLivesAtZeroEvent _playerLivesAtZeroEvent)
    {
        if (_playerLivesAtZeroEvent.Player == 1)
        {
            Destroy(Player1.IngameAvatar);
            player1Active = false;
        }
        else if (_playerLivesAtZeroEvent.Player == 2)
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
}