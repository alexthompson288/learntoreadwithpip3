using UnityEngine;
using System.Collections;
using UnityEditor;

namespace LifeboatCommon
{

    [CustomEditor(typeof(AssetBundleDefinition))]
    public class AssetBundleDefinitionEditor : Editor
    {



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AssetBundleDefinition abd = (AssetBundleDefinition)target;
            if (GUILayout.Button("Build Standalone Windows"))
            {
                abd.BuildForPlatform(BuildTarget.StandaloneWindows);
            }
            if (GUILayout.Button("Build iOS"))
            {
                abd.BuildForPlatform(BuildTarget.iPhone);
            }
        }
    }

}