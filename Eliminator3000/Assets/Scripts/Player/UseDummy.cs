using UnityEngine;
using UnityEngine.InputSystem;

public class UseDummy : MonoBehaviour
{
    private int playerID;

    [SerializeField] private GameObject dummyPrefab;

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

    public void GetDummyInput(InputAction.CallbackContext _context)
    {
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState
            != GameStateManager.GameState.Ingame) return;
        if (!playerProfileManager.PlayerProfiles[playerID].Dummy) return;
        if (_context.performed) SpawnDummy();
    }

    private void SpawnDummy()
    {
        Instantiate(dummyPrefab, transform.position, Quaternion.identity);
    }
}
