using UnityEngine;

public class MoveTrack : MonoBehaviour
{
    public Transform TrackTransform;
    private float moveSpeed = 5f;

    private GameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        if (gameStateManager.CurrentState == GameStateManager.GameState.Ingame)
        {
            MoveTrackBackward();
        }
    }

    private void MoveTrackBackward()
    {   
        TrackTransform.Translate(0, 0, moveSpeed * Time.deltaTime * -1);
    }
}
