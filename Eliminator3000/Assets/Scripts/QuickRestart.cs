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
            || (Keyboard.current.hKey.isPressed && Keyboard.current.nKey.wasPressedThisFrame)
            || (Keyboard.current.numpad6Key.isPressed && Keyboard.current.numpad3Key.wasPressedThisFrame))
        {
            PlayerProfileManager.Instance.ResetProfiles();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
