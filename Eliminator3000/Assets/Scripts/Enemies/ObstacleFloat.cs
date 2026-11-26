using UnityEngine;

public class ObstacleFloat : MonoBehaviour
{
    [Tooltip("Child holding the model. Empty = this object. A child is safer because the Level Manager and the level scroll move the root.")]
    public Transform visual;

    [Header("Bob")]
    public float bobHeight = 0.12f;
    public float bobSpeed = 0.8f;
    public float swayWidth = 0.05f;

    [Header("Tilt (degrees, 0 = off)")]
    public float tiltAmount = 1.5f;
    public float tiltSpeed = 0.5f;

    float phase;
    Vector3 lastOffset;
    Quaternion lastTilt = Quaternion.identity;

    Transform T { get { return visual != null ? visual : transform; } }

    void Start() { phase = Random.value * 10f; }

    void LateUpdate()
    {
        // remove last frame's effect so nothing accumulates
        T.localPosition -= lastOffset;
        T.localRotation = T.localRotation * Quaternion.Inverse(lastTilt);

        float t = Time.time + phase;
        lastOffset = new Vector3(Mathf.Sin(t * bobSpeed * 0.7f) * swayWidth, Mathf.Sin(t * bobSpeed) * bobHeight, 0f);
        lastTilt = Quaternion.Euler(Mathf.Sin(t * tiltSpeed) * tiltAmount, 0f, Mathf.Cos(t * tiltSpeed * 0.8f) * tiltAmount);

        T.localPosition += lastOffset;
        T.localRotation = T.localRotation * lastTilt;
    }
}