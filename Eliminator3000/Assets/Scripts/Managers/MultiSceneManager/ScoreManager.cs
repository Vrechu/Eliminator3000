using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;


public class ScoreManager : MonoBehaviour
{
    private PlayerProfileManager playerProfileManager;
    private int currentLevelIndex = 0;
    private List<KeyValuePair<string, int>> scoreList = new List<KeyValuePair<string, int>>();
    [SerializeField] private string scoreFileName = "Scores.txt";
    private string scoreFilePath;

    private void OnEnable()
    {
        EventBus<PlayerScoredEvent>.Subscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.Subscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.Subscribe(OnPlayerSpawned);
        EventBus<GameWinEvent>.Subscribe(OnlevelFinshed);
    }

    private void OnDestroy()
    {
        EventBus<PlayerScoredEvent>.UnSubscribe(OnPlayerScored);
        EventBus<PlayerHitEvent>.UnSubscribe(OnPlayerHit);
        EventBus<PlayerAvatarInstantiatedEvent>.UnSubscribe(OnPlayerSpawned);
        EventBus<GameWinEvent>.UnSubscribe(OnlevelFinshed);
    }

    private void Start()
    {
        playerProfileManager = PlayerProfileManager.Instance;
        scoreFilePath = Path.Combine(Application.dataPath, scoreFileName);
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

    private void OnlevelFinshed(GameWinEvent _gameWinEvent)
    {
        for (int i = 0; i < playerProfileManager.PlayerProfiles.Length; i++)
        {
            if (!playerProfileManager.PlayersJoined[i]) return;
            playerProfileManager.PlayerProfiles[i].TotalScore += playerProfileManager.PlayerProfiles[i].LevelScores[currentLevelIndex];
        }
    }


    private void CreateScorePair(int _playerIndex, int _score) // NEEDS PLAYER NAMES TO BE FILLED IN, CURRENTLY JUST USING PLAYER INDEX
    {
        scoreList.Add(new KeyValuePair<string, int>(_playerIndex.ToString() + ": " + DateTime.Now.ToString(), _score));
    }

    private void SortScores()
    {
        scoreList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
    }

    private void WriteScoresToTXT()
    {
        Debug.Log("Score file path: " + scoreFilePath);
        if (!File.Exists(scoreFilePath))
        {
            File.Create(scoreFilePath);
        }

        for (int i = 0; i < scoreList.Count; i++)
        {
            using (StreamWriter writer = new StreamWriter(scoreFilePath, true))
            {
                writer.WriteLine(scoreList[i].Key + " :: " + scoreList[i].Value);
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
            scoreList = readScores;
        }
    }
}