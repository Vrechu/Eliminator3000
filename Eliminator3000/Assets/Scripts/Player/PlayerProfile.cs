using Unity.VisualScripting;
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
    public int Health;
    public int Score;
    public int Shield;
    public bool AliveInLevel;

    public PlayerProfile(
        GameObject cPrefab,
        PlayerInput cPlayerInput = null,
        GameObject cIngameAvatar = null,
        int cLives = 3,
        int cHealth = 100,
        int cScore = 0,
        int cShield = 0)
    {
        ProfilePrefab = cPrefab;
        PlayerInput = cPlayerInput;
        IngameAvatar = cIngameAvatar;
        Lives = cLives;
        Health = cHealth;
        Score = cScore;
        Shield = cShield;
        AliveInLevel = false;
    }
}
