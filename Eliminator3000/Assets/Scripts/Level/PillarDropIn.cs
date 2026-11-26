using UnityEngine;

public class PillarDropIn : MonoBehaviour
{
    [Tooltip("Drag a player spawn point here (the ship's Z). Empty = Z 0.")]
    public Transform reference;
    [Tooltip("A pillar starts falling when it is this far ahead of the ship (Z).")]
    public float triggerDistance = 150f;
    public float dropHeight = 60f;
    public float dropDuration = 0.6f;
    [Tooltip("Random extra wait per pillar so they don't all land together.")]
    public float randomDelay = 0.25f;

    [Header("Landing sound (optional, empty = silent)")]
    public AudioClip landClip;
    [Range(0f, 1f)] public float landVolume = 1f;
    public float maxHearDistance = 60f;

    public static bool Previewing;

    private class Pillar
    {
        public Transform t;
        public Vector3 finalPos;
        public Renderer[] rends;
        public float delay, timer;
        public int state; // 0 waiting, 1 falling, 2 landed
    }

    private Pillar[] pillars;

    private void Start()
    {
        pillars = new Pillar[transform.childCount];
        for (int i = 0; i < pillars.Length; i++)
        {
            var p = new Pillar();
            p.t = transform.GetChild(i);
            p.finalPos = p.t.localPosition;
            var list = new System.Collections.Generic.List<Renderer>();
            foreach (var r in p.t.GetComponentsInChildren<Renderer>(true))
                if (r.enabled) list.Add(r);
            p.rends = list.ToArray();
            p.delay = Random.value * randomDelay;
            p.timer = -p.delay;
            SetVisible(p, false);
            p.t.localPosition = p.finalPos + Vector3.up * dropHeight;
            pillars[i] = p;
        }
    }

    private void Update()
    {
        if (pillars == null) return;
        float refZ = reference != null ? reference.position.z : 0f;

        foreach (var p in pillars)
        {
            if (p.t == null || p.state == 2) continue;

            if (p.state == 0)
            {
                if (p.t.position.z - refZ > triggerDistance) continue;
                p.state = 1;
            }

            p.timer += Time.deltaTime;
            if (p.timer < 0f) continue;

            SetVisible(p, true);
            float k = Mathf.Clamp01(p.timer / Mathf.Max(0.01f, dropDuration));
            p.t.localPosition = p.finalPos + Vector3.up * dropHeight * (1f - k * k);
            if (k >= 1f) { p.t.localPosition = p.finalPos; p.state = 2; PlayLand(p); }
        }
    }

    private void PlayLand(Pillar p)
    {
        if (landClip == null) return;
        Vector3 from = reference != null ? reference.position : (Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        float d = Vector3.Distance(p.t.position, from);
        float vol = landVolume * Mathf.Clamp01(1f - d / Mathf.Max(1f, maxHearDistance));
        if (vol > 0.01f) AudioSource.PlayClipAtPoint(landClip, p.t.position, vol);
    }

    private static void SetVisible(Pillar p, bool on)
    {
        foreach (var r in p.rends) if (r != null) r.enabled = on;
    }

#if UNITY_EDITOR
    [System.NonSerialized] public Vector3[] previewFinals;

    public void PreviewSet(float progress)
    {
        if (Application.isPlaying) return;
        Previewing = true;
        int n = transform.childCount;
        if (previewFinals == null || previewFinals.Length != n)
        {
            previewFinals = new Vector3[n];
            for (int i = 0; i < n; i++) previewFinals[i] = transform.GetChild(i).localPosition;
        }
        float total = dropDuration + randomDelay;
        for (int i = 0; i < n; i++)
        {
            float delay = ((i * 0.37f) % 1f) * randomDelay;
            float k = Mathf.Clamp01((progress * total - delay) / Mathf.Max(0.01f, dropDuration));
            transform.GetChild(i).localPosition = previewFinals[i] + Vector3.up * dropHeight * (1f - k * k);
        }
    }

    public void PreviewRestore()
    {
        Previewing = false;
        if (previewFinals == null) return;
        if (previewFinals.Length == transform.childCount)
            for (int i = 0; i < previewFinals.Length; i++) transform.GetChild(i).localPosition = previewFinals[i];
        previewFinals = null;
    }
#endif
}