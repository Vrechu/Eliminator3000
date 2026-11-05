using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

/// <summary>
/// Editor-only. Draws the area the ships can fly in (floor, ceiling, left and right wall, plus cross-section
/// frames along Z) in the Scene view, so you can place objects inside it. Can also record where the ships
/// actually went while you play and use that as the area.
/// Place in Assets\TutorialInfo\Scripts\Editor\
/// </summary>
[InitializeOnLoad]
public static class PlayAreaGuide
{
    public const string ShowKey = "LevelManager.PlayArea.Show";
    public const string FillKey = "LevelManager.PlayArea.Fill";
    public const string MinXKey = "LevelManager.PlayArea.MinX";
    public const string MaxXKey = "LevelManager.PlayArea.MaxX";
    public const string MinYKey = "LevelManager.PlayArea.MinY";
    public const string MaxYKey = "LevelManager.PlayArea.MaxY";
    public const string SpacingKey = "LevelManager.PlayArea.Spacing";
    public const string RecordKey = "LevelManager.PlayArea.Record";

    // Recording lives in SessionState so it survives the domain reload when you enter Play Mode again.
    private const string RecHasKey = "LevelManager.PlayArea.RecHas";
    private const string RecMinXKey = "LevelManager.PlayArea.RecMinX";
    private const string RecMaxXKey = "LevelManager.PlayArea.RecMaxX";
    private const string RecMinYKey = "LevelManager.PlayArea.RecMinY";
    private const string RecMaxYKey = "LevelManager.PlayArea.RecMaxY";

    private const float PlayViewBehind = 30f;   // in Play Mode the guide is drawn around the ship
    private const float PlayViewAhead = 600f;
    private const int MaxFrames = 300;
    private const double RefreshInterval = 0.5;

    private static double nextRefresh;
    private static float cachedStartZ;
    private static float cachedEndZ = 1000f;
    private static float cachedPlayZ;

    public static bool HasRecording { get { return SessionState.GetBool(RecHasKey, false); } }
    public static float RecMinX { get { return SessionState.GetFloat(RecMinXKey, 0f); } }
    public static float RecMaxX { get { return SessionState.GetFloat(RecMaxXKey, 0f); } }
    public static float RecMinY { get { return SessionState.GetFloat(RecMinYKey, 0f); } }
    public static float RecMaxY { get { return SessionState.GetFloat(RecMaxYKey, 0f); } }

    static PlayAreaGuide()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update -= RecordTick;
        EditorApplication.update += RecordTick;
    }

    [MenuItem("Tools/Toggle Play Area Guide")]
    public static void Toggle()
    {
        EditorPrefs.SetBool(ShowKey, !EditorPrefs.GetBool(ShowKey, false));
        SceneView.RepaintAll();
    }

    // ------------------------------------------------------------------ recording

    private static void RecordTick()
    {
        if (!EditorApplication.isPlaying || !EditorPrefs.GetBool(RecordKey, false)) return;

        var players = PlayerInput.all;
        if (players.Count == 0) return;

        bool has = HasRecording;
        float minX = has ? RecMinX : float.MaxValue;
        float maxX = has ? RecMaxX : float.MinValue;
        float minY = has ? RecMinY : float.MaxValue;
        float maxY = has ? RecMaxY : float.MinValue;

        bool any = false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null) continue;
            Vector3 p = players[i].transform.position;
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
            any = true;
        }
        if (!any) return;

        if (!has || !Mathf.Approximately(minX, RecMinX) || !Mathf.Approximately(maxX, RecMaxX) ||
            !Mathf.Approximately(minY, RecMinY) || !Mathf.Approximately(maxY, RecMaxY))
        {
            SessionState.SetBool(RecHasKey, true);
            SessionState.SetFloat(RecMinXKey, minX);
            SessionState.SetFloat(RecMaxXKey, maxX);
            SessionState.SetFloat(RecMinYKey, minY);
            SessionState.SetFloat(RecMaxYKey, maxY);
        }
    }

    public static void ApplyRecording()
    {
        if (!HasRecording) return;
        EditorPrefs.SetFloat(MinXKey, Round2(RecMinX));
        EditorPrefs.SetFloat(MaxXKey, Round2(RecMaxX));
        EditorPrefs.SetFloat(MinYKey, Round2(RecMinY));
        EditorPrefs.SetFloat(MaxYKey, Round2(RecMaxY));
        EditorPrefs.SetBool(ShowKey, true);
        Debug.Log($"Level Manager: Play area set from the recording: X {Round2(RecMinX):F2} to {Round2(RecMaxX):F2}, Y {Round2(RecMinY):F2} to {Round2(RecMaxY):F2} (ship pivot positions).");
        SceneView.RepaintAll();
    }

    public static void ClearRecording()
    {
        SessionState.SetBool(RecHasKey, false);
        SceneView.RepaintAll();
    }

    private static float Round2(float v) { return Mathf.Round(v * 100f) / 100f; }

    // ------------------------------------------------------------------ Z range

    private static void RefreshZ()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now < nextRefresh) return;
        nextRefresh = now + RefreshInterval;

        float spawnZ = 0f;
        var pts = LevelManagerWindow.GetSpawnPoints();
        if (pts.Count > 0)
        {
            float sum = 0f;
            foreach (var t in pts) sum += t.position.z;
            spawnZ = sum / pts.Count;
        }
        cachedPlayZ = spawnZ;

        if (EditorApplication.isPlaying)
        {
            // The level scrolls in Play Mode but the play area is fixed around the ship.
            cachedStartZ = spawnZ - PlayViewBehind;
            cachedEndZ = spawnZ + PlayViewAhead;
            return;
        }

        var sections = Object.FindObjectsByType<LevelSection>(FindObjectsSortMode.None);
        if (sections.Length == 0)
        {
            cachedStartZ = spawnZ - PlayViewBehind;
            cachedEndZ = spawnZ + PlayViewAhead;
            return;
        }

        float min = float.MaxValue, max = float.MinValue;
        foreach (var s in sections)
        {
            if (s == null) continue;
            min = Mathf.Min(min, s.StartZ);
            max = Mathf.Max(max, s.EndZ);
        }
        cachedStartZ = Mathf.Min(min, spawnZ);
        cachedEndZ = Mathf.Max(max, cachedStartZ + 10f);
    }

    // ------------------------------------------------------------------ drawing

    private static void OnSceneGUI(SceneView sv)
    {
        if (UnityEngine.Event.current.type != UnityEngine.EventType.Repaint) return;
        if (!EditorPrefs.GetBool(ShowKey, false)) return;

        float x0 = EditorPrefs.GetFloat(MinXKey, -30f);
        float x1 = EditorPrefs.GetFloat(MaxXKey, 30f);
        float y0 = EditorPrefs.GetFloat(MinYKey, -15f);
        float y1 = EditorPrefs.GetFloat(MaxYKey, 15f);
        if (x1 < x0) { float t = x0; x0 = x1; x1 = t; }
        if (y1 < y0) { float t = y0; y0 = y1; y1 = t; }
        if (x1 - x0 < 0.01f || y1 - y0 < 0.01f) return;

        RefreshZ();
        float z0 = cachedStartZ;
        float z1 = cachedEndZ;
        if (z1 - z0 < 0.1f) return;

        var oldColor = Handles.color;
        var oldZTest = Handles.zTest;

        // Filled faces (hidden behind level geometry so they do not drown the scene).
        if (EditorPrefs.GetBool(FillKey, true))
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Color floorC = new Color(0.2f, 0.9f, 0.4f, 0.10f);
            Color ceilC = new Color(0.2f, 0.7f, 1f, 0.10f);
            Color wallC = new Color(1f, 0.6f, 0.2f, 0.10f);

            Handles.DrawSolidRectangleWithOutline(new[] {
                new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), new Vector3(x1, y0, z1), new Vector3(x0, y0, z1) }, floorC, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {
                new Vector3(x0, y1, z0), new Vector3(x1, y1, z0), new Vector3(x1, y1, z1), new Vector3(x0, y1, z1) }, ceilC, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {
                new Vector3(x0, y0, z0), new Vector3(x0, y1, z0), new Vector3(x0, y1, z1), new Vector3(x0, y0, z1) }, wallC, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {
                new Vector3(x1, y0, z0), new Vector3(x1, y1, z0), new Vector3(x1, y1, z1), new Vector3(x1, y0, z1) }, wallC, Color.clear);
        }

        // Lines are always visible, also through geometry.
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        // Four long edges.
        Handles.color = new Color(1f, 1f, 1f, 0.85f);
        Handles.DrawLine(new Vector3(x0, y0, z0), new Vector3(x0, y0, z1));
        Handles.DrawLine(new Vector3(x1, y0, z0), new Vector3(x1, y0, z1));
        Handles.DrawLine(new Vector3(x0, y1, z0), new Vector3(x0, y1, z1));
        Handles.DrawLine(new Vector3(x1, y1, z0), new Vector3(x1, y1, z1));

        // Cross-section frames along Z.
        float spacing = Mathf.Max(1f, EditorPrefs.GetFloat(SpacingKey, 100f));
        spacing = Mathf.Max(spacing, (z1 - z0) / MaxFrames);
        Handles.color = new Color(1f, 1f, 1f, 0.35f);
        for (float z = z0; z <= z1; z += spacing) DrawFrame(x0, x1, y0, y1, z);
        DrawFrame(x0, x1, y0, y1, z1);

        // The ship plane: where the ship actually is (Play Z).
        Handles.color = new Color(1f, 0.9f, 0.1f, 1f);
        DrawFrame(x0, x1, y0, y1, cachedPlayZ);
        Handles.Label(new Vector3(x0, y1, cachedPlayZ),
            $"  Ship plane (Play Z {cachedPlayZ:F1})   X {x0:F1}..{x1:F1}   Y {y0:F1}..{y1:F1}");

        Handles.color = oldColor;
        Handles.zTest = oldZTest;
    }

    private static void DrawFrame(float x0, float x1, float y0, float y1, float z)
    {
        Handles.DrawPolyLine(
            new Vector3(x0, y0, z), new Vector3(x1, y0, z), new Vector3(x1, y1, z),
            new Vector3(x0, y1, z), new Vector3(x0, y0, z));
    }
}