using UnityEngine;
using System.Collections;
using UnityEditor;

namespace WingroveAudio
{
    [CustomEditor(typeof(WingroveGroupInformation))]
    public class WingroveGroupInformationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty prop = serializedObject.FindProperty("m_handleRepeatedAudio");
            EditorGUILayout.PropertyField(prop);
            WingroveGroupInformation wgi = (WingroveGroupInformation)target;
            float timeTilFinished = wgi.GetTimeUntilFinished();
            if (timeTilFinished == float.MaxValue)
            {
                GUILayout.Label("Time until finished: float.maxValue -- looping sounds");
            }
            else
            {
                if (timeTilFinished == -1 )
                {
                    GUILayout.Label(System.String.Format("Time until finished {0:00.00} -- looping sounds", timeTilFinished));
                }
                else
                {
                    GUILayout.Label(System.String.Format("Time until finished {0:00.00}", wgi.GetTimeUntilFinished()));
                }
            }
            GUILayout.Label(wgi.IsAnyPlaying() ? "Active" : "Inactive");

            

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
    }

}