using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace WingroveAudio
{

    public class EditorUtilities
    {


        public static void DBLabel(string prefix, float amt)
        {
            if (WingroveRoot.Instance.UseDBScale)
            {
                float dbMix = 20 * Mathf.Log10(amt);
                if (dbMix == 0)
                {
                    GUILayout.Label(prefix + "-0.00 dB");
                }
                else if (float.IsInfinity(dbMix))
                {
                    GUILayout.Label(prefix + "-inf dB");
                }
                else
                {
                    GUILayout.Label(prefix + System.String.Format("{0:0.00}", dbMix) + " dB");
                }
            }
            else
            {
                GUILayout.Label(amt * 100.0f + "%");
            }
        }

        public static float DBSlider(string inName, float amt)
        {

            GUILayout.BeginVertical(GUILayout.Width(100));
            GUISkin oldSkin = GUI.skin;
            GUILayout.Label(inName);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUI.skin = WingroveRoot.Instance.GetSkin();
            amt = Mathf.Pow(GUILayout.VerticalSlider(Mathf.Sqrt(amt), 1, 0, GUILayout.Height(100)), 2);
            GUILayout.EndHorizontal();
            GUI.skin = oldSkin;
            EditorUtilities.DBLabel("", amt);
            GUILayout.EndVertical();

            return amt;
        }

        [MenuItem("Assets/Create/WingroveAudio/Event Group")]
        public static void CreateEventGroup()
        {
            CreateAsset<EventGroup>();
        }

        [MenuItem("Assets/Create/WingroveAudio/3D Settings Group")]
        public static void Create3DSettings()
        {
            CreateAsset<Audio3DSetting>();
        }

        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static int EventProperty(int bankIndex, SerializedProperty eventProperty)
        {
            string testString = eventProperty.stringValue;
            if (WingroveRoot.Instance != null)
            {
                if (WingroveRoot.Instance.m_eventGroups == null || WingroveRoot.Instance.m_eventGroups.Length == 0)
                {
                    EditorGUILayout.HelpBox("WingroveRoot does not have any event groups!", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.BeginVertical("box");
                    int inWhichBank = WingroveRoot.Instance.FindEvent(testString);
                    if (bankIndex == -1 && inWhichBank != -1)
                    {
                        bankIndex = inWhichBank;
                    }
                    if (bankIndex == -1 && inWhichBank == -1)
                    {
                        bankIndex = 0;
                    }
                    bool offerAdd = false;
                    if (inWhichBank == -1)
                    {
                        EditorGUILayout.HelpBox("Event name " + testString + " does not exist in any EventGroup", MessageType.Warning);
                        offerAdd = true;
                    }
                    else if (inWhichBank != bankIndex)
                    {
                        EditorGUILayout.HelpBox("Event name \"" + testString + "\" has been found in a different EventGroup", MessageType.Warning);
                        if (GUILayout.Button("Select correct bank"))
                        {
                            bankIndex = inWhichBank;
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("", MessageType.None);
                    }

                    List<GUIContent> displayOptions = new List<GUIContent>();
                    foreach (EventGroup evg in WingroveRoot.Instance.m_eventGroups)
                    {
                        if (evg != null)
                        {
                            displayOptions.Add(new GUIContent(evg.name));
                        }
                        else
                        {
                            displayOptions.Add(new GUIContent("null event group - check your WingroveRoot"));
                        }
                    }

                    bankIndex = EditorGUILayout.Popup(new GUIContent("Select bank"), bankIndex, displayOptions.ToArray());

                    testString = EditorGUILayout.TextField("Type event name:", testString);

                    List<GUIContent> eventDropdown = new List<GUIContent>();
                    eventDropdown.Add(new GUIContent(testString));
                    int selected = 0;
                    if (WingroveRoot.Instance.m_eventGroups[bankIndex] != null)
                    {
                        foreach (string evName in WingroveRoot.Instance.m_eventGroups[bankIndex].m_events)
                        {
                            eventDropdown.Add(new GUIContent(evName));
                        }
                    }
                    selected = EditorGUILayout.Popup(new GUIContent("or select:"), selected, eventDropdown.ToArray());
                    if (selected != 0)
                    {
                        testString = WingroveRoot.Instance.m_eventGroups[bankIndex].m_events[selected - 1];
                    }

                    if (offerAdd)
                    {
                        if (GUILayout.Button("Add \"" + testString + "\" to bank \"" +
                            WingroveRoot.Instance.m_eventGroups[bankIndex] + "\""))
                        {
                            ArrayUtility.Add(ref WingroveRoot.Instance.m_eventGroups[bankIndex].m_events, testString);
                        }
                    }

                    EditorGUILayout.EndVertical();

                    eventProperty.stringValue = testString;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("WingroveRoot does not exist!", MessageType.Error);
            }

            return bankIndex;
        }
    }

}