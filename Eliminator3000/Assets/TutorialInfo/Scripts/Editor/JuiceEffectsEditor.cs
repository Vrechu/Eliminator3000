using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JuiceEffects))]
public class JuiceEffectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var j = (JuiceEffects)target;
        EditorGUILayout.Space(4);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.LabelField("Test in Play Mode", EditorStyles.boldLabel);
            for (int i = 0; i < j.effects.Count; i++)
                if (GUILayout.Button("Play: " + j.effects[i].name)) j.Play(i);
            if (GUILayout.Button("Stop all")) j.StopAll();
        }
    }
}