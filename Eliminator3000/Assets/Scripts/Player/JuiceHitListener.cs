using UnityEngine;

public class JuiceHitListener : MonoBehaviour
{
    public string effectName = "Hit";

    void OnEnable() { EventBus<PlayerHitEvent>.Subscribe(OnHit); }
    void OnDisable() { EventBus<PlayerHitEvent>.UnSubscribe(OnHit); }

    void OnHit(PlayerHitEvent e)
    {
        if (JuiceEffects.Instance != null) JuiceEffects.Instance.Play(effectName);
    }
}