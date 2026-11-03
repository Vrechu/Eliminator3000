using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class UseDummy : MonoBehaviour
{
    private int playerIndex;

    [SerializeField] private GameObject dummyPrefab;

    private PlayerProfileManager playerProfileManager;
    private GameStateManager gameStateManager;
    [SerializeField] private bool instant;

    private void OnEnable()
    {
        EventBus<PlayerDummyPickupEvent>.Subscribe(OnPickup);
    }
    private void OnDestroy()
    {
        EventBus<PlayerDummyPickupEvent>.UnSubscribe(OnPickup);
    }

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
            playerIndex = _info.PlayerID;
            if (playerIndex < 0 || playerIndex > 1) Debug.LogError("Invalid PLayerID set on: " + gameObject.name);
        }
        else Debug.LogError("No InGameAvatarInfo component found. Every Playerprefab should have one!");
    }

    public void GetDummyInput(InputAction.CallbackContext _context)
    {
        if (instant) return;
        if (gameStateManager == null) return;
        if (gameStateManager.CurrentState
            != GameStateManager.GameState.Ingame) return;
        if (!playerProfileManager.PlayerProfiles[playerIndex].Dummy) return;
        if (_context.performed) SpawnDummy();
    }

    private void OnPickup(PlayerDummyPickupEvent _pickupEvent)
    {
        if (instant && _pickupEvent.PlayerIndex == playerIndex) SpawnDummy();
    }

    private void SpawnDummy()
    {
        GameObject newDummy = Instantiate(dummyPrefab, transform.position, Quaternion.identity);
        
        if (newDummy.TryGetComponent<DummyBehaviour>(out DummyBehaviour _dummyBehaviour)){
            _dummyBehaviour.PlayerID = playerIndex;
            if (_dummyBehaviour.PlayerID < 0 || _dummyBehaviour.PlayerID > 1) Debug.LogError("Invalid PlayerID in dummy!");
        }
        else { Debug.LogError("No Dummy Behaviour component found in dummy prefab!");
        }
        playerProfileManager.PlayerProfiles[playerIndex].Dummy = false;
    }
}
