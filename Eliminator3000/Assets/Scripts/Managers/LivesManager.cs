using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }
    [SerializeField]
    private int lives = 3;

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
        OnLivesChanged?.Invoke();
        CheckLives();
    }
    
    private void CheckLives()
    {
        if (lives <= 0)
        {
            OnLivesAtZero?.Invoke();
        }
    }
}
