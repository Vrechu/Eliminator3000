using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField]private Transform[] spawnPoints;
    private PlayerProfileManager profileManager;
    private bool[] avatarsIngame = new bool[2] {false, false};
    private GameObject[] playerAvatars = new GameObject[2];

    private void Start()
    {
        profileManager = PlayerProfileManager.Instance;
    }

    /// <summary>
    /// Publishes a PlayerSpawnEvent when a player joins, providing the appropriate spawn point based on the player's profile.
    /// </summary>
    /// <param name="_playerJoinedEvent">Event info</param>
    public void InstantiatePlayerAvatar(int _playerIndex)
    {
        if (avatarsIngame[_playerIndex]) return;
        if (profileManager.PlayerProfiles[0].IngameAvatar != null) return;

        PlayerInput player = PlayerInput.Instantiate(
        profileManager.PlayerProfiles[_playerIndex].ProfilePrefab,
        controlScheme: CheckControlScheme(_playerIndex),
        pairWithDevice: Keyboard.current);

        avatarsIngame[_playerIndex] = true;
        playerAvatars[_playerIndex] = player.gameObject;
        player.transform.position = spawnPoints[_playerIndex].position;
    }

    private string CheckControlScheme(int _playerIndex)
    {
        switch (_playerIndex)
        {
            case 1:
                return "WASD";
            case 2:
                return "Arrows";
            default:
                Debug.LogError("Invalid player index: " + _playerIndex);
                return null;
        }
    }

    private void DestroyAvatars()
    {
        for (int i = 0; i < playerAvatars.Length; i++)
        {
            if (playerAvatars[i] != null)
            {
                Destroy(playerAvatars[i]);
                avatarsIngame[i] = false;
            }
        }
    }
}