using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UpdateGUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI
        p1LivesGUI, p2LivesGUI,
        p1HealthGUI, p2HealthGUI,
        p1ScoreGUI, p2ScoreGUI,
        winGUI,
        loseGUI;
    [SerializeField]
    private GameObject
        p1GunGUI, p2GunGUI,
        p1BaseGunGUI, p2BaseGunGUI,
        p1RapidGunGUI, p2RapidGunGUI,
        p1ClusterGunGUI, p2ClusterGunGUI;

    private void OnEnable()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(EnablePlayerGUI);
        EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.Subscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.Subscribe(SetScoreGUI);
        EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        EventBus<PlayerGunSwapEvent>.Subscribe(SwitchGunGUI);
    }

    private void OnDestroy()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(EnablePlayerGUI);
        EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.UnSubscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.UnSubscribe(SetScoreGUI);
        EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        EventBus<PlayerGunSwapEvent>.UnSubscribe(SwitchGunGUI);
    }

    private void Start()
    {
        DisableGUI();
    }

    private void SetLivesGUI(PlayerLivesChangedEvent _playerLivesChangedEvent)
    {
        if (_playerLivesChangedEvent.Player == 0)
        {
            p1LivesGUI.text = _playerLivesChangedEvent.NewLives.ToString();
        }
        else if (_playerLivesChangedEvent.Player == 1)
        {
            p2LivesGUI.text = _playerLivesChangedEvent.NewLives.ToString();
        }
    }

    private void SetHealthGUI(PlayerHealthChangedEvent _playerHealthChangedEvent)
    {
        if (_playerHealthChangedEvent.Player == 0)
        {
            p1HealthGUI.text = _playerHealthChangedEvent.NewHealth.ToString();
        }
        else if (_playerHealthChangedEvent.Player == 1)
        {
            p2HealthGUI.text = _playerHealthChangedEvent.NewHealth.ToString();
        }
    }

    private void SetScoreGUI(ScoreChangedEvent _scoreChangedEvent)
    {
        if (_scoreChangedEvent.Player == 0)
        {
            p1ScoreGUI.text = _scoreChangedEvent.Score.ToString();
        }
        else if (_scoreChangedEvent.Player == 1)
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
        if (p1HealthGUI.enabled) p1HealthGUI.enabled = false;
        if (p2HealthGUI.enabled) p2HealthGUI.enabled = false;
        if (p1ScoreGUI.enabled) p1ScoreGUI.enabled = false;
        if (p2ScoreGUI.enabled) p2ScoreGUI.enabled = false;

        if (p1GunGUI.activeSelf) p1GunGUI.SetActive(false);
        if (p2GunGUI.activeSelf) p2GunGUI.SetActive(false);
        if (p1BaseGunGUI.activeSelf) p1BaseGunGUI.SetActive(false);
        if (p2BaseGunGUI.activeSelf) p2BaseGunGUI.SetActive(false);
        if (p1RapidGunGUI.activeSelf) p1RapidGunGUI.SetActive(false);
        if (p2RapidGunGUI.activeSelf) p2RapidGunGUI.SetActive(false);
        if (p1ClusterGunGUI.activeSelf) p1ClusterGunGUI.SetActive(false);
        if (p2ClusterGunGUI.activeSelf) p2ClusterGunGUI.SetActive(false);
    }

    private void EnableWinGUI(GameWinEvent _winEvent)
    {
        winGUI.enabled = true;
    }

    private void EnableLoseGUIgame(GameLoseEvent _loseEvent)
    {
        loseGUI.enabled = true;
    }

    private void EnablePlayerGUI(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        if (_playerAvatarInstantiatedEvent.PlayerIndex == 0)
        {
            p1LivesGUI.enabled = true;
            p1HealthGUI.enabled = true;
            p1ScoreGUI.enabled = true;
            p1GunGUI.SetActive(true);
            p1BaseGunGUI.SetActive(true);
        }
        else if (_playerAvatarInstantiatedEvent.PlayerIndex == 1)
        {
            p2LivesGUI.enabled = true;
            p2HealthGUI.enabled = true;
            p2ScoreGUI.enabled = true;
            p2GunGUI.SetActive(true);
            p2BaseGunGUI.SetActive(true);
        }
    }

    private void SwitchGunGUI(PlayerGunSwapEvent playerGunSwapEvent)
    {
        if (playerGunSwapEvent.Player == 0)
        {
            switch (playerGunSwapEvent.GunIndex)
            {
                case 0:
                    p1BaseGunGUI.SetActive(true);
                    p1RapidGunGUI.SetActive(false);
                    p1ClusterGunGUI.SetActive(false);

                    break;
                case 1:
                    p1BaseGunGUI.SetActive(false);
                    p1RapidGunGUI.SetActive(true);
                    p1ClusterGunGUI.SetActive(false);
                    break;
                case 2:
                    p1BaseGunGUI.SetActive(false);
                    p1RapidGunGUI.SetActive(false);
                    p1ClusterGunGUI.SetActive(true);
                    break;
            }

        }
        else if (playerGunSwapEvent.Player == 1)
        {
            switch (playerGunSwapEvent.GunIndex)
            {
                case 0:
                    p2BaseGunGUI.SetActive(true);
                    p2RapidGunGUI.SetActive(false);
                    p2ClusterGunGUI.SetActive(false);
                    break;
                case 1:
                    p2BaseGunGUI.SetActive(false);
                    p2RapidGunGUI.SetActive(true);
                    p2ClusterGunGUI.SetActive(false);
                    break;
                case 2:
                    p2BaseGunGUI.SetActive(false);
                    p2RapidGunGUI.SetActive(false);
                    p2ClusterGunGUI.SetActive(true);
                    break;
            }
        }
    }
}
