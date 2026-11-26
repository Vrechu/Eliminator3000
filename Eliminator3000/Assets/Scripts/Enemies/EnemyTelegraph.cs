using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class EnemyTelegraph : MonoBehaviour
{
    public enum Style { Blink, Pulse, ColorShift, Flicker }
    public enum Preset { Custom, SlowBlink, FastBlink, Flicker, SoftPulse, RampUp }

    [Header("Style")]
    public Preset preset = Preset.SlowBlink;
    [Tooltip("On = switches between normal and Flash Color. Off = multiplies brightness.")]
    public bool useFlashColor = true;
    public Style style = Style.Blink;
    public float intensityMultiplier = 6f;
    [ColorUsage(false, true)] public Color flashColor = new Color(6f, 5f, 0.5f);

    [Header("Speed")]
    public float blinkRate = 2f;
    public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
    [Range(0f, 1f)] public float flickerBias = 0.5f;

    [Header("Brightness over the warning")]
    public bool useCurve = true;
    public AnimationCurve warningCurve = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(1f, 1f));

    [Header("Triggers")]
    public bool warnBeforeShooting = true;
    public float shootWarnTime = 1.2f;
    public bool warnBeforeCharge = true;
    [Tooltip("Seconds before the charge that the warning starts.")]
    public float chargeWarnTime = 1f;
    [Tooltip("Steady colour while the kamikaze is actually charging.")]
    [ColorUsage(false, true)] public Color chargeColor = new Color(6f, 0.3f, 0.2f);

    [Header("Vibrate during warning (not while charging)")]
    public bool vibrate = true;
    public float vibrateStrength = 0.15f;
    public float vibrateSpeed = 40f;
    [Tooltip("Model child to shake. Empty = root.")]
    public Transform vibrateTarget;

    struct Slot { public Renderer r; public int index; public Color baseColor; }
    readonly List<Slot> slots = new List<Slot>();
    MaterialPropertyBlock mpb;
    MoveEnemy move;
    FieldInfo kamiField, indexField, speedField;
    float arrivedAt = -1f;
    int arrivedIdx = -1;
    float shootInterval = -1f, shotClock, manualUntil, phase, id;
    bool charging;
    Vector3 lastVib;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    void OnValidate()
    {
        if (preset == Preset.Custom) return;
        speedCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        switch (preset)
        {
            case Preset.SlowBlink: style = Style.Blink; blinkRate = 2f; break;
            case Preset.FastBlink: style = Style.Blink; blinkRate = 6f; break;
            case Preset.Flicker: style = Style.Flicker; blinkRate = 10f; flickerBias = 0.5f; break;
            case Preset.SoftPulse: style = Style.Pulse; blinkRate = 1.5f; break;
            case Preset.RampUp:
                style = Style.Blink; blinkRate = 2f;
                speedCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 3f)); break;
        }
    }

    void Start()
    {
        BuildSlots();
        move = GetComponent<MoveEnemy>();
        var f = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        if (move != null)
        {
            kamiField = typeof(MoveEnemy).GetField("kamikaze", f);
            indexField = typeof(MoveEnemy).GetField("currentWaypointIndex", f);
            speedField = typeof(MoveEnemy).GetField("speed", f);
        }
        var shoot = GetComponent<EnemyShoot>();
        if (shoot != null)
        {
            var sf = typeof(EnemyShoot).GetField("shootInterval", f);
            if (sf != null) shootInterval = (float)sf.GetValue(shoot);
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

    float Amount(out float t)
    {
        t = 1f;
        charging = false;
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
                    t = 1f;
                    charging = true;
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
        Transform vt = vibrateTarget != null ? vibrateTarget : transform;
        vt.localPosition -= lastVib;
        lastVib = Vector3.zero;
        if (vibrate && a > 0f && !charging)
        {
            float nx = Mathf.PerlinNoise(Time.time * vibrateSpeed, id) * 2f - 1f;
            float ny = Mathf.PerlinNoise(id, Time.time * vibrateSpeed) * 2f - 1f;
            lastVib = new Vector3(nx, ny, 0f) * (vibrateStrength * a);
            vt.localPosition += lastVib;
        }

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
            Color c = (style == Style.ColorShift || useFlashColor)
                ? Color.Lerp(s.baseColor, flashColor, w)
                : s.baseColor * Mathf.Lerp(1f, intensityMultiplier, w);
            s.r.GetPropertyBlock(mpb, s.index);
            mpb.SetColor(EmissionId, charging ? chargeColor : c);
            s.r.SetPropertyBlock(mpb, s.index);
        }
    }

    void LateUpdate()
    {
        float t;
        float a = Amount(out t);
        Apply(a, t, Time.deltaTime);
    }

    public void Preview(float t, float dt, bool fullFlash)
    {
        BuildSlots();
        charging = fullFlash;
        float a = fullFlash ? 1f : (useCurve ? Mathf.Clamp01(warningCurve.Evaluate(t)) : t);
        Apply(a, fullFlash ? 1f : t, dt);
    }

    public void PreviewEnd()
    {
        (vibrateTarget != null ? vibrateTarget : transform).localPosition -= lastVib;
        lastVib = Vector3.zero;
        charging = false;
        foreach (var s in slots)
            if (s.r != null) s.r.SetPropertyBlock(null, s.index);
        slots.Clear();
    }
}