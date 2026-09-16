using UnityEngine;

public class MoveTrack : MonoBehaviour
{
    public Transform TrackTransform;
    [SerializeField]
    private float moveSpeed = 5f;

    private bool isMoving = false;

    private void OnEnable()
    {
        EventBus<GameStartEvent>.Subscribe(StartTrack);
        EventBus<GamePauseEvent>.Subscribe(StopTrack);
    }
    private void OnDestroy()
    {
        EventBus<GameStartEvent>.UnSubscribe(StartTrack);
        EventBus<GamePauseEvent>.UnSubscribe(StopTrack);
    }

    private void Update()
    {
        if (isMoving) MoveTrackBackward();
    }

    private void MoveTrackBackward()
    {   
        TrackTransform.Translate(0, 0, moveSpeed * Time.deltaTime * -1);
    }

    private void StartTrack(GameStartEvent _gameStartEvent)
    {
        isMoving = true;
    }

    private void StopTrack(GamePauseEvent _gamePauseEvent)
    {
        isMoving = false;
    }
}
