using UnityEngine;

public class BlackHoleBehaviour : MonoBehaviour
{
    public int PlayerID = -1;
    [SerializeField] private float blackHoleLifeTime = 10;
    [SerializeField] private float suckSpeed = 50;
    private Timer despawnTimer;

    private void Start()
    {
        EventBus<BlackHoleSpawnEvent>.Publish(new BlackHoleSpawnEvent(PlayerID, transform.position, suckSpeed));
        despawnTimer = new Timer(blackHoleLifeTime);
    }

    private void Update()
    {
        if (despawnTimer.IsFinished())
        {
            EventBus<BlackHoleDespawnEvent>.Publish(new BlackHoleDespawnEvent(PlayerID));
            Destroy(gameObject);
        }
    }
}
