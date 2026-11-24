using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyTelegraph))]
public class EnemyTelegraphEditor : Editor
{
    bool preview, full;
    float t, speed = 1f;
    double last;

    void OnEnable() { EditorApplication.update += Tick; }
    void OnDisable()
    {
        EditorApplication.update -= Tick;
        if (target != null) ((EnemyTelegraph)target).PreviewEnd();
    }

    void Tick()
    {
        if (!preview || target == null || Application.isPlaying) return;
        var tg = (EnemyTelegraph)target;
        double now = EditorApplication.timeSinceStartup;
        float dt = (float)(now - last) * speed;
        last = now;
        t += dt / Mathf.Max(0.05f, tg.shootWarnTime);
        if (t > 1.15f) t = 0f;   // short pause at full, then loop
        tg.Preview(Mathf.Clamp01(t), dt, full);
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var tg = (EnemyTelegraph)target;
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Preview (no Play needed)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        preview = GUILayout.Toggle(preview, preview ? "Stop preview" : "Start preview", "Button", GUILayout.Height(24));
        if (EditorGUI.EndChangeCheck())
        {
            last = EditorApplication.timeSinceStartup;
            t = 0f;
            if (!preview) { tg.PreviewEnd(); SceneView.RepaintAll(); }
        }
        full = EditorGUILayout.Toggle("Full flash (like kamikaze charge)", full);
        speed = EditorGUILayout.Slider("Preview speed", speed, 0.1f, 3f);
    }
}