using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>Editor-only. Whole-level + section progress for the Scene view, the Game view and the Level Manager window. Seconds respect per-section speed.</summary>
[InitializeOnLoad]
public static class LevelProgressOverlay
{
    public const string ShowKey = "LevelManager.OverallProgress.Show";            // Scene view overlay
    public const string ModeKey = "LevelManager.OverallProgress.Mode";            // 0 = %, 1 = seconds, 2 = both (Level Manager window only)
    public const string ShowSectionKey = "LevelManager.OverallProgress.ShowSection";
    public const string ShowGameKey = "LevelManager.OverallProgress.ShowGame";    // Game view overlay

    private struct Seg { public float s, e, m; }

    private static float progress = -1f;
    private static float timeProg = -1f;
    private static float totalLength;
    private static float speed;
    private static float wholeTotalT, wholeDoneT;
    private static float secTotalT, secDoneT;
    private static bool hasRange;
    private static bool hasSection;
    private static string sectionName = "";
    private static float secProgress;
    private static float secTimeProg;
    private static double nextTime;
    private static double nextEditTime;
    private static readonly List<Seg> segs = new List<Seg>();

    public static float Progress { get { return progress; } }
    public static float SecProgress { get { return hasSection ? secProgress : -1f; } }

    static LevelProgressOverlay()
    {
        SceneView.duringSceneGui -= OnScene;
        SceneView.duringSceneGui += OnScene;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static int Mode() { return Mathf.Clamp(EditorPrefs.GetInt(ModeKey, 1), 0, 2); }

    private static void Tick()
    {
        bool show = EditorPrefs.GetBool(ShowKey, false);
        bool playing = EditorApplication.isPlaying;
        if (!playing)
        {
            progress = -1f;
            timeProg = -1f;
            hasSection = false;
            LevelProgressData.active = false;
        }
        if (!show && !playing) return;

        double now = EditorApplication.timeSinceStartup;
        if (now < nextTime) return;
        nextTime = now + (playing ? 0.1 : 1.0);

        Recompute(playing);
        if (playing)
        {
            ManageGameOverlay();
            PushData();
        }
        if (show) SceneView.RepaintAll();
    }

    // Called by the Level Manager window so the numbers are also fresh in Edit Mode.
    public static void EnsureFresh()
    {
        if (EditorApplication.isPlaying) return;
        double now = EditorApplication.timeSinceStartup;
        if (now < nextEditTime) return;
        nextEditTime = now + 1.0;
        Recompute(false);
    }

    private static void ManageGameOverlay()
    {
        bool want = EditorPrefs.GetBool(ShowGameKey, true);
        var comp = Object.FindFirstObjectByType<LevelProgressGameOverlay>();
        if (want && comp == null)
        {
            var go = new GameObject("[Level Manager] Progress overlay");
            go.hideFlags = HideFlags.HideInHierarchy;
            go.AddComponent<LevelProgressGameOverlay>();
            Debug.Log("Level Manager: [Overlay] Game view progress overlay started (untick it in the Section tracker foldout to hide it).");
        }
        else if (!want && comp != null) Object.Destroy(comp.gameObject);
    }

    // Game view: bar 1 = whole level, bar 2 = current section (seconds, filling up).
    private static void PushData()
    {
        LevelProgressData.active = EditorPrefs.GetBool(ShowGameKey, true) && hasRange;
        LevelProgressData.showSection = EditorPrefs.GetBool(ShowSectionKey, true);
        LevelProgressData.twoBars = false;
        LevelProgressData.wholeProg = timeProg >= 0f ? timeProg : progress;
        LevelProgressData.wholeTimeProg = -1f;
        LevelProgressData.secProg = hasSection ? secTimeProg : -1f;
        LevelProgressData.wholeText = GameWholeText();
        LevelProgressData.wholeTimeText = "";
        LevelProgressData.sectionText = GameSectionText();
    }

    // Base speed. While playing, the speed controller changes moveSpeed, so ask it for the original value.
    private static float ReadSpeed()
    {
        if (EditorApplication.isPlaying)
        {
            var c = Object.FindFirstObjectByType<LevelSpeedController>();
            if (c != null && c.BaseSpeed > 0.0001f) return c.BaseSpeed;
        }
        foreach (var m in Object.FindObjectsByType<MoveTrack>(FindObjectsSortMode.None))
        {
            if (m == null || !m.enabled) continue;
            var p = new SerializedObject(m).FindProperty("moveSpeed");
            if (p != null) return Mathf.Abs(p.floatValue);
        }
        return 0f;
    }

    // Seconds needed to travel from Z a to Z b, with the per-section speed multipliers.
    private static float TimeBetween(float a, float b)
    {
        if (b <= a || speed < 0.0001f) return 0f;
        float t = (b - a) / speed;
        foreach (var g in segs)
        {
            float ov = Mathf.Min(b, g.e) - Mathf.Max(a, g.s);
            if (ov > 0f) t += ov * (1f / (speed * g.m) - 1f / speed);
        }
        return t;
    }

    private static void Recompute(bool playing)
    {
        var secs = LevelManagerWindow.GetSortedSections();
        hasRange = false;
        hasSection = false;
        speed = ReadSpeed();
        if (secs.Count == 0) { progress = -1f; timeProg = -1f; return; }

        segs.Clear();
        foreach (var s in secs)
        {
            var sp = s.GetComponent<SectionSpeed>();
            if (sp == null || Mathf.Approximately(sp.speedMultiplier, 1f)) continue;
            segs.Add(new Seg { s = s.StartZ, e = s.EndZ, m = Mathf.Max(0.05f, sp.speedMultiplier) });
        }

        float start = secs[0].StartZ;
        float end;
        var win = Object.FindFirstObjectByType<WinTrigger>();
        if (win != null) end = win.transform.position.z;
        else end = secs[secs.Count - 1].EndZ;
        if (end - start < 0.5f) { progress = -1f; timeProg = -1f; return; }

        hasRange = true;
        totalLength = end - start;
        wholeTotalT = TimeBetween(start, end);
        if (!playing) { progress = -1f; timeProg = -1f; return; }

        var pts = LevelManagerWindow.GetSpawnPoints();
        float pz = 0f;
        if (pts.Count > 0)
        {
            float sum = 0f;
            foreach (var t in pts) sum += t.position.z;
            pz = sum / pts.Count;
        }
        progress = Mathf.Clamp01((pz - start) / totalLength);
        wholeDoneT = TimeBetween(start, Mathf.Clamp(pz, start, end));
        timeProg = wholeTotalT > 0.0001f ? Mathf.Clamp01(wholeDoneT / wholeTotalT) : progress;

        foreach (var s in secs)
        {
            if (pz >= s.StartZ && pz < s.EndZ && s.Length > 0.01f)
            {
                hasSection = true;
                sectionName = $"#{s.sectionNumber} {s.sectionLabel}";
                secProgress = Mathf.Clamp01((pz - s.StartZ) / s.Length);
                secTotalT = TimeBetween(s.StartZ, s.EndZ);
                secDoneT = TimeBetween(s.StartZ, pz);
                secTimeProg = secTotalT > 0.0001f ? Mathf.Clamp01(secDoneT / secTotalT) : secProgress;
                break;
            }
        }
    }

    private static string Sec(float done, float total)
    {
        float left = Mathf.Max(0f, total - done);
        return $"{done:F0}s / {total:F0}s  ({left:F0}s left)";
    }

    // ---- texts for the Game view + Scene view

    public static string GameWholeText()
    {
        if (!hasRange) return "Whole level: no sections found";
        if (speed < 0.0001f) return $"Whole level {Mathf.RoundToInt(progress * 100f)}%  (no MoveTrack speed found)";
        return $"Whole level {Mathf.RoundToInt(progress * 100f)}%   {Sec(wholeDoneT, wholeTotalT)}";
    }

    public static string GameSectionText()
    {
        if (!hasSection) return "Section: player is not inside a section";
        if (speed < 0.0001f) return $"Section {sectionName} {Mathf.RoundToInt(secProgress * 100f)}%";
        return $"Section {sectionName}: {Sec(secDoneT, secTotalT)}";
    }

    // ---- texts for the Level Manager window (follow the "Show progress as" dropdown)

    private static string Txt(string label, float prog, float doneT, float totalT, int mode)
    {
        int pct = Mathf.RoundToInt(prog * 100f);
        if (speed < 0.0001f) return $"{label} {pct}%  (no MoveTrack speed found)";
        string sec = Sec(doneT, totalT);
        if (mode == 0) return $"{label} {pct}%";
        if (mode == 1) return $"{label} {sec}";
        return $"{label} {pct}%   {sec}";
    }

    public static string WholeText()
    {
        int mode = Mode();
        if (!hasRange) return "Whole level: no sections found";
        if (progress < 0f)
        {
            if (speed > 0.0001f && mode != 0) return $"Whole level: {wholeTotalT:F0}s total ({totalLength:F0} units)";
            return $"Whole level: {totalLength:F0} units";
        }
        return Txt("Whole level", progress, wholeDoneT, wholeTotalT, mode);
    }

    public static string SectionText()
    {
        if (!hasSection) return EditorApplication.isPlaying ? "Section: player is not inside a section" : "Section: (shows in Play Mode)";
        return Txt("Section " + sectionName, secProgress, secDoneT, secTotalT, Mode());
    }

    private static void SettingToggle(string label, string key, bool def)
    {
        bool v = EditorPrefs.GetBool(key, def);
        bool nv = EditorGUILayout.ToggleLeft(label, v);
        if (nv != v) { EditorPrefs.SetBool(key, nv); SceneView.RepaintAll(); }
    }

    /// <summary>The settings shown in the Level Manager window (Section tracker foldout).</summary>
    public static void DrawSettings()
    {
        int mode = Mode();
        int nm = EditorGUILayout.Popup("Show progress as (this window)", mode, new[] { "% only", "seconds only", "% and seconds" });
        if (nm != mode)
        {
            EditorPrefs.SetInt(ModeKey, nm);
            nextTime = 0;
            SceneView.RepaintAll();
        }
        SettingToggle("Show progress in the Game view (Play Mode)", ShowGameKey, true);
        SettingToggle("Show progress in the Scene view (start to win trigger)", ShowKey, false);
        SettingToggle("Include the section bar", ShowSectionKey, true);
    }

    private static void OnScene(SceneView sv)
    {
        if (!EditorPrefs.GetBool(ShowKey, false)) return;
        if (sv == null || sv.camera == null) return;

        bool showSec = EditorPrefs.GetBool(ShowSectionKey, true);
        bool playing = EditorApplication.isPlaying;
        bool secRow = showSec && playing;

        float w = 420f;
        float h = 8f + 22f + 4f + (secRow ? 26f : 0f) + 6f;
        float sh = sv.camera.pixelHeight / EditorGUIUtility.pixelsPerPoint;

        Handles.BeginGUI();
        Rect r = new Rect(10f, sh - h - 12f, w, h);
        GUI.Box(r, GUIContent.none);

        float y = r.y + 8f;
        Rect lr = new Rect(r.x + 6f, y, w - 12f, 22f);
        if (progress < 0f) EditorGUI.LabelField(lr, WholeText());
        else EditorGUI.ProgressBar(lr, timeProg >= 0f ? timeProg : progress, GameWholeText());
        y += 26f;

        if (secRow)
        {
            Rect sr = new Rect(r.x + 6f, y, w - 12f, 22f);
            if (hasSection) EditorGUI.ProgressBar(sr, secTimeProg, GameSectionText());
            else EditorGUI.LabelField(sr, GameSectionText());
        }
        Handles.EndGUI();
    }
}