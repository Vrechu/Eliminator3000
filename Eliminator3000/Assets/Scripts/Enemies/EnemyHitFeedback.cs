using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[DefaultExecutionOrder(200)]
public class EnemyHitFeedback : MonoBehaviour
{
    public enum Mode { WholeModel, EmissionZonesOnly, Both }

    [Header("Flash")]
    public Mode mode = Mode.WholeModel;
    [ColorUsage(false, true)] public Color flashColor = new Color(3f, 3f, 3f);
    public float emissionBoost = 8f;
    public float duration = 0.15f;
    public AnimationCurve fade = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Vibrate (optional)")]
    public bool vibrate = true;
    public float vibrateStrength = 0.2f;
    public float vibrateSpeed = 60f;
    [Tooltip("Model child to shake. Empty = root.")]
    public Transform vibrateTarget;

    [Header("Punch scale (0 = off)")]
    public float punchScale = 0.08f;

    struct Slot { public Renderer r; public int index; public Color baseColor, emission; public bool hasBase, hasEmission; }
    readonly List<Slot> slots = new List<Slot>();
    MaterialPropertyBlock mpb;
    float timer = 999f, id;
    Vector3 lastVib, baseScale;
    bool dirty;
    EnemyInfo info;
    FieldInfo healthField;
    int lastHealth;
    static readonly int BaseId = Shader.PropertyToID("_BaseColor");
    static readonly int EmId = Shader.PropertyToID("_EmissionColor");

    Transform VT { get { return vibrateTarget != null ? vibrateTarget : transform; } }

    void Start()
    {
        mpb = new MaterialPropertyBlock();
        id = Random.value * 100f;
        baseScale = VT.localScale;

        info = GetComponent<EnemyInfo>();
        if (info != null)
        {
            healthField = typeof(EnemyInfo).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (healthField != null) lastHealth = (int)healthField.GetValue(info);
        }

        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                var s = new Slot { r = r, index = i };
                if (m.HasProperty(BaseId)) { s.hasBase = true; s.baseColor = m.GetColor(BaseId); }
                if (m.HasProperty(EmId) && m.IsKeywordEnabled("_EMISSION")) { s.hasEmission = true; s.emission = m.GetColor(EmId); }
                if (s.hasBase || s.hasEmission) slots.Add(s);
            }
        }
    }

    public void Hit() { timer = 0f; }
    // ---- Edit Mode preview
    public void PreviewSet(float w)
    {
        if (Application.isPlaying) return;
        if (mpb == null) mpb = new MaterialPropertyBlock();
        if (slots.Count == 0)
        {
            id = Random.value * 100f;
            baseScale = VT.localScale;
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    var s = new Slot { r = r, index = i };
                    if (m.HasProperty(BaseId)) { s.hasBase = true; s.baseColor = m.GetColor(BaseId); }
                    if (m.HasProperty(EmId) && m.IsKeywordEnabled("_EMISSION")) { s.hasEmission = true; s.emission = m.GetColor(EmId); }
                    if (s.hasBase || s.hasEmission) slots.Add(s);
                }
            }
        }
        VT.localPosition -= lastVib;
        lastVib = Vector3.zero;
        VT.localScale = baseScale;
        Apply(w);
        if (vibrate)
        {
            float nx = Mathf.PerlinNoise(Time.realtimeSinceStartup * vibrateSpeed, id) * 2f - 1f;
            float ny = Mathf.PerlinNoise(id, Time.realtimeSinceStartup * vibrateSpeed) * 2f - 1f;
            lastVib = new Vector3(nx, ny, 0f) * (vibrateStrength * w);
            VT.localPosition += lastVib;
        }
        if (punchScale > 0f) VT.localScale = baseScale * (1f + punchScale * w);
    }

    public void PreviewEnd()
    {
        if (Application.isPlaying) return;
        VT.localPosition -= lastVib;
        lastVib = Vector3.zero;
        if (slots.Count > 0) VT.localScale = baseScale;
        foreach (var s in slots) if (s.r != null) s.r.SetPropertyBlock(null, s.index);
        slots.Clear();
    }

    void LateUpdate()
    {
        // auto-detect damage: health went down
        if (healthField != null && info != null)
        {
            int h = (int)healthField.GetValue(info);
            if (h < lastHealth) Hit();
            lastHealth = h;
        }

        VT.localPosition -= lastVib;
        lastVib = Vector3.zero;
        VT.localScale = baseScale;

        if (timer > duration)
        {
            if (dirty) { Apply(0f); dirty = false; }
            return;
        }

        timer += Time.deltaTime;
        float w = Mathf.Clamp01(fade.Evaluate(Mathf.Clamp01(timer / Mathf.Max(0.01f, duration))));
        Apply(w);
        dirty = true;

        if (vibrate)
        {
            float nx = Mathf.PerlinNoise(Time.time * vibrateSpeed, id) * 2f - 1f;
            float ny = Mathf.PerlinNoise(id, Time.time * vibrateSpeed) * 2f - 1f;
            lastVib = new Vector3(nx, ny, 0f) * (vibrateStrength * w);
            VT.localPosition += lastVib;
        }
        if (punchScale > 0f) VT.localScale = baseScale * (1f + punchScale * w);
    }

    void Apply(float w)
    {
        bool whole = mode != Mode.EmissionZonesOnly;
        bool zones = mode != Mode.WholeModel;
        foreach (var s in slots)
        {
            if (s.r == null) continue;
            s.r.GetPropertyBlock(mpb, s.index);
            if (whole && s.hasBase)
                mpb.SetColor(BaseId, Color.Lerp(s.baseColor, new Color(flashColor.r, flashColor.g, flashColor.b, s.baseColor.a), w));
            if (s.hasEmission)
            {
                if (zones) mpb.SetColor(EmId, Color.Lerp(s.emission, flashColor * emissionBoost * 0.3f, w));
                else if (whole) mpb.SetColor(EmId, Color.Lerp(s.emission, flashColor, w));
            }
            s.r.SetPropertyBlock(mpb, s.index);
        }
    }
}