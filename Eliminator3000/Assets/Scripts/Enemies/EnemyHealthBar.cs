using UnityEngine;
using System.Reflection;

public class EnemyHealthBar : MonoBehaviour
{
    public float height = 3.2f;
    public float width = 2.4f;
    public float thickness = 0.22f;
    [Tooltip("Pulls the bar towards the camera so the model can't hide it.")]
    public float towardCamera = 2f;
    public bool onlyAfterFirstHit = false;
    [Tooltip("Turns off the number text from the engineer's script.")]
    public bool hideNumbers = true;
    [ColorUsage(false, true)] public Color fullColor = new Color(0.1f, 1f, 1f);
    [ColorUsage(false, true)] public Color midColor = new Color(1f, 0.2f, 1f);
    [ColorUsage(false, true)] public Color lowColor = new Color(1f, 0.15f, 0.1f);
    public Material materialOverride;   // optional: any unlit/emissive material

    EnemyInfo info;
    FieldInfo healthField;
    int maxHealth = 1, lastHealth;
    GameObject root;
    Transform fill, back, frame;
    Material fillMat, backMat, frameMat;
    float bump;

    void Start()
    {
        info = GetComponent<EnemyInfo>();
        if (info == null) { enabled = false; return; }
        var f = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        healthField = typeof(EnemyInfo).GetField("health", f);
        if (healthField == null) { enabled = false; return; }
        maxHealth = Mathf.Max(1, (int)healthField.GetValue(info));
        lastHealth = maxHealth;

        if (hideNumbers)
        {
            var tf = typeof(EnemyInfo).GetField("healthText", f);
            var t = tf != null ? tf.GetValue(info) as Behaviour : null;
            if (t != null) t.enabled = false;
        }

        root = new GameObject("HealthBar_" + name);
        frame = MakeQuad("Frame", out frameMat, new Color(0.05f, 0.05f, 0.1f, 0.9f));
        back = MakeQuad("Back", out backMat, new Color(0.02f, 0.02f, 0.05f, 0.9f));
        fill = MakeQuad("Fill", out fillMat, fullColor);
        root.SetActive(!onlyAfterFirstHit);
    }

    Transform MakeQuad(string n, out Material mat, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
        g.name = n;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(root.transform, false);
        var r = g.GetComponent<Renderer>();
        mat = materialOverride != null ? new Material(materialOverride) : new Material(Shader.Find("Sprites/Default"));
        SetColor(mat, c);
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return g.transform;
    }

    static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c);
    }

    void LateUpdate()
    {
        if (root == null) return;
        int h = Mathf.Max(0, (int)healthField.GetValue(info));
        if (h < lastHealth) { bump = 1f; if (onlyAfterFirstHit) root.SetActive(true); }
        lastHealth = h;
        bump = Mathf.MoveTowards(bump, 0f, Time.deltaTime * 4f);

        Camera cam = Camera.main;
        Vector3 pos = transform.position + Vector3.up * height;
        if (cam != null)
        {
            pos -= cam.transform.forward * towardCamera;
            root.transform.rotation = cam.transform.rotation;
        }
        root.transform.position = pos;

        float frac = Mathf.Clamp01(h / (float)maxHealth);
        float w = width;
        frame.localScale = new Vector3(w + thickness * 2f, thickness * 3f, 1f);
        back.localScale = new Vector3(w, thickness, 1f);
        back.localPosition = new Vector3(0f, 0f, -0.01f);
        frame.localPosition = new Vector3(0f, 0f, 0f);
        fill.localScale = new Vector3(Mathf.Max(0.001f, w * frac), thickness * 0.8f, 1f);
        fill.localPosition = new Vector3(-w * 0.5f + w * frac * 0.5f, 0f, -0.02f);

        Color c = frac > 0.5f ? Color.Lerp(midColor, fullColor, (frac - 0.5f) * 2f) : Color.Lerp(lowColor, midColor, frac * 2f);
        c = Color.Lerp(c, Color.white * 3f, bump * 0.6f);
        SetColor(fillMat, c);
    }

    void OnDestroy() { if (root != null) Destroy(root); }
}