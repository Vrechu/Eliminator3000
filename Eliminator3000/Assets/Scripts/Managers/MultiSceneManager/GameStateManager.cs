using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Pregame,
        Ingame,
        Paused,
        Lost,
        Won,
        MainMenu
    }
    public GameState CurrentState { get; private set; }

    private PlayerProfileManager playerProfileManager;

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
        EventBus<BothPlayersDeadEvent>.Subscribe(OnAllPlayersDead);
        EventBus<FinishedLevelEvent>.Subscribe(OnFinishedLevel);
        EventBus<LevelEnteredEvent>.Subscribe(OnLevelEntered);
        EventBus<MainMenuEnterEvent>.Subscribe(OnMainMenuEntered);
    }

    private void OnDestroy()
    {
        EventBus<BothPlayersDeadEvent>.UnSubscribe(OnAllPlayersDead);
        EventBus<FinishedLevelEvent>.UnSubscribe(OnFinishedLevel);
        EventBus<LevelEnteredEvent>.UnSubscribe(OnLevelEntered);
        EventBus<MainMenuEnterEvent>.UnSubscribe(OnMainMenuEntered);
    }

    private void Start()
    {
        playerProfileManager = PlayerProfileManager.Instance;
    }

    private void Update()
    {
        PlayPauseGame();
    }    

    /// <summary>
    /// Toggles the game state between Ingame and Paused when the Enter key is pressed.
    /// </summary>
    private void PlayPauseGame()
    {
        if (!Keyboard.current.enterKey.wasPressedThisFrame) return;

        if (!playerProfileManager.BothPlayersAlive()) return;

        if (CurrentState == GameState.Pregame
        || CurrentState == GameState.Paused)

        {
            CurrentState = GameState.Ingame;
        }

        else if (CurrentState == GameState.Ingame)
        {
            CurrentState = GameState.Paused;
        }
    }

    /// <summary>
    /// Sets the game state to Lost and publishes a GameLoseEvent when all players are dead.
    /// </summary>
    /// <param name="_allPlayersDeadEvent">The event data for all players dead.</param>
    private void OnAllPlayersDead(BothPlayersDeadEvent _allPlayersDeadEvent)
    {
        Debug.Log("All players dead, game lost.");
        CurrentState = GameState.Lost;
        EventBus<GameLoseEvent>.Publish(new GameLoseEvent());
    }

    /// <summary>
    /// Sets the game state to Won and publishes a GameWinEvent when the level is finished.
    /// </summary>
    /// <param name="_finishedLevelEvent">The event data for the finished level.</param>
    private void OnFinishedLevel(FinishedLevelEvent _finishedLevelEvent)
    {
        CurrentState = GameState.Won;
        EventBus<GameWinEvent>.Publish(new GameWinEvent());
    }

    /// <summary>
    /// Sets the game state to MainMenu if the level number is 0, otherwise sets it to Pregame when a level is entered.
    /// </summary>
    /// <param name="_levelEnteredEvent">Event info</param>
    private void OnLevelEntered(LevelEnteredEvent _levelEnteredEvent)
    {
        if (_levelEnteredEvent.LevelNumber == 0)
        {
            Debug.LogError("Level number is 0, setting game state to MainMenu.");
        }
        else
        {
            CurrentState = GameState.Pregame;
        }
        Debug.Log($"Level {_levelEnteredEvent.LevelNumber} entered, setting game state to {CurrentState}.   ");
    }

    private void OnMainMenuEntered(MainMenuEnterEvent _mainMenuEnterEvent)
    {
        CurrentState = GameState.MainMenu;
    }
}