using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField]private Transform[] spawnPoints;
    private PlayerProfileManager playerProfileManager;
    private bool[] avatarsIngame = new bool[2] {false, false};
    private GameObject[] playerAvatars = new GameObject[2];


    private void OnEnable()
    {
        playerProfileManager = PlayerProfileManager.Instance;
        EventBus<LevelEnteredEvent>.Subscribe(InstantiateOnLevelEnter);
        EventBus<PlayerLivesAtZeroEvent>.Subscribe(OnPlayerLivesAtZero);
    }

    private void OnDestroy()
    {
        EventBus<LevelEnteredEvent>.UnSubscribe(InstantiateOnLevelEnter);
        EventBus<PlayerLivesAtZeroEvent>.UnSubscribe(OnPlayerLivesAtZero);
    }

    private void InstantiateOnLevelEnter(LevelEnteredEvent _levelEnteredEvent)
    {
        for (int i = 0; i < playerProfileManager.PlayerProfiles.Length; i++)
        {
            if (playerProfileManager.PlayersJoined[i])
            {
                InstantiatePlayerAvatar(i);
            }
        }
    }


    /// <summary>
    /// Publishes a PlayerSpawnEvent when a player joins, providing the appropriate spawn point based on the player's profile.
    /// </summary>
    /// <param name="_playerJoinedEvent">Event info</param>
    public void InstantiatePlayerAvatar(int _playerIndex)
    {
        if (avatarsIngame[_playerIndex]) return;
        if (playerProfileManager.PlayerProfiles[0].IngameAvatar != null) return;

        PlayerInput player = PlayerInput.Instantiate(
        playerProfileManager.PlayerProfiles[_playerIndex].ProfilePrefab,
        controlScheme: CheckControlScheme(_playerIndex),
        pairWithDevice: Keyboard.current);

        avatarsIngame[_playerIndex] = true;
        playerAvatars[_playerIndex] = player.gameObject;
        playerProfileManager.PlayerProfiles[_playerIndex].AliveInLevel = true;
        player.transform.position = spawnPoints[_playerIndex].position;
        EventBus<PlayerAvatarInstantiatedEvent>.Publish(new PlayerAvatarInstantiatedEvent(_playerIndex));
    }

    private string CheckControlScheme(int _playerIndex)
    {
        switch (_playerIndex)
        {
            case 0:
                return "WASD";
            case 1:
                return "Arrows";
            default:
                Debug.LogError("Invalid player index: " + _playerIndex);
                return null;
        }
    }

    private void OnPlayerLivesAtZero(PlayerLivesAtZeroEvent _playerLivesAtZeroEvent)
    {
        DestroyAvatar(_playerLivesAtZeroEvent.Player);
    }

    private void DestroyAvatar(int _playerIndex)
    {
        if (playerAvatars[_playerIndex] != null)
        {
            Destroy(playerAvatars[_playerIndex]);
            avatarsIngame[_playerIndex] = false;
            playerAvatars[_playerIndex] = null;
        }
    }
}