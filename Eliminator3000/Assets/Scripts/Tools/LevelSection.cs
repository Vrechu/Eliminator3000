using UnityEngine;

public class LevelSection : MonoBehaviour
{
    public const string TestTargetKey = "LevelManager.TestTargetSection";

    private static readonly Vector3 FallbackCenterOffset = new Vector3(0f, 5f, 25f);
    private static readonly Vector3 FallbackSize = new Vector3(10f, 10f, 50f);

    [Header("Section Metadata")]
    public int sectionNumber = 1;
    public string sectionLabel = "New Section";

    [Header("Length along Z")]
    [Tooltip("Off: length comes from the measured Renderer bounds. On: use Manual Length (start edge stays at the measured start).")]
    public bool useManualLength = false;
    public float manualLength = 100f;

    [Header("Visual Debug")]
    public Color gizmoColor = Color.cyan;

    // Measured once (by the Level Manager window), stored relative to this transform so it follows moves.
    [SerializeField, HideInInspector] private Vector3 centerOffset;
    [SerializeField, HideInInspector] private Vector3 measuredSize;
    [SerializeField, HideInInspector] private bool hasMeasurement;

    public bool HasMeasurement { get { return hasMeasurement; } }
    public float AutoLength { get { return hasMeasurement ? measuredSize.z : FallbackSize.z; } }

    public bool IsTestSpawnTarget
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.SessionState.GetInt(TestTargetKey, -1) == sectionNumber;
#else
            return false;
#endif
        }
    }

    /// <summary>World-space bounds of the section, using the manual length if enabled.</summary>
    public Bounds GetBounds()
    {
        Vector3 size;
        Vector3 center;
        if (hasMeasurement)
        {
            size = measuredSize;
            center = transform.position + centerOffset;
        }
        else
        {
            size = FallbackSize;
            center = transform.position + FallbackCenterOffset;
        }

        if (useManualLength)
        {
            float start = center.z - size.z * 0.5f;
            size.z = Mathf.Max(0.01f, manualLength);
            center.z = start + size.z * 0.5f;
        }
        return new Bounds(center, size);
    }

    public float StartZ { get { return GetBounds().min.z; } }
    public float EndZ { get { return GetBounds().max.z; } }
    public float Length { get { return GetBounds().size.z; } }

    /// <summary>
    /// Measures the combined Renderer bounds of this section's children (ignoring nested LevelSections,
    /// particles, trails, lines and the debug meshes of EnemySpawnTrigger / WinTrigger objects).
    /// Returns false if no renderer was found and a fallback box is used.
    /// </summary>
    public bool Measure()
    {
        bool found = false;
        Bounds total = new Bounds(transform.position, Vector3.zero);
        Renderer[] renderers = GetComponentsInChildren<Renderer>(false);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || !r.enabled) continue;
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (r.GetComponentInParent<LevelSection>() != this) continue; // belongs to a nested section
            if (r.GetComponent<WinTrigger>() != null || r.GetComponent<EnemySpawnTrigger>() != null) continue; // trigger volumes are not level geometry
            if (!found) { total = r.bounds; found = true; }
            else total.Encapsulate(r.bounds);
        }

        if (found)
        {
            centerOffset = total.center - transform.position;
            measuredSize = total.size;
        }
        else
        {
            centerOffset = FallbackCenterOffset;
            measuredSize = FallbackSize;
        }
        hasMeasurement = true;
        return found;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Bounds b = GetBounds();
        bool isTarget = IsTestSpawnTarget;

        Gizmos.color = isTarget ? Color.green : gizmoColor;
        Gizmos.DrawWireCube(b.center, b.size);

        // Start edge (where the player enters) and end edge (seam to the next section).
        Gizmos.color = isTarget ? Color.yellow : new Color(1f, 1f, 1f, 0.7f);
        Gizmos.DrawWireCube(new Vector3(b.center.x, b.center.y, b.min.z), new Vector3(b.size.x, b.size.y, 0.5f));
        Gizmos.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        Gizmos.DrawWireCube(new Vector3(b.center.x, b.center.y, b.max.z), new Vector3(b.size.x, b.size.y, 0.5f));
    }
#endif
}