using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(PillarDropIn))]
public class PillarDropInEditor : Editor
{
    float progress;
    bool playing;
    double last;

    void OnEnable() { EditorApplication.update += Tick; }
    void OnDisable()
    {
        EditorApplication.update -= Tick;
        if (target != null) ((PillarDropIn)target).PreviewRestore();
    }

    void Tick()
    {
        if (!playing || target == null) return;
        var p = (PillarDropIn)target;
        double now = EditorApplication.timeSinceStartup;
        float dt = (float)(now - last);
        last = now;
        float total = p.dropDuration + p.randomDelay;
        progress += dt / Mathf.Max(0.05f, total);
        if (progress > 1.3f) progress = 0f;   // short pause when landed, then loop
        p.PreviewSet(Mathf.Clamp01(progress));
        SceneView.RepaintAll();
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var p = (PillarDropIn)target;
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Preview (no Play needed)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        float np = EditorGUILayout.Slider("Progress", progress, 0f, 1f);
        if (EditorGUI.EndChangeCheck()) { playing = false; progress = np; p.PreviewSet(progress); SceneView.RepaintAll(); }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(playing ? "Pause" : "Play preview"))
        {
            playing = !playing;
            last = EditorApplication.timeSinceStartup;
        }
        if (GUILayout.Button("Reset (pillars back)"))
        {
            playing = false; progress = 0f;
            p.PreviewRestore();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("Positions are restored when you press Reset, deselect the object, save or press Play.", MessageType.None);
    }
}

// Safety: never leave pillars displaced in a saved scene or when entering Play.
[InitializeOnLoad]
public static class PillarPreviewGuard
{
    static PillarPreviewGuard()
    {
        EditorSceneManager.sceneSaving += (s, path) => RestoreAll();
        EditorApplication.playModeStateChanged += st => { if (st == PlayModeStateChange.ExitingEditMode) RestoreAll(); };
        AssemblyReloadEvents.beforeAssemblyReload += RestoreAll;
    }

    static void RestoreAll()
    {
        foreach (var p in Object.FindObjectsByType<PillarDropIn>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            p.PreviewRestore();
    }
}