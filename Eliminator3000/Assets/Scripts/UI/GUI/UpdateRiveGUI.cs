using Rive;
using Rive.Components;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

public class UpdateRiveGUI : MonoBehaviour
{
    [Header("PLAYER GUI OBJECTS")]    
    [SerializeField] private GameObject player0GUIObject;
    [SerializeField] private GameObject player1GUIObject;

    [Header("HEALTH AND LIVES"), SerializeField]
    private bool showHealthAndLives = false;
    [SerializeField] private RiveWidget p0HealthWidget;
    [SerializeField] private RiveWidget p1HealthWidget;

    [SerializeField] private string healthAmountPropertyName, livesAmountPropertyName;
    private ViewModelInstanceNumberProperty
        p0HealthAmountProperty, p0LivesAmountProperty,
        p1HealthAmountProperty, p1LivesAmountProperty;

    [Header("GUNS AND AMMO"),SerializeField] private bool showGunsAndAmmo = false;
    [Header("Player 0 gun widgets: "), SerializeField] private RiveWidget p0BaseGunWidget;
    [SerializeField] private RiveWidget p0RapidGunWidget, p0ClusterGunWidget, p0BoomerangGunWidget, p0HomingGunWidget;
    [Header("Player 1 gun widgets: "), SerializeField] private RiveWidget p1BaseGunWidget;
    [SerializeField] private RiveWidget p1RapidGunWidget, p1ClusterGunWidget, p1BoomerangGunWidget, p1HomingGunWidget;

    [Header("Gun Property names: "), SerializeField] private string gunStatePropertyName;
    [SerializeField] private string gunIconPropertyName;
    [SerializeField] private string gunAmmoPropertyName;

    private ViewModelInstanceEnumProperty
        p0BaseGunStateProperty, p0RapidGunStateProperty, p0ClusterGunStateProperty, p0BoomerangGunStateProperty, p0HomingGunStateProperty, // p0
        p1BaseGunStateProperty, p1RapidGunStateProperty, p1ClusterGunStateProperty, p1BoomerangGunStateProperty, p1HomingGunStateProperty,
        p0BaseGunIconProperty, p0RapidGunIconProperty, p0ClusterGunIconProperty, p0BoomerangGunIconProperty, p0HomingGunIconProperty, // p1
        p1BaseGunIconProperty, p1RapidGunIconProperty, p1ClusterGunIconProperty, p1BoomerangGunIconProperty, p1HomingGunIconProperty;

    private ViewModelInstanceEnumProperty[] p0GunStateProperties, p1GunStateProperties, p0GunIconProperties, p1GunIconProperties;

    private ViewModelInstanceNumberProperty 
        p0BaseGunAmmoProperty, p0RapidGunAmmoProperty, p0ClusterGunAmmoProperty, p0BoomerangGunAmmoProperty, p0HomingGunAmmoProperty,
        p1BaseGunAmmoProperty, p1RapidGunAmmoProperty, p1ClusterGunAmmoProperty, p1BoomerangGunAmmoProperty, p1HomingGunAmmoProperty;
    private ViewModelInstanceNumberProperty[] p0GunAmmoProperties, p1GunAmmoProperties;
    [Header("SCORE AND MULTIPLIER"), SerializeField]
    private bool showScoreAndMultiplier = false;
    [SerializeField] private RiveWidget p0ScoreWidget;
    [SerializeField] private RiveWidget p1ScoreWidget;
    [SerializeField] private string levelScoreAmountPropertyName, totalScoreAmountPropertyName, multiplierAmountPropertyname;
    private ViewModelInstanceNumberProperty
        p0LevelScoreAmountProperty, p0TotalScoreAmountProperty, p0MultiplierAmountProperty,
        p1LevelScoreAmountProperty, p1TotalScoreAmountProperty, p1MultiplierAmountProperty;

    //private bool levelScoreCalculated = false;

    private void OnEnable()
    {
        if (showGunsAndAmmo) ListGunProperties();

        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerAvatarInitiated);

        if (showHealthAndLives)
        {
            if (p0HealthWidget != null) p0HealthWidget.OnWidgetStatusChanged += HandleHealthWidgetStatusChanges;
            if (p1HealthWidget != null) p1HealthWidget.OnWidgetStatusChanged += HandleHealthWidgetStatusChanges;
            EventBus<PlayerLivesChangedEvent>.Subscribe(SetLivesGUI);
            EventBus<PlayerHealthChangedEvent>.Subscribe(SetHealthGUI);
        }

        if (showGunsAndAmmo)
        {
            if (p0BaseGunWidget != null) p0BaseGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p0RapidGunWidget != null) p0RapidGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p0ClusterGunWidget != null) p0ClusterGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p0BoomerangGunWidget != null) p0BoomerangGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p0HomingGunWidget != null) p0HomingGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p1BaseGunWidget != null) p1BaseGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p1RapidGunWidget != null) p1RapidGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p1ClusterGunWidget != null) p1ClusterGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p1BoomerangGunWidget != null) p1BoomerangGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            if (p1HomingGunWidget != null) p1HomingGunWidget.OnWidgetStatusChanged += HandleGunWidgetStatusChanges;
            EventBus<PlayerGunSwapEvent>.Subscribe(OnGunSwap);
            EventBus<PlayerGunAmmoChangedEvent>.Subscribe(OnAmmoChanged);
        }

        if(showScoreAndMultiplier)
        {
            if (p0ScoreWidget != null) p0ScoreWidget.OnWidgetStatusChanged += HandleScoreWidgetStatusChanges;
            if (p1ScoreWidget != null) p1ScoreWidget.OnWidgetStatusChanged += HandleScoreWidgetStatusChanges;
            EventBus<ScoreChangedEvent>.Subscribe(SetLevelScoreGUI);
            EventBus<PlayerScoreMultiplierChangedEvent>.Subscribe(SetMultiplierGUI);
        }

        //EventBus<GameWinEvent>.Subscribe(EnableWinGUI);
        //EventBus<GameLoseEvent>.Subscribe(EnableLoseGUIgame);
        //EventBus<LevelScoreCalculatedEvent>.Subscribe(OnLevelScoreCalculated);
    }

    private void OnDestroy()
    {
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerAvatarInitiated);

        if (p0HealthWidget != null) p0HealthWidget.OnWidgetStatusChanged -= HandleHealthWidgetStatusChanges;
        if (p1HealthWidget != null) p1HealthWidget.OnWidgetStatusChanged -= HandleHealthWidgetStatusChanges;
        EventBus<PlayerLivesChangedEvent>.UnSubscribe(SetLivesGUI);
        EventBus<PlayerHealthChangedEvent>.UnSubscribe(SetHealthGUI);

        if (p0BaseGunWidget != null) p0BaseGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p0RapidGunWidget != null) p0RapidGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p0ClusterGunWidget != null) p0ClusterGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p0BoomerangGunWidget != null) p0BoomerangGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p0HomingGunWidget != null) p0HomingGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p1BaseGunWidget != null) p1BaseGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p1RapidGunWidget != null) p1RapidGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p1ClusterGunWidget != null) p1ClusterGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p1BoomerangGunWidget != null) p1BoomerangGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        if (p1HomingGunWidget != null) p1HomingGunWidget.OnWidgetStatusChanged -= HandleGunWidgetStatusChanges;
        EventBus<PlayerGunSwapEvent>.UnSubscribe(OnGunSwap);
        EventBus<PlayerGunAmmoChangedEvent>.UnSubscribe(OnAmmoChanged);

        if (p0ScoreWidget != null) p0ScoreWidget.OnWidgetStatusChanged -= HandleScoreWidgetStatusChanges;
        if (p1ScoreWidget != null) p1ScoreWidget.OnWidgetStatusChanged -= HandleScoreWidgetStatusChanges;
        EventBus<ScoreChangedEvent>.UnSubscribe(SetLevelScoreGUI);
        EventBus<PlayerScoreMultiplierChangedEvent>.UnSubscribe(SetMultiplierGUI);


        //EventBus<GameWinEvent>.UnSubscribe(EnableWinGUI);
        //EventBus<GameLoseEvent>.UnSubscribe(EnableLoseGUIgame);
        //EventBus<LevelScoreCalculatedEvent>.UnSubscribe(OnLevelScoreCalculated);
    }

    private void Start()
    {
        if (showGunsAndAmmo) SetGunIcons();
        DisableGUI();
    }

    private void DisableGUI()
    {
        //if (winGUI.enabled) winGUI.enabled = false;
        //if (loseGUI.enabled) loseGUI.enabled = false;
        //if (finalScoreGUI.enabled) finalScoreGUI.enabled = false;

        if (player0GUIObject.activeSelf) player0GUIObject.SetActive(false);
        if (player1GUIObject.activeSelf) player1GUIObject.SetActive(false);
    }

    private void ListGunProperties()
    {
        if (p0BaseGunStateProperty != null && p0RapidGunStateProperty != null && p0ClusterGunStateProperty != null && p0BoomerangGunStateProperty != null && p0HomingGunStateProperty != null)
        {
            p0GunStateProperties = new ViewModelInstanceEnumProperty[] { p0BaseGunStateProperty, p0RapidGunStateProperty, p0ClusterGunStateProperty, p0BoomerangGunStateProperty, p0HomingGunStateProperty };
        }
        if (p1BaseGunStateProperty != null && p1RapidGunStateProperty != null && p1ClusterGunStateProperty != null && p1BoomerangGunStateProperty != null && p1HomingGunStateProperty != null)
        {
            p1GunStateProperties = new ViewModelInstanceEnumProperty[] { p1BaseGunStateProperty, p1RapidGunStateProperty, p1ClusterGunStateProperty, p1BoomerangGunStateProperty, p1HomingGunStateProperty };
        }

        if (p0BaseGunIconProperty != null && p0RapidGunIconProperty != null && p0ClusterGunIconProperty != null && p0BoomerangGunIconProperty != null && p0HomingGunIconProperty != null)
        {
            p0GunIconProperties = new ViewModelInstanceEnumProperty[] { p0BaseGunIconProperty, p0RapidGunIconProperty, p0ClusterGunIconProperty, p0BoomerangGunIconProperty, p0HomingGunIconProperty };
        }
        if (p1BaseGunIconProperty != null && p1RapidGunIconProperty != null && p1ClusterGunIconProperty != null && p1BoomerangGunIconProperty != null && p1HomingGunIconProperty != null)
        {
            p1GunIconProperties = new ViewModelInstanceEnumProperty[] { p1BaseGunIconProperty, p1RapidGunIconProperty, p1ClusterGunIconProperty, p1BoomerangGunIconProperty, p1HomingGunIconProperty };
        }

        if (p0BaseGunAmmoProperty != null && p0RapidGunAmmoProperty != null && p0ClusterGunAmmoProperty != null && p0BoomerangGunAmmoProperty != null && p0HomingGunAmmoProperty != null)
        {
            p0GunAmmoProperties = new ViewModelInstanceNumberProperty[] { p0BaseGunAmmoProperty, p0RapidGunAmmoProperty, p0ClusterGunAmmoProperty, p0BoomerangGunAmmoProperty, p0HomingGunAmmoProperty };
        }
        if (p1BaseGunAmmoProperty != null && p1RapidGunAmmoProperty != null && p1ClusterGunAmmoProperty != null && p1BoomerangGunAmmoProperty != null && p1HomingGunAmmoProperty != null)
        {
            p1GunAmmoProperties = new ViewModelInstanceNumberProperty[] { p1BaseGunAmmoProperty, p1RapidGunAmmoProperty, p1ClusterGunAmmoProperty, p1BoomerangGunAmmoProperty, p1HomingGunAmmoProperty };
        }
    }

    private void HandleHealthWidgetStatusChanges()
    {
        if (!showHealthAndLives) return;
        if (p0HealthWidget.Status == WidgetStatus.Loaded)
        {
            p0HealthAmountProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(healthAmountPropertyName);
            p0LivesAmountProperty = p0HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(livesAmountPropertyName);
        }
        if (p1HealthWidget.Status == WidgetStatus.Loaded)
        {
            p1HealthAmountProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(healthAmountPropertyName);
            p1LivesAmountProperty = p1HealthWidget.StateMachine.ViewModelInstance.GetNumberProperty(livesAmountPropertyName);
        }
    }

    private void HandleGunWidgetStatusChanges()
    {
        if (!showGunsAndAmmo) return;

        if (p0BaseGunWidget.Status == WidgetStatus.Loaded)
        {
            p0BaseGunStateProperty = p0BaseGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p0BaseGunIconProperty = p0BaseGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p0BaseGunAmmoProperty = p0BaseGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p0RapidGunWidget.Status == WidgetStatus.Loaded)
        {
            p0RapidGunStateProperty = p0RapidGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p0RapidGunIconProperty = p0RapidGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p0RapidGunAmmoProperty = p0RapidGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p0ClusterGunWidget.Status == WidgetStatus.Loaded)
        {
            p0ClusterGunStateProperty = p0ClusterGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p0ClusterGunIconProperty = p0ClusterGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p0ClusterGunAmmoProperty = p0ClusterGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p0BoomerangGunWidget.Status == WidgetStatus.Loaded)
        {
            p0BoomerangGunStateProperty = p0BoomerangGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p0BoomerangGunIconProperty = p0BoomerangGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p0BoomerangGunAmmoProperty = p0BoomerangGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p0HomingGunWidget.Status == WidgetStatus.Loaded)
        {
            p0HomingGunStateProperty = p0HomingGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p0HomingGunIconProperty = p0HomingGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p0HomingGunAmmoProperty = p0HomingGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p1BaseGunWidget.Status == WidgetStatus.Loaded)
        {
            p1BaseGunStateProperty = p1BaseGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p1BaseGunIconProperty = p1BaseGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p1BaseGunAmmoProperty = p1BaseGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p1RapidGunWidget.Status == WidgetStatus.Loaded)
        {
            p1RapidGunStateProperty = p1RapidGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p1RapidGunIconProperty = p1RapidGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p1RapidGunAmmoProperty = p1RapidGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p1ClusterGunWidget.Status == WidgetStatus.Loaded)
        {
            p1ClusterGunStateProperty = p1ClusterGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p1ClusterGunIconProperty = p1ClusterGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p1ClusterGunAmmoProperty = p1ClusterGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p1BoomerangGunWidget.Status == WidgetStatus.Loaded)
        {
            p1BoomerangGunStateProperty = p1BoomerangGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p1BoomerangGunIconProperty = p1BoomerangGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p1BoomerangGunAmmoProperty = p1BoomerangGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }

        if (p1HomingGunWidget.Status == WidgetStatus.Loaded)
        {
            p1HomingGunStateProperty = p1HomingGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunStatePropertyName);
            p1HomingGunIconProperty = p1HomingGunWidget.StateMachine.ViewModelInstance.GetEnumProperty(gunIconPropertyName);
            p1HomingGunAmmoProperty = p1HomingGunWidget.StateMachine.ViewModelInstance.GetNumberProperty(gunAmmoPropertyName);
        }        
    }

    private void HandleScoreWidgetStatusChanges()
    {
        if (!showScoreAndMultiplier) return;
        if (p0ScoreWidget.Status == WidgetStatus.Loaded)
        {
            p0LevelScoreAmountProperty = p0ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(levelScoreAmountPropertyName);
            p0TotalScoreAmountProperty = p0ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(totalScoreAmountPropertyName);
            p0MultiplierAmountProperty = p0ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(multiplierAmountPropertyname);
        }
        if (p1ScoreWidget.Status == WidgetStatus.Loaded)
        {
            p1LevelScoreAmountProperty = p1ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(levelScoreAmountPropertyName);
            p1TotalScoreAmountProperty = p1ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(totalScoreAmountPropertyName);
            p1MultiplierAmountProperty = p1ScoreWidget.StateMachine.ViewModelInstance.GetNumberProperty(multiplierAmountPropertyname);
        }
    }

    private void OnPlayerAvatarInitiated(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        EnablePlayerGUI(_playerAvatarInstantiatedEvent.PlayerIndex, true);
    }

    private void EnablePlayerGUI(int _playerIndex, bool _enable)
    {
        if (_playerIndex == 0)
        {
            player0GUIObject.SetActive(_enable);
        }
        else if (_playerIndex == 1)
        {
            player1GUIObject.SetActive(_enable);
        }
    }

    private void SetLivesGUI(PlayerLivesChangedEvent _playerLivesChangedEvent)
    {
        if (!showHealthAndLives) return;
        if (_playerLivesChangedEvent.Player == 0)
        {
            p0LivesAmountProperty.Value = _playerLivesChangedEvent.NewLives;
        }
        else if (_playerLivesChangedEvent.Player == 1)
        {
            p1LivesAmountProperty.Value = _playerLivesChangedEvent.NewLives;
        }
    }

    private void SetHealthGUI(PlayerHealthChangedEvent _playerHealthChangedEvent)
    {
        if (!showHealthAndLives) return;
        if (_playerHealthChangedEvent.Player == 0)
        {
            p0HealthAmountProperty.Value = _playerHealthChangedEvent.NewHealth;
        }
        else if (_playerHealthChangedEvent.Player == 1)
        {
            p1HealthAmountProperty.Value = _playerHealthChangedEvent.NewHealth;
        }
    }

    private void SetLevelScoreGUI(ScoreChangedEvent _scoreChangedEvent)
    {
        if (!showScoreAndMultiplier) return;
        if (_scoreChangedEvent.PlayerIndex == 0)
        {
            p0LevelScoreAmountProperty.Value = _scoreChangedEvent.NewScore;
        }
        else if (_scoreChangedEvent.PlayerIndex == 1)
        {
            p1LevelScoreAmountProperty.Value = _scoreChangedEvent.NewScore;
        }
    }

    private void SetMultiplierGUI(PlayerScoreMultiplierChangedEvent _playerScoreMultiplierChangedEvent)
    {
        if (!showScoreAndMultiplier) return;
        if (_playerScoreMultiplierChangedEvent.Player == 0)
        {
            p0LevelScoreAmountProperty.Value = _playerScoreMultiplierChangedEvent.NewMultiplier;
        }
        else if (_playerScoreMultiplierChangedEvent.Player == 1)
        {
            p1LevelScoreAmountProperty.Value = _playerScoreMultiplierChangedEvent.NewMultiplier;
        }
    }

    private void SetGunIcons()
    {
        for (int i = 0; i < p0GunIconProperties.Length; i++)
        {
            switch (i)
            {
                case 0:
                    p0GunIconProperties[i].Value = "Default";
                    p1GunIconProperties[i].Value = "Default";
                    break;
                case 1:
                    p0GunIconProperties[i].Value = "RapidFire";
                    p1GunIconProperties[i].Value = "RapidFire";
                    break;
                case 2:
                    p0GunIconProperties[i].Value = "WeaponBarrage";
                    p1GunIconProperties[i].Value = "WeaponBarrage";
                    break;
                case 3:
                    p0GunIconProperties[i].Value = "Boomerang";
                    p1GunIconProperties[i].Value = "Boomerang";
                    break;
                case 4:
                    p0GunIconProperties[i].Value = "Homing";
                    p1GunIconProperties[i].Value = "Homing";
                    break;
            }
        }
    }


    private void OnGunSwap(PlayerGunSwapEvent _playerGunSwapEvent)
    {
        SwitchGunGUI(_playerGunSwapEvent.Player, _playerGunSwapEvent.GunIndex, _playerGunSwapEvent.AmmoCounts);
    }

    private void SwitchGunGUI(int _playerIndex, int _gunIndex, int[] _ammos)
    {
        if (!showGunsAndAmmo) return;
        if (_playerIndex == 0)
        {
            if (_gunIndex >= p0GunStateProperties.Length || _gunIndex < 0)
            {
                Debug.LogError("GunIndex Out of bounds!");
                return;
            }

            for (int i = 0; p0GunStateProperties.Length > i; i++)
            {
                if (i == _gunIndex)
                {
                    p0GunStateProperties[i].Value = "Selected";
                    continue;
                }
                if (_ammos[i] > 0)
                {
                    p0GunStateProperties[i].Value = "Active";
                    continue;
                }
                if (_ammos[i] < 0)
                {
                    p0GunStateProperties[i].Value = "Unequipped";
                    continue;
                }
                p0GunStateProperties[i].Value = "Inactive";
            }
        }
        if (_playerIndex == 1)
        {
            if (_gunIndex >= p1GunStateProperties.Length || _gunIndex < 0)
            {
                Debug.LogError("GunIndex Out of bounds!");
                return;
            }

            for (int i = 0; p1GunStateProperties.Length > i; i++)
            {
                if (i == _gunIndex)
                {
                    p1GunStateProperties[i].Value = "Selected";
                    continue;
                }
                if (_ammos[i] > 0)
                {
                    p1GunStateProperties[i].Value = "Active";
                    continue;
                }
                if (_ammos[i] < 0)
                {
                    p1GunStateProperties[i].Value = "Unequipped";
                    continue;
                }
                p1GunStateProperties[i].Value = "Inactive";
            }
        }
    }

    private void OnAmmoChanged(PlayerGunAmmoChangedEvent _playerGunAmmoChangedEvent)
    {
        if (!showGunsAndAmmo) return;
        if (_playerGunAmmoChangedEvent.PlayerIndex == 0)
        {
            if (_playerGunAmmoChangedEvent.GunIndex >= p0GunAmmoProperties.Length || _playerGunAmmoChangedEvent.GunIndex < 0)
            {
                Debug.LogError("GunIndex Out of bounds!");
                return;
            }
            p0GunAmmoProperties[_playerGunAmmoChangedEvent.GunIndex].Value = _playerGunAmmoChangedEvent.NewAmmo;
        }
        if (_playerGunAmmoChangedEvent.PlayerIndex == 1)
        {
            if (_playerGunAmmoChangedEvent.GunIndex >= p1GunAmmoProperties.Length || _playerGunAmmoChangedEvent.GunIndex < 0)
            {
                Debug.LogError("GunIndex Out of bounds!");
                return;
            }
            p1GunAmmoProperties[_playerGunAmmoChangedEvent.GunIndex].Value = _playerGunAmmoChangedEvent.NewAmmo;
        }
    }


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
