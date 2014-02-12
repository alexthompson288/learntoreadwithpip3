using UnityEngine;
using System.Collections;
using UnityEditor;
namespace WingroveAudio
{
    [CustomEditor(typeof(NGUIClickEvents))]
    public class NGUIClickEventsEditor : Editor
    {
        int m_onDoubleClickIndex = -1;
        int m_onClickIndex = -1;
        int m_onPressIndex = -1;
        int m_onReleaseIndex = -1;

        void ShowEvent(string eventStringPropertyName, string boolName, string funcName, ref int bankIndex)
        {
            
            SerializedProperty eventProperty = serializedObject.FindProperty(eventStringPropertyName);
            SerializedProperty eventFProperty = serializedObject.FindProperty(boolName);
            GUILayout.BeginVertical("box");
            GUILayout.Label("Event to fire on " + funcName);
            if (eventFProperty.boolValue == true)
            {
                if (GUILayout.Button("Remove " + funcName + " action"))
                {
                    eventFProperty.boolValue = false;
                }
                bankIndex = EditorUtilities.EventProperty(bankIndex, eventProperty);
            }
            else
            {
                if ( GUILayout.Button("Add " + funcName + " action") )
                {
                    eventFProperty.boolValue = true;
                }
            }
            GUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShowEvent("m_onPressEvent", "m_fireEventOnPress", "OnPress(true)", ref m_onPressIndex);
            ShowEvent("m_onReleaseEvent", "m_fireEventOnRelease", "OnPress(false)", ref m_onReleaseIndex);
            ShowEvent("m_onClickEvent", "m_fireEventOnClick", "OnClick()", ref m_onClickIndex);
            ShowEvent("m_onDoubleClickEvent", "m_fireEventOnDoubleClick", "OnDoubleClick()", ref m_onDoubleClickIndex);

            serializedObject.ApplyModifiedProperties();

        }
    }

}