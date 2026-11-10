using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor-only. Enemy wave viewer / editor / preview. Menu: Tools > Enemy Waves.
/// Writes only DATA (EnemyWaveInfo assets + SpawnEnemy arrays + point objects under "EnemyPaths").
/// Place in Assets\TutorialInfo\Scripts\Editor\
/// </summary>
public class EnemyWaveTool : EditorWindow
{
    private const string RootName = "EnemyPaths";
    private const string SelKey = "EnemyWaves.Selected";
    private const string ShowBoxKey = "EnemyWaves.ShowBox";
    private const string ShowDropKey = "EnemyWaves.ShowDrop";
    private const string ShowProjKey = "EnemyWaves.ShowProj";
    private const string ShowLabelsKey = "EnemyWaves.ShowLabels";
    private const string ShowHandlesKey = "EnemyWaves.ShowHandles";
    private const string ShowGhostKey = "EnemyWaves.ShowGhost";
    private const string AllEnemiesKey = "EnemyWaves.AllEnemies";
    private const string SpeedKey = "EnemyWaves.TimeScale";

    // ------------------------------------------------------------------ types

    private class PathInfo
    {
        public Transform[] tr;      // [0] spawn, [1..N] waypoints, [N+1] end
        public Vector3[] pos;
        public float[] segSpeed;    // N+1 segments
        public float[] wait;        // N waits
        public float[] arrive;      // arrival time at every point
        public float duration;
        public bool ok;
        public bool canPlay;
        public bool lengthIssue;
        public List<string> problems = new List<string>();

        public int N { get { return wait.Length; } }

        // Position after t seconds of flight (MoveEnemy rules: straight lines, per-point speed, wait on arrival).
        public Vector3 Eval(float t, out bool alive)
        {
            alive = true;
            float t0 = 0f;
            Vector3 cur = pos[0];
            for (int i = 0; i <= N; i++)
            {
                Vector3 tgt = pos[i + 1];
                float dur = Vector3.Distance(cur, tgt) / Mathf.Max(segSpeed[i], 0.0001f);
                if (t < t0 + dur) return Vector3.Lerp(cur, tgt, dur > 0f ? (t - t0) / dur : 1f);
                t0 += dur;
                cur = tgt;
                if (i < N)
                {
                    if (t < t0 + wait[i]) return tgt;
                    t0 += wait[i];
                }
            }
            alive = false;
            return cur;
        }
    }

    private class GhostPart
    {
        public Mesh mesh;
        public Material[] mats;
        public Matrix4x4 local;
    }

    // ------------------------------------------------------------------ state

    private SpawnEnemy spawner;
    private SerializedObject spSo;
    private SerializedProperty pWaves, pSpawns, pWps, pEnds;

    private Vector2 scroll;
    private int activePt = -1;
    private float previewTime;
    private bool playing;
    private double lastTick;
    private float cachedTotal;
    private int previewEnemy;
    private int opGroup;
    private GUIStyle labelStyle;
    private float cachedPlayZ;
    private double nextPlayZ;

    private readonly Dictionary<string, List<string>> sharedCache = new Dictionary<string, List<string>>();
    private readonly Dictionary<GameObject, List<GhostPart>> ghostCache = new Dictionary<GameObject, List<GhostPart>>();

    private int Selected
    {
        get { return SessionState.GetInt(SelKey, 0); }
        set { SessionState.SetInt(SelKey, value); }
    }

    [MenuItem("Tools/Enemy Waves")]
    public static void ShowWindow() { GetWindow<EnemyWaveTool>("Enemy Waves"); }

    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Undo.undoRedoPerformed -= OnUndo;
        Undo.undoRedoPerformed += OnUndo;
        lastTick = EditorApplication.timeSinceStartup;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= Tick;
        Undo.undoRedoPerformed -= OnUndo;
    }

    private void OnUndo() { spSo = null; Repaint(); SceneView.RepaintAll(); }
    private void OnHierarchyChange() { Repaint(); }

    // Selecting an EnemySpawnTrigger shows its wave.
    private void OnSelectionChange()
    {
        var go = Selection.activeGameObject;
        if (go == null) return;
        var trg = go.GetComponentInParent<EnemySpawnTrigger>();
        if (trg == null) return;
        var prop = new SerializedObject(trg).FindProperty("waveNumber");
        if (prop == null || !EnsureSpawner()) return;
        int n = prop.intValue;
        if (n < 0 || n >= pWaves.arraySize) return;
        Selected = n;
        activePt = -1;
        previewTime = 0f;
        Repaint();
        SceneView.RepaintAll();
    }

    private void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        float dt = (float)(now - lastTick);
        lastTick = now;
        if (!playing || cachedTotal <= 0f) return;
        previewTime += dt * EditorPrefs.GetFloat(SpeedKey, 1f);
        if (previewTime > cachedTotal) previewTime = 0f;
        SceneView.RepaintAll();
        Repaint();
    }

    // ------------------------------------------------------------------ data access

    private bool EnsureSpawner()
    {
        if (spawner == null)
        {
            spawner = Object.FindFirstObjectByType<SpawnEnemy>(FindObjectsInactive.Include);
            spSo = null;
        }
        if (spawner == null) return false;
        if (spSo == null || spSo.targetObject != spawner)
        {
            spSo = new SerializedObject(spawner);
            pWaves = spSo.FindProperty("enemyWaveInfos");
            pSpawns = spSo.FindProperty("spawnPoints");
            pWps = spSo.FindProperty("waypoints");
            pEnds = spSo.FindProperty("endPoints");
        }
        spSo.Update();
        return pWaves != null && pSpawns != null && pWps != null && pEnds != null;
    }

    private EnemyWaveInfo GetWave(int i)
    {
        if (pWaves == null || i < 0 || i >= pWaves.arraySize) return null;
        return pWaves.GetArrayElementAtIndex(i).objectReferenceValue as EnemyWaveInfo;
    }

    private static Transform GetT(SerializedProperty arr, int i)
    {
        if (arr == null || i < 0 || i >= arr.arraySize) return null;
        return arr.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
    }

    private static int[] A(int[] a) { return a ?? new int[0]; }
    private static float[] A(float[] a) { return a ?? new float[0]; }
    private static GameObject[] A(GameObject[] a) { return a ?? new GameObject[0]; }
    private static Vector3 PosOf(Transform t) { return t != null ? t.position : Vector3.zero; }

    private static void Edit(EnemyWaveInfo w, System.Action<SerializedObject> a)
    {
        using (var so = new SerializedObject(w))
        {
            so.Update();
            a(so);
            so.ApplyModifiedProperties();
        }
        EditorUtility.SetDirty(w);
    }

    private static void SetInts(SerializedProperty p, IList<int> v)
    {
        p.arraySize = v.Count;
        for (int i = 0; i < v.Count; i++) p.GetArrayElementAtIndex(i).intValue = v[i];
    }

    private static void SetFloats(SerializedProperty p, IList<float> v)
    {
        p.arraySize = v.Count;
        for (int i = 0; i < v.Count; i++) p.GetArrayElementAtIndex(i).floatValue = v[i];
    }

    private static void SetObjs(SerializedProperty p, IList<GameObject> v)
    {
        p.arraySize = v.Count;
        for (int i = 0; i < v.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = v[i];
    }

    private static void ResizeFloats(SerializedProperty p, int n, float fill)
    {
        int old = p.arraySize;
        p.arraySize = n;
        for (int i = old; i < n; i++) p.GetArrayElementAtIndex(i).floatValue = fill;
    }

    private static void Pad(List<float> l, int n) { while (l.Count < n) l.Add(0f); }

    private float PlayZ()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now >= nextPlayZ)
        {
            nextPlayZ = now + 0.5;
            var pts = LevelManagerWindow.GetSpawnPoints();
            float s = 0f;
            foreach (var t in pts) s += t.position.z;
            cachedPlayZ = pts.Count > 0 ? s / pts.Count : 0f;
        }
        return cachedPlayZ;
    }

    // ------------------------------------------------------------------ path building / simulation

    private PathInfo BuildPath(EnemyWaveInfo w)
    {
        var p = new PathInfo();
        int[] order = A(w.WaypointOrder);
        float[] spd = A(w.WaypointSpeeds);
        float[] wt = A(w.WaitTimes);
        GameObject[] pf = A(w.EnemyPrefabs);
        float[] gaps = A(w.TimesBetweenSpawns);
        int n = order.Length;

        p.tr = new Transform[n + 2];
        p.pos = new Vector3[n + 2];
        p.segSpeed = new float[n + 1];
        p.wait = new float[n];
        p.arrive = new float[n + 2];
        p.ok = true;

        p.tr[0] = GetT(pSpawns, w.SpawnPointIndex);
        if (p.tr[0] == null) { p.ok = false; p.problems.Add($"Spawn point index {w.SpawnPointIndex} is empty or out of range."); }
        for (int i = 0; i < n; i++)
        {
            p.tr[i + 1] = GetT(pWps, order[i]);
            if (p.tr[i + 1] == null) { p.ok = false; p.problems.Add($"Waypoint #{i + 1} (index {order[i]}) is empty or out of range."); }
        }
        p.tr[n + 1] = GetT(pEnds, w.EndPointIndex);
        if (p.tr[n + 1] == null) { p.ok = false; p.problems.Add($"End point index {w.EndPointIndex} is empty or out of range."); }

        if (pf.Length == 0) p.problems.Add("No enemy prefabs in this wave (SpawnEnemy refuses to spawn it).");
        if (n == 0) p.problems.Add("No waypoints: MoveEnemy.Start reads WaitTimes[0] and will throw (runtime bug, add at least one waypoint).");
        if (pf.Length != gaps.Length || n != spd.Length || n != wt.Length)
        {
            p.lengthIssue = true;
            p.problems.Add("Array lengths do not match (prefabs/delays or order/speeds/waits). SpawnEnemy refuses such waves.");
        }
        if (w.EnemySpeed <= 0f) p.problems.Add("EnemySpeed is 0 or negative.");

        for (int i = 0; i <= n; i++)
        {
            float s = (i < n && i < spd.Length) ? spd[i] : 0f;
            if (s <= 0f) s = w.EnemySpeed;
            p.segSpeed[i] = s;
            if (i < n) p.wait[i] = i < wt.Length ? Mathf.Max(0f, wt[i]) : 0f;
        }

        p.canPlay = p.ok && w.EnemySpeed > 0f;
        if (p.ok)
        {
            for (int j = 0; j < p.tr.Length; j++) p.pos[j] = p.tr[j].position;
            float t = 0f;
            for (int i = 0; i <= n; i++)
            {
                t += Vector3.Distance(p.pos[i], p.pos[i + 1]) / Mathf.Max(p.segSpeed[i], 0.0001f);
                p.arrive[i + 1] = t;
                if (i < n) t += p.wait[i];
            }
            p.duration = t;
        }
        return p;
    }

    private float TotalTime(EnemyWaveInfo w, PathInfo p)
    {
        if (!p.canPlay) return 0f;
        if (!EditorPrefs.GetBool(AllEnemiesKey, true)) return p.duration;
        float sum = 0f;
        foreach (float g in A(w.TimesBetweenSpawns)) sum += Mathf.Max(0f, g);
        return sum + p.duration;
    }

    // How many OTHER waves use this point (kind 0 = spawn, 1 = waypoint, 2 = end).
    private int OtherUsers(int kind, int idx, EnemyWaveInfo self)
    {
        int c = 0;
        for (int j = 0; j < pWaves.arraySize; j++)
        {
            var o = GetWave(j);
            if (o == null || o == self) continue;
            if (kind == 0 && o.SpawnPointIndex == idx) c++;
            else if (kind == 2 && o.EndPointIndex == idx) c++;
            else if (kind == 1 && o.WaypointOrder != null && System.Array.IndexOf(o.WaypointOrder, idx) >= 0) c++;
        }
        return c;
    }

    // Other scenes (saved files) that reference this wave asset.
    private List<string> OtherScenes(EnemyWaveInfo w)
    {
        string path = AssetDatabase.GetAssetPath(w);
        List<string> list;
        if (sharedCache.TryGetValue(path, out list)) return list;
        list = new List<string>();
        string cur = spawner.gameObject.scene.path;
        foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string sp = AssetDatabase.GUIDToAssetPath(guid);
            if (sp == cur) continue;
            string[] deps = AssetDatabase.GetDependencies(sp, true);
            if (System.Array.IndexOf(deps, path) >= 0) list.Add(System.IO.Path.GetFileNameWithoutExtension(sp));
        }
        sharedCache[path] = list;
        return list;
    }

    // ------------------------------------------------------------------ scene objects (EnemyPaths)

    private static string GroupName(EnemyWaveInfo w) { return w.name.Replace('/', '_'); }

    private Transform FindRoot()
    {
        foreach (var go in spawner.gameObject.scene.GetRootGameObjects())
            if (go.name == RootName) return go.transform;
        return null;
    }

    private Transform EnsureRoot()
    {
        var r = FindRoot();
        if (r != null) return r;
        var go = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(go, "Create EnemyPaths");
        if (go.scene != spawner.gameObject.scene) SceneManager.MoveGameObjectToScene(go, spawner.gameObject.scene);
        return go.transform;
    }

    private Transform FindGroup(string name)
    {
        var r = FindRoot();
        return r != null ? r.Find(name) : null;
    }

    private Transform EnsureGroup(string name)
    {
        var root = EnsureRoot();
        var g = root.Find(name);
        if (g != null) return g;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create wave path group");
        go.transform.SetParent(root, false);
        return go.transform;
    }

    private static Transform CreatePoint(Transform group, string name, Vector3 worldPos)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create wave point");
        go.transform.SetParent(group, false);
        go.transform.position = worldPos;
        return go.transform;
    }

    // Reuses an empty slot if there is one, otherwise appends. Returns the index.
    private static int AssignSlot(SerializedProperty arr, Transform t)
    {
        for (int i = 0; i < arr.arraySize; i++)
        {
            if (arr.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                arr.GetArrayElementAtIndex(i).objectReferenceValue = t;
                return i;
            }
        }
        int n = arr.arraySize;
        arr.arraySize = n + 1;
        arr.GetArrayElementAtIndex(n).objectReferenceValue = t;
        return n;
    }

    private void RenameOwned(EnemyWaveInfo w)
    {
        Transform g = FindGroup(GroupName(w));
        if (g == null) return;
        Rename(GetT(pSpawns, w.SpawnPointIndex), g, "Spawn");
        Rename(GetT(pEnds, w.EndPointIndex), g, "End");
        int[] order = A(w.WaypointOrder);
        for (int k = 0; k < order.Length; k++) Rename(GetT(pWps, order[k]), g, "Point_" + (k + 1));
    }

    private static void Rename(Transform t, Transform group, string name)
    {
        if (t == null || t.parent != group || t.name == name) return;
        Undo.RecordObject(t.gameObject, "Rename wave point");
        t.name = name;
    }

    // ------------------------------------------------------------------ operations

    private void BeginOp(string name)
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(name);
        opGroup = Undo.GetCurrentGroup();
    }

    private void EndOp()
    {
        spSo.ApplyModifiedProperties();
        Undo.CollapseUndoOperations(opGroup);
        EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Repaint();
    }

    private void AddWaypoint(EnemyWaveInfo w, int insertAt)
    {
        BeginOp("Add wave point");
        var order = new List<int>(A(w.WaypointOrder));
        var spd = new List<float>(A(w.WaypointSpeeds));
        var wt = new List<float>(A(w.WaitTimes));
        Pad(spd, order.Count);
        Pad(wt, order.Count);
        insertAt = Mathf.Clamp(insertAt, 0, order.Count);

        Vector3 a = insertAt == 0 ? PosOf(GetT(pSpawns, w.SpawnPointIndex)) : PosOf(GetT(pWps, order[insertAt - 1]));
        Vector3 b = insertAt < order.Count ? PosOf(GetT(pWps, order[insertAt])) : PosOf(GetT(pEnds, w.EndPointIndex));

        Transform group = EnsureGroup(GroupName(w));
        Transform t = CreatePoint(group, "Point_" + (insertAt + 1), (a + b) * 0.5f);
        int idx = AssignSlot(pWps, t);
        order.Insert(insertAt, idx);
        spd.Insert(insertAt, 0f);
        wt.Insert(insertAt, 0f);
        spSo.ApplyModifiedProperties();

        Edit(w, so =>
        {
            SetInts(so.FindProperty("WaypointOrder"), order);
            SetFloats(so.FindProperty("WaypointSpeeds"), spd);
            SetFloats(so.FindProperty("WaitTimes"), wt);
        });
        RenameOwned(w);
        activePt = insertAt + 1;
        EndOp();
        Debug.Log($"[Waves] Added waypoint to '{w.name}' (index {idx} in SpawnEnemy waypoints).");
    }

    private void RemoveWaypoint(EnemyWaveInfo w, int k)
    {
        BeginOp("Remove wave point");
        var order = new List<int>(A(w.WaypointOrder));
        var spd = new List<float>(A(w.WaypointSpeeds));
        var wt = new List<float>(A(w.WaitTimes));
        Pad(spd, order.Count);
        Pad(wt, order.Count);
        if (k < 0 || k >= order.Count) { EndOp(); return; }

        int idx = order[k];
        Transform t = GetT(pWps, idx);
        order.RemoveAt(k);
        spd.RemoveAt(k);
        wt.RemoveAt(k);
        Edit(w, so =>
        {
            SetInts(so.FindProperty("WaypointOrder"), order);
            SetFloats(so.FindProperty("WaypointSpeeds"), spd);
            SetFloats(so.FindProperty("WaitTimes"), wt);
        });

        // Only delete the object if it belongs to this wave and nothing else uses it.
        if (t != null && !order.Contains(idx) && OtherUsers(1, idx, w) == 0 && t.parent == FindGroup(GroupName(w)))
        {
            pWps.GetArrayElementAtIndex(idx).objectReferenceValue = null;
            spSo.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(t.gameObject);
        }
        RenameOwned(w);
        activePt = -1;
        EndOp();
    }

    private void MoveWaypoint(EnemyWaveInfo w, int k, int dir)
    {
        BeginOp("Reorder wave point");
        var order = new List<int>(A(w.WaypointOrder));
        var spd = new List<float>(A(w.WaypointSpeeds));
        var wt = new List<float>(A(w.WaitTimes));
        Pad(spd, order.Count);
        Pad(wt, order.Count);
        int k2 = k + dir;
        if (k < 0 || k2 < 0 || k >= order.Count || k2 >= order.Count) { EndOp(); return; }

        int ti = order[k]; order[k] = order[k2]; order[k2] = ti;
        float tf = spd[k]; spd[k] = spd[k2]; spd[k2] = tf;
        tf = wt[k]; wt[k] = wt[k2]; wt[k2] = tf;
        Edit(w, so =>
        {
            SetInts(so.FindProperty("WaypointOrder"), order);
            SetFloats(so.FindProperty("WaypointSpeeds"), spd);
            SetFloats(so.FindProperty("WaitTimes"), wt);
        });
        RenameOwned(w);
        activePt = k2 + 1;
        EndOp();
    }

    // Clones every point this wave uses that is not owned by its own group. No Begin/EndOp (callers do it).
    private void MakeUnique(EnemyWaveInfo w)
    {
        Transform g = EnsureGroup(GroupName(w));

        int newSi = w.SpawnPointIndex;
        Transform ts = GetT(pSpawns, newSi);
        if (ts != null && ts.parent != g) newSi = AssignSlot(pSpawns, CreatePoint(g, "Spawn", ts.position));

        int newEi = w.EndPointIndex;
        Transform te = GetT(pEnds, newEi);
        if (te != null && te.parent != g) newEi = AssignSlot(pEnds, CreatePoint(g, "End", te.position));

        var order = new List<int>(A(w.WaypointOrder));
        var map = new Dictionary<int, int>();
        for (int k = 0; k < order.Count; k++)
        {
            int old = order[k];
            Transform t = GetT(pWps, old);
            if (t == null || t.parent == g) continue;
            int ni;
            if (!map.TryGetValue(old, out ni))
            {
                ni = AssignSlot(pWps, CreatePoint(g, "Point_" + (k + 1), t.position));
                map[old] = ni;
            }
            order[k] = ni;
        }
        spSo.ApplyModifiedProperties();

        Edit(w, so =>
        {
            so.FindProperty("SpawnPointIndex").intValue = newSi;
            so.FindProperty("EndPointIndex").intValue = newEi;
            SetInts(so.FindProperty("WaypointOrder"), order);
        });
        RenameOwned(w);
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    private string SceneFolder()
    {
        string sn = spawner.gameObject.scene.name;
        if (string.IsNullOrEmpty(sn)) sn = "UnsavedScene";
        foreach (char c in System.IO.Path.GetInvalidFileNameChars()) sn = sn.Replace(c, '_');
        return "Assets/EnemyWaves/" + sn;
    }

    // Clones the asset into this scene's folder, assigns the clone to slot idx and gives it its own points.
    private void MakeSceneSpecific(int idx, EnemyWaveInfo w)
    {
        string folder = SceneFolder();
        EnsureFolder(folder);
        string src = AssetDatabase.GetAssetPath(w);
        string dst = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + w.name + ".asset");
        if (!AssetDatabase.CopyAsset(src, dst)) { Debug.LogError($"[Waves] Could not copy '{src}' to '{dst}'."); return; }
        var c = AssetDatabase.LoadAssetAtPath<EnemyWaveInfo>(dst);
        if (c == null) return;

        Transform oldGroup = FindGroup(GroupName(w));
        pWaves.GetArrayElementAtIndex(idx).objectReferenceValue = c;
        spSo.ApplyModifiedProperties();

        string newName = GroupName(c);
        if (oldGroup != null && oldGroup.name != newName && FindGroup(newName) == null)
        {
            Undo.RecordObject(oldGroup.gameObject, "Rename path group");
            oldGroup.name = newName;
        }
        MakeUnique(c);
        Debug.Log($"[Waves] Scene-specific copy '{dst}' now used by wave slot {idx} in this scene.");
    }

    private void DuplicateWave(int idx, EnemyWaveInfo w)
    {
        BeginOp("Duplicate wave");
        string src = AssetDatabase.GetAssetPath(w);
        string dir = System.IO.Path.GetDirectoryName(src).Replace('\\', '/');
        string dst = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + w.name + "_Copy.asset");
        if (!AssetDatabase.CopyAsset(src, dst)) { Debug.LogError($"[Waves] Could not copy '{src}'."); EndOp(); return; }
        var c = AssetDatabase.LoadAssetAtPath<EnemyWaveInfo>(dst);
        int n = pWaves.arraySize;
        pWaves.arraySize = n + 1;
        pWaves.GetArrayElementAtIndex(n).objectReferenceValue = c;
        spSo.ApplyModifiedProperties();
        MakeUnique(c);
        Selected = n;
        activePt = -1;
        EndOp();
        Debug.Log($"[Waves] Duplicated '{w.name}' as '{c.name}' (wave number {n}) with its own points.");
    }

    private void NewWave()
    {
        BeginOp("New enemy wave");
        string folder = SceneFolder();
        EnsureFolder(folder);
        var cur = GetWave(Mathf.Clamp(Selected, 0, Mathf.Max(0, pWaves.arraySize - 1)));
        GameObject seed = (cur != null && A(cur.EnemyPrefabs).Length > 0) ? cur.EnemyPrefabs[0] : null;
        float speed = cur != null && cur.EnemySpeed > 0f ? cur.EnemySpeed : 5f;

        var asset = ScriptableObject.CreateInstance<EnemyWaveInfo>();
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/Wave_" + pWaves.arraySize + ".asset");
        AssetDatabase.CreateAsset(asset, path);

        float cx = (EditorPrefs.GetFloat(PlayAreaGuide.MinXKey, -30f) + EditorPrefs.GetFloat(PlayAreaGuide.MaxXKey, 30f)) * 0.5f;
        float cy = (EditorPrefs.GetFloat(PlayAreaGuide.MinYKey, -15f) + EditorPrefs.GetFloat(PlayAreaGuide.MaxYKey, 15f)) * 0.5f;
        float z = PlayZ();

        Transform g = EnsureGroup(GroupName(asset));
        int si = AssignSlot(pSpawns, CreatePoint(g, "Spawn", new Vector3(cx, cy, z + 150f)));
        int wi = AssignSlot(pWps, CreatePoint(g, "Point_1", new Vector3(cx, cy, z + 80f)));
        int ei = AssignSlot(pEnds, CreatePoint(g, "End", new Vector3(cx, cy, z - 20f)));

        int n = pWaves.arraySize;
        pWaves.arraySize = n + 1;
        pWaves.GetArrayElementAtIndex(n).objectReferenceValue = asset;
        spSo.ApplyModifiedProperties();

        Edit(asset, so =>
        {
            so.FindProperty("SpawnPointIndex").intValue = si;
            so.FindProperty("EndPointIndex").intValue = ei;
            so.FindProperty("EnemySpeed").floatValue = speed;
            SetInts(so.FindProperty("WaypointOrder"), new List<int> { wi });
            SetFloats(so.FindProperty("WaypointSpeeds"), new List<float> { 0f });
            SetFloats(so.FindProperty("WaitTimes"), new List<float> { 0f });
            if (seed != null)
            {
                SetObjs(so.FindProperty("EnemyPrefabs"), new List<GameObject> { seed });
                SetFloats(so.FindProperty("TimesBetweenSpawns"), new List<float> { 1f });
            }
        });
        Selected = n;
        activePt = -1;
        EndOp();
        Debug.Log($"[Waves] Created wave {n} '{asset.name}' in {folder}.");
    }

    private void FixAll()
    {
        if (!EditorUtility.DisplayDialog("Fix everything in this scene",
            "For every wave in this scene's SpawnEnemy:\n" +
            "- waves whose asset is also used by another scene get a scene-specific copy in '" + SceneFolder() + "'\n" +
            "- every wave gets its own points under 'EnemyPaths'.\n\n" +
            "Other scenes keep the original assets. The new asset copies are not removed by Undo.", "Do it", "Cancel")) return;

        BeginOp("Fix enemy waves");
        for (int i = 0; i < pWaves.arraySize; i++)
        {
            var w = GetWave(i);
            if (w == null) continue;
            if (OtherScenes(w).Count > 0) MakeSceneSpecific(i, w);
            else MakeUnique(w);
        }
        EndOp();
        sharedCache.Clear();
    }

    private void FixLengths(EnemyWaveInfo w)
    {
        BeginOp("Fix wave array lengths");
        int n = A(w.WaypointOrder).Length;
        int m = A(w.EnemyPrefabs).Length;
        Edit(w, so =>
        {
            ResizeFloats(so.FindProperty("WaypointSpeeds"), n, 0f);
            ResizeFloats(so.FindProperty("WaitTimes"), n, 0f);
            ResizeFloats(so.FindProperty("TimesBetweenSpawns"), m, 1f);
        });
        EndOp();
    }

    // ------------------------------------------------------------------ scene view

    private void OnSceneGUI(SceneView sv)
    {
        if (!EnsureSpawner()) return;
        var w = GetWave(Mathf.Clamp(Selected, 0, Mathf.Max(0, pWaves.arraySize - 1)));
        if (w == null) return;
        PathInfo p = BuildPath(w);
        if (!p.ok) return;
        cachedTotal = TotalTime(w, p);

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 11;
        }

        bool repaint = UnityEngine.Event.current.type == UnityEngine.EventType.Repaint;
        var oldColor = Handles.color;
        var oldZ = Handles.zTest;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        if (repaint)
        {
            if (EditorPrefs.GetBool(ShowBoxKey, true)) DrawBox(p);
            DrawGuides(p);
            DrawLines(p);
            if (EditorPrefs.GetBool(ShowLabelsKey, true)) DrawLabels(w, p);
            if (p.canPlay) DrawPreviewGhosts(w, p, sv);
        }
        DrawPointHandles(p);

        Handles.color = oldColor;
        Handles.zTest = oldZ;
    }

    private void DrawBox(PathInfo p)
    {
        float x0 = EditorPrefs.GetFloat(PlayAreaGuide.MinXKey, -30f);
        float x1 = EditorPrefs.GetFloat(PlayAreaGuide.MaxXKey, 30f);
        float y0 = EditorPrefs.GetFloat(PlayAreaGuide.MinYKey, -15f);
        float y1 = EditorPrefs.GetFloat(PlayAreaGuide.MaxYKey, 15f);
        float zNear = PlayZ();
        float zFar = zNear + 10f;
        foreach (var v in p.pos) zFar = Mathf.Max(zFar, v.z);

        Handles.color = new Color(0.3f, 0.9f, 0.9f, 0.45f);
        Handles.DrawLine(new Vector3(x0, y0, zNear), new Vector3(x0, y0, zFar));
        Handles.DrawLine(new Vector3(x1, y0, zNear), new Vector3(x1, y0, zFar));
        Handles.DrawLine(new Vector3(x0, y1, zNear), new Vector3(x0, y1, zFar));
        Handles.DrawLine(new Vector3(x1, y1, zNear), new Vector3(x1, y1, zFar));
        Frame(x0, x1, y0, y1, zFar);
        Handles.color = new Color(1f, 0.9f, 0.1f, 1f);
        Frame(x0, x1, y0, y1, zNear);
        Handles.Label(new Vector3(x0, y1, zNear), "  Ship plane", labelStyle);
    }

    private static void Frame(float x0, float x1, float y0, float y1, float z)
    {
        Handles.DrawPolyLine(new Vector3(x0, y0, z), new Vector3(x1, y0, z), new Vector3(x1, y1, z), new Vector3(x0, y1, z), new Vector3(x0, y0, z));
    }

    private void DrawGuides(PathInfo p)
    {
        float floorY = EditorPrefs.GetFloat(PlayAreaGuide.MinYKey, -15f);
        float playZ = PlayZ();
        bool drop = EditorPrefs.GetBool(ShowDropKey, true);
        bool proj = EditorPrefs.GetBool(ShowProjKey, true);
        if (!drop && !proj) return;

        var floorPts = new Vector3[p.pos.Length];
        var projPts = new Vector3[p.pos.Length];
        for (int j = 0; j < p.pos.Length; j++)
        {
            Vector3 v = p.pos[j];
            float s = HandleUtility.GetHandleSize(v) * 0.08f;
            floorPts[j] = new Vector3(v.x, floorY, v.z);
            projPts[j] = new Vector3(v.x, v.y, playZ);
            if (drop)
            {
                Handles.color = new Color(1f, 1f, 1f, 0.35f);
                Handles.DrawDottedLine(v, floorPts[j], 4f);
                Handles.DrawWireDisc(floorPts[j], Vector3.up, s);
            }
            if (proj)
            {
                Handles.color = new Color(0.3f, 0.9f, 1f, 0.55f);
                Handles.DrawDottedLine(v, projPts[j], 4f);
                Handles.DrawWireDisc(projPts[j], Vector3.forward, s);
            }
        }
        if (drop) { Handles.color = new Color(1f, 1f, 1f, 0.2f); Handles.DrawAAPolyLine(2f, floorPts); }
        if (proj) { Handles.color = new Color(0.3f, 0.9f, 1f, 0.45f); Handles.DrawAAPolyLine(2f, projPts); }
    }

    private void DrawLines(PathInfo p)
    {
        float maxS = 0f;
        foreach (float s in p.segSpeed) maxS = Mathf.Max(maxS, s);
        for (int i = 0; i <= p.N; i++)
        {
            Vector3 a = p.pos[i], b = p.pos[i + 1];
            float k = maxS > 0f ? Mathf.Clamp01(p.segSpeed[i] / maxS) : 1f;
            Handles.color = Color.Lerp(new Color(0.3f, 0.12f, 0f), new Color(1f, 0.85f, 0.1f), k);
            Handles.DrawAAPolyLine(5f, a, b);
            Vector3 dir = b - a;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 mid = (a + b) * 0.5f;
                float s = HandleUtility.GetHandleSize(mid) * 0.15f;
                Handles.ConeHandleCap(0, mid, Quaternion.LookRotation(dir.normalized), s, UnityEngine.EventType.Repaint);
            }
        }
    }

    private void DrawLabels(EnemyWaveInfo w, PathInfo p)
    {
        int[] order = A(w.WaypointOrder);
        for (int j = 0; j < p.pos.Length; j++)
        {
            Vector3 v = p.pos[j];
            string text;
            if (j == 0) text = "S  spawn";
            else if (j == p.N + 1) text = $"E  end  @ {p.arrive[j]:0.0}s";
            else
            {
                text = $"{j}   spd {p.segSpeed[j - 1]:0.##}   wait {p.wait[j - 1]:0.##}s   @ {p.arrive[j]:0.0}s";
                if (OtherUsers(1, order[j - 1], w) > 0) text += "   (SHARED)";
            }
            Handles.Label(v + Vector3.up * HandleUtility.GetHandleSize(v) * 0.2f, text, labelStyle);
        }
    }

    private void DrawPointHandles(PathInfo p)
    {
        for (int j = 0; j < p.pos.Length; j++)
        {
            Vector3 v = p.pos[j];
            Color c = j == 0 ? new Color(0.3f, 1f, 0.4f) : (j == p.N + 1 ? new Color(1f, 0.3f, 0.3f) : Color.white);
            if (j == activePt) c = new Color(1f, 0.95f, 0.2f);
            Handles.color = c;
            float s = HandleUtility.GetHandleSize(v) * 0.09f;
            if (Handles.Button(v, Quaternion.identity, s, s * 1.6f, Handles.SphereHandleCap))
            {
                activePt = j;
                Repaint();
            }
        }

        if (EditorPrefs.GetBool(ShowHandlesKey, true) && activePt >= 0 && activePt < p.pos.Length && !EditorApplication.isPlaying)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 np = Handles.PositionHandle(p.pos[activePt], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Transform t = p.tr[activePt];
                Undo.RecordObject(t, "Move wave point");
                t.position = np;
                Repaint();
            }
        }
    }

    private void DrawPreviewGhosts(EnemyWaveInfo w, PathInfo p, SceneView sv)
    {
        GameObject[] pf = A(w.EnemyPrefabs);
        float[] gaps = A(w.TimesBetweenSpawns);
        bool alive;
        if (EditorPrefs.GetBool(AllEnemiesKey, true) && pf.Length > 0)
        {
            float off = 0f;
            for (int k = 0; k < pf.Length; k++)
            {
                off += k < gaps.Length ? Mathf.Max(0f, gaps[k]) : 0f;
                float tt = previewTime - off;
                if (tt < 0f) continue;
                Vector3 v = p.Eval(tt, out alive);
                if (alive) DrawGhost(pf[k], v, sv, "#" + (k + 1));
            }
        }
        else
        {
            GameObject pre = pf.Length > 0 ? pf[Mathf.Clamp(previewEnemy, 0, pf.Length - 1)] : null;
            Vector3 v = p.Eval(previewTime, out alive);
            if (alive) DrawGhost(pre, v, sv, "");
        }
    }

    private void DrawGhost(GameObject prefab, Vector3 pos, SceneView sv, string label)
    {
        if (EditorPrefs.GetBool(ShowGhostKey, true) && sv.camera != null)
        {
            var parts = GetGhost(prefab);
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part.mats == null || part.mats.Length == 0) continue;
                    Matrix4x4 m = Matrix4x4.Translate(pos) * part.local;
                    for (int s = 0; s < part.mesh.subMeshCount; s++)
                    {
                        Material mat = part.mats[Mathf.Min(s, part.mats.Length - 1)];
                        if (mat != null) Graphics.DrawMesh(part.mesh, m, mat, 0, sv.camera, s);
                    }
                }
            }
        }
        float size = HandleUtility.GetHandleSize(pos) * 0.12f;
        Handles.color = new Color(1f, 0.45f, 0.1f, 1f);
        Handles.DrawWireDisc(pos, sv.camera.transform.forward, size);
        if (!string.IsNullOrEmpty(label)) Handles.Label(pos + Vector3.up * size * 1.5f, label, labelStyle);
    }

    private List<GhostPart> GetGhost(GameObject prefab)
    {
        if (prefab == null) return null;
        List<GhostPart> cached;
        if (ghostCache.TryGetValue(prefab, out cached)) return cached;
        var list = new List<GhostPart>();
        Matrix4x4 rootScale = Matrix4x4.Scale(prefab.transform.localScale);
        Matrix4x4 w2l = prefab.transform.worldToLocalMatrix;
        foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            list.Add(new GhostPart { mesh = mf.sharedMesh, mats = mr.sharedMaterials, local = rootScale * (w2l * mf.transform.localToWorldMatrix) });
        }
        ghostCache[prefab] = list;
        return list;
    }

    // ------------------------------------------------------------------ window GUI

    private static bool Fold(string key, string title, bool def)
    {
        bool v = EditorPrefs.GetBool(key, def);
        bool nv = EditorGUILayout.Foldout(v, title, true, EditorStyles.foldoutHeader);
        if (nv != v) EditorPrefs.SetBool(key, nv);
        return nv;
    }

    private static void PrefToggle(string label, string key, bool def)
    {
        bool v = EditorPrefs.GetBool(key, def);
        bool nv = EditorGUILayout.ToggleLeft(label, v);
        if (nv != v) { EditorPrefs.SetBool(key, nv); SceneView.RepaintAll(); }
    }

    private void OnGUI()
    {
        if (!EnsureSpawner())
        {
            EditorGUILayout.HelpBox("No SpawnEnemy component found in the loaded scene.", MessageType.Warning);
            return;
        }

        bool play = EditorApplication.isPlaying;
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField($"Scene: {spawner.gameObject.scene.name}   |   waves: {pWaves.arraySize}   |   waypoints: {pWps.arraySize}", EditorStyles.miniLabel);
        if (play) EditorGUILayout.HelpBox("Editing is locked in Play Mode. The preview still works.", MessageType.Info);

        int count = pWaves.arraySize;
        int sel = Mathf.Clamp(Selected, 0, Mathf.Max(0, count - 1));

        if (count > 0)
        {
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                var wi = GetWave(i);
                names[i] = i + ": " + (wi != null ? wi.name : "(empty)");
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(26))) sel = (sel + count - 1) % count;
            sel = EditorGUILayout.Popup(sel, names);
            if (GUILayout.Button(">", GUILayout.Width(26))) sel = (sel + 1) % count;
            EditorGUILayout.EndHorizontal();
            if (sel != Selected)
            {
                Selected = sel;
                activePt = -1;
                previewTime = 0f;
                SceneView.RepaintAll();
            }
        }

        EnemyWaveInfo w = GetWave(sel);

        using (new EditorGUI.DisabledScope(play))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New wave")) { NewWave(); GUIUtility.ExitGUI(); }
            using (new EditorGUI.DisabledScope(w == null))
            {
                if (GUILayout.Button("Duplicate wave")) { DuplicateWave(sel, w); GUIUtility.ExitGUI(); }
                if (GUILayout.Button("Select its triggers"))
                {
                    var objs = new List<Object>();
                    foreach (var t in Object.FindObjectsByType<EnemySpawnTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        var pr = new SerializedObject(t).FindProperty("waveNumber");
                        if (pr != null && pr.intValue == sel) objs.Add(t.gameObject);
                    }
                    Selection.objects = objs.ToArray();
                }
                if (GUILayout.Button("Ping", GUILayout.Width(40))) EditorGUIUtility.PingObject(w);
            }
            EditorGUILayout.EndHorizontal();

            if (w != null)
            {
                EditorGUI.BeginChangeCheck();
                var nw = EditorGUILayout.ObjectField("Wave asset", w, typeof(EnemyWaveInfo), false) as EnemyWaveInfo;
                if (EditorGUI.EndChangeCheck() && nw != null)
                {
                    pWaves.GetArrayElementAtIndex(sel).objectReferenceValue = nw;
                    spSo.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }
            }
        }

        if (w == null)
        {
            EditorGUILayout.HelpBox(count == 0 ? "This scene has no waves yet. Click 'New wave'." : "This wave slot is empty.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        PathInfo p = BuildPath(w);
        cachedTotal = TotalTime(w, p);
        List<string> others = OtherScenes(w);
        bool shared = others.Count > 0;

        if (shared)
        {
            EditorGUILayout.HelpBox("This wave asset is also used in: " + string.Join(", ", others) +
                ".\nIndices differ per scene, so adding / removing / reordering points is locked here. " +
                "Make a scene-specific copy first.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(play))
            {
                if (GUILayout.Button("Make scene-specific copy (with its own points)", GUILayout.Height(24)))
                {
                    BeginOp("Make scene-specific wave");
                    MakeSceneSpecific(sel, w);
                    EndOp();
                    GUIUtility.ExitGUI();
                }
            }
        }

        foreach (string pr in p.problems) EditorGUILayout.HelpBox(pr, MessageType.Warning);
        if (p.lengthIssue)
            using (new EditorGUI.DisabledScope(play))
                if (GUILayout.Button("Fix array lengths (pad / trim)")) { FixLengths(w); GUIUtility.ExitGUI(); }

        using (new EditorGUI.DisabledScope(play))
        {
            if (Fold("EnemyWaves.Fold.Settings", "Wave settings & enemies", true)) DrawSettings(w, p);
            if (Fold("EnemyWaves.Fold.Points", $"Path points ({p.N} waypoints)", true)) DrawPoints(w, p, shared, sel);
        }
        if (Fold("EnemyWaves.Fold.Preview", "Preview", true)) DrawPreviewUI(w, p);
        if (Fold("EnemyWaves.Fold.Display", "Scene display options", false)) DrawDisplayUI();
        using (new EditorGUI.DisabledScope(play))
        {
            if (Fold("EnemyWaves.Fold.Tools", "Fix / clean-up tools", false)) DrawToolsUI(w, shared);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSettings(EnemyWaveInfo w, PathInfo p)
    {
        EditorGUI.BeginChangeCheck();
        float ns = EditorGUILayout.FloatField(new GUIContent("Enemy speed", "Base speed (used when a point's speed is 0, and after the last waypoint)."), w.EnemySpeed);
        if (EditorGUI.EndChangeCheck()) Edit(w, so => so.FindProperty("EnemySpeed").floatValue = ns);

        EditorGUILayout.LabelField("Enemies (delay = seconds before this enemy spawns)", EditorStyles.miniBoldLabel);
        var pf = new List<GameObject>(A(w.EnemyPrefabs));
        var gaps = new List<float>(A(w.TimesBetweenSpawns));
        int n = Mathf.Min(pf.Count, gaps.Count);

        using (new EditorGUI.DisabledScope(p.lengthIssue))
        {
            for (int k = 0; k < n; k++)
            {
                int kk = k;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("#" + (k + 1), GUILayout.Width(26));
                EditorGUI.BeginChangeCheck();
                var np = EditorGUILayout.ObjectField(pf[k], typeof(GameObject), false, GUILayout.MinWidth(80)) as GameObject;
                if (EditorGUI.EndChangeCheck()) Edit(w, so => so.FindProperty("EnemyPrefabs").GetArrayElementAtIndex(kk).objectReferenceValue = np);
                GUILayout.Label("delay", GUILayout.Width(34));
                EditorGUI.BeginChangeCheck();
                float ng = EditorGUILayout.FloatField(gaps[k], GUILayout.Width(46));
                if (EditorGUI.EndChangeCheck()) Edit(w, so => so.FindProperty("TimesBetweenSpawns").GetArrayElementAtIndex(kk).floatValue = Mathf.Max(0f, ng));
                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    BeginOp("Remove enemy");
                    pf.RemoveAt(k);
                    gaps.RemoveAt(k);
                    Edit(w, so => { SetObjs(so.FindProperty("EnemyPrefabs"), pf); SetFloats(so.FindProperty("TimesBetweenSpawns"), gaps); });
                    EndOp();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add enemy"))
            {
                BeginOp("Add enemy");
                pf.Add(pf.Count > 0 ? pf[pf.Count - 1] : null);
                gaps.Add(1f);
                Edit(w, so => { SetObjs(so.FindProperty("EnemyPrefabs"), pf); SetFloats(so.FindProperty("TimesBetweenSpawns"), gaps); });
                EndOp();
                GUIUtility.ExitGUI();
            }
        }
    }

    private void DrawPoints(EnemyWaveInfo w, PathInfo p, bool shared, int sel)
    {
        EditorGUILayout.LabelField("Line colour: darker = slower. Click a dot in the Scene view to select and move it.", EditorStyles.miniLabel);
        int n = p.N;
        int[] order = A(w.WaypointOrder);
        float[] spd = A(w.WaypointSpeeds);
        float[] wt = A(w.WaitTimes);
        Transform group = FindGroup(GroupName(w));

        for (int j = 0; j < p.tr.Length; j++)
        {
            Transform t = p.tr[j];
            bool isWp = j >= 1 && j <= n;
            int kind = j == 0 ? 0 : (isWp ? 1 : 2);
            int idx = j == 0 ? w.SpawnPointIndex : (isWp ? order[j - 1] : w.EndPointIndex);
            int k = j - 1;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            var prevBg = GUI.backgroundColor;
            if (j == activePt) GUI.backgroundColor = new Color(1f, 0.9f, 0.3f);
            string tag = j == 0 ? "S" : (j == n + 1 ? "E" : j.ToString());
            if (GUILayout.Button(tag, GUILayout.Width(28))) { activePt = j; SceneView.RepaintAll(); }
            GUI.backgroundColor = prevBg;

            string status = "";
            if (t != null)
            {
                int users = OtherUsers(kind, idx, w);
                bool owned = group != null && t.parent == group;
                if (owned) status = users > 0 ? "  SHARED with " + users : "";
                else status = users > 0 ? "  (shared with " + users + ", not tool-owned)" : "  (not tool-owned)";
            }
            EditorGUILayout.LabelField(t != null ? t.name + status : "(missing)", GUILayout.MinWidth(60));

            if (t != null && GUILayout.Button("Sel", GUILayout.Width(32))) { Selection.activeTransform = t; EditorGUIUtility.PingObject(t); }

            if (isWp)
            {
                using (new EditorGUI.DisabledScope(p.lengthIssue))
                {
                    GUILayout.Label("spd", GUILayout.Width(26));
                    EditorGUI.BeginChangeCheck();
                    float sv = EditorGUILayout.FloatField(k < spd.Length ? spd[k] : 0f, GUILayout.Width(40));
                    if (EditorGUI.EndChangeCheck()) Edit(w, so => so.FindProperty("WaypointSpeeds").GetArrayElementAtIndex(k).floatValue = sv);
                    GUILayout.Label("wait", GUILayout.Width(28));
                    EditorGUI.BeginChangeCheck();
                    float wv = EditorGUILayout.FloatField(k < wt.Length ? wt[k] : 0f, GUILayout.Width(40));
                    if (EditorGUI.EndChangeCheck()) Edit(w, so => so.FindProperty("WaitTimes").GetArrayElementAtIndex(k).floatValue = Mathf.Max(0f, wv));
                }
            }

            using (new EditorGUI.DisabledScope(shared || p.lengthIssue))
            {
                if (isWp)
                {
                    if (k > 0 && GUILayout.Button("▲", GUILayout.Width(22))) { MoveWaypoint(w, k, -1); GUIUtility.ExitGUI(); }
                    if (k < n - 1 && GUILayout.Button("▼", GUILayout.Width(22))) { MoveWaypoint(w, k, 1); GUIUtility.ExitGUI(); }
                }
                if (j <= n && GUILayout.Button(new GUIContent("+", "Insert a new waypoint after this point"), GUILayout.Width(22)))
                {
                    AddWaypoint(w, j);
                    GUIUtility.ExitGUI();
                }
                if (isWp && GUILayout.Button("X", GUILayout.Width(22)))
                {
                    RemoveWaypoint(w, k);
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (t != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 np = EditorGUILayout.Vector3Field(GUIContent.none, t.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Move wave point");
                    t.position = np;
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.EndVertical();
        }

        using (new EditorGUI.DisabledScope(shared || p.lengthIssue))
        {
            if (GUILayout.Button("Add waypoint (before the end point)", GUILayout.Height(22)))
            {
                AddWaypoint(w, n);
                GUIUtility.ExitGUI();
            }
        }
    }

    private void DrawPreviewUI(EnemyWaveInfo w, PathInfo p)
    {
        if (!p.canPlay)
        {
            EditorGUILayout.HelpBox("Preview needs a complete path and Enemy speed > 0.", MessageType.Info);
            return;
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(playing ? "Pause" : "Play", GUILayout.Height(24)))
        {
            playing = !playing;
            if (playing && previewTime >= cachedTotal) previewTime = 0f;
            lastTick = EditorApplication.timeSinceStartup;
        }
        if (GUILayout.Button("Stop", GUILayout.Height(24), GUILayout.Width(60)))
        {
            playing = false;
            previewTime = 0f;
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        previewTime = EditorGUILayout.Slider("Time (s)", previewTime, 0f, Mathf.Max(0.1f, cachedTotal));
        if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

        float ts = EditorPrefs.GetFloat(SpeedKey, 1f);
        float nts = EditorGUILayout.Slider("Preview speed", ts, 0.1f, 4f);
        if (!Mathf.Approximately(ts, nts)) EditorPrefs.SetFloat(SpeedKey, nts);

        PrefToggle("Show enemy ghost (prefab mesh)", ShowGhostKey, true);
        PrefToggle("Whole wave (each enemy staggered by its delay)", AllEnemiesKey, true);
        if (!EditorPrefs.GetBool(AllEnemiesKey, true))
        {
            var pf = A(w.EnemyPrefabs);
            if (pf.Length > 0)
            {
                string[] names = new string[pf.Length];
                for (int i = 0; i < pf.Length; i++) names[i] = "#" + (i + 1) + " " + (pf[i] != null ? pf[i].name : "(empty)");
                previewEnemy = EditorGUILayout.Popup("Enemy", Mathf.Clamp(previewEnemy, 0, pf.Length - 1), names);
            }
        }
        EditorGUILayout.LabelField($"Flight time per enemy: {p.duration:0.0}s   |   whole wave: {cachedTotal:0.0}s", EditorStyles.miniLabel);
    }

    private void DrawDisplayUI()
    {
        PrefToggle("Play-area box + ship plane", ShowBoxKey, true);
        PrefToggle("Drop lines to the floor", ShowDropKey, true);
        PrefToggle("Projection onto the ship plane (what the player sees)", ShowProjKey, true);
        PrefToggle("Labels (speed, wait, arrival time)", ShowLabelsKey, true);
        PrefToggle("Move handle on the selected point", ShowHandlesKey, true);
    }

    private void DrawToolsUI(EnemyWaveInfo w, bool shared)
    {
        using (new EditorGUI.DisabledScope(shared))
        {
            if (GUILayout.Button("Make THIS wave's points unique"))
            {
                BeginOp("Make wave points unique");
                MakeUnique(w);
                EndOp();
                GUIUtility.ExitGUI();
            }
        }
        if (GUILayout.Button("Make ALL waves' points unique (this scene)"))
        {
            BeginOp("Make all wave points unique");
            for (int i = 0; i < pWaves.arraySize; i++)
            {
                var o = GetWave(i);
                if (o == null || OtherScenes(o).Count > 0) continue;
                MakeUnique(o);
            }
            EndOp();
            GUIUtility.ExitGUI();
        }
        if (GUILayout.Button("Fix everything in this scene (copies + unique points)"))
        {
            FixAll();
            GUIUtility.ExitGUI();
        }
        if (GUILayout.Button("Re-check other scenes"))
        {
            sharedCache.Clear();
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.HelpBox("'Make ALL' skips waves that are shared with other scenes (use 'Fix everything' for those).", MessageType.None);
    }
}