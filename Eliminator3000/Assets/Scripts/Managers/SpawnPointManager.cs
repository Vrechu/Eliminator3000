using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance { get; private set; }
    [SerializeField] private Transform p1SpawnPoint, p2SpawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        EventBus<PlayerJoinedEvent>.Subscribe(OnPlayerJoined);
    }

    private void OnDestroy()
    {
        EventBus<PlayerJoinedEvent>.UnSubscribe(OnPlayerJoined);
    }

    public Transform GetSpawnPoint(int _playerNumber)
    {
        return _playerNumber switch
        {
            1 => p1SpawnPoint,
            2 => p2SpawnPoint,
            _ => null
        };
    }

    public void OnPlayerJoined(PlayerJoinedEvent _playerJoinedEvent)
    {
        if (_playerJoinedEvent.PlayerProfile == 1)
        {
            EventBus<PlayerSpawnEvent>.Publish(new PlayerSpawnEvent(1, p1SpawnPoint));
        }
        else if (_playerJoinedEvent.PlayerProfile == 2)
        {
            EventBus<PlayerSpawnEvent>.Publish(new PlayerSpawnEvent(2, p2SpawnPoint));
        }
    }
}
