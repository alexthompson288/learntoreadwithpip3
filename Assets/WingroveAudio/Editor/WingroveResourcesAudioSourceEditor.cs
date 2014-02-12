using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace WingroveAudio
{
    [CustomEditor(typeof(WingroveResourcesAudioSource))]
    public class WingroveResourcesAudioSourceEditor : Editor
    {
       
        void CreateStreamLoader(AudioClip clip)
        {
            string assetPath = AssetDatabase.GetAssetPath(clip);
            GameObject newObject = new GameObject();
            StreamLoader sLoader = newObject.AddComponent<StreamLoader>();
            sLoader.m_referencedAudioClip = clip;
            string path = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + "_SL.prefab";
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            PrefabUtility.CreatePrefab(path, newObject);
            GameObject.DestroyImmediate(newObject);
        
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty audioResourceName = serializedObject.FindProperty("m_audioClipResourceName");
            EditorGUILayout.PropertyField(audioResourceName, new GUIContent("Type resource filename"));
            GUILayout.Label("or drag file below...");
            AudioClip newClip = (AudioClip)EditorGUILayout.ObjectField(null, typeof(AudioClip), true);
            if (newClip != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(newClip);
                if ( assetPath.Contains("Resources/") )
                {
                    string filePath = assetPath.Substring(assetPath.IndexOf("Resources/") + "Resources/".Length);
                    audioResourceName.stringValue = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath);
                    if ( audioResourceName.stringValue.StartsWith("/") )
                    {
                        audioResourceName.stringValue = audioResourceName.stringValue.Substring(1);
                    }
                }
            }
            
            SerializedProperty unloadOnLevelChange = serializedObject.FindProperty("m_onlyUnloadOnLevelChange");
            EditorGUILayout.PropertyField(unloadOnLevelChange);

            SerializedProperty useStreamLoader = serializedObject.FindProperty("m_useStreamLoader");

            AudioClip clip = (AudioClip)Resources.Load(audioResourceName.stringValue);
            if (clip == null)
            {
                EditorGUILayout.HelpBox("Audio clip resource not found", MessageType.Error);
            }
            if (!useStreamLoader.boolValue)
            {
                if (clip != null)
                {
                    if (GUILayout.Button("Convert to use Stream Loader (faster)"))
                    {
                        CreateStreamLoader(clip);
                        useStreamLoader.boolValue = true;
                    }
                }
            }
            else
            {
                if (clip != null)
                {
                    GameObject streamLoader = (GameObject)Resources.Load(audioResourceName.stringValue + "_SL");
                    if (streamLoader == null)
                    {
                        EditorGUILayout.HelpBox("Stream loader not found", MessageType.Error);
                        if (GUILayout.Button("Re-create it!"))
                        {
                            CreateStreamLoader(clip);
                        }
                    }
                    else
                    {
                        GUILayout.Label("Stream loader enabled on this resource!");
                        StreamLoader loader = streamLoader.GetComponent<StreamLoader>();
                        if (loader == null)
                        {
                            EditorGUILayout.HelpBox("Stream loader doesn't have correct components", MessageType.Error);
                            if (GUILayout.Button("Re-create it!"))
                            {
                                CreateStreamLoader(clip);
                            }
                        }
                        else if (loader.m_referencedAudioClip != clip)
                        {
                            EditorGUILayout.HelpBox("Stream loader references the wrong audio clip", MessageType.Error);
                            if (GUILayout.Button("Re-create it!"))
                            {
                                CreateStreamLoader(clip);
                            }
                        }
                    }
                }
            }

            SerializedProperty clipMixVolume = serializedObject.FindProperty("m_clipMixVolume");
            clipMixVolume.floatValue = EditorUtilities.DBSlider("Clip Mix Volume", clipMixVolume.floatValue);
            //EditorUtilities.DBLabel("Clip mix volume " + 
            //clipMixVolume.floatValue = 
            //    Mathf.Pow(EditorGUILayout.Slider(Mathf.Sqrt(clipMixVolume.floatValue), 0, 1), 2);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();      
            
        }
    }

}