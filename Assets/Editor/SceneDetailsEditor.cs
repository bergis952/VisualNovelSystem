using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneDetails))]
public class SceneDetaisEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogue"), true);

        serializedObject.ApplyModifiedProperties();
    }
}