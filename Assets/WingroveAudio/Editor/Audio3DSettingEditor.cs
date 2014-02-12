using UnityEngine;
using System.Collections;
using UnityEditor;

namespace WingroveAudio
{
    [CustomEditor(typeof(Audio3DSetting))]
    public class Audio3DSettingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            SerializedProperty rolloffProp = serializedObject.FindProperty("m_rolloffMode");
            EditorGUILayout.PropertyField(rolloffProp);

            SerializedProperty maxDistProp = serializedObject.FindProperty("m_maxDistance");
            SerializedProperty minDistProp = serializedObject.FindProperty("m_minDistance");
            if (rolloffProp.enumValueIndex != (int)AudioRolloffMode.Custom)
            {
                EditorGUILayout.PropertyField(minDistProp);
                EditorGUILayout.PropertyField(maxDistProp);
            }


            SerializedProperty customCurve = serializedObject.FindProperty("m_customRolloffCurve");
            if (rolloffProp.enumValueIndex == (int)AudioRolloffMode.Custom)
            {

                SerializedProperty maxNorm = serializedObject.FindProperty("m_useMaxToNormalizeCustomCurve");
                EditorGUILayout.PropertyField(maxNorm );

                if (maxNorm.boolValue == true)
                {
                    EditorGUILayout.PropertyField(maxDistProp);
                    customCurve.animationCurveValue = EditorGUILayout.CurveField("Custom curve (normalized to max distance)", customCurve.animationCurveValue, Color.green, new Rect(0, 0, 1, 1));
                }
                else
                {
                    customCurve.animationCurveValue = EditorGUILayout.CurveField("Custom curve", customCurve.animationCurveValue);
                }

            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}