using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace WingroveAudio
{
    [CustomEditor(typeof(WingroveAudioSource))]
    public class WingroveAudioSourceEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();           

            SerializedProperty clipMixVolume = serializedObject.FindProperty("m_clipMixVolume");
            clipMixVolume.floatValue = EditorUtilities.DBSlider("Clip Mix Volume", clipMixVolume.floatValue);


            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();

        }
    }

}