using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(EnemyHitFeedback))]
public class EnemyHitFeedbackEditor : Editor
{
    bool playing, loop = true;
    float time;
    double last;

    void OnEnable() { EditorApplication.update += Tick; }
    void OnDisable()
    {
        EditorApplication.update -= Tick;
        if (target != null) ((EnemyHitFeedback)target).PreviewEnd();
    }

    void Tick()
    {
        if (!playing || target == null || Application.isPlaying) return;
        var f = (EnemyHitFeedback)target;
        double now = EditorApplication.timeSinceStartup;
        time += (float)(now - last);
        last = now;
        float total = f.duration * f.flashCount + f.flashGap * (f.flashCount - 1);
        if (time > total + 0.5f)
        {
            time = 0f;
            if (!loop) { playing = false; f.PreviewEnd(); SceneView.RepaintAll(); Repaint(); return; }
        }
        float cycle = f.duration + f.flashGap;
        int fi = Mathf.Clamp(Mathf.FloorToInt(time / Mathf.Max(0.02f, cycle)), 0, f.flashCount - 1);
        float local = time - fi * cycle;
        float w = (time <= total && local <= f.duration) ? Mathf.Clamp01(f.fade.Evaluate(local / Mathf.Max(0.02f, f.duration))) : 0f;
        f.PreviewSet(w);
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var f = (EnemyHitFeedback)target;
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Preview (no Play needed)", EditorStyles.boldLabel);
        loop = EditorGUILayout.Toggle("Loop", loop);
        if (GUILayout.Button(playing ? "Stop preview" : "Play flash", GUILayout.Height(24)))
        {
            playing = !playing;
            time = 0f;
            last = EditorApplication.timeSinceStartup;
            if (!playing) { f.PreviewEnd(); SceneView.RepaintAll(); }
        }
        EditorGUILayout.HelpBox("Change Mode, colour or duration while it loops to see the result live.", MessageType.None);
    }
}

[InitializeOnLoad]
public static class HitFeedbackPreviewGuard
{
    static HitFeedbackPreviewGuard()
    {
        EditorSceneManager.sceneSaving += (s, p) => RestoreAll();
        EditorApplication.playModeStateChanged += st => { if (st == PlayModeStateChange.ExitingEditMode) RestoreAll(); };
        AssemblyReloadEvents.beforeAssemblyReload += RestoreAll;
    }

    static void RestoreAll()
    {
        foreach (var f in Object.FindObjectsByType<EnemyHitFeedback>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            f.PreviewEnd();
    }
}