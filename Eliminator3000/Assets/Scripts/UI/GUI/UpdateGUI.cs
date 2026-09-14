using UnityEngine;
using UnityEngine.UI;

public class UpdateGUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI 
        p1LivesGUI, p2LivesGUI, 
        p1ScoreGUI, p2ScoreGUI,
        winGUI,
        loseGUI;

    private ProfileManager profileManager;

    private void OnEnable()
    {
        EventBus<ScoreChangedEvent>.Subscribe(SetScoreGUI);
    }

    private void OnDestroy()
    {
        EventBus<ScoreChangedEvent>.UnSubscribe(SetScoreGUI);
    }

    private void Start()
    {
        profileManager = ProfileManager.Instance;
        DisableGUI();
    }

    public void SetLivesGUI()
    {
        if (profileManager.player1Active) 
            p1LivesGUI.text = profileManager.Player1.Lives.ToString();  
        if (profileManager.player2Active)
            p2LivesGUI.text = profileManager.Player2.Lives.ToString();
    }

    public void SetScoreGUI(ScoreChangedEvent pScoreChangedEvent)
    {
        if (pScoreChangedEvent.Player == 1)
        {
            p1ScoreGUI.text = pScoreChangedEvent.Score.ToString();
        }
        else if (pScoreChangedEvent.Player == 2)
        {
            p2ScoreGUI.text = pScoreChangedEvent.Score.ToString();
        }
    }

    private void DisableGUI()
    {
        if (winGUI.enabled) winGUI.enabled = false;
        if (loseGUI.enabled) loseGUI.enabled = false;
        if (p1LivesGUI.enabled) p1LivesGUI.enabled = false;
        if (p2LivesGUI.enabled) p2LivesGUI.enabled = false;
        if (p1ScoreGUI.enabled) p1ScoreGUI.enabled = false;
        if (p2ScoreGUI.enabled) p2ScoreGUI.enabled = false;
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
        p1ScoreGUI.enabled = true;
    }

    public void EnableP2GUI()
    {
        p2LivesGUI.enabled = true;
        p2ScoreGUI.enabled = true;
    }
}
