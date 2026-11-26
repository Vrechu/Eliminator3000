using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyHealthBar))]
public class EnemyHealthBarEditor : Editor
{
    static bool show = true;
    static float frac = 0.7f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Preview (no Play needed)", EditorStyles.boldLabel);
        show = EditorGUILayout.Toggle("Show bar in Scene view", show);
        frac = EditorGUILayout.Slider("Health", frac, 0f, 1f);
        SceneView.RepaintAll();
    }

    void OnSceneGUI()
    {
        if (!show || Application.isPlaying) return;
        var b = (EnemyHealthBar)target;
        Camera cam = Camera.main != null ? Camera.main : SceneView.currentDrawingSceneView.camera;
        Vector3 right = cam.transform.right, up = cam.transform.up, fwd = cam.transform.forward;
        Vector3 c = b.transform.position + Vector3.up * b.height - fwd * b.towardCamera;

        var old = Handles.zTest;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        Rect3(c, right, up, b.width + b.thickness * 2f, b.thickness * 3f, new Color(0.05f, 0.05f, 0.1f, 0.9f));
        Rect3(c, right, up, b.width, b.thickness, new Color(0.02f, 0.02f, 0.05f, 0.95f));

        Color col = frac > 0.5f ? Color.Lerp(b.midColor, b.fullColor, (frac - 0.5f) * 2f) : Color.Lerp(b.lowColor, b.midColor, frac * 2f);
        col = new Color(Mathf.Clamp01(col.r), Mathf.Clamp01(col.g), Mathf.Clamp01(col.b), 1f);
        float fw = Mathf.Max(0.001f, b.width * frac);
        Vector3 fc = c + right * (-b.width * 0.5f + fw * 0.5f);
        Rect3(fc, right, up, fw, b.thickness * 0.8f, col);

        Handles.zTest = old;
    }

    static void Rect3(Vector3 c, Vector3 r, Vector3 u, float w, float h, Color col)
    {
        var v = new[]
        {
            c - r * (w * 0.5f) - u * (h * 0.5f), c + r * (w * 0.5f) - u * (h * 0.5f),
            c + r * (w * 0.5f) + u * (h * 0.5f), c - r * (w * 0.5f) + u * (h * 0.5f)
        };
        Handles.DrawSolidRectangleWithOutline(v, col, Color.clear);
    }
}