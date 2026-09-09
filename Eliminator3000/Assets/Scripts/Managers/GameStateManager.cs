using UnityEngine;
using UnityEngine.Events;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

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

    public void LoseGame()
    {
        OnLose?.Invoke();
        Debug.Log("Game Over!");
    }

    public void WinGame()
    {
        OnWin?.Invoke();
        Debug.Log("You Win!");
    }
}
