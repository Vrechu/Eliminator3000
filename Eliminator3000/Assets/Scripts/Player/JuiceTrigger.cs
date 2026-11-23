using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JuiceTrigger : MonoBehaviour
{
    public string effectName = "Boost";
    public LayerMask targetLayers = ~0;
    public bool onlyOnce = true;
    bool used;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (used && onlyOnce) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        if (JuiceEffects.Instance == null) return;
        JuiceEffects.Instance.Play(effectName);
        used = true;
    }
}