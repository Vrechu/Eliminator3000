using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    private ProfileManager profileManager;


    private void OnEnable()
    {
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
    }

    private void OnDestroy()
    {
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
    }

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

    private void OnPlayerHit(PlayerHitEvent context)
    {
        LoseLife(context.Player, context.Damage);
    }


    public void LoseLife(int player, int amount)
    {
        Debug.Log(1);
        if (player == 1)
        {
            profileManager.Player1.Lives -= amount;
        }
        else if (player == 2)
        {
            profileManager.Player2.Lives -= amount;
        }
        EventBus<PlayerLivesChangedEvent>.Publish(new PlayerLivesChangedEvent(player, profileManager.AllProfiles()[player - 1].Lives));
        CheckLives(player);
    }

    public void CheckLives(int player)
    {
        Debug.Log(2);
        if (profileManager.AllProfiles()[player - 1].Lives <= 0)
        {
            Debug.Log(3);
            EventBus<PlayerLivesAtZeroEvent>.Publish(new (player));
        }
    }
}
