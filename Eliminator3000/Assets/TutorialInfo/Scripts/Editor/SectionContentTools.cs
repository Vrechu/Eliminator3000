using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor-only. Obstacle / powerup placement (5x5 grid per entry) and enemy trigger tools for each LevelSection.
/// Place in Assets\TutorialInfo\Scripts\Editor\
/// </summary>
[InitializeOnLoad]
public static class SectionContentTools
{
    private const int Grid = 5;
    private const int Cells = Grid * Grid;
    private const int PickerId = 774012;
    public const string ObstacleFolderKey = "LevelManager.Library.Obstacles";
    public const string PowerupFolderKey = "LevelManager.Library.Powerups";
    private static readonly string[] PartNames = { "Start", "Mid", "End" };

    private static string[] cachedTitles;
    private static double cacheTime;
    private static LevelSection pickerSection;
    private static bool pickerPower;
    private static Texture2D dotTex;
    private static double nextSyncTime;

    static SectionContentTools()
    {
        EditorApplication.update -= SyncPoll;
        EditorApplication.update += SyncPoll;
    }

    // ------------------------------------------------------------------ helpers

    private static Texture2D Dot()
    {
        if (dotTex == null)
        {
            int n = 48;
            dotTex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            dotTex.hideFlags = HideFlags.HideAndDontSave;
            dotTex.filterMode = FilterMode.Bilinear;
            var px = new Color[n * n];
            Vector2 c = new Vector2(n * 0.5f, n * 0.5f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                    px[y * n + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(n * 0.5f - d));
                }
            dotTex.SetPixels(px);
            dotTex.Apply();
        }
        return dotTex;
    }

    private static bool Alive(LevelSection s, LevelSection.PlacedAsset e)
    {
        return e.instance != null && e.instance.transform.IsChildOf(s.transform);
    }

    private static Transform EnsureContainer(LevelSection s, string name)
    {
        Transform t = s.FindContainer(name);
        if (t != null) return t;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.layer = s.gameObject.layer;
        go.transform.SetParent(s.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private static void PlayArea(out float x0, out float x1, out float y0, out float y1)
    {
        x0 = EditorPrefs.GetFloat(PlayAreaGuide.MinXKey, -30f);
        x1 = EditorPrefs.GetFloat(PlayAreaGuide.MaxXKey, 30f);
        y0 = EditorPrefs.GetFloat(PlayAreaGuide.MinYKey, -15f);
        y1 = EditorPrefs.GetFloat(PlayAreaGuide.MaxYKey, 15f);
        if (x1 < x0) { float t = x0; x0 = x1; x1 = t; }
        if (y1 < y0) { float t = y0; y0 = y1; y1 = t; }
        if (x1 - x0 < 0.01f) x1 = x0 + 0.01f;
        if (y1 - y0 < 0.01f) y1 = y0 + 0.01f;
    }

    // World position of an entry: cell gives X/Y, zone + along give Z inside the section.
    private static Vector3 EntryPos(LevelSection s, LevelSection.PlacedAsset e)
    {
        PlayArea(out float x0, out float x1, out float y0, out float y1);
        Bounds b = s.GetBounds();
        float m = Mathf.Min(Mathf.Max(0f, s.gridEdgeMargin), b.size.z * 0.4f);
        float z0 = b.min.z + m;
        float z1 = b.max.z - m;
        int zone = Mathf.Clamp(e.zone, 0, 2);
        float zl = z0 + (z1 - z0) * zone / 3f;
        float zh = z0 + (z1 - z0) * (zone + 1) / 3f;
        float z = Mathf.Lerp(zl, zh, Mathf.Clamp01(e.along));

        int cell = Mathf.Clamp(e.cell, 0, Cells - 1);
        int cx = cell % Grid;
        int cy = cell / Grid;
        float x = x0 + (cx + 0.5f) * (x1 - x0) / Grid;
        float y = y0 + (cy + 0.5f) * (y1 - y0) / Grid + s.gridYOffset;
        return new Vector3(x, y, z);
    }

    // Opposite of EntryPos: works out zone / along / cell from where an object really is (used for objects found in the Hierarchy).
    private static void FitEntryToPosition(LevelSection s, LevelSection.PlacedAsset e, Vector3 p)
    {
        PlayArea(out float x0, out float x1, out float y0, out float y1);
        Bounds b = s.GetBounds();
        float m = Mathf.Min(Mathf.Max(0f, s.gridEdgeMargin), b.size.z * 0.4f);
        float z0 = b.min.z + m;
        float z1 = b.max.z - m;
        float t = z1 > z0 ? Mathf.Clamp01((p.z - z0) / (z1 - z0)) * 3f : 1.5f;
        e.zone = Mathf.Clamp(Mathf.FloorToInt(t), 0, 2);
        e.along = Mathf.Clamp01(t - e.zone);
        int cx = Mathf.Clamp(Mathf.FloorToInt((p.x - x0) / (x1 - x0) * Grid), 0, Grid - 1);
        int cy = Mathf.Clamp(Mathf.FloorToInt((p.y - s.gridYOffset - y0) / (y1 - y0) * Grid), 0, Grid - 1);
        e.cell = cy * Grid + cx;
    }

    private static void ApplyEntry(LevelSection s, LevelSection.PlacedAsset e)
    {
        if (!Alive(s, e)) return;
        Undo.RecordObject(e.instance.transform, "Place asset");
        e.instance.transform.position = EntryPos(s, e);
    }

    private static void ApplyAll(LevelSection s)
    {
        foreach (var e in s.assets) ApplyEntry(s, e);
        SceneView.RepaintAll();
    }

    // An entry counts as "in use" when it has a prefab or a live object.
    private static bool HasContent(LevelSection.PlacedAsset o)
    {
        return o.prefab != null || o.instance != null;
    }

    private static bool[] UsedCells(LevelSection s, int zone, LevelSection.PlacedAsset except)
    {
        var u = new bool[Cells];
        foreach (var o in s.assets)
        {
            if (o == except || !HasContent(o) || o.zone != zone) continue;
            u[Mathf.Clamp(o.cell, 0, Cells - 1)] = true;
        }
        return u;
    }

    private static int FreeCell(LevelSection s, int zone, LevelSection.PlacedAsset except)
    {
        var u = UsedCells(s, zone, except);
        if (!u[12]) return 12;
        for (int i = 0; i < Cells; i++) if (!u[i]) return i;
        return 12;
    }

    private static int RandomCell(LevelSection s, LevelSection.PlacedAsset e)
    {
        var u = UsedCells(s, e.zone, e);
        var free = new List<int>();
        for (int i = 0; i < Cells; i++) if (!u[i]) free.Add(i);
        if (free.Count == 0) return Random.Range(0, Cells);
        return free[Random.Range(0, free.Count)];
    }

    private static GameObject Spawn(LevelSection s, GameObject prefab, bool power)
    {
        Transform cont = EnsureContainer(s, power ? LevelSection.PowerupsName : LevelSection.ObstaclesName);
        GameObject go = null;
        if (PrefabUtility.IsPartOfPrefabAsset(prefab)) go = PrefabUtility.InstantiatePrefab(prefab, cont) as GameObject;
        if (go == null) go = Object.Instantiate(prefab, cont);
        Undo.RegisterCreatedObjectUndo(go, "Add asset");
        return go;
    }

    // ------------------------------------------------------------------ hierarchy sync (finds duplicated / deleted objects)

    private static void SyncPoll()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now < nextSyncTime) return;
        nextSyncTime = now + 0.5;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return;
        SyncAll();
    }

    private static void SyncAll()
    {
        var all = Object.FindObjectsByType<LevelSection>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool changed = false;
        foreach (var s in all)
        {
            if (s == null || EditorUtility.IsPersistent(s)) continue;
            if (SyncSection(s)) changed = true;
        }
        if (changed)
        {
            SceneView.RepaintAll();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }

    // Keeps the entry list in line with the objects in the Obstacles / Powerups groups.
    // No Undo is recorded on purpose: Ctrl+Z then undoes the Hierarchy action itself and the list follows.
    private static bool SyncSection(LevelSection s)
    {
        if (!s.enabled || !s.gameObject.activeInHierarchy) return false;
        bool changed = false;

        // 1. objects deleted in the Hierarchy: drop their entry
        for (int i = s.assets.Count - 1; i >= 0; i--)
        {
            var e = s.assets[i];
            if (e != null && e.instance != null) continue;
            s.assets.RemoveAt(i);
            changed = true;
            Debug.Log($"[Content] Section #{s.sectionNumber}: an obstacle/powerup was deleted in the Hierarchy, its entry was removed from the manager.");
        }

        // 2. objects in the groups without an entry (Ctrl+D in the Hierarchy, dragged in, undo of a delete)
        var known = new HashSet<GameObject>();
        foreach (var e in s.assets) if (e.instance != null) known.Add(e.instance);

        for (int c = 0; c < 2; c++)
        {
            bool power = c == 1;
            Transform cont = s.FindContainer(power ? LevelSection.PowerupsName : LevelSection.ObstaclesName);
            if (cont == null) continue;
            for (int k = 0; k < cont.childCount; k++)
            {
                GameObject go = cont.GetChild(k).gameObject;
                if (known.Contains(go)) continue;
                known.Add(go);

                var ne = new LevelSection.PlacedAsset();
                ne.prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
                ne.isPowerup = power;
                ne.instance = go;
                ne.open = false;
                FitEntryToPosition(s, ne, go.transform.position);
                s.assets.Insert(0, ne);
                changed = true;
                Debug.Log($"[Content] Section #{s.sectionNumber}: found '{go.name}' in {cont.name}, added to the manager ({PartNames[ne.zone]}, cell {ne.cell}). It was not moved.", go);
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(s);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(s.gameObject.scene);
        }
        return changed;
    }

    // ------------------------------------------------------------------ add / change / remove

    private static void AddAsset(LevelSection s, GameObject prefab, bool power)
    {
        if (s == null || prefab == null) return;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add asset");
        int g = Undo.GetCurrentGroup();

        Undo.RecordObject(s, "Add asset");
        var e = new LevelSection.PlacedAsset();
        e.prefab = prefab;
        e.isPowerup = power;
        e.zone = 1;
        e.along = 0.5f;
        e.cell = FreeCell(s, e.zone, null);
        e.instance = Spawn(s, prefab, power);
        s.assets.Insert(0, e);
        ApplyEntry(s, e);
        EditorUtility.SetDirty(s);

        Undo.CollapseUndoOperations(g);
        Debug.Log($"[Content] Section #{s.sectionNumber}: added {(power ? "powerup" : "obstacle")} '{prefab.name}' (Mid, cell {e.cell}).", e.instance);
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    // Copies an entry and its object (scale / rotation kept) into the first free cell of the same zone.
    private static void DuplicateEntry(LevelSection s, int idx)
    {
        if (idx < 0 || idx >= s.assets.Count) return;
        var src = s.assets[idx];
        bool alive = Alive(s, src);
        if (!alive && src.prefab == null) return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Duplicate asset");
        int g = Undo.GetCurrentGroup();

        Undo.RecordObject(s, "Duplicate asset");
        var e = new LevelSection.PlacedAsset();
        e.prefab = src.prefab;
        e.isPowerup = src.isPowerup;
        e.zone = src.zone;
        e.along = src.along;
        e.open = true;
        e.cell = FreeCell(s, src.zone, null);

        if (alive)
        {
            Transform cont = EnsureContainer(s, src.isPowerup ? LevelSection.PowerupsName : LevelSection.ObstaclesName);
            GameObject go = Object.Instantiate(src.instance, cont);
            go.name = src.instance.name;
            Undo.RegisterCreatedObjectUndo(go, "Duplicate asset");
            e.instance = go;
        }
        else e.instance = Spawn(s, src.prefab, src.isPowerup);

        s.assets.Insert(idx, e);
        ApplyEntry(s, e);
        EditorUtility.SetDirty(s);
        Undo.CollapseUndoOperations(g);

        Debug.Log($"[Content] Section #{s.sectionNumber}: duplicated '{e.instance.name}' ({PartNames[Mathf.Clamp(e.zone, 0, 2)]}, cell {e.cell}).", e.instance);
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private static void SetEntry(LevelSection s, LevelSection.PlacedAsset e, int zone, int cell, float along)
    {
        Undo.RecordObject(s, "Edit asset placement");
        e.zone = Mathf.Clamp(zone, 0, 2);
        e.cell = Mathf.Clamp(cell, 0, Cells - 1);
        e.along = Mathf.Clamp01(along);
        EditorUtility.SetDirty(s);
        ApplyEntry(s, e);
        SceneView.RepaintAll();
    }

    private static void SwapPrefab(LevelSection s, LevelSection.PlacedAsset e, GameObject np)
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Change asset prefab");
        int g = Undo.GetCurrentGroup();
        Undo.RecordObject(s, "Change asset prefab");
        if (Alive(s, e)) Undo.DestroyObjectImmediate(e.instance);
        e.prefab = np;
        e.instance = Spawn(s, np, e.isPowerup);
        ApplyEntry(s, e);
        EditorUtility.SetDirty(s);
        Undo.CollapseUndoOperations(g);
        SceneView.RepaintAll();
    }

    private static void RecreateEntry(LevelSection s, LevelSection.PlacedAsset e)
    {
        if (e.prefab == null) return;
        Undo.RecordObject(s, "Recreate asset");
        e.instance = Spawn(s, e.prefab, e.isPowerup);
        ApplyEntry(s, e);
        EditorUtility.SetDirty(s);
        SceneView.RepaintAll();
    }

    private static void RemoveEntry(LevelSection s, int idx)
    {
        if (idx < 0 || idx >= s.assets.Count) return;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Remove asset");
        int g = Undo.GetCurrentGroup();
        Undo.RecordObject(s, "Remove asset");
        var e = s.assets[idx];
        if (Alive(s, e)) Undo.DestroyObjectImmediate(e.instance);
        s.assets.RemoveAt(idx);
        EditorUtility.SetDirty(s);
        Undo.CollapseUndoOperations(g);
        SceneView.RepaintAll();
    }

    // ------------------------------------------------------------------ add-asset menu and picker

    private static void AddLibraryItems(GenericMenu m, LevelSection s, bool power)
    {
        string label = power ? "Powerups" : "Obstacles";
        string folder = EditorPrefs.GetString(power ? PowerupFolderKey : ObstacleFolderKey, "");
        if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
        {
            m.AddDisabledItem(new GUIContent(label + "/(set the folder in 'Asset library')"));
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        if (guids.Length == 0)
        {
            m.AddDisabledItem(new GUIContent(label + "/(no prefabs in " + folder + ")"));
            return;
        }
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string nm = System.IO.Path.GetFileNameWithoutExtension(path);
            m.AddItem(new GUIContent(label + "/" + nm), false, () =>
            {
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (pf != null) AddAsset(s, pf, power);
            });
        }
    }

    private static void ShowAddMenu(LevelSection s)
    {
        var m = new GenericMenu();
        AddLibraryItems(m, s, false);
        AddLibraryItems(m, s, true);
        m.AddSeparator("");
        m.AddItem(new GUIContent("Browse all prefabs (as obstacle)..."), false, () => OpenPicker(s, false));
        m.AddItem(new GUIContent("Browse all prefabs (as powerup)..."), false, () => OpenPicker(s, true));
        m.ShowAsContext();
    }

    private static void OpenPicker(LevelSection s, bool power)
    {
        pickerSection = s;
        pickerPower = power;
        EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", PickerId);
    }

    /// <summary>Call once at the start of the window's OnGUI. Handles the "Browse all prefabs" picker.</summary>
    public static void HandlePicker()
    {
        var e = UnityEngine.Event.current;
        if (e.type != UnityEngine.EventType.ExecuteCommand) return;
        if (e.commandName != "ObjectSelectorClosed") return;
        if (EditorGUIUtility.GetObjectPickerControlID() != PickerId) return;
        var o = EditorGUIUtility.GetObjectPickerObject() as GameObject;
        if (o != null && pickerSection != null) AddAsset(pickerSection, o, pickerPower);
        pickerSection = null;
        e.Use();
    }

    private static void FolderField(string label, string key)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(150));
        string cur = EditorPrefs.GetString(key, "");
        string nv = EditorGUILayout.TextField(cur);
        if (nv != cur) EditorPrefs.SetString(key, nv);
        if (GUILayout.Button("...", GUILayout.Width(28)))
        {
            string p = EditorUtility.OpenFolderPanel(label, Application.dataPath, "");
            if (!string.IsNullOrEmpty(p))
            {
                string dp = Application.dataPath.Replace('\\', '/');
                p = p.Replace('\\', '/');
                if (p.StartsWith(dp)) EditorPrefs.SetString(key, "Assets" + p.Substring(dp.Length));
                else Debug.LogWarning("[Content] The folder must be inside this project's Assets folder.");
            }
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>Two folder fields (obstacle prefabs / powerup prefabs) that feed the 'Add asset' menu.</summary>
    public static void DrawLibrarySettings()
    {
        FolderField("Obstacle prefabs folder", ObstacleFolderKey);
        FolderField("Powerup prefabs folder", PowerupFolderKey);
        EditorGUILayout.LabelField("Example: Assets/Prefabs/Obstacles. 'Add asset' lists every prefab in these folders.", EditorStyles.miniLabel);
    }

    // ------------------------------------------------------------------ area covered

    private static float Coverage(LevelSection s, out float[] perZone)
    {
        perZone = new float[3];
        float sum = 0f;
        for (int z = 0; z < 3; z++)
        {
            var used = UsedCells(s, z, null);
            int n = 0;
            for (int i = 0; i < Cells; i++) if (used[i]) n++;
            perZone[z] = n / (float)Cells;
            sum += perZone[z];
        }
        return sum / 3f;
    }

    // ------------------------------------------------------------------ enemy triggers

    private static string[] GetWaveTitles()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now < cacheTime) return cachedTitles;
        cacheTime = now + 1.0;
        cachedTitles = null;

        var sp = Object.FindFirstObjectByType<SpawnEnemy>();
        if (sp == null) return null;
        var prop = new SerializedObject(sp).FindProperty("enemyWaveInfos");
        if (prop == null || !prop.isArray || prop.arraySize == 0) return null;
        var arr = new string[prop.arraySize];
        for (int i = 0; i < arr.Length; i++)
        {
            Object o = prop.GetArrayElementAtIndex(i).objectReferenceValue;
            arr[i] = $"Wave {i} ({(o != null ? o.name : "empty")})";
        }
        cachedTitles = arr;
        return arr;
    }

    private static List<EnemySpawnTrigger> GetTriggers(LevelSection s)
    {
        var list = new List<EnemySpawnTrigger>();
        foreach (var t in s.GetComponentsInChildren<EnemySpawnTrigger>(true))
            if (t.GetComponentInParent<LevelSection>(true) == s) list.Add(t);
        list.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));
        return list;
    }

    private static int GetWave(EnemySpawnTrigger t)
    {
        var p = new SerializedObject(t).FindProperty("waveNumber");
        return p != null ? p.intValue : 0;
    }

    private static string TriggerName(int wave, string[] titles)
    {
        if (titles != null && wave >= 0 && wave < titles.Length) return "Enemy Trigger - " + titles[wave];
        return "Enemy Trigger - Wave " + wave;
    }

    private static void RenameIfNeeded(EnemySpawnTrigger t, int wave, string[] titles)
    {
        string want = TriggerName(wave, titles);
        if (t.gameObject.name == want) return;
        Undo.RecordObject(t.gameObject, "Rename trigger");
        t.gameObject.name = want;
    }

    // part: 0 = start, 1 = middle, 2 = end
    private static void PlaceTrigger(EnemySpawnTrigger t, LevelSection s, int part)
    {
        Bounds b = s.GetBounds();
        float off = Mathf.Min(3f, b.size.z * 0.1f);
        float z = part == 0 ? b.min.z + off : (part == 2 ? b.max.z - off : b.center.z);
        Undo.RecordObject(t.transform, "Place trigger");
        Vector3 p = t.transform.position;
        p.z = z;
        t.transform.position = p;
        EditorUtility.SetDirty(t.transform);
        SceneView.RepaintAll();
    }

    private static int PartOf(EnemySpawnTrigger t, LevelSection s)
    {
        Bounds b = s.GetBounds();
        float f = (t.transform.position.z - b.min.z) / Mathf.Max(0.01f, b.size.z);
        return f < 1f / 3f ? 0 : (f < 2f / 3f ? 1 : 2);
    }

    private static void FocusTrigger(EnemySpawnTrigger t)
    {
        Selection.activeGameObject = t.gameObject;
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        Physics.SyncTransforms();
        var col = t.GetComponentInChildren<Collider>();
        Bounds fb = col != null ? col.bounds : new Bounds(t.transform.position, Vector3.one * 20f);
        Vector3 sz = Vector3.Max(Vector3.Min(fb.size, new Vector3(120f, 80f, 120f)), Vector3.one * 10f);
        sv.Frame(new Bounds(fb.center, sz), false);
    }

    private static void AddTrigger(LevelSection s, string[] titles)
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add enemy trigger");
        int g = Undo.GetCurrentGroup();

        Transform cont = EnsureContainer(s, LevelSection.EnemyTriggersName);
        var existing = GetTriggers(s);

        EnemySpawnTrigger template = existing.Count > 0 ? existing[0] : null;
        if (template == null)
        {
            var all = Object.FindObjectsByType<EnemySpawnTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (all.Length > 0) template = all[0];
        }

        Bounds b = s.GetBounds();
        GameObject go;
        Vector3 pos;
        if (template != null)
        {
            go = Object.Instantiate(template.gameObject);
            pos = template.transform.position;
        }
        else
        {
            go = new GameObject("Enemy Trigger");
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(80f, 60f, 2f);
            var trig = go.AddComponent<EnemySpawnTrigger>();
            int pl = LayerMask.NameToLayer("Player");
            var tso = new SerializedObject(trig);
            var lp = tso.FindProperty("targetLayers");
            if (lp != null) { lp.intValue = pl >= 0 ? (1 << pl) : 0; tso.ApplyModifiedProperties(); }
            float cx = (EditorPrefs.GetFloat(PlayAreaGuide.MinXKey, -30f) + EditorPrefs.GetFloat(PlayAreaGuide.MaxXKey, 30f)) * 0.5f;
            float cy = (EditorPrefs.GetFloat(PlayAreaGuide.MinYKey, -15f) + EditorPrefs.GetFloat(PlayAreaGuide.MaxYKey, 15f)) * 0.5f;
            pos = new Vector3(cx, cy, b.center.z);
            Debug.LogWarning("[Triggers] No existing EnemySpawnTrigger found to copy, so a basic one was created. Check its Target Layers and collider size once; later triggers copy from it.", go);
        }
        Undo.RegisterCreatedObjectUndo(go, "Add enemy trigger");
        go.transform.SetParent(cont, true);
        pos.z = b.center.z;
        go.transform.position = pos;

        int next = 0;
        foreach (var t in existing) next = Mathf.Max(next, GetWave(t) + 1);
        if (titles != null && titles.Length > 0) next = Mathf.Clamp(next, 0, titles.Length - 1);
        var newTrig = go.GetComponent<EnemySpawnTrigger>();
        var so = new SerializedObject(newTrig);
        var wp = so.FindProperty("waveNumber");
        if (wp != null) { wp.intValue = next; so.ApplyModifiedProperties(); }
        go.name = TriggerName(next, titles);

        Undo.CollapseUndoOperations(g);
        Selection.activeGameObject = go;
        Debug.Log($"[Triggers] Added '{go.name}' in Section #{s.sectionNumber} (middle). Pick its wave in the Level Manager.", go);
        SceneView.RepaintAll();
    }

    // ------------------------------------------------------------------ GUI

    // Draws the 5x5 grid, returns the (possibly new) selected cell.
    private static int DrawGrid(Rect r, LevelSection s, LevelSection.PlacedAsset e)
    {
        EditorGUI.DrawRect(r, new Color(0.85f, 0.85f, 0.85f));
        var used = UsedCells(s, e.zone, e);
        float cw = r.width / Grid;
        float ch = r.height / Grid;
        float pad = Mathf.Min(cw, ch) * 0.15f;
        var ev = UnityEngine.Event.current;
        int result = e.cell;
        Color free = e.isPowerup ? new Color(0.1f, 0.75f, 0.25f) : new Color(1f, 0f, 0f);

        for (int row = 0; row < Grid; row++)
        {
            for (int cx = 0; cx < Grid; cx++)
            {
                int cy = Grid - 1 - row;   // top row = highest Y
                int idx = cy * Grid + cx;
                Rect cr = new Rect(r.x + cx * cw, r.y + row * ch, cw, ch);
                Rect dr = new Rect(cr.x + pad, cr.y + pad, cr.width - 2f * pad, cr.height - 2f * pad);

                if (ev.type == UnityEngine.EventType.Repaint)
                {
                    if (idx == e.cell)
                    {
                        Rect ring = new Rect(dr.x - 3f, dr.y - 3f, dr.width + 6f, dr.height + 6f);
                        GUI.DrawTexture(ring, Dot(), ScaleMode.StretchToFill, true, 0f, new Color(0.15f, 0.15f, 0.15f), 0f, 0f);
                        GUI.DrawTexture(dr, Dot(), ScaleMode.StretchToFill, true, 0f, new Color(0.6f, 0.6f, 0.6f), 0f, 0f);
                    }
                    else
                    {
                        Color c = used[idx] ? new Color(0.45f, 0f, 0f) : free;
                        GUI.DrawTexture(dr, Dot(), ScaleMode.StretchToFill, true, 0f, c, 0f, 0f);
                    }
                }
                if (ev.type == UnityEngine.EventType.MouseDown && ev.button == 0 && cr.Contains(ev.mousePosition))
                {
                    result = idx;
                    ev.Use();
                }
            }
        }
        return result;
    }

    private static void DrawEntry(LevelSection s, int i, bool on, ref int removeIdx, ref int dupIdx)
    {
        var e = s.assets[i];
        bool alive = Alive(s, e);
        int number = 0;
        for (int j = i; j < s.assets.Count; j++) if (s.assets[j].isPowerup == e.isPowerup) number++;
        string kind = e.isPowerup ? "Powerup" : "Obstacle";
        string pname = e.prefab != null ? e.prefab.name : (e.instance != null ? e.instance.name : "(empty)");

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        bool open = EditorGUILayout.Foldout(e.open, $"{kind} {number}: {pname}", true);
        if (open != e.open) e.open = open;

        using (new EditorGUI.DisabledScope(!on))
        {
            EditorGUI.BeginChangeCheck();
            int nz = GUILayout.Toolbar(Mathf.Clamp(e.zone, 0, 2), PartNames, EditorStyles.miniButton, GUILayout.Width(150));
            if (EditorGUI.EndChangeCheck()) SetEntry(s, e, nz, e.cell, e.along);

            if (GUILayout.Button(new GUIContent("Dup", "Duplicate this entry and its object (first free cell in the same part)"), GUILayout.Width(34)))
                dupIdx = i;
        }
        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button(new GUIContent("X", "Remove this entry and its object from the section"), GUILayout.Width(22))) removeIdx = i;
        GUI.backgroundColor = prevBg;
        EditorGUILayout.EndHorizontal();

        if (open)
        {
            using (new EditorGUI.DisabledScope(!on))
            {
                EditorGUI.BeginChangeCheck();
                var np = EditorGUILayout.ObjectField("Prefab", e.prefab, typeof(GameObject), false) as GameObject;
                if (EditorGUI.EndChangeCheck() && np != null && np != e.prefab)
                {
                    SwapPrefab(s, e, np);
                    GUIUtility.ExitGUI();
                }

                if (!alive && e.prefab != null)
                {
                    EditorGUILayout.HelpBox("The object of this entry is missing in the section.", MessageType.Warning);
                    if (GUILayout.Button("Recreate object"))
                    {
                        RecreateEntry(s, e);
                        GUIUtility.ExitGUI();
                    }
                }

                Rect row = GUILayoutUtility.GetRect(0f, 150f, GUILayout.ExpandWidth(true));
                float size = Mathf.Min(150f, row.width);
                Rect gr = new Rect(row.x + (row.width - size) * 0.5f, row.y, size, size);
                int nc = DrawGrid(gr, s, e);
                if (nc != e.cell) SetEntry(s, e, e.zone, nc, e.along);

                EditorGUI.BeginChangeCheck();
                float na = EditorGUILayout.Slider(new GUIContent("Position in zone", "Moves it along the track inside the Start / Mid / End part."), e.along, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) SetEntry(s, e, e.zone, e.cell, na);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Random cell")) SetEntry(s, e, e.zone, RandomCell(s, e), e.along);
                using (new EditorGUI.DisabledScope(!alive))
                {
                    if (GUILayout.Button("Select in scene")) Selection.activeGameObject = e.instance;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();
    }

    public static void DrawContent(LevelSection s, bool on)
    {
        int nObs = 0, nPow = 0;
        foreach (var en in s.assets)
        {
            if (!HasContent(en)) continue;
            if (en.isPowerup) nPow++; else nObs++;
        }
        var trigs = GetTriggers(s);

        string showKey = "LevelManager.SecShow." + s.GetInstanceID();
        bool show = EditorPrefs.GetBool(showKey, false);
        bool ns = EditorGUILayout.ToggleLeft($"Show assets grid   ({nObs} obstacles, {nPow} powerups, {trigs.Count} enemies)", show);
        if (ns != show) EditorPrefs.SetBool(showKey, ns);
        if (!ns)
        {
            EditorGUILayout.Space(2);
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(14);
        GUILayout.BeginVertical();

        if (!on) EditorGUILayout.HelpBox("Section is off (parked). Switch it on to place things.", MessageType.None);

        // obstacle / powerup entries, newest on top
        int removeIdx = -1;
        int dupIdx = -1;
        for (int i = 0; i < s.assets.Count; i++) DrawEntry(s, i, on, ref removeIdx, ref dupIdx);
        if (removeIdx >= 0)
        {
            RemoveEntry(s, removeIdx);
            GUIUtility.ExitGUI();
        }
        if (dupIdx >= 0)
        {
            DuplicateEntry(s, dupIdx);
            GUIUtility.ExitGUI();
        }

        using (new EditorGUI.DisabledScope(!on))
        {
            if (EditorGUILayout.DropdownButton(new GUIContent("Add asset  (obstacle / powerup)"), FocusType.Passive))
                ShowAddMenu(s);
        }

        // area covered
        float cov = Coverage(s, out float[] pz);
        EditorGUILayout.LabelField($"Area covered: {Mathf.RoundToInt(cov * 100f)}%    (Start {Mathf.RoundToInt(pz[0] * 100f)}%, Mid {Mathf.RoundToInt(pz[1] * 100f)}%, End {Mathf.RoundToInt(pz[2] * 100f)}%)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Share of the 5x5 screen grid used by obstacles and powerups, averaged over the three parts.", EditorStyles.miniLabel);
        EditorGUILayout.Space(4);

        // enemies
        DrawEnemies(s, on, trigs);

        // placement settings
        string pk = "LevelManager.SecShow.Place." + s.GetInstanceID();
        bool pv = EditorPrefs.GetBool(pk, false);
        bool pn = EditorGUILayout.Foldout(pv, "Placement settings", true);
        if (pn != pv) EditorPrefs.SetBool(pk, pn);
        if (pn)
        {
            EditorGUI.BeginChangeCheck();
            float em = EditorGUILayout.FloatField(new GUIContent("Edge margin (Z)", "Nothing is placed this close to the start / end of the section."), s.gridEdgeMargin);
            float yo = EditorGUILayout.FloatField(new GUIContent("Height offset (Y)", "Lifts or lowers all placed objects."), s.gridYOffset);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(s, "Edit placement settings");
                s.gridEdgeMargin = Mathf.Max(0f, em);
                s.gridYOffset = yo;
                EditorUtility.SetDirty(s);
                ApplyAll(s);
            }
            PlayArea(out float x0, out float x1, out float y0, out float y1);
            EditorGUILayout.LabelField($"Play area X {x0:F0}..{x1:F0}, Y {y0:F0}..{y1:F0} (from the Play area guide)", EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(!on))
            {
                if (GUILayout.Button(new GUIContent("Re-place all", "Moves every object back to its cell. Hand-moved objects are reset.")))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("Re-place all");
                    int g = Undo.GetCurrentGroup();
                    ApplyAll(s);
                    Undo.CollapseUndoOperations(g);
                    GUIUtility.ExitGUI();
                }
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private static void DrawEnemies(LevelSection s, bool on, List<EnemySpawnTrigger> trigs)
    {
        string[] titles = GetWaveTitles();
        EnemySpawnTrigger toDelete = null;
        int n = 0;
        foreach (var t in trigs)
        {
            n++;
            if (t == null) continue;
            var so = new SerializedObject(t);
            var wp = so.FindProperty("waveNumber");
            if (wp == null) continue;
            int wave = wp.intValue;
            RenameIfNeeded(t, wave, titles);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Enemy {n}", EditorStyles.boldLabel, GUILayout.Width(60));
            using (new EditorGUI.DisabledScope(!on))
            {
                EditorGUI.BeginChangeCheck();
                int np = GUILayout.Toolbar(PartOf(t, s), PartNames, EditorStyles.miniButton, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck()) PlaceTrigger(t, s, np);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Focus", "Select and frame this trigger in the Scene view"), GUILayout.Width(50)))
                FocusTrigger(t);
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button(new GUIContent("X", "Delete this enemy trigger"), GUILayout.Width(22))) toDelete = t;
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wave:", GUILayout.Width(40));
            EditorGUI.BeginChangeCheck();
            int nw;
            if (titles != null && wave >= 0 && wave < titles.Length) nw = EditorGUILayout.Popup(wave, titles);
            else nw = EditorGUILayout.IntField(wave);
            if (EditorGUI.EndChangeCheck())
            {
                so.Update();
                wp.intValue = Mathf.Max(0, nw);
                so.ApplyModifiedProperties();
                RenameIfNeeded(t, wp.intValue, titles);
            }
            EditorGUILayout.EndHorizontal();

            if (titles != null && (wave < 0 || wave >= titles.Length))
                EditorGUILayout.HelpBox($"Wave {wave} does not exist in this scene's SpawnEnemy (it has {titles.Length}).", MessageType.Warning);
            EditorGUILayout.EndVertical();
        }

        if (toDelete != null)
        {
            Undo.DestroyObjectImmediate(toDelete.gameObject);
            SceneView.RepaintAll();
            GUIUtility.ExitGUI();
        }

        using (new EditorGUI.DisabledScope(!on))
        {
            if (GUILayout.Button("Add enemy"))
            {
                AddTrigger(s, titles);
                GUIUtility.ExitGUI();
            }
        }
        EditorGUILayout.Space(4);
    }
}