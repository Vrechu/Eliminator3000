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
    [SerializeField] private TextMeshProUGUI
        p1LivesGUI, p1HealthGUI, p1ScoreGUI, P1MultiplierGUI;
    [SerializeField] private GameObject
        p1GunGUI, p1BaseGunGUI, p1RapidGunGUI, p1ClusterGunGUI;
    [SerializeField] private TextMeshProUGUI p1AmmoGUI;
    //player2
    [SerializeField] private GameObject player2GUI;
    [SerializeField] private TextMeshProUGUI
        p2LivesGUI, p2HealthGUI, p2ScoreGUI, P2MultiplierGUI;
    [SerializeField]
    private GameObject
        p2GunGUI, p2BaseGunGUI, p2RapidGunGUI, p2ClusterGunGUI;
    [SerializeField] private TextMeshProUGUI p2AmmoGUI;

    [SerializeField] private TextMeshProUGUI winGUI, loseGUI, finalScoreGUI;

    private bool levelScoreCalculated = false;

    private void OnEnable()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerAvatarInitiated);
        EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.Subscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.Subscribe(SetScoreGUI);
        EventBus<PlayerScoreMultiplierChangedEvent>.Subscribe(SetMultiplierGUI);
        EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        EventBus<PlayerGunSwapEvent>.Subscribe(OnGunSwap);
        EventBus<PlayerGunAmmoChangedEvent>.Subscribe(OnAmmoChanged);
        EventBus<LevelScoreCalculatedEvent>.Subscribe(OnLevelScoreCalculated);
    }

    private void OnDestroy()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerAvatarInitiated);
        EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.UnSubscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.UnSubscribe(SetScoreGUI);
        EventBus<PlayerScoreMultiplierChangedEvent>.UnSubscribe(SetMultiplierGUI);
        EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        EventBus<PlayerGunSwapEvent>.UnSubscribe(OnGunSwap);
        EventBus<PlayerGunAmmoChangedEvent>.UnSubscribe(OnAmmoChanged);
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
        if (_scoreChangedEvent.PlayerIndex == 0)
        {
            p1ScoreGUI.text = _scoreChangedEvent.NewScore.ToString();
        }
        else if (_scoreChangedEvent.PlayerIndex == 1)
        {
            p2ScoreGUI.text = _scoreChangedEvent.NewScore.ToString();
        }
    }

    private void SetMultiplierGUI(PlayerScoreMultiplierChangedEvent _playerScoreMultiplierChangedEvent)
    {
        if (_playerScoreMultiplierChangedEvent.Player == 0)
        {
            P1MultiplierGUI.text = "X" + _playerScoreMultiplierChangedEvent.NewMultiplier.ToString();
        }
        else if (_playerScoreMultiplierChangedEvent.Player == 1)
        {
            P2MultiplierGUI.text = "X" + _playerScoreMultiplierChangedEvent.NewMultiplier.ToString();
        }
    }

    private void DisableGUI()
    {
        if (winGUI.enabled) winGUI.enabled = false;
        if (loseGUI.enabled) loseGUI.enabled = false;
        if (finalScoreGUI.enabled) finalScoreGUI.enabled = false;

        if (player1GUI.activeSelf) player1GUI.SetActive(false);
        if (player2GUI.activeSelf) player2GUI.SetActive(false);
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
            SwitchGunGUI(_playerIndex, 0);
        }
        else if (_playerIndex == 1)
        {
            player2GUI.SetActive(_enable);
            SwitchGunGUI(_playerIndex, 0);
        }
    }

    private void OnGunSwap(PlayerGunSwapEvent _playerGunSwapEvent)
    {
        SwitchGunGUI(_playerGunSwapEvent.Player, _playerGunSwapEvent.GunIndex);
    }

    private void SwitchGunGUI(int _playerIndex, int _gunIndex)
    {
        if (_playerIndex == 0)
        {
            DisableGunGUI(_playerIndex);
            switch (_gunIndex)
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
        else if (_playerIndex == 1)
        {
            switch (_gunIndex)
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
            if (p1BaseGunGUI.activeSelf) p1BaseGunGUI.SetActive(false);
            if (p1RapidGunGUI.activeSelf) p1RapidGunGUI.SetActive(false);
            if (p1ClusterGunGUI.activeSelf) p1ClusterGunGUI.SetActive(false);
        }
        else if (_playerIndex == 1)
        {
            if (p2BaseGunGUI.activeSelf) p2BaseGunGUI.SetActive(false);
            if (p2RapidGunGUI.activeSelf) p2RapidGunGUI.SetActive(false);
            if (p2ClusterGunGUI.activeSelf) p2ClusterGunGUI.SetActive(false);
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

    private void OnAmmoChanged(PlayerGunAmmoChangedEvent _playerGunAmmoChangedEvent)
    {
        if (_playerGunAmmoChangedEvent.PlayerIndex == 0)
        {
            if (_playerGunAmmoChangedEvent.InfiniteAmmo)
            {
                p1AmmoGUI.text = "∞";
            }
            else
            p1AmmoGUI.text = _playerGunAmmoChangedEvent.NewAmmo.ToString();
        }
        else if (_playerGunAmmoChangedEvent.PlayerIndex == 1)
        {
            if (_playerGunAmmoChangedEvent.InfiniteAmmo)
            {
                p2AmmoGUI.text = "∞";
            }
            else
                p2AmmoGUI.text = _playerGunAmmoChangedEvent.NewAmmo.ToString();
        }
    }
}