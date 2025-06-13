using UnityEditor;
using UnityEngine;
using UI;

[CustomEditor(typeof(CharacterButton))]
public class CharacterButtonEditor : Editor
{
  public override void OnInspectorGUI()
  {
    // Exclude from default drawing so we can draw it manually
    DrawPropertiesExcluding(serializedObject, "unitData", "gemCount");

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Character Button Settings", EditorStyles.boldLabel);

    SerializedProperty unitDataProp = serializedObject.FindProperty("unitData");
    EditorGUILayout.PropertyField(unitDataProp, new GUIContent("Unit Data"));

    SerializedProperty gemCountProp = serializedObject.FindProperty("gemCount");
    EditorGUILayout.PropertyField(gemCountProp, new GUIContent("Gem Count"));
    
    serializedObject.ApplyModifiedProperties();
  }
}