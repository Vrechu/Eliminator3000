using UnityEngine;

public class BlackHoleManager : MonoBehaviour
{
    public static BlackHoleManager Instance;
    public bool[] BlackHolesUp { get; private set; }
    public Vector3 BlackHolePosition { get; private set; }
    public float BlackHoleSuckSpeed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        BlackHolesUp = new bool[2] { false, false };
    }

    private void OnEnable()
    {
        EventBus<BlackHoleSpawnEvent>.Subscribe(OnBlackHoleSpawn);
        EventBus<BlackHoleDespawnEvent>.Subscribe(OnBlackHoleDespawn);
    }

    private void OnDestroy()
    {
        EventBus<BlackHoleSpawnEvent>.UnSubscribe(OnBlackHoleSpawn);
        EventBus<BlackHoleDespawnEvent>.UnSubscribe(OnBlackHoleDespawn);
    }

    private void OnBlackHoleSpawn(BlackHoleSpawnEvent _blackHoleSpawnEvent)
    {
        BlackHolesUp[_blackHoleSpawnEvent.Player] = true;
        BlackHolePosition = _blackHoleSpawnEvent.BlackHolePosition;
        BlackHoleSuckSpeed = _blackHoleSpawnEvent.SuckSpeed;
    }

    private void OnBlackHoleDespawn(BlackHoleDespawnEvent _blackHoleDespawnEvent)
    {
        BlackHolesUp[_blackHoleDespawnEvent.Player] = false;
    }
}
