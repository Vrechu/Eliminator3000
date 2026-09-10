using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    public UnityEvent OnLivesChanged;
    public UnityEvent OnP1LivesAtZero;
    public UnityEvent OnP2LivesAtZero;

    private ProfileManager profileManager;


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

    private void Start()
    {
        profileManager = ProfileManager.Instance;
    }

    public void LoseLife(int player, int amount)
    {
        if (player == 1)
        {
            profileManager.Player1.Lives -= amount;
        }
        else if (player == 2)
        {
            profileManager.Player2.Lives -= amount;
        }

        CheckLives(player);
    }
    
    public void CheckLives(int player)
    {
        OnLivesChanged?.Invoke();
        Debug.Log(player + " has "+ profileManager.AllProfiles()[player - 1].Lives);
        if (profileManager.AllProfiles()[player - 1].Lives <= 0)
        {
            if (player == 1)
            {
                OnP1LivesAtZero?.Invoke();
            }
            else if (player == 2)
            {
                OnP2LivesAtZero?.Invoke();
            }
        }
    }
}
