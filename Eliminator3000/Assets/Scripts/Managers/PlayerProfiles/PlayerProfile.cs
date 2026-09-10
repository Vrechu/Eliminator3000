using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerProfile 
{
    public GameObject PlayerPrefab;
    public PlayerInput PlayerInput;
    public int Lives;
    public int Score;

    public PlayerProfile(
        GameObject playerPrefab, 
        PlayerInput playerInput,
        int lives,
        int score)
    {
        PlayerPrefab = playerPrefab;
        PlayerInput = playerInput;
        Lives = lives;
        Score = score;
    }

    public PlayerProfile(
        GameObject playerPrefab,
        PlayerInput playerInput)
    {
        PlayerPrefab = playerPrefab;
        PlayerInput = playerInput;
        Lives = 3;
        Score = 0;
    }
}
