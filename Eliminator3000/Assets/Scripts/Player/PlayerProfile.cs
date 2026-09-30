using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Profile for a player, containing their prefab, input, avatar, lives, and score.
/// </summary>
public struct PlayerProfile 
{
    public GameObject ProfilePrefab { get; private set; }
    public PlayerInput PlayerInput;
    public GameObject IngameAvatar;
    public int Lives;
    public int Score;
    public bool Alive;


    public PlayerProfile(
        GameObject cPrefab,
        PlayerInput cPlayerInput = null,
        GameObject cIngameAvatar = null,
        int cLives = 3,
        int cScore = 0)
    {
        ProfilePrefab = cPrefab;
        PlayerInput = cPlayerInput;
        IngameAvatar = cIngameAvatar;
        Lives = cLives;
        Score = cScore;
        Alive = true;
    }
}
