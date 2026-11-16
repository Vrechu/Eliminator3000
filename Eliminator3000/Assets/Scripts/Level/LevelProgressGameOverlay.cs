using UnityEngine;

/// <summary>Values written by the Level Manager (editor) and read by the Game view overlay.</summary>
public static class LevelProgressData
{
    public static bool active;
    public static bool showSection;
    public static bool twoBars;
    public static float wholeProg = -1f;
    public static float wholeTimeProg = -1f;
    public static float secProg = -1f;
    public static string wholeText = "";
    public static string wholeTimeText = "";
    public static string sectionText = "";
}

/// <summary>
/// Draws the level / section progress bars in the Game view. Created and removed by the Level Manager
/// while playing in the Editor. Does nothing in a built game.
/// </summary>
public class LevelProgressGameOverlay : MonoBehaviour
{
#if UNITY_EDITOR
    private GUIStyle label;

    private void DrawBar(Rect r, float prog, string text)
    {
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = new Color(0.25f, 0.8f, 0.4f, 0.9f);
        GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(prog), r.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(r, text, label);
        GUI.color = old;
    }

    private void OnGUI()
    {
        if (!LevelProgressData.active) return;
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 12;
            label.normal.textColor = Color.white;
        }

        float s = Mathf.Max(1f, Screen.height / 720f);
        Matrix4x4 oldM = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));

        float x = 10f, y = 10f, w = 380f, h = 22f;
        DrawBar(new Rect(x, y, w, h), Mathf.Max(0f, LevelProgressData.wholeProg), LevelProgressData.wholeText);
        y += h + 4f;
        if (LevelProgressData.twoBars)
        {
            DrawBar(new Rect(x, y, w, h), Mathf.Max(0f, LevelProgressData.wholeTimeProg), LevelProgressData.wholeTimeText);
            y += h + 4f;
        }
        if (LevelProgressData.showSection && LevelProgressData.secProg >= 0f)
            DrawBar(new Rect(x, y, w, h), LevelProgressData.secProg, LevelProgressData.sectionText);

        GUI.matrix = oldM;
    }
#endif
}