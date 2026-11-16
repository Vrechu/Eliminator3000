using UnityEngine;
using System.Reflection;

/// <summary>Sits on the LevelRoot next to MoveTrack. Scales its moveSpeed by the SectionSpeed of the section at the player.</summary>
[DisallowMultipleComponent]
public class LevelSpeedController : MonoBehaviour
{
    public float playZ;            // written by the Level Manager
    public float blendSeconds = 1f;

    private MoveTrack mover;
    private FieldInfo field;
    private float baseSpeed;
    private float current = 1f;
    private float from = 1f;
    private float to = 1f;
    private float blendT = 1f;
    private SectionSpeed[] speeds;
    private LevelSection[] sections;
    private LevelSection lastSection;
    private bool hasLast;

    public float BaseSpeed { get { return baseSpeed; } }

    private void Start()
    {
        mover = GetComponent<MoveTrack>();
        if (mover == null) { Debug.LogWarning("[Speed] No MoveTrack on " + name + ". Section speeds are off."); enabled = false; return; }
        field = typeof(MoveTrack).GetField("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null || field.FieldType != typeof(float))
        {
            Debug.LogWarning("[Speed] MoveTrack has no float field 'moveSpeed'. Section speeds are off.");
            enabled = false;
            return;
        }
        baseSpeed = (float)field.GetValue(mover);
        speeds = FindObjectsByType<SectionSpeed>(FindObjectsSortMode.None);
        sections = new LevelSection[speeds.Length];
        for (int i = 0; i < speeds.Length; i++) sections[i] = speeds[i].GetComponent<LevelSection>();
        Debug.Log("[Speed] Controller started. Base speed " + baseSpeed + ", " + speeds.Length + " section(s) with a speed setting, play Z " + playZ + ".");
    }

    private void Update()
    {
        float target = 1f;
        LevelSection found = null;
        for (int i = 0; i < speeds.Length; i++)
        {
            var sec = sections[i];
            if (speeds[i] == null || sec == null || !sec.enabled || !sec.gameObject.activeInHierarchy) continue;
            if (playZ >= sec.StartZ && playZ < sec.EndZ)
            {
                target = Mathf.Max(0.05f, speeds[i].speedMultiplier);
                found = sec;
                break;
            }
        }

        if (!Mathf.Approximately(target, to))
        {
            from = current;
            to = target;
            blendT = 0f;
            Debug.Log("[Speed] " + (found != null ? "Section #" + found.sectionNumber : "No speed section") + " -> target " + target.ToString("F2") + "x (from " + from.ToString("F2") + "x, blend " + blendSeconds.ToString("F1") + "s).");
        }
        else if (found != lastSection || !hasLast)
        {
            hasLast = true;
        }
        lastSection = found;

        if (blendT < 1f)
        {
            blendT = Mathf.Min(1f, blendT + Time.deltaTime / Mathf.Max(0.01f, blendSeconds));
            float k = blendT * blendT * (3f - 2f * blendT);
            current = Mathf.Exp(Mathf.Lerp(Mathf.Log(from), Mathf.Log(to), k));
        }
        else current = to;

        field.SetValue(mover, baseSpeed * current);
    }

    private void OnDestroy()
    {
        if (mover != null && field != null) field.SetValue(mover, baseSpeed);
    }
}