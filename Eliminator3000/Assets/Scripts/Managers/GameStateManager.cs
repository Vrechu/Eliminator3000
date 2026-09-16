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
        Won
    }
    public GameState CurrentState { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of GameStateManager detected. Destroying duplicate.");
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
    }

    private void OnDestroy()
    {
        EventBus<AllPlayersDeadEvent>.UnSubscribe(OnAllPlayersDead);
    }

    private void Start()
    {
        CurrentState = GameState.Pregame;
    }
    private void Update()
    {
        PlayPauseGame();
    }

    private void OnAllPlayersDead(AllPlayersDeadEvent pAllPlayersDeadEvent)
    {
        CurrentState = GameState.Lost;
        EventBus<GameLoseEvent>.Publish(new GameLoseEvent());
    }

    public void WinGame()
    {
        CurrentState = GameState.Won;
        EventBus<GameWinEvent>.Publish(new GameWinEvent());
    }

    private void PlayPauseGame()
    {
        if (!Keyboard.current.enterKey.wasPressedThisFrame) return;

        if (!(ProfileManager.Instance.player1Active
            || ProfileManager.Instance.player2Active)) return;

        if (CurrentState == GameState.Pregame
        || CurrentState == GameState.Paused)

        {
            CurrentState = GameState.Ingame;
            EventBus<GameStartEvent>.Publish(new GameStartEvent());
        }

        else if (CurrentState == GameState.Ingame)
        {
            CurrentState = GameState.Paused;
        }
    }
}