using UnityEngine;

public class MoveTrack : MonoBehaviour
{
    public Transform TrackTransform;
    private float moveSpeed = 5f;

    private void Update()
    {
        TrackTransform.Translate(0, 0, moveSpeed * Time.deltaTime * -1);
    }
}
