using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
    public int PlayerID = -1;
    [SerializeField] private float dummyLifeTime = 10;
    private Timer despawnTimer;

    private void Start()
    {
        EventBus<DummySpawnEvent>.Publish(new DummySpawnEvent(PlayerID, transform.position));
        despawnTimer = new Timer(dummyLifeTime);
    }

    private void Update()
    {
        if (despawnTimer.IsFinished())
        {
            EventBus<DummyDespawnEvent>.Publish(new DummyDespawnEvent(PlayerID));
            Destroy(gameObject);
        }
    }
}
