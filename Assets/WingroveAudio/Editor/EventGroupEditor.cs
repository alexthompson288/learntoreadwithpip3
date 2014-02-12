using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace WingroveAudio
{
    [CustomEditor(typeof(EventGroup))]
    public class EventGroupEditor : Editor
    {

        string m_eventToAdd = "";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty sp = serializedObject.FindProperty("m_events");

            int indexToDelete = -1;
            bool alternate = false;
            if (sp.arraySize != 0)
            {
                for (int index = 0; index < sp.arraySize; ++index)
                {
                    if (alternate)
                    {
                        GUI.backgroundColor = new Color(0.8f, 1.0f, 0.8f);
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 1.0f);
                    }

                    SerializedProperty str = sp.GetArrayElementAtIndex(index);
                    GUILayout.BeginHorizontal();
                    str.stringValue = EditorGUILayout.TextField(str.stringValue);
                    if (GUILayout.Button("Remove"))
                    {
                        indexToDelete = index;
                    }
                    GUILayout.EndHorizontal();
                    alternate = !alternate;

                }
            }
            GUI.backgroundColor = Color.white;
            if (indexToDelete != -1)
            {
                sp.DeleteArrayElementAtIndex(indexToDelete);
            }

            GUILayout.Label("Add new event:");
            GUILayout.BeginHorizontal();
            m_eventToAdd = GUILayout.TextField(m_eventToAdd);
            if (GUILayout.Button("Add"))
            {
                sp.arraySize++;
                SerializedProperty newEvent = sp.GetArrayElementAtIndex(sp.arraySize - 1);
                newEvent.stringValue = m_eventToAdd;
                m_eventToAdd = "";
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Sort alphabetically"))
            {
                List<string> eventList = new List<string>();
                for (int index = 0; index < sp.arraySize; ++index)
                {
                    SerializedProperty str = sp.GetArrayElementAtIndex(index);
                    eventList.Add(str.stringValue);
                }

                eventList.Sort();
                for (int index = 0; index < sp.arraySize; ++index)
                {
                    SerializedProperty str = sp.GetArrayElementAtIndex(index);
                    str.stringValue = eventList[index];
                }
            }
            serializedObject.ApplyModifiedProperties();

        }
    }

}