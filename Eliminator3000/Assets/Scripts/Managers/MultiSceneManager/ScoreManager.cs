using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private PlayerProfileManager playerProfileManager;
    private int currentLevelIndex = 0;

    public List<KeyValuePair<string, int>> ScoreList = new List<KeyValuePair<string, int>>();

    [SerializeField] private string scoreFileName = "Scores";
    private string scoreFilePath;

    [Header("Multiplier Settings"),SerializeField] private int minMultiplier = 1;
    [SerializeField] private int maxMultiplier = 99;
    private int[] scoreMultipliers = new int[2] { 1, 1 };

    private Timer[] startDecreaseTimers = new Timer[2], decreaseIntervalTimers = new Timer[2], multiplierPickupTimers = new Timer[2];
    [SerializeField] private float startMultiplierDecreaseTime = 10f, decreaseIntervalTime = 0.5f;
    [SerializeField]private int multiplierDecreaseAmount = 1;

    private float[] multiplierScaleFactors = new float[2] { 1, 1 };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void OnEnable()
    {
        EventBus<PlayerScoredEvent>.Subscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerSpawned);
        EventBus<LevelFinishedEvent>.Subscribe(OnLevelFinished);
        EventBus<GameFinishedEvent>.Subscribe(OnGameFinished);
        EventBus<GameLoseEvent>.Subscribe(OnGameLost);
        EventBus<PlayerScoreMultiplierGainedEvent>.Subscribe(OnEnemyKilled);
        EventBus<PlayerprojectileMissedEvent>.Subscribe(OnPlayerMissed);
        EventBus<PlayerScoreMultiplierPickupEvent>.Subscribe(OnScoreMultiplierPickup);
    }

    private void OnDestroy()
    {
        EventBus<PlayerScoredEvent>.UnSubscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerSpawned);
        EventBus<LevelFinishedEvent>.UnSubscribe(OnLevelFinished);
        EventBus<GameFinishedEvent>.UnSubscribe(OnGameFinished);
        EventBus<GameLoseEvent>.UnSubscribe(OnGameLost);
        EventBus<PlayerScoreMultiplierGainedEvent>.UnSubscribe(OnEnemyKilled);
        EventBus<PlayerprojectileMissedEvent>.UnSubscribe(OnPlayerMissed);
        EventBus<PlayerScoreMultiplierPickupEvent>.UnSubscribe(OnScoreMultiplierPickup);
    }

    private void Start()
    {
        playerProfileManager = PlayerProfileManager.Instance;
        scoreFilePath = Path.Combine(Application.dataPath, scoreFileName + ".txt");
        ReadScoreFile();
        for (int i = 0; i < multiplierPickupTimers.Length; i++)
        {
            multiplierPickupTimers[i] = new Timer(0);
            startDecreaseTimers[i] = new Timer(startMultiplierDecreaseTime);
            decreaseIntervalTimers[i] = new Timer(decreaseIntervalTime, true);
        }
    }

    private void Update()
    {
        if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.Ingame) return;
        CountDownMultiplierTimers();
    }

    private void OnPlayerSpawned(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        playerProfileManager.PlayerProfiles[_playerAvatarInstantiatedEvent.PlayerIndex].LevelScores.Add(0);
    }

    private void OnPlayerScored(PlayerScoredEvent _playerScoredEvent)
    {
        IncreasePlayerScore(_playerScoredEvent.PlayerIndex, _playerScoredEvent.Score);
    }
    

    private void IncreasePlayerScore(int _playerIndex, int _scoreIncrease)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex] += _scoreIncrease * scoreMultipliers[_playerIndex] * (int)multiplierScaleFactors[_playerIndex];
        EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex]));
    }

    private void OnPlayerHit(PlayerHitEvent _playerHitEvent)
    {
        DecreasePlayerScore(_playerHitEvent.Player, _playerHitEvent.ScoreLoss);
        ResetMultiplier(_playerHitEvent.Player);
    }

    private void DecreasePlayerScore(int _playerIndex, int _scoreDecrease)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex] -= _scoreDecrease;
        if (playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex] < 0)
        {
            playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex] = 0;
        }
        EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(_playerIndex, playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex]));
    }

    private void OnLevelFinished(LevelFinishedEvent _levelFinishedEvent)
    {
        CalculateTotalScore();
    }

    private void OnGameLost(GameLoseEvent _gameLoseEvent)
    {
        CalculateTotalScore();
        OnGameFinished(new GameFinishedEvent());
    }

    private void CalculateTotalScore()
    {
        EventBus<LevelScoreCalculatedEvent>.Publish(new LevelScoreCalculatedEvent(currentLevelIndex));
        for (int i = 0; i < playerProfileManager.PlayerProfiles.Length; i++)
        {
            if (!playerProfileManager.PlayersJoined[i]) continue;
            playerProfileManager.PlayerProfiles[i].TotalScore += playerProfileManager.PlayerProfiles[i].LevelScores[currentLevelIndex];
        }
    }

    private void OnGameFinished(GameFinishedEvent _gameFinishedEvent)
    {
        for (int i = 0; i < playerProfileManager.PlayerProfiles.Length; i++)
        {
            if (!playerProfileManager.PlayersJoined[i]) continue;
            CreateScorePair(i, playerProfileManager.PlayerProfiles[i].TotalScore);
        }
        SortScores();
        WriteScoresToTXT();
    }

    private void CreateScorePair(int _playerIndex, int _score) // NEEDS PLAYER NAMES TO BE FILLED IN, CURRENTLY JUST USING PLAYER INDEX
    {
        ScoreList.Add(new KeyValuePair<string, int>(_playerIndex.ToString() + ": " + DateTime.Now.ToString(), _score));
    }

    private void SortScores()
    {
        ScoreList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
    }

    private void WriteScoresToTXT()
    {
        if (File.Exists(scoreFilePath))
        {
            File.Delete(scoreFilePath);
        }
        File.Create(scoreFilePath).Dispose();

        for (int i = 0; i < ScoreList.Count; i++)
        {
            using (StreamWriter writer = new StreamWriter(scoreFilePath, true))
            {
                writer.WriteLine(ScoreList[i].Key + " :: " + ScoreList[i].Value);
            }
        }
    }

    private void ReadScoreFile()
    {
        if (!File.Exists(scoreFilePath))
        {
            Debug.Log("Score file not found.");
            return;
        }
        using (StreamReader reader = new StreamReader(scoreFilePath))
        {
            List<KeyValuePair<string, int>> readScores = new List<KeyValuePair<string, int>>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(new string[] { " :: " }, StringSplitOptions.None);
                if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                {
                    readScores.Add(new KeyValuePair<string, int>(parts[0], score));
                }
            }
            ScoreList = readScores;
        }
        SortScores();
    }

    private void CountDownMultiplierTimers()
    {
        for (int _playerIndex = 0; _playerIndex < playerProfileManager.PlayersJoined.Length; _playerIndex++)
        {
            if (!playerProfileManager.PlayersJoined[_playerIndex]) continue; // Skip to the next player if the current player is not joined.

            if (multiplierPickupTimers[_playerIndex].IsFinished()) // If the multiplier pickup timer has finished, reset the multiplier scale factor to 1 and publish the change event.
            {
                multiplierScaleFactors[_playerIndex] = 1;
                EventBus<PlayerScoreMultiplierChangedEvent>.Publish(new PlayerScoreMultiplierChangedEvent(_playerIndex, scoreMultipliers[_playerIndex] * (int)multiplierScaleFactors[_playerIndex], (int)multiplierScaleFactors[_playerIndex]));
            }

            if (!startDecreaseTimers[_playerIndex].IsFinished() ) continue;

            if (decreaseIntervalTimers[_playerIndex].IsFinished()) // If the decrease interval timer has finished, decrease the multiplier and reset the decrease interval timer.
            {
                DecreaseMultiplier(_playerIndex, multiplierDecreaseAmount);
            }
        }
    }

    private void OnEnemyKilled(PlayerScoreMultiplierGainedEvent _playerScoreMultiplierGainedEvent)
    {
        IncreaseMultiplier(_playerScoreMultiplierGainedEvent.PlayerIndex, _playerScoreMultiplierGainedEvent.MultiplierGained);
    }

    private void IncreaseMultiplier(int _playerIndex, int _multiplierIncrease)
    {        
        if (scoreMultipliers[_playerIndex] < maxMultiplier) scoreMultipliers[_playerIndex] += _multiplierIncrease;
        startDecreaseTimers[_playerIndex].Reset(startMultiplierDecreaseTime);
        decreaseIntervalTimers[_playerIndex].Reset(decreaseIntervalTime);
        EventBus<PlayerScoreMultiplierChangedEvent>.Publish(new PlayerScoreMultiplierChangedEvent(_playerIndex, scoreMultipliers[_playerIndex] * (int)multiplierScaleFactors[_playerIndex], (int)multiplierScaleFactors[_playerIndex]));
    }

    private void OnPlayerMissed(PlayerprojectileMissedEvent _playerprojectileMissedEvent)
    {
        DecreaseMultiplier(_playerprojectileMissedEvent.PlayerIndex, _playerprojectileMissedEvent.MultiplierLost);
    }

    private void DecreaseMultiplier(int _playerIndex, int _multiplierDecrease)
    {
        if (scoreMultipliers[_playerIndex] > minMultiplier) scoreMultipliers[_playerIndex] -= _multiplierDecrease;
        EventBus<PlayerScoreMultiplierChangedEvent>.Publish(new PlayerScoreMultiplierChangedEvent(_playerIndex, scoreMultipliers[_playerIndex] * (int)multiplierScaleFactors[_playerIndex], (int)multiplierScaleFactors[_playerIndex]));
    }

    private void ResetMultiplier(int _playerIndex)
    {
        scoreMultipliers[_playerIndex] = minMultiplier;
        EventBus<PlayerScoreMultiplierChangedEvent>.Publish(new PlayerScoreMultiplierChangedEvent(_playerIndex, scoreMultipliers[_playerIndex] * (int)multiplierScaleFactors[_playerIndex], (int)multiplierScaleFactors[_playerIndex]));
    }

    private void OnScoreMultiplierPickup(PlayerScoreMultiplierPickupEvent _playerScoreMultiplierPickupEvent)
    {
        SetMultiplierScaleFactor(_playerScoreMultiplierPickupEvent.PlayerIndex, _playerScoreMultiplierPickupEvent.ScaleFactor);
        multiplierPickupTimers[_playerScoreMultiplierPickupEvent.PlayerIndex].Reset(_playerScoreMultiplierPickupEvent.Duration);
    }

    private void SetMultiplierScaleFactor(int _playerIndex, float _scaleFactor)
    {
        multiplierScaleFactors[_playerIndex] = _scaleFactor;
        EventBus<PlayerScoreMultiplierChangedEvent>.Publish(new PlayerScoreMultiplierChangedEvent(_playerIndex, scoreMultipliers[_playerIndex] * (int)multiplierScaleFactors[_playerIndex], (int)multiplierScaleFactors[_playerIndex]));
    }    
}