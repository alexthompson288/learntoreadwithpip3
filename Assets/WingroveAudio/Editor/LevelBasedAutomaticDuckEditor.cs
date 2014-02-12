using UnityEngine;
using System.Collections;
using UnityEditor;
namespace WingroveAudio
{
    [CustomEditor(typeof(LevelBasedAutomaticDuck))]
    public class LevelBasedAutomaticDuckEditor : Editor
    {
        void Update()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LevelBasedAutomaticDuck duck = (LevelBasedAutomaticDuck)target;

            SerializedProperty groupProp = serializedObject.FindProperty("m_mixBusToMonitor");
            if (groupProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No group to monitor selected!", MessageType.Error, true);
            }
            EditorGUILayout.PropertyField(groupProp);

            SerializedProperty volumeProperty = serializedObject.FindProperty("m_duckingMixAmount");
            SerializedProperty thresholdProperty = serializedObject.FindProperty("m_threshold");

            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal("box");

            volumeProperty.floatValue = EditorUtilities.DBSlider("Gain limit", volumeProperty.floatValue);

            thresholdProperty.floatValue = EditorUtilities.DBSlider("Threshold", thresholdProperty.floatValue);
            GUILayout.EndHorizontal();


            if (duck.GetMixBusToMonitor())
            {
                GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Level");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Threshold");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Gain out");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.VerticalSlider(Mathf.Sqrt(duck.GetMixBusToMonitor().GetRMS() / 0.707f), 1, 0);
                GUILayout.FlexibleSpace();
                GUILayout.VerticalSlider(Mathf.Sqrt(thresholdProperty.floatValue), 1, 0);
                GUILayout.FlexibleSpace();
                if (duck.GetDuckVol() != 1)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.VerticalSlider(Mathf.Sqrt(duck.GetDuckVol()), 1, 0);
                GUI.backgroundColor = Color.white;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();

            SerializedProperty attackProp = serializedObject.FindProperty("m_attack");
            attackProp.floatValue = EditorGUILayout.FloatField("Attack", attackProp.floatValue);
            SerializedProperty releaseProp = serializedObject.FindProperty("m_release");
            releaseProp.floatValue = EditorGUILayout.FloatField("Release", releaseProp.floatValue);
            SerializedProperty ratio = serializedObject.FindProperty("m_ratio");
            ratio.floatValue = EditorGUILayout.FloatField("Ratio", ratio.floatValue);

            serializedObject.ApplyModifiedProperties();

            Repaint();

        }
    }

}