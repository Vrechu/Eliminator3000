using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class EnemyTelegraph : MonoBehaviour
{
    public enum Style { Blink, Pulse, ColorShift, Flicker }

    [Header("Style")]
    public Style style = Style.Blink;
    public float intensityMultiplier = 6f;
    [ColorUsage(false, true)] public Color flashColor = new Color(6f, 5f, 0.5f);

    [Header("Speed")]
    [Tooltip("Base flashes per second (Blink / Pulse), or jitter speed (Flicker).")]
    public float blinkRate = 4f;
    [Tooltip("Multiplies the speed over the warning. Left = warning starts, right = shot. Start low and end high = slow to fast.")]
    public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 4f));
    [Range(0f, 1f)] public float flickerBias = 0.5f;

    [Header("Brightness over the warning")]
    public bool useCurve = true;
    public AnimationCurve warningCurve = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(1f, 1f));

    [Header("Triggers")]
    public bool warnBeforeShooting = true;
    public float shootWarnTime = 1.2f;
    public bool warnBeforeCharge = true;
    [Tooltip("How long the charge warning lasts (seconds).")]
    public float chargeWarnTime = 1f;
    bool chargeSeen;
    float chargeStart;

    struct Slot { public Renderer r; public int index; public Color baseColor; }
    readonly List<Slot> slots = new List<Slot>();
    MaterialPropertyBlock mpb;
    MoveEnemy move;
    FieldInfo kamiField, kamikazeFlag, indexField, speedField;
    float arrivedAt = -1f;
    int arrivedIdx = -1;
    float shootInterval = -1f, shotClock, manualUntil, phase, id;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        BuildSlots();
        move = GetComponent<MoveEnemy>();
        if (move != null)
        {
            kamiField = typeof(MoveEnemy).GetField("kamikaze", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            kamikazeFlag = typeof(MoveEnemy).GetField("kamikazeReached", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            indexField = typeof(MoveEnemy).GetField("currentWaypointIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            speedField = typeof(MoveEnemy).GetField("speed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        var shoot = GetComponent<EnemyShoot>();
        if (shoot != null)
        {
            var f = typeof(EnemyShoot).GetField("shootInterval", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f != null) shootInterval = (float)f.GetValue(shoot);
        }
    }

    void BuildSlots()
    {
        if (slots.Count > 0) return;
        mpb = new MaterialPropertyBlock();
        id = Random.value * 100f;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null || !m.HasProperty(EmissionId) || !m.IsKeywordEnabled("_EMISSION")) continue;
                slots.Add(new Slot { r = r, index = i, baseColor = m.GetColor(EmissionId) });
            }
        }
    }

    public void Flash(float seconds) { manualUntil = Time.time + seconds; }

    // t = position in the warning (0..1). Returns brightness amount 0..1.
    float Amount(out float t)
    {
        t = 1f;
        if (Time.time < manualUntil) return 1f;

        if (warnBeforeCharge && move != null && kamiField != null && indexField != null && speedField != null
            && (bool)kamiField.GetValue(move))
        {
            int idx = (int)indexField.GetValue(move);
            int count = move.WaypointOrder != null ? move.WaypointOrder.Length : 0;
            if (count > 0)
            {
                if (idx >= count)
                {
                    t = 1f;   // already charging
                    return 1f;
                }
                float spd = Mathf.Max(0.1f, (float)speedField.GetValue(move));
                Vector3 p = transform.position;
                float dist = 0f;
                for (int i = idx; i < count; i++)
                {
                    Vector3 w = move.Waypoints[move.WaypointOrder[i]].position;
                    dist += Vector3.Distance(p, w);
                    p = w;
                }
                bool atWp = Vector3.Distance(transform.position, move.Waypoints[move.WaypointOrder[idx]].position) < 0.15f;
                if (atWp && arrivedIdx != idx) { arrivedIdx = idx; arrivedAt = Time.time; }
                float waits = 0f;
                if (move.WaitTimes != null)
                {
                    for (int i = idx + 1; i < count && i < move.WaitTimes.Length; i++) waits += move.WaitTimes[i];
                    if (idx < move.WaitTimes.Length)
                        waits += atWp ? Mathf.Max(0f, move.WaitTimes[idx] - (Time.time - arrivedAt)) : move.WaitTimes[idx];
                }
                float remaining = dist / spd + waits;
                if (remaining <= chargeWarnTime)
                {
                    t = 1f - remaining / Mathf.Max(0.05f, chargeWarnTime);
                    return useCurve ? Mathf.Clamp01(warningCurve.Evaluate(t)) : 1f;
                }
            }
        }

        if (warnBeforeShooting && shootInterval > 0f
            && GameStateManager.Instance != null
            && GameStateManager.Instance.CurrentState == GameStateManager.GameState.Ingame)
        {
            shotClock += Time.deltaTime;
            if (shotClock >= shootInterval) shotClock -= shootInterval;
            float left = shootInterval - shotClock;
            if (left <= shootWarnTime)
            {
                t = 1f - left / Mathf.Max(0.05f, shootWarnTime);
                return useCurve ? Mathf.Clamp01(warningCurve.Evaluate(t)) : t;
            }
        }
        return 0f;
    }

    void Apply(float a, float t, float dt)
    {
        float w = a;
        if (a > 0f)
        {
            phase += blinkRate * Mathf.Max(0f, speedCurve.Evaluate(t)) * dt;
            if (style == Style.Blink) w = (phase % 1f) < 0.5f ? a : 0f;
            else if (style == Style.Pulse) w = a * (0.5f + 0.5f * Mathf.Sin(phase * Mathf.PI * 2f));
            else if (style == Style.Flicker)
                w = Mathf.PerlinNoise(phase * 3f, id * 7.31f) > (1f - flickerBias) ? a : a * 0.15f;
        }

        foreach (var s in slots)
        {
            if (s.r == null) continue;
            Color c = style == Style.ColorShift
                ? Color.Lerp(s.baseColor, flashColor, w)
                : s.baseColor * Mathf.Lerp(1f, intensityMultiplier, w);
            s.r.GetPropertyBlock(mpb, s.index);
            mpb.SetColor(EmissionId, c);
            s.r.SetPropertyBlock(mpb, s.index);
        }
    }

    void LateUpdate()
    {
        float t;
        float a = Amount(out t);
        Apply(a, t, Time.deltaTime);
    }

    // ---- Edit Mode preview (used by the Inspector button)
    public void Preview(float t, float dt, bool fullFlash)
    {
        BuildSlots();
        float a = fullFlash ? 1f : (useCurve ? Mathf.Clamp01(warningCurve.Evaluate(t)) : t);
        Apply(a, fullFlash ? 1f : t, dt);
    }

    public void PreviewEnd()
    {
        foreach (var s in slots)
            if (s.r != null) s.r.SetPropertyBlock(null, s.index);
        slots.Clear();
    }
}