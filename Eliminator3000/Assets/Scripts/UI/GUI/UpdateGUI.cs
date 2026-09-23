using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UpdateGUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI
        p1LivesGUI, p2LivesGUI,
        p1ScoreGUI, p2ScoreGUI,
        winGUI,
        loseGUI;
    [SerializeField]
    private GameObject
        p1GunGUI, p2GunGUI,
        p1BaseGunGUI, p2BaseGunGUI,
        p1BigGunGUI, p2BigGunGUI;

    private void OnEnable()
    {
        EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
        EventBus<PlayerJoinedEvent>.Subscribe(EnablePlayerGUI);
        EventBus<ScoreChangedEvent>.Subscribe(SetScoreGUI);
        EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        EventBus<PlayerGunPickupEvent>.Subscribe(EnableBigGun);
    }

    private void OnDestroy()
    {
        EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        EventBus<PlayerJoinedEvent>.UnSubscribe(EnablePlayerGUI);
        EventBus<ScoreChangedEvent>.UnSubscribe(SetScoreGUI);
        EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        EventBus<PlayerGunPickupEvent>.UnSubscribe(EnableBigGun);
    }

    private void Start()
    {
        DisableGUI();
    }

    private void SetLivesGUI(PlayerLivesChangedEvent _playerLivesChangedEvent)
    {
        if (_playerLivesChangedEvent.Player == 1)
        {
            p1LivesGUI.text = _playerLivesChangedEvent.Lives.ToString();
        }
        else if (_playerLivesChangedEvent.Player == 2)
        {
            p2LivesGUI.text = _playerLivesChangedEvent.Lives.ToString();
        }
    }

    private void SetScoreGUI(ScoreChangedEvent _scoreChangedEvent)
    {
        if (_scoreChangedEvent.Player == 1)
        {
            p1ScoreGUI.text = _scoreChangedEvent.Score.ToString();
        }
        else if (_scoreChangedEvent.Player == 2)
        {
            p2ScoreGUI.text = _scoreChangedEvent.Score.ToString();
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

        if (p1GunGUI.activeSelf) p1GunGUI.SetActive(false);
        if (p2GunGUI.activeSelf) p2GunGUI.SetActive(false);
        if (p1BaseGunGUI.activeSelf) p1BaseGunGUI.SetActive(false);
        if (p2BaseGunGUI.activeSelf) p2BaseGunGUI.SetActive(false);
        if (p1BigGunGUI.activeSelf) p1BigGunGUI.SetActive(false);
        if (p2BigGunGUI.activeSelf) p2BigGunGUI.SetActive(false);
    }

    private void EnableWinGUI(GameWinEvent _winEvent)
    {
        winGUI.enabled = true;
    }

    private void EnableLoseGUIgame(GameLoseEvent _loseEvent)
    {
        loseGUI.enabled = true;
    }

    private void EnablePlayerGUI(PlayerJoinedEvent _playerJoinedEvent)
    {
        if (_playerJoinedEvent.PlayerProfile == 1)
        {
            p1LivesGUI.enabled = true;
            p1ScoreGUI.enabled = true;
            p1GunGUI.SetActive(true);
            p1BaseGunGUI.SetActive(true);
        }
        else if (_playerJoinedEvent.PlayerProfile == 2)
        {
            p2LivesGUI.enabled = true;
            p2ScoreGUI.enabled = true;
            p2GunGUI.SetActive(true);
            p2BaseGunGUI.SetActive(true);
        }
    }

    private void EnableBigGun(PlayerGunPickupEvent _playerGunPickupEvent)
    {
        if (_playerGunPickupEvent.Player == 1)
        {
            p1BaseGunGUI.SetActive(false);
            p1BigGunGUI.SetActive(true);
        }
        else if (_playerGunPickupEvent.Player == 2)
        {
            p2BaseGunGUI.SetActive(false);
            p2BigGunGUI.SetActive(true);
        }
    }
}
