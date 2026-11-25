using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private bool showHealth = true, showGuns = true, showScore = true, showWin = true, showFinalScore = true, showP2JoinPopup = true;

    private bool p2JoinPopupActive = false, p2CanRequestJoin = true;
    [Header("Player joining"), SerializeField] private GameObject p2JoinConsentPopup;
    private Timer p2JoinConsentTimeOutTimer;
    private Timer p2JoinResetTimer;
    [SerializeField] private float p2JoinConsentTimeOutTime = 10f, p2JoinResetime = 20f;
    [SerializeField] private TextMeshProUGUI p2JoinConsentTimerVisual;
    bool p2joined = false;



    private void OnEnable()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerAvatarInitiated);
        if (showHealth) EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
        if (showHealth) EventBus<PlayerHealthChangedEvent>.Subscribe(SetHealthGUI);
        if (showScore) EventBus<PlayerScoreChangedEvent>.Subscribe(SetScoreGUI);
        if (showScore) EventBus<PlayerScoreMultiplierChangedEvent>.Subscribe(SetMultiplierGUI);
        if (showWin) EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        if (showWin) EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        if (showGuns) EventBus<PlayerGunSwapEvent>.Subscribe(OnGunSwap);
       if (showGuns) EventBus<PlayerGunAmmoChangedEvent>.Subscribe(OnAmmoChanged);
        if (showFinalScore)EventBus<LevelScoreCalculatedEvent>.Subscribe(OnLevelScoreCalculated);
    }

    private void OnDestroy()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerAvatarInitiated);
        if (showHealth) EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        if (showHealth) EventBus<PlayerHealthChangedEvent>.UnSubscribe(SetHealthGUI);
        if (showScore) EventBus<PlayerScoreChangedEvent>.UnSubscribe(SetScoreGUI);
        if (showWin) EventBus<PlayerScoreMultiplierChangedEvent>.UnSubscribe(SetMultiplierGUI);
        if (showWin) EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        if (showWin) EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        if (showGuns) EventBus<PlayerGunSwapEvent>.UnSubscribe(OnGunSwap);
        if (showGuns) EventBus<PlayerGunAmmoChangedEvent>.UnSubscribe(OnAmmoChanged);
        if (showFinalScore) EventBus<LevelScoreCalculatedEvent>.UnSubscribe(OnLevelScoreCalculated);
    }

    private void Start()
    {
        if (!showHealth)
        {
            p1HealthGUI.enabled = false;
            p2HealthGUI.enabled = false;
            p1LivesGUI.enabled = false;
            p2LivesGUI.enabled = false;
        }
        if (!showGuns)
        {
            p1GunGUI.SetActive(false);
            p2GunGUI.SetActive(false);
        }
        if(!showScore)
        {
            p1ScoreGUI.enabled = false;
            p2ScoreGUI.enabled = false;
            P1MultiplierGUI.enabled = false;
            P2MultiplierGUI.enabled = false;
        }

        DisableGUI();

        p2JoinConsentPopup.SetActive(false);
        p2JoinConsentTimeOutTimer = new Timer(p2JoinConsentTimeOutTime);
        p2JoinResetTimer = new Timer(p2JoinResetime);

        if (PlayerProfileManager.Instance.PlayersJoined[1])
        {
            p2CanRequestJoin = false;
        }
    }

    private void Update()
    {
        switch (GameStateManager.Instance.CurrentState)
        {
            case GameStateManager.GameState.Pregame:
            case GameStateManager.GameState.Ingame:
            case GameStateManager.GameState.Paused:
                P2JoinRequest();
                AwnserP2JoinConsentPopup();
                CountdownPopupTimers();
                break;
        }
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

    private void SetScoreGUI(PlayerScoreChangedEvent _scoreChangedEvent)
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


    // ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void SendJoinSignal(int _playerIdex)
    {
        EventBus<PlayerIngameJoinInputEvent>.Publish(new PlayerIngameJoinInputEvent(_playerIdex));
    }

    private void P2JoinRequest()
    {
        if (!showP2JoinPopup) return;
        if (PlayerProfileManager.Instance.PlayersJoined[1]) return;
        if (!PlayerProfileManager.Instance.PlayersJoined[0])
        {
            if (Keyboard.current == null) return;
            else if (Keyboard.current.digit1Key.wasReleasedThisFrame)
            {
                SendJoinSignal(0);
                return;
            }
        }
        
        if (p2joined) return;
        if (!p2CanRequestJoin) return;
        if (Keyboard.current == null) return;
        else if (Keyboard.current.digit2Key.wasReleasedThisFrame)
        {
            OpenP2JoinConsentPopup();
        }
    }

    private void OpenP2JoinConsentPopup()
    {
        p2JoinConsentPopup.SetActive(true);
        p2JoinPopupActive = true;
        p2JoinConsentTimeOutTimer.Reset(p2JoinConsentTimeOutTime);
        p2CanRequestJoin = false;
    }

    private void AwnserP2JoinConsentPopup()
    {
        if (!p2JoinPopupActive) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current.digit1Key.isPressed)
        {
            SendJoinSignal(1);
            CloseP2JoinConsentPopup();
            p2joined = true;
        }
        else if (Keyboard.current.digit2Key.isPressed)
        {
            CloseP2JoinConsentPopup();
            p2JoinResetTimer.Reset(p2JoinResetime);
        }
    }

    private void CloseP2JoinConsentPopup()
    {
        p2JoinConsentPopup.SetActive(false);
        p2JoinPopupActive = false;
    }

    private void CountdownPopupTimers()
    {
        if (p2JoinPopupActive)
        {
            if (p2JoinConsentTimeOutTimer.IsFinished())
            {
                CloseP2JoinConsentPopup();
                p2JoinResetTimer.Reset(p2JoinResetime);
            }
            p2JoinConsentTimerVisual.text = p2JoinConsentTimeOutTimer.TimeLeft.ToString("F1");
        }
        else if (!p2JoinPopupActive && !p2CanRequestJoin)
        {
            if (p2JoinResetTimer.IsFinished())
            {
                p2CanRequestJoin = true;
            }
        }
    }
}