using UnityEngine;
using System.Collections;
using UnityEditor;
namespace WingroveAudio
{
    [CustomEditor(typeof(LifetimeEventTrigger))]
    public class LifetimeEventTriggerEditor : Editor
    {
        int m_startBankIndex = -1;
        int m_onEnableBankIndex = -1;
        int m_onDisableBankIndex = -1;
        int m_onDestroyBankIndex = -1;

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
            ShowEvent("m_startEvent", "m_fireEventOnStart", "Start()", ref m_startBankIndex);
            ShowEvent("m_onEnableEvent", "m_fireEventOnEnable", "OnEnable()", ref m_onEnableBankIndex);
            ShowEvent("m_onDisableEvent", "m_fireEventOnDisable", "OnDisable()", ref m_onDisableBankIndex);
            ShowEvent("m_onDestroyEvent", "m_fireEventOnDestroy", "OnDestroy()", ref m_onDestroyBankIndex);

            SerializedProperty linkProperty = serializedObject.FindProperty("m_linkToObject");
            EditorGUILayout.PropertyField(linkProperty);
            SerializedProperty dontPlay = serializedObject.FindProperty("m_dontPlayDestroyIfDisabled");
            EditorGUILayout.PropertyField(dontPlay);
            serializedObject.ApplyModifiedProperties();

        }
    }

}