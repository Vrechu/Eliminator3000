using UnityEngine;
using UnityEngine.UI;

public class UpdateGUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI livesGUI, winGUI,loseGUI;
    private LivesManager livesManager; 

    private void Start()
    {
        livesManager = LivesManager.Instance;
        DisableWinLose();
    }


    public void SetLivesUI()
    {
        livesGUI.text = "Lives: " + livesManager.lives;
    }

    private void DisableWinLose()
    {
        if (winGUI.enabled) winGUI.enabled = false;
        if (loseGUI.enabled) loseGUI.enabled = false;
    }

    public void EnableWinGUI()
    {
        winGUI.enabled = true;
    }

    public void EnableLoseGUI()
    {
        loseGUI.enabled = true;
    }
}
