using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace LifeboatCommon
{

    public static class ScriptableObjectCreators
    {

        [MenuItem("Assets/Create/Lifeboat/Asset Bundle Definition")]
        public static void Create3DSettings()
        {
            CreateAsset<AssetBundleDefinition>();
        }


        [MenuItem("Assets/Create/Lifeboat/Build Configuration")]
        public static void CreateBC()
        {
            CreateAsset<BuildConfiguration>();
        }


        [MenuItem("Assets/Create/LTRWP/Game Build Settings")]
        public static void CreatePBC()
        {
            CreateAsset<PipGameBuildSettings>();
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
    }

}