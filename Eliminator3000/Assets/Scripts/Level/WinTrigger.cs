using UnityEngine;
using UnityEngine.Events;

public class WinTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Player1") || _other.CompareTag("Player2"))
        {
            EventBus<FinishedLevelEvent>.Publish(new FinishedLevelEvent());
        }
    }
}
