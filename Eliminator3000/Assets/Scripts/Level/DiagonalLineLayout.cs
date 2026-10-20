using UnityEngine;

[ExecuteAlways]
public class DiagonalLineLayout : MonoBehaviour
{
    [Header("Z-Axis Range (Length of the Track)")]
    public float startPositionZ = -150f;
    public float endPositionZ = 2000f;

    [Header("X-Axis Diagonal Spread")]
    [Tooltip("How far left or right (X) the diagonal line stretches from start to end.")]
    public float totalWidthX = 15.0f;

    [Header("Height (Y-Axis)")]
    public float heightY = 0f;

    private void Update()
    {
        // CRITICAL FIX: Stop updating during gameplay so destroyed items don't break positions
        if (Application.isPlaying) return;

        int childCount = transform.childCount;
        if (childCount < 2) return;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            // Normalize progress t between 0.0 and 1.0 based on child order
            float t = (float)i / (childCount - 1);

            // Interpolate Z and X evenly to create a clean diagonal line
            float z = Mathf.Lerp(startPositionZ, endPositionZ, t);
            float x = Mathf.Lerp(0f, totalWidthX, t);

            // Apply the new position
            child.localPosition = new Vector3(x, heightY, z);
        }
    }
}