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
    public bool AliveInLevel;
    public int Score;
    public int Lives;
    public int Health;
    public int Shield;
    public bool Dummy;
    public bool BlackHole;

    public PlayerProfile(
        GameObject cPrefab,
        PlayerInput cPlayerInput = null,
        GameObject cIngameAvatar = null,
        int cScore = 0,
        int cLives = 3,
        int cHealth = 100,
        int cShield = 0,
        bool cDummy = false,
        bool cBlackHole = false)
    {
        ProfilePrefab = cPrefab;
        PlayerInput = cPlayerInput;
        IngameAvatar = cIngameAvatar;
        AliveInLevel = false;
        Score = cScore;
        Lives = cLives;
        Health = cHealth;
        Shield = cShield;
        Dummy = cDummy;
        BlackHole = cBlackHole; 
    }
}
