using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public class LevelManagerWindow : EditorWindow
{
    public const string ShiftPendingKey = "LevelManager.ShiftPending";
    private const string ActiveTestKey = "LevelManager.ActiveTestSection";

    private const string PlayZPrefKey = "LevelManager.PlayZ";
    private const string AutoZPrefKey = "LevelManager.AutoPlayZ";
    private const string LeadInPrefKey = "LevelManager.LeadIn";
    private const string GapPrefKey = "LevelManager.Gap";
    private const string AlignXPrefKey = "LevelManager.AlignX";
    private const string AlignYPrefKey = "LevelManager.AlignY";
    private const string AnchorPrefKey = "LevelManager.AnchorOnSnap";
    private const string AnchorXPrefKey = "LevelManager.AnchorX";
    private const string KeepTargetPrefKey = "LevelManager.KeepTarget";
    private const string RootName = "LevelRoot";
    private const double WaitWarnSeconds = 10.0;
    private const float FocusSliceDepth = 120f;

    private Vector2 scrollPos;
    private static double applyStartTime;
    private static bool applying;
    private static bool waitWarned;
    private static bool appliedThisLoad;
    private static string applyReason = "play start";

    // cached trigger counts (refreshed at most once per second or on hierarchy change)
    private static int enemyTotal, enemyVisible, winTotal, winVisible;
    private static double nextCountTime;
    private static bool countsDirty = true;

    static LevelManagerWindow()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [MenuItem("Tools/Level Section Manager")]
    public static void ShowWindow() { GetWindow<LevelManagerWindow>("Level Manager"); }

    private void OnHierarchyChange() { countsDirty = true; Repaint(); }

    private void OnInspectorUpdate() { if (EditorApplication.isPlaying) Repaint(); }

    // Queued in edit mode (Test Here button).
    private static int TargetSection
    {
        get => SessionState.GetInt(LevelSection.TestTargetKey, -1);
        set => SessionState.SetInt(LevelSection.TestTargetKey, value);
    }

    // Lives for the whole Play session, survives scene reloads (R). Cleared when leaving Play Mode or by "Stop".
    private static int ActiveTestSection
    {
        get => SessionState.GetInt(ActiveTestKey, -1);
        set => SessionState.SetInt(ActiveTestKey, value);
    }

    // ------------------------------------------------------------------ helpers

    internal static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    internal static List<LevelSection> GetSortedSections()
    {
        var sections = new List<LevelSection>(Object.FindObjectsByType<LevelSection>(FindObjectsSortMode.None));
        sections.Sort((a, b) => CompareHierarchy(a.transform, b.transform));
        return sections;
    }

    private static int CompareHierarchy(Transform a, Transform b)
    {
        var pa = GetIndexChain(a);
        var pb = GetIndexChain(b);
        int len = Mathf.Min(pa.Count, pb.Count);
        for (int i = 0; i < len; i++)
        {
            int c = pa[i].CompareTo(pb[i]);
            if (c != 0) return c;
        }
        return pa.Count.CompareTo(pb.Count);
    }

    private static List<int> GetIndexChain(Transform t)
    {
        var chain = new List<int>();
        while (t != null) { chain.Insert(0, t.GetSiblingIndex()); t = t.parent; }
        return chain;
    }

    internal static List<Transform> GetSpawnPoints()
    {
        var list = new List<Transform>();
        var spawner = Object.FindFirstObjectByType<SpawnPlayer>();
        if (spawner == null) return list;
        var prop = new SerializedObject(spawner).FindProperty("spawnPoints");
        if (prop == null || !prop.isArray) return list;
        for (int i = 0; i < prop.arraySize; i++)
        {
            var t = prop.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            if (t != null) list.Add(t);
        }
        return list;
    }

    private static bool TryGetSpawnZ(out float z)
    {
        z = 0f;
        var pts = GetSpawnPoints();
        if (pts.Count == 0) return false;
        float sum = 0f;
        foreach (var t in pts) sum += t.position.z;
        z = sum / pts.Count;
        return true;
    }

    private static bool TryGetSpawnX(out float x)
    {
        x = 0f;
        var pts = GetSpawnPoints();
        if (pts.Count == 0) return false;
        float sum = 0f;
        foreach (var t in pts) sum += t.position.x;
        x = sum / pts.Count;
        return true;
    }

    private static float GetLeadIn() { return EditorPrefs.GetFloat(LeadInPrefKey, 0f); }

    // Play Z for display / edit-mode actions (falls back to manual value).
    private static float GetPlayZ()
    {
        if (EditorPrefs.GetBool(AutoZPrefKey, true) && TryGetSpawnZ(out float z)) return z;
        return EditorPrefs.GetFloat(PlayZPrefKey, 0f);
    }

    private static bool IsUnderEnabledMover(Transform t, MoveTrack[] movers)
    {
        foreach (var m in movers)
            if (m != null && m.enabled && t.IsChildOf(m.transform)) return true;
        return false;
    }

    private static MoveTrack FindActiveMover(Transform t)
    {
        foreach (var m in t.GetComponentsInParent<MoveTrack>(true))
            if (m.enabled) return m;
        return null;
    }

    private static float ReadMoveSpeed(MoveTrack m)
    {
        var p = new SerializedObject(m).FindProperty("moveSpeed");
        return p != null ? p.floatValue : -1f;
    }

    private static void SetMoveSpeed(MoveTrack m, float speed)
    {
        var so = new SerializedObject(m);
        var p = so.FindProperty("moveSpeed");
        if (p == null) return;
        p.floatValue = speed;
        so.ApplyModifiedProperties();
    }

    // part: 0 = start, 1 = middle, 2 = end of the section
    private static void FocusSection(LevelSection section, int part)
    {
        Selection.activeGameObject = section.gameObject;
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;

        Bounds b = section.GetBounds();
        float d = Mathf.Min(b.size.z, FocusSliceDepth);
        float z = part == 0 ? b.min.z + d * 0.5f : (part == 2 ? b.max.z - d * 0.5f : b.center.z);
        Bounds slice = new Bounds(new Vector3(b.center.x, b.center.y, z), new Vector3(b.size.x, b.size.y, d));
        sv.Frame(slice, false);
    }

    // ------------------------------------------------------------------ diagnostics

    [MenuItem("Tools/Level Manager Diagnostics")]
    public static void PrintDiagnostics()
    {
        var movers = Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None);
        int enabledCount = 0;
        foreach (var m in movers)
        {
            if (m.enabled) enabledCount++;
            Debug.Log($"[Diag] MoveTrack on '{GetPath(m.transform)}' enabled={m.enabled} Z={m.transform.position.z}", m);
        }
        Debug.Log($"[Diag] Enabled MoveTracks: {enabledCount} (should be 1 after Build LevelRoot)");

        var spawner = Object.FindFirstObjectByType<SpawnPlayer>();
        if (spawner == null) Debug.Log("[Diag] No SpawnPlayer found in the loaded scenes.");
        else
        {
            var pts = GetSpawnPoints();
            if (pts.Count == 0) Debug.Log("[Diag] SpawnPlayer found but its spawnPoints array is empty.");
            for (int i = 0; i < pts.Count; i++)
                Debug.Log($"[Diag] Player spawn {i}: '{GetPath(pts[i])}' pos={pts[i].position} underEnabledMoveTrack={IsUnderEnabledMover(pts[i], movers)}", pts[i]);
        }

        foreach (var s in GetSortedSections())
        {
            string len = s.HasMeasurement
                ? $"start={s.StartZ:F1} end={s.EndZ:F1} length={s.Length:F1}"
                : "not measured yet";
            Debug.Log($"[Diag] Section #{s.sectionNumber} '{GetPath(s.transform)}' Z={s.transform.position.z} {len} underEnabledMoveTrack={IsUnderEnabledMover(s.transform, movers)}", s);
        }

        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!c.isRootCanvas) continue;
            Debug.Log($"[Diag] Canvas '{GetPath(c.transform)}' mode={c.renderMode} underEnabledMoveTrack={IsUnderEnabledMover(c.transform, movers)}", c);
        }
    }

    // ------------------------------------------------------------------ build / snap

    private static void BuildLevelRoot()
    {
        var sections = GetSortedSections();
        if (sections.Count == 0)
        {
            Debug.LogWarning("Level Manager: No LevelSection components found. Nothing to build.");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Build LevelRoot");
        int undoGroup = Undo.GetCurrentGroup();

        var allMovers = Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None);

        float speed = -1f;
        string speedSource = null;
        foreach (var s in sections)
        {
            var m = s.GetComponent<MoveTrack>();
            if (m != null) { speed = ReadMoveSpeed(m); speedSource = GetPath(s.transform); break; }
        }
        if (speed < 0f && allMovers.Length > 0)
        {
            speed = ReadMoveSpeed(allMovers[0]);
            speedSource = GetPath(allMovers[0].transform);
        }
        foreach (var m in allMovers)
        {
            float other = ReadMoveSpeed(m);
            if (speed > 0f && other > 0f && !Mathf.Approximately(other, speed))
                Debug.LogWarning($"Level Manager: MoveTrack on '{GetPath(m.transform)}' has moveSpeed {other}, differs from {speed}. Using {speed} for {RootName}.");
        }

        GameObject root = GameObject.Find(RootName);
        if (root == null)
        {
            root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create LevelRoot");
            root.transform.position = Vector3.zero;
            Debug.Log($"Level Manager: Created '{RootName}' at the world origin.");
        }
        MoveTrack rootMover = root.GetComponent<MoveTrack>();
        if (rootMover == null)
        {
            rootMover = Undo.AddComponent<MoveTrack>(root);
            if (speed > 0f)
            {
                SetMoveSpeed(rootMover, speed);
                Debug.Log($"Level Manager: Added MoveTrack to '{RootName}' with moveSpeed {speed} (copied from '{speedSource}').");
            }
            else Debug.LogWarning($"Level Manager: Added MoveTrack to '{RootName}' but found no speed to copy. Check its Move Speed in the Inspector.");
        }
        if (!rootMover.enabled) { Undo.RecordObject(rootMover, "Enable LevelRoot MoveTrack"); rootMover.enabled = true; }

        int detached = 0;
        foreach (var sp in GetSpawnPoints())
        {
            bool needs = sp.IsChildOf(root.transform);
            foreach (var s in sections) if (sp.IsChildOf(s.transform)) needs = true;
            foreach (var m in allMovers) if (m != rootMover && sp.IsChildOf(m.transform)) needs = true;
            if (!needs) continue;
            string before = GetPath(sp);
            Undo.SetTransformParent(sp, null, "Detach player spawn point");
            detached++;
            Debug.Log($"Level Manager: Detached player spawn point '{before}' to the scene root (world position kept, Z={sp.position.z:F2}).", sp);
        }
        if (detached == 0) Debug.Log("Level Manager: Player spawn points were already outside the scrolling objects.");

        foreach (var s in sections)
            Undo.SetTransformParent(s.transform, root.transform, "Move section under LevelRoot");
        for (int i = 0; i < sections.Count; i++)
            Undo.SetSiblingIndex(sections[i].transform, i, "Order sections");

        foreach (var m in Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None))
        {
            if (m == rootMover) continue;
            if (m.GetComponent<LevelSection>() != null)
            {
                if (m.enabled)
                {
                    Undo.RecordObject(m, "Disable section MoveTrack");
                    m.enabled = false;
                    Debug.Log($"Level Manager: Disabled MoveTrack on section '{GetPath(m.transform)}'.", m);
                }
            }
            else if (m.enabled)
            {
                string extra = m.transform.IsChildOf(root.transform) ? " It is under LevelRoot, so it will move TWICE." : "";
                Debug.LogWarning($"Level Manager: Another active MoveTrack on non-section '{GetPath(m.transform)}' was left untouched.{extra}", m);
            }
        }

        if (DoSnap(sections))
        {
            if (EditorPrefs.GetBool(AnchorPrefKey, true)) AnchorLevel(sections);
            LevelTriggerTools.EnsureWinTriggerLast(sections);
        }

        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!c.isRootCanvas) continue;
            foreach (var s in sections)
            {
                if (c.transform.IsChildOf(s.transform))
                {
                    Debug.LogWarning($"Level Manager: Canvas '{GetPath(c.transform)}' is still under section '{s.name}' and will scroll with it. If it is a scoreboard canvas, drag it out of the section (keep world position).", c);
                    break;
                }
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = root;
        SceneView.RepaintAll();
        Debug.Log($"Level Manager: Build complete. '{RootName}' holds {sections.Count} sections and is the only active MoveTrack.");
    }

    private static void SnapSections()
    {
        var sections = GetSortedSections();
        if (sections.Count == 0) return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Snap Level Sections");
        int undoGroup = Undo.GetCurrentGroup();
        if (DoSnap(sections))
        {
            if (EditorPrefs.GetBool(AnchorPrefKey, true)) AnchorLevel(sections);
            // The win trigger always sits at the end of the LAST section, whatever the order is.
            LevelTriggerTools.EnsureWinTriggerLast(sections);
        }
        Undo.CollapseUndoOperations(undoGroup);
        SceneView.RepaintAll();
    }

    // Returns false if it could not snap (nested sections).
    private static bool DoSnap(List<LevelSection> sections)
    {
        foreach (var a in sections)
            foreach (var b in sections)
                if (a != b && b.transform.IsChildOf(a.transform))
                {
                    Debug.LogError($"Level Manager: Section '{b.name}' is nested inside section '{a.name}'. Run 'Build LevelRoot' first.", b);
                    return false;
                }

        foreach (var s in sections)
        {
            Undo.RecordObject(s, "Measure Level Section");
            bool real = s.Measure();
            EditorUtility.SetDirty(s);
            if (!real) Debug.LogWarning($"Level Manager: Section '{s.name}' has no renderers. Using a 10x10x50 fallback box. Set a manual length.", s);
        }

        float gap = EditorPrefs.GetFloat(GapPrefKey, 0f);
        bool alignX = EditorPrefs.GetBool(AlignXPrefKey, true);
        bool alignY = EditorPrefs.GetBool(AlignYPrefKey, false);

        Bounds fb = sections[0].GetBounds();
        Debug.Log($"Level Manager: Section #1 '{sections[0].sectionLabel}' start={fb.min.z:F2} end={fb.max.z:F2} (length {fb.size.z:F2}).", sections[0]);

        for (int i = 1; i < sections.Count; i++)
        {
            var prev = sections[i - 1];
            var cur = sections[i];
            Bounds pb = prev.GetBounds();
            Bounds cb = cur.GetBounds();

            Vector3 delta = Vector3.zero;
            delta.z = (pb.max.z + gap) - cb.min.z;
            if (alignX) delta.x = fb.center.x - cb.center.x;
            if (alignY) delta.y = fb.min.y - cb.min.y;

            if (delta.sqrMagnitude > 1e-8f)
            {
                Undo.RecordObject(cur.transform, "Snap Level Section");
                cur.transform.position += delta;
            }
            Bounds nb = cur.GetBounds();
            Debug.Log($"Level Manager: Section #{i + 1} '{cur.sectionLabel}' moved by ({delta.x:F2}, {delta.y:F2}, {delta.z:F2}). Now start={nb.min.z:F2} end={nb.max.z:F2} (length {nb.size.z:F2}).", cur);
        }
        return true;
    }

    // Shifts the whole level. If all sections share a parent (LevelRoot), the parent's CHILDREN are moved,
    // so the parent itself stays where it is (ideally at the origin).
    private static void ShiftLevel(List<LevelSection> sections, Vector3 delta, string undoName)
    {
        Transform common = sections[0].transform.parent;
        bool shared = common != null;
        foreach (var s in sections) if (s.transform.parent != common) shared = false;

        if (shared)
        {
            for (int i = 0; i < common.childCount; i++)
            {
                Transform c = common.GetChild(i);
                Undo.RecordObject(c, undoName);
                c.position += delta;
            }
        }
        else
        {
            foreach (var s in sections)
            {
                Undo.RecordObject(s.transform, undoName);
                s.transform.position += delta;
            }
        }
    }

    // Puts the LevelRoot back at the world origin without moving anything visually.
    private static void RecenterRoot(List<LevelSection> sections)
    {
        Transform common = sections[0].transform.parent;
        if (common == null) return;
        foreach (var s in sections) if (s.transform.parent != common) return;
        if (common.position.sqrMagnitude < 0.0001f) return;

        Vector3 before = common.position;
        int n = common.childCount;
        var positions = new Vector3[n];
        for (int i = 0; i < n; i++) positions[i] = common.GetChild(i).position;

        Undo.RecordObject(common, "Recenter LevelRoot");
        common.position = Vector3.zero;
        for (int i = 0; i < n; i++)
        {
            Undo.RecordObject(common.GetChild(i), "Recenter LevelRoot");
            common.GetChild(i).position = positions[i];
        }
        Debug.Log($"Level Manager: Reset '{common.name}' from {before} to the world origin (everything inside kept its world position).");
    }

    // Moves the whole level so section #1's start edge sits at Play Z + lead-in (and optionally X at the spawn X).
    private static void AnchorLevel(List<LevelSection> sections)
    {
        foreach (var s in sections)
            if (!s.HasMeasurement) { Undo.RecordObject(s, "Measure Level Section"); s.Measure(); EditorUtility.SetDirty(s); }

        RecenterRoot(sections);

        float playZ = GetPlayZ();
        float targetZ = playZ + GetLeadIn();
        Bounds b = sections[0].GetBounds();

        Vector3 delta = new Vector3(0f, 0f, targetZ - b.min.z);
        string xInfo = "X unchanged";
        if (EditorPrefs.GetBool(AnchorXPrefKey, true) && TryGetSpawnX(out float spawnX))
        {
            delta.x = spawnX - b.center.x;
            xInfo = $"X centre -> spawn X {spawnX:F2}";
        }

        ShiftLevel(sections, delta, "Anchor Level To Spawn");
        Bounds nb = sections[0].GetBounds();
        Debug.Log($"Level Manager: Anchored level by ({delta.x:F2}, 0, {delta.z:F2}). Section #1 start edge Z={nb.min.z:F2} (Play Z {playZ:F2} + lead-in {GetLeadIn():F2}), {xInfo}.", sections[0]);
        SceneView.RepaintAll();
    }

    private static void AnchorOnly()
    {
        var sections = GetSortedSections();
        if (sections.Count == 0) return;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Anchor Level To Spawn");
        int g = Undo.GetCurrentGroup();
        AnchorLevel(sections);
        LevelTriggerTools.EnsureWinTriggerLast(sections);
        Undo.CollapseUndoOperations(g);
    }

    private static void MeasureAll()
    {
        foreach (var s in GetSortedSections())
        {
            Undo.RecordObject(s, "Measure Level Section");
            s.Measure();
            EditorUtility.SetDirty(s);
        }
        SceneView.RepaintAll();
    }

    // ------------------------------------------------------------------ GUI

    private static void PrefFloatField(string label, string key, float def)
    {
        float v = EditorPrefs.GetFloat(key, def);
        float nv = EditorGUILayout.FloatField(label, v);
        if (!Mathf.Approximately(nv, v)) EditorPrefs.SetFloat(key, nv);
    }

    private static void PrefToggle(string label, string key, bool def)
    {
        bool v = EditorPrefs.GetBool(key, def);
        bool nv = EditorGUILayout.ToggleLeft(label, v);
        if (nv != v) EditorPrefs.SetBool(key, nv);
    }

    private static void PrefRange(string label, string minKey, string maxKey, float defMin, float defMax)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        float a = EditorPrefs.GetFloat(minKey, defMin);
        float b = EditorPrefs.GetFloat(maxKey, defMax);
        float na = EditorGUILayout.FloatField(a, GUILayout.MinWidth(40));
        float nb = EditorGUILayout.FloatField(b, GUILayout.MinWidth(40));
        if (!Mathf.Approximately(na, a)) EditorPrefs.SetFloat(minKey, na);
        if (!Mathf.Approximately(nb, b)) EditorPrefs.SetFloat(maxKey, nb);
        EditorGUILayout.EndHorizontal();
    }

    private static void RefreshTriggerCounts()
    {
        double now = EditorApplication.timeSinceStartup;
        if (!countsDirty && now < nextCountTime) return;
        countsDirty = false;
        nextCountTime = now + 1.0;
        LevelTriggerTools.Count<EnemySpawnTrigger>(out enemyTotal, out enemyVisible);
        LevelTriggerTools.Count<WinTrigger>(out winTotal, out winVisible);
    }

    private static void DrawVisibilityRow(string label, int total, int visible, System.Action<bool> setVisible)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{label}: {total}  ({visible} visible)");
        if (GUILayout.Button("Show all", GUILayout.Width(65))) { setVisible(true); countsDirty = true; }
        if (GUILayout.Button("Hide all", GUILayout.Width(65))) { setVisible(false); countsDirty = true; }
        EditorGUILayout.EndHorizontal();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Modular Level Section Dashboard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. Build LevelRoot once (one scrolling root, spawn points stay static).\n" +
            "2. Snap sections in order whenever you add, remove, reorder or resize a section. It also moves the level to the spawn points and keeps the win trigger at the end of the last section.\n" +
            "3. Test Here, then press Play: the root is shifted so that section's start edge sits at the Play Z. Pressing R restarts from the same section.",
            MessageType.Info);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Step 1 - Setup (once)", EditorStyles.boldLabel);
        var movers = Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None);
        int enabledCount = 0;
        foreach (var m in movers) if (m.enabled) enabledCount++;
        EditorGUILayout.LabelField("Active MoveTrack components", enabledCount + (enabledCount == 1 ? "  (good)" : "  (should be 1)"));
        if (GUILayout.Button("Build LevelRoot (single moving root)", GUILayout.Height(26)))
        {
            if (EditorUtility.DisplayDialog("Build LevelRoot",
                "This creates 'LevelRoot' with one MoveTrack, moves all sections under it, disables the old MoveTracks on sections, " +
                "detaches the player spawn points from the scrolling objects and snaps the sections. You can undo it with Ctrl+Z.",
                "Build", "Cancel"))
            {
                BuildLevelRoot();
            }
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Step 2 - Chain sections", EditorStyles.boldLabel);
        PrefFloatField("Gap between sections", GapPrefKey, 0f);
        PrefToggle("Align X of sections to section #1 (bounds centre)", AlignXPrefKey, true);
        PrefToggle("Align Y of sections to section #1 (bounds bottom)", AlignYPrefKey, false);
        PrefToggle("Anchor level to spawn points when snapping", AnchorPrefKey, true);
        PrefToggle("   ...also match X to the spawn points' X", AnchorXPrefKey, true);
        PrefFloatField("Lead-in (start edge ahead of Play Z)", LeadInPrefKey, 0f);

        if (GUILayout.Button("Snap sections in order", GUILayout.Height(26)))
        {
            SnapSections();
            GUIUtility.ExitGUI();
        }
        if (GUILayout.Button("Anchor level to spawn points only (no re-snap)"))
        {
            AnchorOnly();
            GUIUtility.ExitGUI();
        }
        if (GUILayout.Button("Re-measure all sections"))
        {
            MeasureAll();
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Test settings", EditorStyles.boldLabel);
        bool auto = EditorPrefs.GetBool(AutoZPrefKey, true);
        bool newAuto = EditorGUILayout.ToggleLeft("Auto Play Z (read from SpawnPlayer spawn points)", auto);
        if (newAuto != auto) EditorPrefs.SetBool(AutoZPrefKey, newAuto);

        using (new EditorGUI.DisabledScope(newAuto))
        {
            float playZ = EditorPrefs.GetFloat(PlayZPrefKey, 0f);
            float newPlayZ = EditorGUILayout.FloatField("Manual Play Z", playZ);
            if (!Mathf.Approximately(newPlayZ, playZ)) EditorPrefs.SetFloat(PlayZPrefKey, newPlayZ);
        }
        EditorGUILayout.LabelField("Effective Play Z (+ lead-in)", $"{GetPlayZ():F2}  ( + {GetLeadIn():F2} )");
        PrefToggle("Keep test start after Play (otherwise it clears itself)", KeepTargetPrefKey, false);

        if (TargetSection != -1)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox($"Test start queued: Section #{TargetSection}", MessageType.None);
            if (GUILayout.Button("Clear", GUILayout.Width(50), GUILayout.Height(20)))
            {
                TargetSection = -1;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (EditorApplication.isPlaying && ActiveTestSection != -1)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox($"R restarts from Section #{ActiveTestSection}", MessageType.None);
            if (GUILayout.Button("Stop", GUILayout.Width(50), GUILayout.Height(20)))
            {
                ActiveTestSection = -1;
                SessionState.SetBool(ShiftPendingKey, false);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Section tracker (Play Mode)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current section", SectionTracker.StatusText);
        Rect pr = GUILayoutUtility.GetRect(18f, 16f, GUILayout.ExpandWidth(true));
        EditorGUI.ProgressBar(pr, Mathf.Max(0f, SectionTracker.Progress),
            SectionTracker.Progress < 0f ? "-" : Mathf.RoundToInt(SectionTracker.Progress * 100f) + "% through section");
        PrefToggle("Log to Console when the section changes", SectionTracker.LogPrefKey, true);
        PrefToggle("Show overlay in the Scene view", SectionTracker.OverlayPrefKey, true);
        if (GUILayout.Button("Log current section now (Ctrl+Shift+L)")) SectionTracker.LogCurrentNow();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Play area guide (Scene view)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        PrefToggle("Show play area (walls, floor, ceiling)", PlayAreaGuide.ShowKey, false);
        PrefToggle("Fill the faces", PlayAreaGuide.FillKey, true);
        PrefRange("X  min / max", PlayAreaGuide.MinXKey, PlayAreaGuide.MaxXKey, -30f, 30f);
        PrefRange("Y  min / max", PlayAreaGuide.MinYKey, PlayAreaGuide.MaxYKey, -15f, 15f);
        PrefFloatField("Frame spacing (Z)", PlayAreaGuide.SpacingKey, 100f);
        if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
        PrefToggle("Record ship extents while playing (fly to every edge)", PlayAreaGuide.RecordKey, false);
        if (PlayAreaGuide.HasRecording)
        {
            EditorGUILayout.LabelField("Recorded X", $"{PlayAreaGuide.RecMinX:F2}  to  {PlayAreaGuide.RecMaxX:F2}");
            EditorGUILayout.LabelField("Recorded Y", $"{PlayAreaGuide.RecMinY:F2}  to  {PlayAreaGuide.RecMaxY:F2}");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Use recorded as play area")) PlayAreaGuide.ApplyRecording();
            if (GUILayout.Button("Clear", GUILayout.Width(50))) PlayAreaGuide.ClearRecording();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
        RefreshTriggerCounts();
        DrawVisibilityRow("Enemy spawn triggers", enemyTotal, enemyVisible,
            v => LevelTriggerTools.SetVisible<EnemySpawnTrigger>(v, "enemy spawn triggers"));
        DrawVisibilityRow("Win triggers", winTotal, winVisible,
            v => LevelTriggerTools.SetVisible<WinTrigger>(v, "win triggers"));
        PrefFloatField("Win trigger depth (Z)", LevelTriggerTools.WinDepthKey, 20f);
        PrefFloatField("Win trigger min size (X/Y)", LevelTriggerTools.WinMinSizeKey, 200f);
        if (GUILayout.Button("Add / move win trigger to the end of the level"))
        {
            LevelTriggerTools.CreateWinTrigger();
            countsDirty = true;
            GUIUtility.ExitGUI();
        }
        if (GUILayout.Button("Check win triggers (why doesn't it fire?)"))
        {
            LevelTriggerTools.CheckWinTriggers();
        }

        EditorGUILayout.Space(6);
        if (GUILayout.Button("Refresh Sections", GUILayout.Height(24))) { countsDirty = true; Repaint(); SceneView.RepaintAll(); }

        EditorGUILayout.Space(6);
        List<LevelSection> sectionList = GetSortedSections();
        if (sectionList.Count == 0)
        {
            EditorGUILayout.HelpBox("No Level Sections found in this scene! Create empty GameObjects and add the 'LevelSection' script to them.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        for (int i = 0; i < sectionList.Count; i++)
        {
            if (sectionList[i] != null && sectionList[i].sectionNumber != i + 1)
            {
                Undo.RecordObject(sectionList[i], "Renumber Level Section");
                sectionList[i].sectionNumber = i + 1;
                EditorUtility.SetDirty(sectionList[i]);
            }
        }

        float gapSetting = EditorPrefs.GetFloat(GapPrefKey, 0f);

        for (int i = 0; i < sectionList.Count; i++)
        {
            var section = sectionList[i];
            if (section == null) continue;

            if (!section.HasMeasurement)
            {
                Undo.RecordObject(section, "Measure Level Section");
                section.Measure();
                EditorUtility.SetDirty(section);
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            // column A: reorder
            EditorGUILayout.BeginVertical(GUILayout.Width(25));
            if (i > 0 && GUILayout.Button("▲", GUILayout.Height(14)))
            {
                Undo.SetSiblingIndex(section.transform, section.transform.GetSiblingIndex() - 1, "Move Section Up");
                GUIUtility.ExitGUI();
            }
            if (i < sectionList.Count - 1 && GUILayout.Button("▼", GUILayout.Height(14)))
            {
                Undo.SetSiblingIndex(section.transform, section.transform.GetSiblingIndex() + 1, "Move Section Down");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndVertical();

            // column B: section info rows
            EditorGUILayout.BeginVertical();

            // row 1: number, label, Test Here, remove
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"#{section.sectionNumber}", GUILayout.Width(28));

            EditorGUI.BeginChangeCheck();
            string newLabel = EditorGUILayout.TextField(section.sectionLabel, GUILayout.MinWidth(40));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(section, "Rename Section");
                section.sectionLabel = newLabel;
                EditorUtility.SetDirty(section);
            }

            bool isQueued = TargetSection == section.sectionNumber;
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = isQueued ? Color.green : Color.white;
            if (GUILayout.Button("Test Here", GUILayout.Width(70)))
            {
                if (isQueued) { TargetSection = -1; Debug.Log("Level Manager: Test start cleared."); }
                else
                {
                    TargetSection = section.sectionNumber;
                    Debug.Log($"Level Manager: Queued test from Section #{section.sectionNumber} ({section.sectionLabel}). Press Play when ready!");
                }
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button(new GUIContent("X", "Remove the LevelSection component from this object (the object itself stays)."), GUILayout.Width(22)))
            {
                Debug.Log($"Level Manager: Removed LevelSection component from '{GetPath(section.transform)}'.");
                Undo.DestroyObjectImmediate(section);
                SceneView.RepaintAll();
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = prevColor;
            EditorGUILayout.EndHorizontal();

            // row 2: manual length
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool manual = EditorGUILayout.ToggleLeft("Manual len", section.useManualLength, GUILayout.Width(85));
            float manLen = section.manualLength;
            using (new EditorGUI.DisabledScope(!manual))
                manLen = EditorGUILayout.FloatField(manLen, GUILayout.MinWidth(40));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(section, "Change Section Length");
                if (manual && !section.useManualLength) manLen = section.AutoLength;
                section.useManualLength = manual;
                section.manualLength = Mathf.Max(0.01f, manLen);
                EditorUtility.SetDirty(section);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            // row 3: Z range, length, seam
            string seam = "";
            bool seamOk = true;
            if (i > 0)
            {
                float g = section.StartZ - sectionList[i - 1].EndZ - gapSetting;
                seamOk = Mathf.Abs(g) < 0.05f;
                seam = seamOk ? "seam OK" : (g > 0f ? $"gap {g:F2}" : $"overlap {-g:F2}");
            }
            var st = new GUIStyle(EditorStyles.miniLabel);
            if (i > 0) st.normal.textColor = seamOk ? new Color(0.4f, 0.85f, 0.4f) : new Color(1f, 0.4f, 0.4f);
            EditorGUILayout.LabelField($"Z {section.StartZ:F1} → {section.EndZ:F1}   len {section.Length:F1}   {seam}", st);

            // row 4: where this section lives in the hierarchy
            string state = section.gameObject.activeInHierarchy ? "" : "  (inactive)";
            EditorGUILayout.LabelField(GetPath(section.transform) + state, EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            // column C: focus buttons stacked vertically
            EditorGUILayout.BeginVertical(GUILayout.Width(46));
            GUILayout.Label("Focus", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button(new GUIContent("Start", "Frame the start of this section"), EditorStyles.miniButton)) FocusSection(section, 0);
            if (GUILayout.Button(new GUIContent("Mid", "Frame the middle of this section"), EditorStyles.miniButton)) FocusSection(section, 1);
            if (GUILayout.Button(new GUIContent("End", "Frame the end of this section"), EditorStyles.miniButton)) FocusSection(section, 2);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    // ------------------------------------------------------------------ test start

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // Promote the queued target to the play-session target (not cleared after the first apply).
                ActiveTestSection = TargetSection;
                SessionState.SetBool(ShiftPendingKey, ActiveTestSection != -1);
                appliedThisLoad = false;
                break;

            case PlayModeStateChange.EnteredPlayMode:
                if (ActiveTestSection == -1 && TargetSection != -1) ActiveTestSection = TargetSection;
                if (ActiveTestSection == -1) { SessionState.SetBool(ShiftPendingKey, false); return; }
                if (!applying && !appliedThisLoad) BeginApply("play start");
                break;

            case PlayModeStateChange.ExitingPlayMode:
            case PlayModeStateChange.EnteredEditMode:
                StopApplying();
                ActiveTestSection = -1;
                SessionState.SetBool(ShiftPendingKey, false);
                break;
        }
    }

    // Pressing R reloads the scene: LevelRoot is back at its saved position, so apply the shift again.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single || !EditorApplication.isPlaying) return;
        if (ActiveTestSection == -1) return;
        BeginApply("scene reload (R)");
    }

    private static void BeginApply(string reason)
    {
        applyReason = reason;
        appliedThisLoad = false;
        applyStartTime = EditorApplication.timeSinceStartup;
        waitWarned = false;
        SessionState.SetBool(ShiftPendingKey, true);
        if (!applying) { applying = true; EditorApplication.update += TryApplyShift; }
    }

    private static void StopApplying()
    {
        if (applying) { EditorApplication.update -= TryApplyShift; applying = false; }
        SessionState.SetBool(ShiftPendingKey, false);
    }

    private static void TryApplyShift()
    {
        int target = ActiveTestSection;
        if (target == -1 || !EditorApplication.isPlaying) { StopApplying(); return; }

        double waited = EditorApplication.timeSinceStartup - applyStartTime;
        if (!waitWarned && waited > WaitWarnSeconds)
        {
            waitWarned = true;
            Debug.LogWarning($"Level Manager: Still waiting after {WaitWarnSeconds:F0}s for Section #{target} and/or the SpawnPlayer spawn points. Will keep trying until you leave Play Mode.");
        }

        LevelSection targetSection = null;
        foreach (var sec in Object.FindObjectsByType<LevelSection>(FindObjectsSortMode.None))
            if (sec.sectionNumber == target) { targetSection = sec; break; }
        if (targetSection == null) return;

        MoveTrack mover = FindActiveMover(targetSection.transform);
        if (mover == null)
        {
            Debug.LogError($"Level Manager: Section #{target} ('{GetPath(targetSection.transform)}') is not under an active MoveTrack. Click 'Build LevelRoot' in the Level Manager window first.", targetSection);
            StopApplying();
            return;
        }

        float playZ;
        if (EditorPrefs.GetBool(AutoZPrefKey, true))
        {
            if (!TryGetSpawnZ(out playZ)) return;
        }
        else playZ = EditorPrefs.GetFloat(PlayZPrefKey, 0f);
        float leadIn = GetLeadIn();
        float desiredStart = playZ + leadIn;

        if (!targetSection.HasMeasurement) targetSection.Measure();

        float startBefore = targetSection.StartZ;
        float rootBefore = mover.transform.position.z;
        float deltaZ = desiredStart - startBefore;

        Vector3 p = mover.transform.position;
        p.z += deltaZ;
        mover.transform.position = p;

        Debug.Log($"Level Manager: [{applyReason}] Shifted '{GetPath(mover.transform)}' by {deltaZ:F2} on Z (root Z {rootBefore:F2} -> {p.z:F2}). " +
                  $"Section #{targetSection.sectionNumber} ({targetSection.sectionLabel}) start edge {startBefore:F2} -> {targetSection.StartZ:F2} " +
                  $"(target {desiredStart:F2} = Play Z {playZ:F2} + lead-in {leadIn:F2}).", targetSection);

        if (!EditorPrefs.GetBool(KeepTargetPrefKey, false)) TargetSection = -1;
        appliedThisLoad = true;
        StopApplying();
    }
}