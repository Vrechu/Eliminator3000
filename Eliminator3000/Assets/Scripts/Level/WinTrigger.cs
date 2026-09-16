using UnityEngine;
using UnityEngine.Events;

public class WinTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            GameStateManager.Instance.WinGame();
        }
    }
}
