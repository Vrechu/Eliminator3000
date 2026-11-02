using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingSideSpawner))]
public class BuildingSideSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BuildingSideSpawner spawner = (BuildingSideSpawner)target;

        EditorGUILayout.Space(15);
        if (GUILayout.Button("Generate Buildings Now", GUILayout.Height(40)))
        {
            spawner.GenerateBuildings();
        }
    }
}