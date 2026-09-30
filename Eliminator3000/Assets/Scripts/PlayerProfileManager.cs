using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerProfileManager : MonoBehaviour
{
    public static PlayerProfileManager Instance { get; private set; }

    public PlayerProfile[] PlayerProfiles { get; private set; } = new PlayerProfile[2];
    [SerializeField] private GameObject[] playerPrefabs;
    private bool[] playersJoined = new bool[2] { false, false };

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
    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        CheckJoinInput();
    }

    private void CreateNewProfile(int _profileIndex)
    {
        switch (_profileIndex)
        {
            case 0:
                PlayerProfiles[_profileIndex] = new PlayerProfile(playerPrefabs[_profileIndex]);
                playersJoined[_profileIndex] = true;
                break;
            case 1:
                PlayerProfiles[_profileIndex] = new PlayerProfile(playerPrefabs[_profileIndex]);
                playersJoined[_profileIndex] = true;
                break;
            default:
                Debug.LogError("Invalid profile index: " + _profileIndex);
                break;
        }
    }

    private void RemoveProfile(int _profileIndex)
    {
        PlayerProfiles[_profileIndex] = default;
        playersJoined[_profileIndex] = false;
    }

    private void ResetProfiles()
    {
        for (int i = 0; i < PlayerProfiles.Length; i++)
        {
            RemoveProfile(i);
        }
    }

    private void CheckJoinInput()
    {
        if (Keyboard.current == null) return;
        if (!(gameStateManager.CurrentState == GameStateManager.GameState.MainMenu
            || gameStateManager.CurrentState == GameStateManager.GameState.Pregame
            || gameStateManager.CurrentState == GameStateManager.GameState.Ingame
            || gameStateManager.CurrentState == GameStateManager.GameState.Paused)) return;

        if (!playersJoined[0]
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CreateNewProfile(0);
            return;
        }

        if (!playersJoined[1]
                && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            CreateNewProfile(1);
            return;
        }
    }
}
