using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    public PlayerProfile Player1;
    public PlayerProfile Player2;

    public bool player1Active = false;
    public bool player2Active = false;

    public UnityEvent OnPlayer1Joined;
    public UnityEvent OnPlayer2Joined;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of ProfileManager detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        else 
        {
            Instance = this;
        }
    }

    public PlayerProfile[] AllProfiles()
    {
               return new PlayerProfile[] { Player1, Player2 };
    }

    public void NewProfile(int player, 
        GameObject playerPrefab, 
        PlayerInput playerInput)
    {
        if (player == 1)
        {
            Player1 = new PlayerProfile(playerPrefab, playerInput);
            Debug.Log(Player1.Lives);
            player1Active = true;
            OnPlayer1Joined?.Invoke();
        }
        else if (player == 2)
        {
            Player2 = new PlayerProfile(playerPrefab, playerInput);
            Debug.Log(Player2.Lives);
            player2Active = true;
            OnPlayer2Joined?.Invoke();
        }
        else
        {
            Debug.LogWarning("Invalid player number. 1 or 2 expected.");
        }
    }

    public void DestroyPlayer1()
    {
        Destroy(Player1.PlayerPrefab);
        player1Active = false;
        CheckPlayersAlive();
    }

    public void DestroyPlayer2()
    {
        Destroy(Player2.PlayerPrefab);
        player2Active = false;
        CheckPlayersAlive();
    }

    public void CheckPlayersAlive()
    {
        if (!player1Active && !player2Active)
        {
            GameStateManager.Instance.LoseGame();
        }
    }
}
