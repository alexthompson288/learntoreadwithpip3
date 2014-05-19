using UnityEngine;
using System.Collections;
using UnityEditor;


public class VideoAssetBundleBuilder : EditorWindow
{
    [MenuItem("Window/LTRWP/Video Asset Bundle Builder")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(VideoAssetBundleBuilder));
    }

    bool m_buildIos = false;
    bool m_buildWindows = false;

    void OnGUI()
    {
        GUILayout.Label("Test Success");
    }
}
