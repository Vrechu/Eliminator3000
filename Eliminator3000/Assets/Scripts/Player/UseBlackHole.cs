using UnityEngine;
using UnityEngine.InputSystem;

public class UseBlackHole : MonoBehaviour
{
    private int playerID;

    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float spawnDistance = 3;

    private PlayerProfileManager playerProfileManager;
    private GameStateManager gameStateManager;

    private void Start()
    {
        SetPlayerID();
        playerProfileManager = PlayerProfileManager.Instance;
        gameStateManager = GameStateManager.Instance;
    }

    private void SetPlayerID()
    {
        if (TryGetComponent<InGameAvatarInfo>(out InGameAvatarInfo _info))
        {
            playerID = _info.PlayerID;
            if (playerID < 0 || playerID > 1) Debug.LogError("Invalid PLayerID set on: " + gameObject.name);
        }
        else Debug.LogError("No InGameAvatarInfo component found. Every Playerprefab should have one!");
    }

    public void GetBlackHoleInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState
            != GameStateManager.GameState.Ingame) return;
        if (!playerProfileManager.PlayerProfiles[playerID].BlackHole) return;
        if (_context.performed) SpawnBlackHole();
    }

    private void SpawnBlackHole()
    {
        GameObject newBlackHole = Instantiate(blackHolePrefab, transform.position + new Vector3(0,0,spawnDistance), Quaternion.Euler(90,0,0));

        if (newBlackHole.TryGetComponent<BlackHoleBehaviour>(out BlackHoleBehaviour _blackHoleBehaviour))
        {
            _blackHoleBehaviour.PlayerID = playerID;
            if (_blackHoleBehaviour.PlayerID < 0 || _blackHoleBehaviour.PlayerID > 1) Debug.LogError("Invalid PlayerID in black hole!");
        }
        else
        {
            Debug.LogError("No Black Hole Behaviour component found in Black Hole prefab!");
        }
        playerProfileManager.PlayerProfiles[playerID].BlackHole = false;
    }
}
