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

    [Header("Spacing mode (off = old behaviour: spread over the whole range)")]
    public bool useFixedSpacing = false;
    [Tooltip("Distance in Z between pillars when Use Fixed Spacing is on. Starts at Start Position Z.")]
    public float spacing = 40f;

    private void Update()
    {
        // Stop updating during gameplay so destroyed items don't break positions
        if (Application.isPlaying) return;
        if (PillarDropIn.Previewing) return;
        int childCount = transform.childCount;
        if (childCount < 2) return;

        float range = endPositionZ - startPositionZ;
        float slope = Mathf.Abs(range) > 0.001f ? totalWidthX / range : 0f;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            float z, x;
            if (useFixedSpacing)
            {
                z = startPositionZ + i * spacing;
                x = (z - startPositionZ) * slope;   // same diagonal angle as before
            }
            else
            {
                float t = (float)i / (childCount - 1);
                z = Mathf.Lerp(startPositionZ, endPositionZ, t);
                x = Mathf.Lerp(0f, totalWidthX, t);
            }

            child.localPosition = new Vector3(x, heightY, z);
        }
    }
}