using UnityEngine;

public class EnemyProjectileHit : MonoBehaviour
{
    [SerializeField]
    private string[] ignoreTags;

    private void OnTriggerEnter(Collider _other)
    {
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            if (_other.CompareTag(ignoreTags[i]))
            {
                return;
            }
        }

        if (_other.CompareTag("Player1"))
        {
            EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, 1));            
        }
        if (_other.CompareTag("Player2"))
        {
            EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(2, 1));            
        }        
        Destroy(gameObject);
    }
}
