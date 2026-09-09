using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }
    public int lives { get; private set; } = 3;

    public UnityEvent OnLivesChanged;
    public UnityEvent OnLivesAtZero;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of LivesManager detected. Destroying duplicate.");
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void LoseLife(int amount)
    {
        lives -= amount;
        CheckLives();
    }
    
    public void CheckLives()
    {
        OnLivesChanged?.Invoke();
        if (lives <= 0)
        {
            OnLivesAtZero?.Invoke();
        }
    }
}
