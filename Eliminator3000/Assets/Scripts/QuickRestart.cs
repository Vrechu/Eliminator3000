using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class QuickRestart : MonoBehaviour
{
    [SerializeField] private bool enableQuickRestart = true, enableBackToMainMenu = true, enableNextlevel = true;
    [SerializeField] private string nextLevelName = "Level 2";

    private void Update()
    {
        if (enableQuickRestart) ResetLevel();
        if (enableBackToMainMenu) BackToMainMenu();
        if (enableNextlevel) NextLevel();
    }

    private void ResetLevel()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame
            || (Keyboard.current.hKey.isPressed && Keyboard.current.nKey.wasPressedThisFrame))
        {
            PlayerProfileManager.Instance.ResetProfiles();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void BackToMainMenu()
    {
               if (Keyboard.current.tKey.wasPressedThisFrame
            || (Keyboard.current.fKey.isPressed && Keyboard.current.numpad4Key.wasPressedThisFrame))
        {
            PlayerProfileManager.Instance.ResetProfiles();
            SceneManager.LoadScene("MenuScene");
        }
    }

    private void NextLevel()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame
            || (Keyboard.current.nKey.isPressed && Keyboard.current.hKey.wasPressedThisFrame))
        {
            PlayerProfileManager.Instance.ResetProfiles();
            SceneManager.LoadScene(nextLevelName);
        }
    }
}
