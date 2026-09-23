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

    private ProfileManager profileManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        EventBus<AllPlayersDeadEvent>.Subscribe(OnAllPlayersDead);
        EventBus<FinishedLevelEvent>.Subscribe(OnFinishedLevel);
        EventBus<LevelEnteredEvent>.Subscribe(OnLevelEntered);
    }

    private void OnDestroy()
    {
        EventBus<AllPlayersDeadEvent>.UnSubscribe(OnAllPlayersDead);
        EventBus<FinishedLevelEvent>.UnSubscribe(OnFinishedLevel);
        EventBus<LevelEnteredEvent>.UnSubscribe(OnLevelEntered);
    }

    private void Start()
    {
        profileManager = ProfileManager.Instance;
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

        if (!(profileManager.player1Alive
            || profileManager.player2Alive)) return;

        if (CurrentState == GameState.Pregame
        || CurrentState == GameState.Paused)

        {
            CurrentState = GameState.Ingame;
            EventBus<GameStartEvent>.Publish(new GameStartEvent());
        }

        else if (CurrentState == GameState.Ingame)
        {
            CurrentState = GameState.Paused;
            EventBus<GamePauseEvent>.Publish(new GamePauseEvent());
        }
    }

    /// <summary>
    /// Sets the game state to Lost and publishes a GameLoseEvent when all players are dead.
    /// </summary>
    /// <param name="_allPlayersDeadEvent">The event data for all players dead.</param>
    private void OnAllPlayersDead(AllPlayersDeadEvent _allPlayersDeadEvent)
    {
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

    private void OnLevelEntered(LevelEnteredEvent _levelEnteredEvent)
    {
        if (_levelEnteredEvent.LevelNumber == 0)
        {
            CurrentState = GameState.MainMenu;
        }
        else
        {
            CurrentState = GameState.Pregame;
        }
    }
}