using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor-only helpers for trigger volumes: create a WinTrigger at the end of the level,
/// keep it at the end of the LAST section whenever sections are snapped,
/// show/hide the meshes of all EnemySpawnTrigger / WinTrigger objects at once,
/// and check why a WinTrigger might not fire.
/// Place in Assets\TutorialInfo\Scripts\Editor\
/// </summary>
public static class LevelTriggerTools
{
    public const string WinDepthKey = "LevelManager.WinDepth";
    public const string WinMinSizeKey = "LevelManager.WinMinSize";

    private const string DebugMatPath = "Assets/LevelTool_TriggerDebug.mat";

    // ------------------------------------------------------------------ visibility

    private static List<Renderer> RenderersOf(Component c)
    {
        var result = new List<Renderer>();
        var own = c.GetComponents<Renderer>();
        if (own.Length > 0) result.AddRange(own);
        else result.AddRange(c.GetComponentsInChildren<Renderer>(true));
        return result;
    }

    public static void Count<T>(out int total, out int visible) where T : Component
    {
        total = 0;
        visible = 0;
        foreach (var c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            total++;
            foreach (var r in RenderersOf(c))
                if (r != null && r.enabled) { visible++; break; }
        }
    }

    public static void SetVisible<T>(bool visible, string label) where T : Component
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(visible ? "Show " + label : "Hide " + label);
        int group = Undo.GetCurrentGroup();

        int triggers = 0;
        int changed = 0;
        foreach (var c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            triggers++;
            foreach (var r in RenderersOf(c))
            {
                if (r == null || r.enabled == visible) continue;
                Undo.RecordObject(r, "Toggle trigger mesh");
                r.enabled = visible;
                EditorUtility.SetDirty(r);
                changed++;
            }
        }
        Undo.CollapseUndoOperations(group);
        SceneView.RepaintAll();
        Debug.Log($"Level Manager: {(visible ? "Showed" : "Hid")} the meshes of {triggers} {label} ({changed} renderers changed).");
    }

    // ------------------------------------------------------------------ win trigger

    private static Material GetDebugMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(DebugMatPath);
        if (mat != null) return mat;

        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        mat = new Material(sh);

        Color c = new Color(1f, 0.2f, 0.8f, 0.25f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        AssetDatabase.CreateAsset(mat, DebugMatPath);
        AssetDatabase.SaveAssets();
        return mat;
    }

    // Reference object whose Physics layer and targetLayers the new trigger should copy:
    // another WinTrigger first (it already works in this project), otherwise an EnemySpawnTrigger.
    private static Component FindReference(WinTrigger exclude)
    {
        foreach (var w in Object.FindObjectsByType<WinTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (w != exclude) return w;
        return Object.FindFirstObjectByType<EnemySpawnTrigger>(FindObjectsInactive.Include);
    }

    private static bool TryCopyTargetLayers(WinTrigger win, Component source)
    {
        if (source == null) return false;

        var srcProp = new SerializedObject(source).FindProperty("targetLayers");
        if (srcProp == null || srcProp.intValue == 0) return false;

        var dst = new SerializedObject(win);
        var dstProp = dst.FindProperty("targetLayers");
        if (dstProp == null) return false;
        dstProp.intValue = srcProp.intValue;
        dst.ApplyModifiedProperties();
        return true;
    }

    // The shared parent of all sections (LevelRoot), or null if they do not share one.
    private static Transform GetLevelParent(List<LevelSection> sections)
    {
        if (sections == null || sections.Count == 0) return null;
        Transform common = sections[0].transform.parent;
        if (common == null) return null;
        foreach (var s in sections) if (s.transform.parent != common) return null;
        return common;
    }

    /// <summary>
    /// Keeps the win trigger at the very end of the level: moves it under LevelRoot (so it no longer
    /// travels with whichever section it was created in), puts its front face on the end edge of the
    /// LAST section and makes it the last child. Returns the trigger it handled (or null if none exists).
    /// Called after Build / Snap / Anchor.
    /// </summary>
    public static WinTrigger EnsureWinTriggerLast(List<LevelSection> sections)
    {
        if (sections == null || sections.Count == 0) return null;
        var wins = Object.FindObjectsByType<WinTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (wins.Length == 0) return null;

        Transform parent = GetLevelParent(sections);
        LevelSection last = sections[sections.Count - 1];

        WinTrigger primary = wins[0];
        if (parent != null)
            foreach (var w in wins)
                if (w.transform.parent == parent) { primary = w; break; }

        foreach (var w in wins)
            if (w != primary)
                Debug.LogWarning($"Level Manager: There is more than one WinTrigger. Only '{LevelManagerWindow.GetPath(primary.transform)}' is kept at the end of the level; '{LevelManagerWindow.GetPath(w.transform)}' was left alone.", w);

        if (!last.HasMeasurement) { Undo.RecordObject(last, "Measure Level Section"); last.Measure(); EditorUtility.SetDirty(last); }
        Bounds b = last.GetBounds();

        Transform t = primary.transform;
        if (parent != null && t.parent != parent)
            Undo.SetTransformParent(t, parent, "Move win trigger under LevelRoot");

        // A trigger that is moved by its transform every frame is most reliable with a kinematic Rigidbody.
        if (primary.GetComponent<Rigidbody>() == null && primary.GetComponentInParent<Rigidbody>() == null)
        {
            var rb = Undo.AddComponent<Rigidbody>(primary.gameObject);
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log($"Level Manager: Added a kinematic Rigidbody to '{LevelManagerWindow.GetPath(t)}' so its trigger events fire reliably while it scrolls.", primary);
        }

        // Make sure collider bounds reflect the latest transform changes (edit mode can be stale).
        Physics.SyncTransforms();

        Vector3 delta;
        var col = primary.GetComponent<Collider>();
        if (col != null && col.enabled && col.bounds.size.z > 0.001f)
        {
            Bounds cb = col.bounds;
            // Front face (min Z) of the trigger on the end edge of the last section, X centred on the section.
            delta = new Vector3(b.center.x - cb.center.x, 0f, b.max.z - cb.min.z);
        }
        else
        {
            float depth = Mathf.Max(1f, EditorPrefs.GetFloat(WinDepthKey, 20f));
            delta = new Vector3(b.center.x - t.position.x, 0f, (b.max.z + depth * 0.5f) - t.position.z);
        }

        if (delta.sqrMagnitude > 1e-8f)
        {
            Undo.RecordObject(t, "Move win trigger to end of level");
            t.position += delta;
        }

        if (parent != null && t.GetSiblingIndex() != parent.childCount - 1)
            Undo.SetSiblingIndex(t, parent.childCount - 1, "Win trigger last");

        EditorUtility.SetDirty(t);
        Debug.Log($"Level Manager: Win trigger '{LevelManagerWindow.GetPath(t)}' kept at the end of the level (front face on Z={b.max.z:F2}, end edge of Section #{last.sectionNumber} '{last.sectionLabel}').", primary);
        SceneView.RepaintAll();
        return primary;
    }

    public static void CreateWinTrigger()
    {
        var sections = LevelManagerWindow.GetSortedSections();
        if (sections.Count == 0)
        {
            Debug.LogWarning("Level Manager: No LevelSection found. Nothing to attach a win trigger to.");
            return;
        }

        LevelSection last = sections[sections.Count - 1];

        // A win trigger already exists: just move it to the end of the last section.
        var anyExisting = Object.FindFirstObjectByType<WinTrigger>(FindObjectsInactive.Include);
        if (anyExisting != null)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Move win trigger to end of level");
            int g = Undo.GetCurrentGroup();
            var handled = EnsureWinTriggerLast(sections);
            Undo.CollapseUndoOperations(g);
            if (handled != null) Selection.activeGameObject = handled.gameObject;
            return;
        }

        if (!last.HasMeasurement) { Undo.RecordObject(last, "Measure Level Section"); last.Measure(); EditorUtility.SetDirty(last); }
        Bounds b = last.GetBounds();

        float depth = Mathf.Max(1f, EditorPrefs.GetFloat(WinDepthKey, 20f));
        float minSize = Mathf.Max(1f, EditorPrefs.GetFloat(WinMinSizeKey, 200f));
        Vector3 size = new Vector3(Mathf.Max(b.size.x, minSize), Mathf.Max(b.size.y, minSize), depth);
        // Front face sits exactly on the end edge of the last section, the box extends beyond it.
        Vector3 center = new Vector3(b.center.x, b.center.y, b.max.z + depth * 0.5f);

        Component reference = FindReference(null);

        // Lives under LevelRoot (not under a section), so it stays at the end when sections are reordered.
        Transform host = GetLevelParent(sections);
        if (host == null) host = last.transform;

        var go = new GameObject("WinTrigger");
        Undo.RegisterCreatedObjectUndo(go, "Create Win Trigger");
        go.transform.SetParent(host, false);
        go.transform.position = center;
        Vector3 ps = host.lossyScale;
        go.transform.localScale = new Vector3(
            size.x / Mathf.Max(0.0001f, Mathf.Abs(ps.x)),
            size.y / Mathf.Max(0.0001f, Mathf.Abs(ps.y)),
            size.z / Mathf.Max(0.0001f, Mathf.Abs(ps.z)));

        // Same Physics layer as the working triggers, otherwise the layer collision matrix can block it.
        if (reference != null) go.layer = reference.gameObject.layer;

        var mf = Undo.AddComponent<MeshFilter>(go);
        mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        var mr = Undo.AddComponent<MeshRenderer>(go);
        mr.sharedMaterial = GetDebugMaterial();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.enabled = false; // invisible by default, use "Show all" to see it

        var col = Undo.AddComponent<BoxCollider>(go);
        col.isTrigger = true;

        var win = Undo.AddComponent<WinTrigger>(go);
        bool layersOk = TryCopyTargetLayers(win, reference);

        // Final placement + last sibling.
        EnsureWinTriggerLast(sections);

        Selection.activeGameObject = go;
        SceneView.RepaintAll();
        string layerInfo = reference != null ? $" Physics layer '{LayerName(go.layer)}' copied from '{reference.name}'." : "";
        Debug.Log($"Level Manager: Created win trigger under '{host.name}' (size {size.x:F0} x {size.y:F0} x {size.z:F0}). Mesh is hidden.{layerInfo}", go);
        if (!layersOk)
            Debug.LogWarning("Level Manager: Could not copy 'Target Layers' from another trigger. Set Target Layers on the WinTrigger component by hand.", go);
    }

    // ------------------------------------------------------------------ win trigger check

    private static string LayerName(int layer)
    {
        string n = LayerMask.LayerToName(layer);
        return string.IsNullOrEmpty(n) ? "Layer " + layer : n;
    }

    [MenuItem("Tools/Level Manager Win Trigger Check")]
    public static void CheckWinTriggers()
    {
        var wins = Object.FindObjectsByType<WinTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (wins.Length == 0)
        {
            Debug.Log("[WinCheck] No WinTrigger found in the loaded scenes.");
            return;
        }

        var sections = LevelManagerWindow.GetSortedSections();
        LevelSection last = sections.Count > 0 ? sections[sections.Count - 1] : null;

        var players = new List<PlayerInput>();
        if (EditorApplication.isPlaying)
            foreach (var p in PlayerInput.all) if (p != null) players.Add(p);

        Debug.Log($"[WinCheck] {wins.Length} WinTrigger(s) found. Last section: {(last != null ? "#" + last.sectionNumber + " '" + last.sectionLabel + "' (ends at Z " + last.EndZ.ToString("F1") + ")" : "none")}. Players in scene: {players.Count}{(EditorApplication.isPlaying ? "" : " (not playing, player checks skipped)")}.");

        var movers = Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None);

        foreach (var w in wins)
        {
            GameObject go = w.gameObject;
            var sp = new SerializedObject(w).FindProperty("targetLayers");
            int mask = sp != null ? sp.intValue : 0;

            var maskNames = new List<string>();
            for (int i = 0; i < 32; i++)
                if ((mask & (1 << i)) != 0) maskNames.Add(LayerName(i));

            var sec = w.GetComponentInParent<LevelSection>();
            string where = sec == null ? "not inside a section (good, it is kept at the end by Snap)"
                : (sec == last ? "inside the LAST section (#" + sec.sectionNumber + ")" : "inside Section #" + sec.sectionNumber + " (NOT the last one, run Snap)");

            Debug.Log($"[WinCheck] '{LevelManagerWindow.GetPath(go.transform)}': Physics layer '{LayerName(go.layer)}', activeInHierarchy={go.activeInHierarchy}, scriptEnabled={w.enabled}, {where}, targetLayers=[{string.Join(", ", maskNames)}]", w);

            if (!go.activeInHierarchy) Debug.LogWarning("[WinCheck]   -> object is inactive, it can never fire.", w);
            if (!w.enabled) Debug.LogWarning("[WinCheck]   -> WinTrigger script is disabled.", w);
            if (mask == 0) Debug.LogWarning("[WinCheck]   -> Target Layers is empty (Nothing), nothing can trigger it.", w);

            bool scrolls = false;
            foreach (var m in movers)
                if (m != null && m.enabled && go.transform.IsChildOf(m.transform)) { scrolls = true; break; }
            if (!scrolls)
                Debug.LogWarning("[WinCheck]   -> not under an enabled MoveTrack, so it never scrolls towards the player. Run 'Snap sections in order' (it moves the trigger under LevelRoot).", w);

            var cols = go.GetComponents<Collider>();
            if (cols.Length == 0) Debug.LogWarning("[WinCheck]   -> no Collider on this object, OnTriggerEnter can never be called.", w);
            foreach (var c in cols)
            {
                if (!c.enabled) Debug.LogWarning("[WinCheck]   -> a Collider on this object is disabled.", w);
                if (!c.isTrigger) Debug.LogWarning("[WinCheck]   -> Collider is not 'Is Trigger'.", w);
            }

            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) == 0) continue;
                if (Physics.GetIgnoreLayerCollision(go.layer, i))
                    Debug.LogWarning($"[WinCheck]   -> the Physics layer collision matrix IGNORES '{LayerName(go.layer)}' x '{LayerName(i)}'. Put this trigger on a layer that collides with '{LayerName(i)}' (Project Settings > Physics).", w);
            }

            if (cols.Length > 0)
                Debug.Log($"[WinCheck]   trigger Z range {cols[0].bounds.min.z:F1} to {cols[0].bounds.max.z:F1}, X range {cols[0].bounds.min.x:F1} to {cols[0].bounds.max.x:F1}.", w);

            foreach (var p in players)
            {
                int pl = p.gameObject.layer;
                if ((mask & (1 << pl)) == 0)
                    Debug.LogWarning($"[WinCheck]   -> player '{p.name}' is on layer '{LayerName(pl)}', which is NOT in Target Layers.", w);
                if (Physics.GetIgnoreLayerCollision(go.layer, pl))
                    Debug.LogWarning($"[WinCheck]   -> collision matrix ignores trigger layer '{LayerName(go.layer)}' x player layer '{LayerName(pl)}'.", w);

                bool playerRb = p.GetComponentInChildren<Rigidbody>() != null || p.GetComponentInParent<Rigidbody>() != null;
                bool triggerRb = go.GetComponent<Rigidbody>() != null || go.GetComponentInParent<Rigidbody>() != null;
                if (!playerRb && !triggerRb)
                    Debug.LogWarning($"[WinCheck]   -> neither the trigger nor player '{p.name}' has a Rigidbody, Unity will not send trigger events.", w);

                Vector3 pp = p.transform.position;
                if (cols.Length > 0)
                    Debug.Log($"[WinCheck]   player '{p.name}' (layer '{LayerName(pl)}', pos {pp.x:F1}, {pp.y:F1}, {pp.z:F1}) {(cols[0].bounds.Contains(pp) ? "IS" : "is not")} inside this trigger right now.", w);
            }
        }
    }

    // Always draw the win trigger outline in the Scene view, even when its mesh is hidden.
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    private static void DrawWinGizmo(WinTrigger t, GizmoType type)
    {
        var col = t.GetComponent<Collider>();
        if (col == null) return;
        Bounds b = col.bounds;
        Gizmos.color = new Color(1f, 0.2f, 0.8f, 0.9f);
        Gizmos.DrawWireCube(b.center, b.size);
    }
}