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

    private UnityEvent OnPlayerJoined;

    private ProfileManager profileManager;


    private void Start()
    {
        profileManager = ProfileManager.Instance;
    }


    void Update()
    {
        if (Keyboard.current == null) return;

        if (!wasdJoined
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayerInput player = PlayerInput.Instantiate(player1Prefab,
                controlScheme: "WASD",
                pairWithDevice: Keyboard.current);

            profileManager.NewProfile(1, player.gameObject, player);
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
            PlayerInput player = PlayerInput.Instantiate(player2Prefab,
                controlScheme: "Arrows",
                pairWithDevice: Keyboard.current);

            profileManager.NewProfile(2, player.gameObject, player);
            if (spawnPoints.Length > 1)
            {
                player.transform.position = spawnPoints[1].position;
            }
            arrowsJoined = true;
            OnPlayerJoined?.Invoke();
        }

        /*foreach (var gamePad in Gamepad.all)
        {
            if (gamePad.buttonSouth.wasPressedThisFrame)
            {
                PlayerInput player = PlayerInput.Instantiate(playerPrefab[2],
                    controlScheme: "Gamepad",
                    pairWithDevice: gamePad);


                OnPlayerJoined?.Invoke();
            }
        }*/
    }



}
