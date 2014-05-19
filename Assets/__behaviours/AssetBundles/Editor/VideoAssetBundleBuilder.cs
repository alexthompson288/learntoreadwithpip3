using UnityEngine;
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
        GUILayout.Label("File Path");
        GUILayout.TextField("__BundleSource/videos/");

        GUILayout.Label("File Name");
        GUILayout.TextField("");

        if (GUILayout.Button("Build Bundle"))
        {
            //string path = EditorUtility.SaveFilePanel("Save Videos", "", "default_pipisode", "asset");
            string path = EditorUtility.SaveFilePanel("Save Videos", "", "default_pipisode", "mp4");

            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

            foreach (object asset in selection) 
            {
                string assetPath = AssetDatabase.GetAssetPath((UnityEngine.Object) asset);
                if (asset is Texture2D) 
                {
                    // Force reimport thru TextureProcessor.
                    //AssetDatabase.ImportAsset(assetPath);
                }
            }

            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
            Selection.objects = selection;
        }
    }
}
