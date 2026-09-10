using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] playerPrefab;
    [SerializeField]
    private Transform[] spawnPoints;

    private bool wasdJoined = false;
    private bool arrowsJoined = false;

    private GameObject[] players = new GameObject[2];

    private UnityEvent OnPlayerJoined;



    void Update()
    {
        if (Keyboard.current == null) return;

        if (!wasdJoined
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayerInput player = PlayerInput.Instantiate(playerPrefab[0],
                controlScheme: "WASD",
                pairWithDevice: Keyboard.current);

            players[players.Length - 1] = player.gameObject;

            if (spawnPoints.Length > 0)
            {
                player.transform.position = spawnPoints[0].position;
            }

            wasdJoined = true;
            OnPlayerJoined?.Invoke();
        }

        if (!arrowsJoined
            && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            PlayerInput player = PlayerInput.Instantiate(playerPrefab[1],
                controlScheme: "Arrows",
                pairWithDevice: Keyboard.current);

            players[players.Length - 1] = player.gameObject;

            if (spawnPoints.Length > 1)
            {
                player.transform.position = spawnPoints[1].position;
            }
            arrowsJoined = true;
            OnPlayerJoined?.Invoke();
        }

        foreach (var gamePad in Gamepad.all)
        {
            if (gamePad.buttonSouth.wasPressedThisFrame)
            {
                PlayerInput player = PlayerInput.Instantiate(playerPrefab[2],
                    controlScheme: "Gamepad",
                    pairWithDevice: gamePad);

                players[players.Length - 1] = player.gameObject;

                OnPlayerJoined?.Invoke();
            }
        }
    }



}
