using UnityEngine;

public class PlayerProjectileHit : MonoBehaviour
{
    [SerializeField]
    private string[] 
        targetTags,
        ignoreTags;

    private void OnTriggerEnter(Collider _other)
    {
        for (int i = 0; i < targetTags.Length; i++)
        {
            if (_other.CompareTag(targetTags[i]))
            {                
                Destroy(_other.gameObject);
                Destroy(gameObject);
                return;
            }
        }
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            if (_other.CompareTag(ignoreTags[i]))
            {
                return;
            }
        }       
        Destroy(gameObject);
    }
}
