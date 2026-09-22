using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerProfile 
{
    public GameObject ProfilePrefab { get; private set; }
    public PlayerInput PlayerInput;
    public GameObject IngameAvatar;
    public int Lives;
    public int Score;
        

    public PlayerProfile(
        GameObject cPrefab,
        PlayerInput cPlayerInput,
        GameObject cIngameAvatar,
        int cLives = 3,
        int cScore = 0)
    {
        ProfilePrefab = cPrefab;
        PlayerInput = cPlayerInput;
        IngameAvatar = cIngameAvatar;
        Lives = cLives;
        Score = cScore;
    }
}
