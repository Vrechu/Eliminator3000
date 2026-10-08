using UnityEngine;

[ExecuteAlways]
public class SpiralLayout : MonoBehaviour
{
    [Header("Spiral Range")]
    public float startPositionZ = -150f;
    public float endPositionZ = 2000f;

    [Header("Spiral Shape")]
    public float radius = 5.0f;
    public float rotations = 3.0f; // Total full turns along the length

    private void Update()
    {
        int childCount = transform.childCount;
        if (childCount < 2) return;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            // Normalize progress t between 0.0 and 1.0
            float t = (float)i / (childCount - 1);

            // Interpolate Z position evenly
            float z = Mathf.Lerp(startPositionZ, endPositionZ, t);

            // Calculate angle around the Z axis
            float angle = t * rotations * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            child.localPosition = new Vector3(x, y, z);
        }
    }
}