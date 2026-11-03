using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private PlayerProfileManager playerProfileManager;
    private int currentLevelIndex = 0;

    public List<KeyValuePair<string, int>> ScoreList = new List<KeyValuePair<string, int>>();

    [SerializeField] private string scoreFileName = "Scores";
    private string scoreFilePath;

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
    }

    private void OnDestroy()
    {
        EventBus<PlayerScoredEvent>.UnSubscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerSpawned);
        EventBus<LevelFinishedEvent>.UnSubscribe(OnLevelFinished);
        EventBus<GameFinishedEvent>.UnSubscribe(OnGameFinished);
        EventBus<GameLoseEvent>.UnSubscribe(OnGameLost);
    }

    private void Start()
    {
        playerProfileManager = PlayerProfileManager.Instance;
        scoreFilePath = Path.Combine(Application.dataPath, scoreFileName + ".txt");
        ReadScoreFile();
    }

    private void OnPlayerSpawned(PlayerAvatarInstantiatedEvent _playerAvatarInstantiatedEvent)
    {
        playerProfileManager.PlayerProfiles[_playerAvatarInstantiatedEvent.PlayerIndex].LevelScores.Add(0);
    }

    private void OnPlayerScored(PlayerScoredEvent _playerScoredEvent)
    {
        ChangePlayerScore(_playerScoredEvent.Player, _playerScoredEvent.Score);
    }

    private void OnPlayerHit(PlayerHitEvent _playerHitEvent)
    {
        ChangePlayerScore(_playerHitEvent.Player, -_playerHitEvent.ScoreLoss);
    }

    /// <summary>
    /// Changes the score of the specified player by the given amount. If the resulting score is less than 0, it will be set to 0. 
    /// After changing the score, a ScoreChangedEvent is published to notify other systems of the change.
    /// </summary>
    /// <param name="_playerIndex">The player whose score is to be changed.</param>
    /// <param name="_score">The amount by which to change the player's score.</param>
    private void ChangePlayerScore(int _playerIndex, int _score)
    {
        playerProfileManager.PlayerProfiles[_playerIndex].LevelScores[currentLevelIndex] += _score;
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
}