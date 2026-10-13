using UnityEngine;

public class DummyManager : MonoBehaviour
{
    public static DummyManager Instance;
    public bool[] DummiesUp { get; private set; }
    public Vector3 DummyPosition { get; private set; }

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
        DummiesUp = new bool[2] {false, false};
    }

    private void OnEnable()
    {
        EventBus<DummySpawnEvent>.Subscribe(OnDummySpawn);
        EventBus<DummyDespawnEvent>.Subscribe(OnDummyDespawn);
    }

    private void OnDestroy()
    {
        EventBus<DummySpawnEvent>.UnSubscribe(OnDummySpawn);
        EventBus<DummyDespawnEvent>.UnSubscribe(OnDummyDespawn);
    }

    private void OnDummySpawn(DummySpawnEvent _dummySpawnEvent)
    {
        DummiesUp[_dummySpawnEvent.Player] = true;
        DummyPosition = _dummySpawnEvent.DummyPosition;
    }

    private void OnDummyDespawn(DummyDespawnEvent _dummyDespawnEvent)
    {
        DummiesUp[_dummyDespawnEvent.Player] = false;
    }
}
