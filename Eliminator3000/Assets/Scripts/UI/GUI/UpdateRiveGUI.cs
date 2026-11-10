using Rive;
using Rive.Components;
using System.Collections.Generic;
using UnityEngine;

public class RiveTest : MonoBehaviour
{
    #region player 0
    [SerializeField] private RiveWidget p0HealthWidget, p0GunWidget, p0ScoreWidget;
    [SerializeField] private string p0HealthPropertyName, p0LivesPropertyName, p0BaseGunUnlockedPropertyName,
        p0RapidGunUnlockedPropertyName, p0ClusterGunUnlockedPropertyName,
        p0BaseGunAmmoPropertyName, p0RapidGunAmmoPropertyName, p0ClusterGunAmmoPropertyName,
        p0ScorePropertyName;
    private ViewModelInstanceNumberProperty p0HealthProperty, p0LivesProperty,
        p0BaseGunUnlockedProperty, p0RapidGunUnlockedProperty, p0ClusterGunUnlockedProperty,
        p0BaseGunAmmoProperty, p0RapidGunAmmoProperty, p0ClusterGunAmmoProperty,
        p0ScoreProperty;
    #endregion

    #region player 1
    [SerializeField] private RiveWidget p1HealthWidget, p1GunWidget, p1ScoreWidget;
    [SerializeField] private string p1HealthPropertyName, p1LivesPropertyName, p1BaseGunUnlockedPropertyName,
        p1RapidGunUnlockedPropertyName, p1ClusterGunUnlockedPropertyName,
        p1BaseGunAmmoPropertyName, p1RapidGunAmmoPropertyName, p1ClusterGunAmmoPropertyName,
        p1ScorePropertyName;
    private ViewModelInstanceNumberProperty p1HealthProperty, p1LivesProperty,
        p1BaseGunUnlockedProperty, p1RapidGunUnlockedProperty, p1ClusterGunUnlockedProperty,
        p1BaseGunAmmoProperty, p1RapidGunAmmoProperty, p1ClusterGunAmmoProperty,
        p1ScoreProperty;
    #endregion

   
    //private bool levelScoreCalculated = false;

    private void OnEnable()
    {
        p0HealthWidget.OnWidgetStatusChanged += HandleHealthWidgetStatusChanges;

        //EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerAvatarInitiated);
        EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.Subscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.Subscribe(SetScoreGUI);
        //EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        //EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        //EventBus<PlayerGunSwapEvent>.Subscribe(OnGunSwap);
        //EventBus<PlayerGunAmmoChangedEvent>.Subscribe(OnAmmoChanged);
        //EventBus<LevelScoreCalculatedEvent>.Subscribe(OnLevelScoreCalculated);
    }

    private void OnDestroy()
    {
        p0HealthWidget.OnWidgetStatusChanged -= HandleHealthWidgetStatusChanges;

        //EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerAvatarInitiated);
        EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.UnSubscribe(SetHealthGUI);
        EventBus<ScoreChangedEvent>.UnSubscribe(SetScoreGUI);
        //EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        //EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        //EventBus<PlayerGunSwapEvent>.UnSubscribe(OnGunSwap);
        //EventBus<PlayerGunAmmoChangedEvent>.UnSubscribe(OnAmmoChanged);
        //EventBus<LevelScoreCalculatedEvent>.UnSubscribe(OnLevelScoreCalculated);
    }

    private void HandleHealthWidgetStatusChanges()
    {
        if (p0HealthWidget.Status == WidgetStatus.Loaded)
        {
            p0HealthProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0HealthPropertyName);
            p0LivesProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0LivesPropertyName);
        }
        if (p1HealthWidget.Status == WidgetStatus.Loaded)
        {
            p1HealthProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1HealthPropertyName);
            p1LivesProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1LivesPropertyName);
        }
    }

    private void HandleGunWidgetStatusChanges()
    {
        if (p0GunWidget.Status == WidgetStatus.Loaded)
        {
            p0BaseGunUnlockedProperty = p0GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0BaseGunUnlockedPropertyName);
            p0RapidGunUnlockedProperty = p0GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0RapidGunUnlockedPropertyName);
            p0ClusterGunUnlockedProperty = p0GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0ClusterGunUnlockedPropertyName);
            p0BaseGunAmmoProperty = p0GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0BaseGunAmmoPropertyName);
            p0RapidGunAmmoProperty = p0GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0RapidGunAmmoPropertyName);
            p0ClusterGunAmmoProperty = p0GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0ClusterGunAmmoPropertyName);
        }
        if (p1GunWidget.Status == WidgetStatus.Loaded)
        {
            p1BaseGunUnlockedProperty = p1GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1BaseGunUnlockedPropertyName);
            p1RapidGunUnlockedProperty = p1GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1RapidGunUnlockedPropertyName);
            p1ClusterGunUnlockedProperty = p1GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1ClusterGunUnlockedPropertyName);
            p1BaseGunAmmoProperty = p1GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1BaseGunAmmoPropertyName);
            p1RapidGunAmmoProperty = p1GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1RapidGunAmmoPropertyName);
            p1ClusterGunAmmoProperty = p1GunWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1ClusterGunAmmoPropertyName);
        }
    }

    private void HandleScoreWidgetStatusChanges()
    {
        if (p0ScoreWidget.Status == WidgetStatus.Loaded)
        {
            p0ScoreProperty = p0ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(p0ScorePropertyName);
        }
        if (p1ScoreWidget.Status == WidgetStatus.Loaded)
        {
            p1ScoreProperty = p1ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(p1ScorePropertyName);
        }
    }

    private void Start()
    {

        //DisableGUI();
    }

    /*private void DisableGUI()
    {
        if (winGUI.enabled) winGUI.enabled = false;
        if (loseGUI.enabled) loseGUI.enabled = false;
        if (finalScoreGUI.enabled) finalScoreGUI.enabled = false;

        if (player1GUI.activeSelf) player1GUI.SetActive(false);
        if (player2GUI.activeSelf) player2GUI.SetActive(false);
    }*/

    /*private void OnPlayerAvatarInitiated(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        EnablePlayerGUI(_playerAvatarInstantiatedEvent.PlayerIndex, true);
    }*/

    /*private void EnablePlayerGUI(int _playerIndex, bool _enable)
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
    }*/


    private void SetLivesGUI(PlayerLivesChangedEvent _playerLivesChangedEvent)
    {
        if (_playerLivesChangedEvent.Player == 0)
        {
            p0LivesProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty("livesAmount");
        }
        else if (_playerLivesChangedEvent.Player == 1)
        {
            p1LivesProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty("livesAmount");
        }
    }

    private void SetHealthGUI(PlayerHealthChangedEvent _playerHealthChangedEvent)
    {
        if (_playerHealthChangedEvent.Player == 0)
        {
            p0HealthProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty("healthAmount");
        }
        else if (_playerHealthChangedEvent.Player == 1)
        {
            p1HealthProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty("healthAmount");
        }
    }

    private void SetScoreGUI(ScoreChangedEvent _scoreChangedEvent)
    {
        if (_scoreChangedEvent.Player == 0)
        {
            p0ScoreProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty("scoreAmount");
        }
        else if (_scoreChangedEvent.Player == 1)
        {
            p1ScoreProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty("scoreAmount");
        }
    }


    /*private void DisableGunGUI(int _playerIndex)
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


    }*/


    /*private void OnGunSwap(PlayerGunSwapEvent _playerGunSwapEvent)
    {
        SwitchGunGUI(_playerGunSwapEvent.Player, _playerGunSwapEvent.GunIndex);
    }*/

    /*private void SwitchGunGUI(int _playerIndex, int _gunIndex)
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
    }*/


    /*private void OnAmmoChanged(PlayerGunAmmoChangedEvent _playerGunAmmoChangedEvent)
    {
        if (_playerGunAmmoChangedEvent.Player == 0)
        {
            if (_playerGunAmmoChangedEvent.InfiniteAmmo)
            {
                p1AmmoGUI.text = "∞";
            }
            else
                p1AmmoGUI.text = _playerGunAmmoChangedEvent.NewAmmo.ToString();
        }
        else if (_playerGunAmmoChangedEvent.Player == 1)
        {
            if (_playerGunAmmoChangedEvent.InfiniteAmmo)
            {
                p2AmmoGUI.text = "∞";
            }
            else
                p2AmmoGUI.text = _playerGunAmmoChangedEvent.NewAmmo.ToString();
        }
    }*/




    /*private void EnableWinGUI(GameWinEvent _winEvent)
    {
        winGUI.enabled = true;
        finalScoreGUI.enabled = true;
    }

    private void EnableLoseGUIgame(GameLoseEvent _loseEvent)
    {
        loseGUI.enabled = true;
        finalScoreGUI.enabled = true;
    }*/


    /*private void OnLevelScoreCalculated(LevelScoreCalculatedEvent _levelScoreCalculatedEvent)
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
    }*/


}
