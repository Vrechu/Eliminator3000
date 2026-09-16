using UnityEngine;

public class PlayerProjectileHit : MonoBehaviour
{
    [SerializeField]
    private string[] 
        targetTags,
        ignoreTags;

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < targetTags.Length; i++)
        {
            if (other.CompareTag(targetTags[i]))
            {                
                Destroy(other.gameObject);
                Destroy(gameObject);
                return;
            }
        }
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            if (other.CompareTag(ignoreTags[i]))
            {
                return;
            }
        }       
        Destroy(gameObject);
    }
}
