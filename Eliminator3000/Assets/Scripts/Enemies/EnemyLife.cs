using UnityEngine;
using System.Reflection;

public class EnemyLife : MonoBehaviour
{
    [Header("Visual (optional)")]
    public Transform visual;

    [Header("Look at player")]
    public bool lookAtPlayer = true;
    public float turnSpeed = 4f;
    public float maxAngle = 60f;
    public Vector3 rotationOffsetEuler = Vector3.zero;
    public float stopTrackingBehind = 0f;

    [Header("Entry spin (roll around the look direction)")]
    public bool spinOnEntry = true;
    [Tooltip("Degrees per second while flying to the first point.")]
    public float spinSpeed = 120f;
    [Tooltip("Degrees per second afterwards. 0 = settle and stop.")]
    public float idleSpinSpeed = 0f;
    public float settleSpeed = 2f;

    [Header("Hover")]
    public bool hover = true;
    public float bobHeight = 0.35f;
    public float bobSpeed = 1.6f;
    public float swayWidth = 0.15f;

    Transform target;
    float nextScan, phase;
    bool frozen;
    Quaternion lastRot, smoothBase;
    Vector3 lastOffset;
    MoveEnemy move;
    FieldInfo finalField, chargeField, indexField;
    float roll, rollSpeed, settleTarget;
    bool arrived;

    Transform T { get { return visual != null ? visual : transform; } }

    void Start()
    {
        phase = Random.value * 10f;
        smoothBase = Quaternion.LookRotation(Vector3.back);
        rollSpeed = spinSpeed;
        move = GetComponent<MoveEnemy>();
        if (move != null)
        {
            var f = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            finalField = typeof(MoveEnemy).GetField("finalWaypointReached", f);
            chargeField = typeof(MoveEnemy).GetField("kamikazeReached", f);
            indexField = typeof(MoveEnemy).GetField("currentWaypointIndex", f);
        }
        if (!spinOnEntry) arrived = true;
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

    bool ReachedFirstPoint()
    {
        if (move == null || indexField == null || move.WaypointOrder == null || move.WaypointOrder.Length == 0) return true;
        if ((int)indexField.GetValue(move) >= 1) return true;
        return Vector3.Distance(transform.position, move.Waypoints[move.WaypointOrder[0]].position) < 0.3f;
    }

    bool HeadingToEnd()
    {
        if (move == null) return false;
        if (finalField != null && (bool)finalField.GetValue(move)) return true;
        if (chargeField != null && (bool)chargeField.GetValue(move)) return true;
        return false;
    }

    void LateUpdate()
    {
        T.localPosition -= lastOffset;
        float dt = Time.deltaTime;

        if (Time.time >= nextScan) { nextScan = Time.time + 0.5f; FindPlayer(); }

        if (!frozen && HeadingToEnd()) { frozen = true; lastRot = T.rotation; }

        if (frozen) T.rotation = lastRot;
        else
        {
            if (lookAtPlayer && target != null)
            {
                if (transform.position.z < target.position.z - stopTrackingBehind)
                {
                    frozen = true;
                    lastRot = T.rotation;
                }
                else
                {
                    Vector3 dir = target.position - transform.position;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        Quaternion travel = Quaternion.LookRotation(Vector3.back);
                        Quaternion want = Quaternion.RotateTowards(travel, Quaternion.LookRotation(dir), maxAngle);
                        smoothBase = Quaternion.Slerp(smoothBase, want, 1f - Mathf.Exp(-turnSpeed * dt));
                    }
                }
            }

            // entry spin
            if (!arrived && ReachedFirstPoint())
            {
                arrived = true;
                settleTarget = Mathf.Ceil(roll / 360f) * 360f;
            }
            if (!arrived) roll += rollSpeed * dt;
            else if (idleSpinSpeed > 0f)
            {
                rollSpeed = Mathf.Lerp(rollSpeed, idleSpinSpeed, 1f - Mathf.Exp(-settleSpeed * dt));
                roll += rollSpeed * dt;
            }
            else roll = Mathf.Lerp(roll, settleTarget, 1f - Mathf.Exp(-settleSpeed * dt));

            Vector3 axis = smoothBase * Vector3.forward;
            Quaternion final = Quaternion.AngleAxis(roll, axis) * smoothBase * Quaternion.Euler(rotationOffsetEuler);
            T.rotation = final;
            lastRot = final;
        }

        float t = Time.time * bobSpeed + phase;
        lastOffset = hover ? new Vector3(Mathf.Sin(t * 0.7f) * swayWidth, Mathf.Sin(t) * bobHeight, 0f) : Vector3.zero;
        T.localPosition += lastOffset;
    }
}