using UnityEngine;

public class EnemyProjectileHit : MonoBehaviour
{
    [SerializeField]
    private string[] ignoreTags;

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            if (other.CompareTag(ignoreTags[i]))
            {
                return;
            }
        }

        if (other.CompareTag("Player1"))
        {
            EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(1, 1));            
        }
        if (other.CompareTag("Player2"))
        {
            EventBus<PlayerHitEvent>.Publish(new PlayerHitEvent(2, 1));            
        }        
        Destroy(gameObject);
    }
}
