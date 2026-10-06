using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerProfileManager : MonoBehaviour
{
    public static PlayerProfileManager Instance { get; private set; }

    public PlayerProfile[] PlayerProfiles { get; private set; } = new PlayerProfile[2];
    [SerializeField] private GameObject[] playerPrefabs;
    public bool[] PlayersJoined = new bool[2] { false, false };

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
        EventBus<MainMenuEnterEvent>.Subscribe(OnGameBoot);
    }

    private void OnDestroy()
    {
        EventBus<MainMenuEnterEvent>.UnSubscribe(OnGameBoot);
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        CheckJoinInput();
    }

    private void OnGameBoot(MainMenuEnterEvent _mainMenuEnterEvent)
    {
        ResetProfiles();
        CreateNewProfile(0);
    }

    private void CreateNewProfile(int _profileIndex)
    {
        if (_profileIndex < 0 || _profileIndex >= PlayerProfiles.Length) return;
        if (PlayersJoined[_profileIndex]) return;
        PlayerProfiles[_profileIndex] = new PlayerProfile(playerPrefabs[_profileIndex]);
        PlayersJoined[_profileIndex] = true;
        EventBus<PlayerJoinedEvent>.Publish(new PlayerJoinedEvent(_profileIndex));
    }


    public void ResetProfiles()
    {
        for (int i = 0; i < PlayerProfiles.Length; i++)
        {
            RemoveProfile(i);
        }
    }
    private void RemoveProfile(int _profileIndex)
    {
        PlayerProfiles[_profileIndex] = default;
        PlayersJoined[_profileIndex] = false;
    }

    private void CheckJoinInput()
    {
        if (Keyboard.current == null) return;
        if (!(gameStateManager.CurrentState == GameStateManager.GameState.MainMenu
            || gameStateManager.CurrentState == GameStateManager.GameState.Pregame
            || gameStateManager.CurrentState == GameStateManager.GameState.Ingame
            || gameStateManager.CurrentState == GameStateManager.GameState.Paused)) return;
        if (!PlayersJoined[0]
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CreateNewProfile(0);
            return;
        }

        if (!PlayersJoined[1]
                && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            CreateNewProfile(1);
            return;
        }
    }

    public bool BothPlayersAlive()
    {
        for (int i = 0; i < PlayerProfiles.Length; i++)
        {
            if (PlayersJoined[i] && !PlayerProfiles[i].AliveInLevel) return false;
        }
        return true;
    }

    public bool BothPlayersDead()
    {
        for (int i = 0; i < PlayerProfiles.Length; i++)
        {
            if (PlayerProfiles[i].AliveInLevel) return false;
        }
        return true;
    }
}
