using UnityEngine;
using UnityEngine.UI;

public class UpdateGUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI 
        p1LivesGUI, p2LivesGUI, 
        winGUI,
        loseGUI;

    private ProfileManager profileManager;

    private void Start()
    {
        profileManager = ProfileManager.Instance;
        DisableUI();
    }

    public void SetLivesUI()
    {
        if (profileManager.player1Active) 
            p1LivesGUI.text = profileManager.Player1.Lives.ToString();  
        if (profileManager.player2Active)
            p2LivesGUI.text = profileManager.Player2.Lives.ToString();
    }

    private void DisableUI()
    {
        if (winGUI.enabled) winGUI.enabled = false;
        if (loseGUI.enabled) loseGUI.enabled = false;
        if (p1LivesGUI.enabled) p1LivesGUI.enabled = false;
        if (p2LivesGUI.enabled) p2LivesGUI.enabled = false;
    }

    public void EnableWinGUI()
    {
        winGUI.enabled = true;
    }

    public void EnableLoseGUI()
    {
        loseGUI.enabled = true;
    }

    public void EnableP1GUI()
    {
        p1LivesGUI.enabled = true;
    }

    public void EnableP2GUI()
    {
        p2LivesGUI.enabled = true;
    }
}
