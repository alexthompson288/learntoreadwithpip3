using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;


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
        GUILayout.Label("File Path");
        GUILayout.TextField("__BundleSource/videos/");

        GUILayout.Label("File Name");
        GUILayout.TextField("");

        if (GUILayout.Button("Build Bundle"))
        {

        }
    }
}
