using UnityEngine;

public class PlayerProjectileHit : MonoBehaviour
{
    public int PlayerID;
    [SerializeField] LayerMask targetLayers;

    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            int scoreGained;
            if (_other.gameObject.TryGetComponent(out EnemyInfo EnemyInfo))
            {
                scoreGained = EnemyInfo.ScoreValue;
            }
            else
            {
                scoreGained = 0;
            }
            Destroy(_other.gameObject);
            EventBus<PlayerScoredEvent>.Publish(new PlayerScoredEvent(PlayerID, scoreGained));
        }
        Destroy(gameObject);
    }
}
