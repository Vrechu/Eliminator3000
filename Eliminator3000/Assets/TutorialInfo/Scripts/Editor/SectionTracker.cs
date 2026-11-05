using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor-only. While playing, works out which LevelSection the player is in, logs when it changes,
/// and exposes a live readout (Level Manager window + Scene view overlay).
/// Place in Assets\TutorialInfo\Scripts\Editor\
/// </summary>
[InitializeOnLoad]
public static class SectionTracker
{
    public const string LogPrefKey = "LevelManager.TrackLog";
    public const string OverlayPrefKey = "LevelManager.TrackOverlay";

    private const double TickInterval = 0.2;
    private const double RefreshInterval = 1.0;
    private const string IdleText = "Not playing";

    private static readonly List<LevelSection> sections = new List<LevelSection>();
    private static double nextTick;
    private static double nextRefresh;
    private static int lastIndex = int.MinValue;

    public static string StatusText = IdleText;
    public static float Progress = -1f;

    static SectionTracker()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void ResetState()
    {
        lastIndex = int.MinValue;
        sections.Clear();
        nextRefresh = 0;
        StatusText = EditorApplication.isPlaying ? "Waiting for the player to spawn..." : IdleText;
        Progress = -1f;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode) ResetState();
    }

    // R reloads the scene: start fresh so the first section after a restart is logged again.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single) ResetState();
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying)
        {
            if (StatusText != IdleText) ResetState();
            return;
        }
        double now = EditorApplication.timeSinceStartup;
        if (now < nextTick) return;
        nextTick = now + TickInterval;
        Step(false);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Log Current Section %#l")]
    public static void LogCurrentNow()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.Log("[Section] Not in Play Mode.");
            return;
        }
        Step(true);
    }

    private static void RefreshSections()
    {
        sections.Clear();
        sections.AddRange(LevelManagerWindow.GetSortedSections());
        nextRefresh = EditorApplication.timeSinceStartup + RefreshInterval;
    }

    private static bool TryGetPlayerZ(out float z)
    {
        z = 0f;
        var players = PlayerInput.all;
        float sum = 0f;
        int n = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null) continue;
            sum += players[i].transform.position.z;
            n++;
        }
        if (n > 0) { z = sum / n; return true; }

        // No avatar yet: the ship is static on Z, so the spawn points give the same Z.
        var pts = LevelManagerWindow.GetSpawnPoints();
        if (pts.Count == 0) return false;
        foreach (var t in pts) sum += t.position.z;
        z = sum / pts.Count;
        return true;
    }

    private static void SetStatus(string text, float progress)
    {
        StatusText = text;
        Progress = progress;
    }

    private static void Step(bool force)
    {
        if (!TryGetPlayerZ(out float z))
        {
            SetStatus("Waiting for the player to spawn...", -1f);
            return;
        }

        if (force || sections.Count == 0 || EditorApplication.timeSinceStartup >= nextRefresh || sections.Exists(s => s == null))
            RefreshSections();

        if (sections.Count == 0)
        {
            SetStatus("No LevelSection in the loaded scene", -1f);
            return;
        }

        // A Test Here shift has not been applied yet: do not report the pre-shift position.
        if (SessionState.GetBool(LevelManagerWindow.ShiftPendingKey, false))
        {
            SetStatus("Applying test start...", -1f);
            return;
        }

        int n = sections.Count;
        int idx = int.MinValue;
        for (int i = 0; i < n; i++)
        {
            if (z >= sections[i].StartZ && z < sections[i].EndZ) { idx = i; break; }
        }
        if (idx == int.MinValue)
        {
            if (z < sections[0].StartZ) idx = -1;
            else if (z >= sections[n - 1].EndZ) idx = n;
            else
            {
                // In a gap between two sections: stay in the previous one.
                idx = 0;
                for (int i = 0; i < n; i++) if (sections[i].StartZ <= z) idx = i;
            }
        }

        string text;
        float progress = -1f;
        string logMsg;
        Object ctx = null;

        if (idx == -1)
        {
            text = "Before the first section";
            logMsg = $"[Section] Player is BEFORE the first section (player Z {z:F2}, Section #{sections[0].sectionNumber} starts at {sections[0].StartZ:F2}).";
            ctx = sections[0];
        }
        else if (idx == n)
        {
            LevelSection lastSec = sections[n - 1];
            text = "Past the last section";
            progress = 1f;
            logMsg = $"[Section] Player is PAST the last section (player Z {z:F2}, Section #{lastSec.sectionNumber} '{lastSec.sectionLabel}' ends at {lastSec.EndZ:F2}).";
            ctx = lastSec;
        }
        else
        {
            LevelSection s = sections[idx];
            float len = s.Length;
            progress = len > 0.0001f ? Mathf.Clamp01((z - s.StartZ) / len) : 0f;
            text = $"Section #{s.sectionNumber} '{s.sectionLabel}'  ({Mathf.RoundToInt(progress * 100f)}%)";
            string rootInfo = "";
            if (s.transform.parent != null)
                rootInfo = $" | {s.transform.parent.name} Z {s.transform.parent.position.z:F2}";
            logMsg = $"[Section] Player entered Section #{s.sectionNumber} '{s.sectionLabel}' ({LevelManagerWindow.GetPath(s.transform)}) at {Time.timeSinceLevelLoad:F1} s{rootInfo} | player Z {z:F2}";
            ctx = s;
        }

        SetStatus(text, progress);

        if ((idx != lastIndex || force) && (force || EditorPrefs.GetBool(LogPrefKey, true)))
            Debug.Log(logMsg, ctx);
        lastIndex = idx;
    }

    private static void OnSceneGUI(SceneView sv)
    {
        if (!EditorApplication.isPlaying || !EditorPrefs.GetBool(OverlayPrefKey, true)) return;

        Handles.BeginGUI();
        Rect box = new Rect(10f, sv.position.height - 110f, 360f, 52f);
        GUI.Box(box, GUIContent.none);

        var style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = 13;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(box.x + 8f, box.y + 4f, box.width - 16f, 20f), StatusText, style);
        EditorGUI.ProgressBar(new Rect(box.x + 8f, box.y + 28f, box.width - 16f, 16f), Mathf.Max(0f, Progress), "");
        Handles.EndGUI();
    }
}