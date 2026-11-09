using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Rive;
using System.ComponentModel;
using Rive.Components;

public class UpdateGUI : MonoBehaviour
{

    //player1
    [SerializeField] private GameObject player1GUI;
    [SerializeField]
    private TextMeshProUGUI
        p1LivesGUI, p1HealthGUI, p1ScoreGUI;
    [SerializeField]
    private GameObject
        p1GunGUI, p1BaseGunGUI, p1RapidGunGUI, p1ClusterGunGUI;

    //player2
    [SerializeField] private GameObject player2GUI;
    [SerializeField]
    private TextMeshProUGUI
        p2LivesGUI, p2HealthGUI, p2ScoreGUI;
    [SerializeField]
    private GameObject
        p2GunGUI, p2BaseGunGUI, p2RapidGunGUI, p2ClusterGunGUI;

    [SerializeField] private TextMeshProUGUI winGUI, loseGUI, finalScoreGUI;

    private bool levelScoreCalculated = false;

    private void OnEnable()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerAvatarInitiated);
        EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.Subscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.Subscribe(SetScoreGUI);
        EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        EventBus<PlayerGunSwapEvent>.Subscribe(SwitchGunGUI);
        EventBus<LevelScoreCalculatedEvent>.Subscribe(OnLevelScoreCalculated);
    }

    private void OnDestroy()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerAvatarInitiated);
        EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.UnSubscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.UnSubscribe(SetScoreGUI);
        EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        EventBus<PlayerGunSwapEvent>.UnSubscribe(SwitchGunGUI);
        EventBus<LevelScoreCalculatedEvent>.UnSubscribe(OnLevelScoreCalculated);
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
        if (finalScoreGUI.enabled) finalScoreGUI.enabled = false;

        if (player1GUI.activeSelf) player1GUI.SetActive(false);
        if (player2GUI.activeSelf) player2GUI.SetActive(false);

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
        finalScoreGUI.enabled = true;
    }

    private void EnableLoseGUIgame(GameLoseEvent _loseEvent)
    {
        loseGUI.enabled = true;
        finalScoreGUI.enabled = true;
    }

    private void OnPlayerAvatarInitiated(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        EnablePlayerGUI(_playerAvatarInstantiatedEvent.PlayerIndex, true);
    }


    private void EnablePlayerGUI(int _playerIndex, bool _enable)
    {
        if (_playerIndex == 0)
        {
            player1GUI.SetActive(_enable);
            p1BaseGunGUI.SetActive(_enable);
        }
        else if (_playerIndex == 1)
        {
            player2GUI.SetActive(_enable);
            p2BaseGunGUI.SetActive(_enable);
        }
    }

    private void SwitchGunGUI(PlayerGunSwapEvent _playerGunSwapEvent)
    {
        if (_playerGunSwapEvent.Player == 0)
        {
            DisableGunGUI(_playerGunSwapEvent.Player);
            switch (_playerGunSwapEvent.GunIndex)
            {
                case 0:
                    p1BaseGunGUI.SetActive(true);
                    break;
                case 1:
                    p1RapidGunGUI.SetActive(true);
                    break;
                case 2:
                    p1ClusterGunGUI.SetActive(true);
                    break;
            }

        }
        else if (_playerGunSwapEvent.Player == 1)
        {
            switch (_playerGunSwapEvent.GunIndex)
            {
                case 0:
                    p2BaseGunGUI.SetActive(true);
                    break;
                case 1:
                    p2RapidGunGUI.SetActive(true);
                    break;
                case 2:
                    p2ClusterGunGUI.SetActive(true);
                    break;
            }
        }
    }

    private void DisableGunGUI(int _playerIndex)
    {
        if (_playerIndex == 0)
        {
            p1BaseGunGUI.SetActive(false);
            p1RapidGunGUI.SetActive(false);
            p1ClusterGunGUI.SetActive(false);
        }
        else if (_playerIndex == 1)
        {
            p2BaseGunGUI.SetActive(false);
            p2RapidGunGUI.SetActive(false);
            p2ClusterGunGUI.SetActive(false);
        }
    }

    private void OnLevelScoreCalculated(LevelScoreCalculatedEvent _levelScoreCalculatedEvent)
    {
        if (levelScoreCalculated) return;
        levelScoreCalculated = true;
        List<KeyValuePair<int, int>> playerLevelScores = new List<KeyValuePair<int, int>> { new KeyValuePair<int, int>(0, 0), new KeyValuePair<int, int>(1, 0) };

        for (int i = 0; i < PlayerProfileManager.Instance.PlayerProfiles.Length; i++)
        {
            if (PlayerProfileManager.Instance.PlayersJoined[i])
            {
                playerLevelScores[i] = new KeyValuePair<int, int>(i, PlayerProfileManager.Instance.PlayerProfiles[i].LevelScores[_levelScoreCalculatedEvent.LevelIndex]);
            }
        }

        playerLevelScores.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
        finalScoreGUI.text = "Final Scores:\n";
        for (int i = 0; i < 2; i++)
        {
            if (!PlayerProfileManager.Instance.PlayersJoined[playerLevelScores[i].Key]) continue;
            finalScoreGUI.text += $"Player {playerLevelScores[i].Key + 1} Score: {playerLevelScores[i].Value} \n";
        }
    }
}