using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
namespace WingroveAudio
{
    public class WingroveMixerWindow : EditorWindow
    {

        [MenuItem("Window/Wingrove Audio/Mixer Window")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(WingroveMixerWindow));
        }

        private Vector2 m_scrollPos;

        void Update()
        {
            Repaint();
        }



        void OnGUI()
        {
            title = "Wingrove Mixer";
            minSize = new Vector2(2, 460);
            maxSize = new Vector2(4000, 460);
            if (WingroveRoot.Instance == null)
            {
                GUILayout.Label("Wingrove Root not found. Create your audio hierarchy.");
            }
            else
            {
                UnityEngine.Object[] mixBuses = WingroveRoot.Instance.GetComponentsInChildren<WingroveMixBus>();
                if (mixBuses.Length == 0)
                {
                    GUILayout.Label("No active mix buses found. Add some mix buses.");
                }
                else
                {
                    WingroveRoot.Instance.UseDBScale = EditorGUILayout.Toggle("Mix in dB", WingroveRoot.Instance.UseDBScale);
                    m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
                    GUILayout.BeginHorizontal(GUILayout.Height(320));
                    GUILayout.Space(50);
                    foreach (WingroveMixBus wmb in mixBuses)
                    {
                        GUILayout.Space(25);
                        GUILayout.BeginVertical(GUILayout.Width(75));
                        GUILayout.Space(10);
                        GUISkin oldSkin = GUI.skin;
                        GUI.skin = WingroveRoot.Instance.GetSkin();
                        GUILayout.BeginHorizontal();
                        wmb.Volume = Mathf.Pow(GUILayout.VerticalSlider(Mathf.Sqrt(wmb.Volume), 1, 0, GUILayout.Height(300)), 2);
                        GUI.skin = oldSkin;
                        GUI.color = Color.green;
                        Mathf.Pow(GUILayout.VerticalSlider(Mathf.Sqrt(wmb.GetRMS() / 0.707f), 1, 0, GUILayout.Height(300)), 2);
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Height(100));
                    GUILayout.Space(50);
                    foreach (WingroveMixBus wmb in mixBuses)
                    {
                        GUILayout.BeginVertical(GUILayout.Width(100));
                        GUILayout.Space(10);

                        GUILayout.Label(wmb.name);
                        EditorUtilities.DBLabel("", wmb.Volume);

                        GUILayout.Space(10);
                        GUILayout.Label("Active ducks:");
                        List<BaseAutomaticDuck> ducks = wmb.GetDuckList();
                        foreach (BaseAutomaticDuck duck in ducks)
                        {
                            if (duck.GetDuckVol() != 1)
                            {
                                EditorUtilities.DBLabel(duck.GetGroupName() + " ", duck.GetDuckVol());
                            }
                        }

                        GUILayout.EndVertical();

                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();
                }
            }
        }
    }

}