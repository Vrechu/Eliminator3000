using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player1Prefab, player2Prefab;
    [SerializeField]
    private Transform[] spawnPoints;

    private bool wasdJoined = false;
    private bool arrowsJoined = false;

    private ProfileManager profileManager;

    private GameStateManager gameStateManager;


    private void Start()
    {
        profileManager = ProfileManager.Instance;
        gameStateManager = GameStateManager.Instance;
    }


    void Update()
    {
        if (Keyboard.current == null) return;
        if (gameStateManager.CurrentState == GameStateManager.GameState.Pregame
            || gameStateManager.CurrentState == GameStateManager.GameState.Ingame
            || gameStateManager.CurrentState == GameStateManager.GameState.Paused)
        {
            if (!wasdJoined
                && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlayerInput player = PlayerInput.Instantiate(
                    player1Prefab,
                    controlScheme: "WASD",
                    pairWithDevice: Keyboard.current);

                profileManager.NewProfile(1, player1Prefab, player, player.gameObject);
                if (spawnPoints.Length > 0)
                {
                    player.transform.position = spawnPoints[0].position;
                }

                wasdJoined = true;
            }

            if (!arrowsJoined
                && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
            {
                PlayerInput player = PlayerInput.Instantiate(
                    player2Prefab,
                    controlScheme: "Arrows",
                    pairWithDevice: Keyboard.current);

                profileManager.NewProfile(2,player2Prefab, player, player.gameObject);
                if (spawnPoints.Length > 1)
                {
                    player.transform.position = spawnPoints[1].position;
                }
                arrowsJoined = true;
            }
        }
    }
}
