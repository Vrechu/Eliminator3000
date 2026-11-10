using UnityEngine;

public class PlayerProjectileHit : MonoBehaviour
{
    public int PlayerID;
    [SerializeField] LayerMask targetLayers;
    [SerializeField] private int damage = 1;

    /// <summary>
    /// When the projectile collides with another object, check if the object is in the target layer mask. 
    /// If it is, destroy the object and publish a PlayerScoredEvent with the player's ID 
    /// and the score gained from destroying the object.
    /// </summary>
    /// <param name="_other">The collider of the object the projectile has collided with.</param>
    private void OnTriggerEnter(Collider _other)
    {
        if (targetLayers == (targetLayers | (1 << _other.gameObject.layer)))
        {
            int scoreGained;
            if (_other.gameObject.TryGetComponent(out EnemyInfo EnemyInfo))
            {
                scoreGained = EnemyInfo.ScoreValue;
                EnemyInfo.TakeDamage(damage, PlayerID);
            }
            else
            {
                scoreGained = 0;
                Destroy(_other.gameObject);
                Debug.LogWarning($"PlayerProjectileHit: Object {_other.gameObject.name} does not have an EnemyInfo component. Score gained is set to 0.");
            }
        }
        Destroy(gameObject);
    }
}
