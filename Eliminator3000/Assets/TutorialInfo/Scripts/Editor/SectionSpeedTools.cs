using UnityEngine;
using UnityEditor;

/// <summary>Editor-only. Per-section speed slider in the Level Manager + keeps the LevelSpeedController set up.</summary>
[InitializeOnLoad]
public static class SectionSpeedTools
{
    static SectionSpeedTools()
    {
        EditorApplication.playModeStateChanged -= OnPlay;
        EditorApplication.playModeStateChanged += OnPlay;
    }

    private static void OnPlay(PlayModeStateChange s)
    {
        if (s != PlayModeStateChange.ExitingEditMode) return;
        bool any = false;
        foreach (var sp in Object.FindObjectsByType<SectionSpeed>(FindObjectsSortMode.None))
            if (!Mathf.Approximately(sp.speedMultiplier, 1f)) any = true;
        if (any) EnsureController();
        else RefreshPlayZ();
    }

    private static bool AvgSpawnZ(out float z)
    {
        z = 0f;
        var pts = LevelManagerWindow.GetSpawnPoints();
        if (pts.Count == 0) return false;
        float sum = 0f;
        foreach (var t in pts) sum += t.position.z;
        z = sum / pts.Count;
        return true;
    }

    private static MoveTrack FindRootMover()
    {
        foreach (var m in Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None))
            if (m.enabled && m.GetComponent<LevelSection>() == null) return m;
        return null;
    }

    private static void RefreshPlayZ()
    {
        if (!AvgSpawnZ(out float z)) return;
        foreach (var c in Object.FindObjectsByType<LevelSpeedController>(FindObjectsSortMode.None))
        {
            if (Mathf.Approximately(c.playZ, z)) continue;
            Undo.RecordObject(c, "Update play Z");
            c.playZ = z;
            EditorUtility.SetDirty(c);
        }
    }

    private static void EnsureController()
    {
        var root = FindRootMover();
        if (root == null)
        {
            Debug.LogWarning("[Speed] No active MoveTrack on the LevelRoot. Click 'Build LevelRoot' first.");
            return;
        }
        if (root.GetComponent<LevelSpeedController>() == null)
        {
            Undo.AddComponent<LevelSpeedController>(root.gameObject);
            Debug.Log("[Speed] Added LevelSpeedController to '" + root.name + "'.", root);
        }
        RefreshPlayZ();
    }

    private static float BaseSpeed()
    {
        var root = FindRootMover();
        if (root == null) return -1f;
        var p = new SerializedObject(root).FindProperty("moveSpeed");
        return p != null ? Mathf.Abs(p.floatValue) : -1f;
    }

    private static void SetMult(LevelSection s, SectionSpeed sp, float n)
    {
        n = Mathf.Clamp(n, 0.1f, SectionSpeed.MaxMultiplier);
        if (sp == null)
        {
            if (Mathf.Approximately(n, 1f)) return;
            sp = Undo.AddComponent<SectionSpeed>(s.gameObject);
            Debug.Log($"[Speed] Section #{s.sectionNumber}: speed set to {n:F2}x. Save the scene (Ctrl+S).", s);
            sp.speedMultiplier = n;
            EditorUtility.SetDirty(sp);
            EnsureController();
            GUIUtility.ExitGUI();
            return;
        }
        Undo.RecordObject(sp, "Change section speed");
        sp.speedMultiplier = n;
        EditorUtility.SetDirty(sp);
        EnsureController();
    }

    public static void DrawRow(LevelSection s, bool on)
    {
        var sp = s.GetComponent<SectionSpeed>();
        float cur = sp != null ? sp.speedMultiplier : 1f;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        using (new EditorGUI.DisabledScope(!on))
        {
            EditorGUILayout.LabelField(new GUIContent("Speed", "Track speed while the player is inside this section. 1 = default."), GUILayout.Width(40));
            EditorGUI.BeginChangeCheck();
            float n = EditorGUILayout.Slider(cur, 0.1f, SectionSpeed.MaxMultiplier);
            if (EditorGUI.EndChangeCheck()) SetMult(s, sp, n);
            if (GUILayout.Button("1x", GUILayout.Width(30))) SetMult(s, sp, 1f);
        }
        EditorGUILayout.EndHorizontal();

        float bs = BaseSpeed();
        if (bs > 0.0001f && s.HasMeasurement && on)
        {
            float m = sp != null ? sp.speedMultiplier : 1f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14);
            EditorGUILayout.LabelField($"{bs * m:F1} units/s, section takes {s.Length / (bs * m):F1}s (default {s.Length / bs:F1}s)", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }
    }
}