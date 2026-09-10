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

    public UnityEvent OnGameStart;
    public UnityEvent OnLose;
    public UnityEvent OnWin;



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
    private void Start()
    {
        CurrentState = GameState.Pregame;        
        Debug.Log("Game Started!");
    }
    private void Update()
    {
        PlayPauseGame();
    }

    public void LoseGame()
    {
        CurrentState = GameState.Lost;
        OnLose?.Invoke();
        Debug.Log("Game Over!");
    }

    public void WinGame()
    {
        CurrentState = GameState.Won;
        OnWin?.Invoke();
        Debug.Log("You Win!");
    }

    private void PlayPauseGame()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (CurrentState == GameState.Pregame)
            {
            CurrentState = GameState.Ingame;
            OnGameStart?.Invoke();
            }
            else if (CurrentState == GameState.Ingame)
            {
                CurrentState = GameState.Paused;
                Debug.Log("Game Paused!");
            }
        }
    }
}
