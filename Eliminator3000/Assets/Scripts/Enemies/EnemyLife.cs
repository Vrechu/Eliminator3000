using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    [Header("Visual (optional)")]
    [Tooltip("Child object holding the model. Best choice: it is rotated and bobbed instead of the root, so it can't fight MoveEnemy. Empty = use this object.")]
    public Transform visual;

    [Header("Look at player")]
    public bool lookAtPlayer = true;
    public float turnSpeed = 4f;
    [Tooltip("Max angle away from the travel direction. Stops wild flips.")]
    public float maxAngle = 60f;
    [Tooltip("Add 180 on Y if the model's front faces backwards.")]
    public Vector3 rotationOffsetEuler = Vector3.zero;
    [Tooltip("Stops tracking once the enemy is this far past the player in Z (default 0 = level with the player). It then keeps its last pose while flying away.")]
    public float stopTrackingBehind = 0f;

    [Header("Hover")]
    public bool hover = true;
    public float bobHeight = 0.35f;
    public float bobSpeed = 1.6f;
    public float swayWidth = 0.15f;

    Transform target;
    float nextScan, phase;
    bool frozen;
    Quaternion baseRot, lastRot;
    Vector3 lastOffset;

    Transform T { get { return visual != null ? visual : transform; } }

    void Start()
    {
        phase = Random.value * 10f;
        baseRot = T.rotation;
    }

    void FindPlayer()
    {
        Transform best = null; float bd = float.MaxValue;
        foreach (var tag in new[] { "Player1", "Player2" })
        {
            GameObject g = null;
            try { g = GameObject.FindWithTag(tag); } catch { }
            if (g == null) continue;
            float d = (g.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = g.transform; }
        }
        target = best;
    }

    void LateUpdate()
    {
        // undo last frame's hover offset so we never fight MoveEnemy
        T.localPosition -= lastOffset;

        if (Time.time >= nextScan) { nextScan = Time.time + 0.5f; FindPlayer(); }

        if (lookAtPlayer && target != null && !frozen)
        {
            if (transform.position.z < target.position.z - stopTrackingBehind)
                frozen = true;   // flew past: keep last animation
            else
            {
                Vector3 dir = target.position - transform.position;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion want = Quaternion.LookRotation(dir) * Quaternion.Euler(rotationOffsetEuler);
                    Quaternion travel = Quaternion.LookRotation(Vector3.back) * Quaternion.Euler(rotationOffsetEuler);
                    want = Quaternion.RotateTowards(travel, want, maxAngle);
                    lastRot = Quaternion.Slerp(T.rotation, want, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
                    T.rotation = lastRot;
                }
            }
        }
        else if (frozen) T.rotation = lastRot;

        float t = Time.time * bobSpeed + phase;
        lastOffset = hover ? new Vector3(Mathf.Sin(t * 0.7f) * swayWidth, Mathf.Sin(t) * bobHeight, 0f) : Vector3.zero;
        T.localPosition += lastOffset;
    }
}