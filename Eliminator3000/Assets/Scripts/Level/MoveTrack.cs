using UnityEngine;

public class MoveTrack : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private GameStateManager gameStateManager;


    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        if (gameStateManager.CurrentState == GameStateManager.GameState.Ingame) MoveTrackBackward();
    }

    private void MoveTrackBackward()
    {
        transform.Translate(0, 0, moveSpeed * Time.deltaTime * -1);
    }
}