using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class QuickRestart : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerProfileManager.Instance.ResetProfiles();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
