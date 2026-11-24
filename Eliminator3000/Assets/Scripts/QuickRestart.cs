using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class QuickRestart : MonoBehaviour
{
    [SerializeField] private bool enableQuickRestart = true;
    private void Update()
    {
        if (!enableQuickRestart) return;
        if (Keyboard.current.rKey.wasPressedThisFrame
            || (Keyboard.current.vKey.isPressed && Keyboard.current.bKey.wasPressedThisFrame)
            || (Keyboard.current.numpad1Key.isPressed && Keyboard.current.numpad2Key.wasPressedThisFrame))
        {
            PlayerProfileManager.Instance.ResetProfiles();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
