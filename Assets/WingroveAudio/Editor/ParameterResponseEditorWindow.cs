using UnityEngine;
using System.Collections;
using UnityEditor;

namespace WingroveAudio
{

    public class ParameterResponseEditorWindow : EditorWindow
    {
        [MenuItem("Window/Wingrove Audio/Parameter Response Editing")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(ParameterResponseEditorWindow));
        }

        GameObject m_lockedHierarchy;
        bool m_showPitch = true;
        bool m_showVolume = true;

        int m_counter;

        void Update()
        {
            m_counter++;
            if (m_counter % 5 == 0)
            {
                Repaint();
            }
        }

        void ShowEditorCurves(GameObject focusObject, string path)
        {
            ParameterModifier[] paramMod = focusObject.GetComponents<ParameterModifier>();
            foreach (ParameterModifier pm in paramMod)
            {
                GUILayout.Label("Node: " + path + focusObject.name + " Parameter: " + pm.m_parameter);
                if (m_showPitch || m_showVolume)
                {
                    Rect curveRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(100));
                    Color bgColor = Color.grey;

                    if (m_showPitch)
                    {
                        //EditorGUILayout.CurveField(pm.m_pitchCurve, GUILayout.Height(100));
                        EditorGUIUtility.DrawCurveSwatch(curveRect, pm.m_pitchCurve, null, Color.red, bgColor, new Rect(0,0,1,3));
                        bgColor.a = 0;
                    }
                    if (m_showVolume)
                    {
                        EditorGUIUtility.DrawCurveSwatch(curveRect, pm.m_volumeCurve, null, Color.green, bgColor, new Rect(0,0,1,1));
                        bgColor.a = 0;
                    }

                    if (m_showPitch || m_showVolume)
                    {
                        float param = WingroveRoot.Instance.GetParameterForGameObject(pm.m_parameter, null);
                        AnimationCurve fakeCurve = new AnimationCurve(
                            new Keyframe[] { new Keyframe(param - 0.0001f, 0), new Keyframe(param + 0.0001f, 1) });
                        EditorGUIUtility.DrawCurveSwatch(curveRect, fakeCurve, null, Color.white, bgColor, new Rect(0,0.1f,1,0.8f));
                    }

                    GUILayout.Space(100);
                    EditorGUILayout.EndHorizontal();
                    if (m_showPitch)
                    {
                        EditorGUILayout.CurveField("Edit pitch curve -->", pm.m_pitchCurve, Color.red, new Rect(0, 0, 1, 3));
                    }
                    if (m_showVolume)
                    {
                        EditorGUILayout.CurveField("Edit volume curve -->", pm.m_volumeCurve, Color.green, new Rect(0, 0, 1, 1));
                    }
                    GUILayout.Space(10);
                    
                }
            }
            foreach (Transform t in focusObject.transform)
            {
                ShowEditorCurves(t.gameObject, path + focusObject.name + "/");
            }
        }

        Vector2 m_scrollPosition;

        void OnGUI()
        {            
            title = "ParamEdit";
            GameObject focusObject = m_lockedHierarchy;
            if (focusObject == null)
            {                
                if (Selection.activeGameObject != null)
                {
                    focusObject = Selection.activeGameObject;
                    if (GUILayout.Button("Lock to object"))
                    {
                        m_lockedHierarchy = focusObject;
                    }
                }
                else
                {
                    GUILayout.Label("Select an object in the Audio Hierarchy");
                }
            }
            else
            {
                EditorGUILayout.ObjectField(focusObject, typeof(GameObject), true);
                if (GUILayout.Button("Unlock"))
                {
                    m_lockedHierarchy = null;
                    focusObject = Selection.activeGameObject;
                }
            }

            m_showPitch = GUILayout.Toggle(m_showPitch, "Show Pitch");
            m_showVolume = GUILayout.Toggle(m_showVolume, "Show Volume");

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
            if (focusObject != null)
            {
                ShowEditorCurves(focusObject, "");
            }
            GUILayout.EndScrollView();
        }
    }

}