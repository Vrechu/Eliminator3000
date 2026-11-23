using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class JuiceEffects : MonoBehaviour
{
    [System.Serializable]
    public class Effect
    {
        public string name = "Boost";
        [Header("Camera")]
        public float fovAdd = 12f;
        [Range(0f, 1f)] public float shake = 0f;
        [Header("Level speed (1 = unchanged, 0.4 = slow down, 1.6 = faster)")]
        public float speedMultiplier = 1.6f;
        [Header("Player movement (1 = unchanged, 0.4 = sluggish)")]
        public float playerMoveMultiplier = 1f;
        [Header("Timing (seconds)")]
        public float attack = 0.4f;   // ease in
        public float hold = 2f;       // stay at full strength
        public float release = 0.8f;  // ease out
    }

    class Active { public Effect e; public float t; }

    public static JuiceEffects Instance;

    [Header("Base look")]
    float baseFov;
    [Header("Shake feel")]
    public float maxShakeAngle = 2.5f;
    public float maxShakeOffset = 0.25f;
    public float shakeFrequency = 25f;
    [Header("Effects")]
    public List<Effect> effects = new List<Effect>
    {
        new Effect { name = "Boost", fovAdd = 12f, shake = 0.25f, speedMultiplier = 1.6f, attack = 0.4f, hold = 2f, release = 0.8f },
        new Effect { name = "Slow",  fovAdd = -6f, shake = 0f,    speedMultiplier = 0.5f, attack = 0.3f, hold = 2f, release = 1f },
        new Effect { name = "Hit",   fovAdd = 3f,  shake = 0.7f,  speedMultiplier = 1f,   attack = 0.02f, hold = 0.1f, release = 0.5f },
    };

    readonly List<Active> active = new List<Active>();
    class ShipRef { public MoveShip ship; public float[] baseVals; }
    readonly List<ShipRef> ships = new List<ShipRef>();
    static readonly string[] shipFieldNames = { "horizontalMaxSpeed", "verticalMaxSpeed", "horizontalAcceleration", "verticalAcceleration" };
    FieldInfo[] shipInfos;
    float nextShipScan;

    void ApplyShips(float move)
    {
        if (shipInfos == null)
        {
            shipInfos = new FieldInfo[shipFieldNames.Length];
            for (int i = 0; i < shipInfos.Length; i++)
                shipInfos[i] = typeof(MoveShip).GetField(shipFieldNames[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        if (Time.time >= nextShipScan)
        {
            nextShipScan = Time.time + 1f;
            foreach (var m in FindObjectsByType<MoveShip>(FindObjectsSortMode.None))
            {
                bool known = false;
                foreach (var r in ships) if (r.ship == m) { known = true; break; }
                if (known) continue;
                var nr = new ShipRef { ship = m, baseVals = new float[shipInfos.Length] };
                for (int i = 0; i < shipInfos.Length; i++)
                    nr.baseVals[i] = shipInfos[i] != null ? (float)shipInfos[i].GetValue(m) : 0f;
                ships.Add(nr);
            }
        }
        for (int s = ships.Count - 1; s >= 0; s--)
        {
            var r = ships[s];
            if (r.ship == null) { ships.RemoveAt(s); continue; }
            for (int i = 0; i < shipInfos.Length; i++)
                if (shipInfos[i] != null) shipInfos[i].SetValue(r.ship, r.baseVals[i] * move);
        }
    }
    Camera cam;
    Component mover;
    FieldInfo speedField;
    float lastWritten = float.NaN;
    Vector3 lastPosOff;
    Quaternion lastRotOff = Quaternion.identity;

    void Awake() { Instance = this; cam = GetComponent<Camera>(); if (cam != null) baseFov = cam.fieldOfView; }

    public void Play(string effectName)
    {
        foreach (var e in effects) if (e.name == effectName) { active.Add(new Active { e = e }); return; }
        Debug.LogWarning("[Juice] No effect named '" + effectName + "'.");
    }
    public void Play(int index) { if (index >= 0 && index < effects.Count) active.Add(new Active { e = effects[index] }); }
    public void StopAll() { active.Clear(); }

    float Weight(Active a)
    {
        var e = a.e;
        if (a.t < e.attack) return e.attack > 0f ? a.t / e.attack : 1f;
        if (a.t < e.attack + e.hold) return 1f;
        float r = a.t - e.attack - e.hold;
        return e.release > 0f ? 1f - r / e.release : 0f;
    }

    void FindMover()
    {
        foreach (var m in FindObjectsByType<MoveTrack>(FindObjectsSortMode.None))
        {
            if (!m.enabled) continue;
            var f = typeof(MoveTrack).GetField("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null) return;
            mover = m; speedField = f; lastWritten = float.NaN;
            return;
        }
    }

    void LateUpdate()
    {
        // remove last frame's shake offset first, so we never fight other camera scripts
        transform.localPosition -= lastPosOff;
        transform.localRotation = transform.localRotation * Quaternion.Inverse(lastRotOff);
        float fov = 0f, speed = 1f, trauma = 0f, move = 1f;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var a = active[i];
            a.t += Time.deltaTime;
            float w = Mathf.Clamp01(Weight(a));
            if (a.t >= a.e.attack + a.e.hold + a.e.release) { active.RemoveAt(i); continue; }
            fov += a.e.fovAdd * w;
            speed *= Mathf.Lerp(1f, a.e.speedMultiplier, w);
            move *= Mathf.Lerp(1f, a.e.playerMoveMultiplier, w);
            trauma = Mathf.Max(trauma, a.e.shake * w);
        }

        if (cam != null) cam.fieldOfView = baseFov + fov;
                ApplyShips(move);
        if (mover == null) FindMover();
        if (mover != null && speedField != null && GameStateManager.Instance != null
            && GameStateManager.Instance.CurrentState == GameStateManager.GameState.Ingame)
        {
            float cur = Mathf.Abs((float)speedField.GetValue(mover));
            float extra = cur * (speed - 1f) * Time.deltaTime;   // speed < 1 pushes the level back forward
            mover.transform.Translate(0f, 0f, -extra);
        }

        float s = trauma * trauma, tm = Time.time * shakeFrequency;
        float nx = Mathf.PerlinNoise(tm, 0f) * 2f - 1f;
        float ny = Mathf.PerlinNoise(0f, tm) * 2f - 1f;
        float nz = Mathf.PerlinNoise(tm, tm) * 2f - 1f;
        lastPosOff = new Vector3(nx, ny, 0f) * (maxShakeOffset * s);
        lastRotOff = Quaternion.Euler(ny * maxShakeAngle * s, nx * maxShakeAngle * s, nz * maxShakeAngle * s);
        transform.localPosition += lastPosOff;
        transform.localRotation = transform.localRotation * lastRotOff;
    }


    void OnDisable() { ApplyShips(1f); }
}