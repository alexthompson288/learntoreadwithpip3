using UnityEngine;
using System.Collections;
using UnityEditor;

namespace LifeboatCommon
{

    [CustomPropertyDrawer(typeof(AssetReference))]
    public class AssetReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty assetNameProp = property.FindPropertyRelative("m_assetName");
            if (string.IsNullOrEmpty(assetNameProp.stringValue))
            {
                Object asset = EditorGUI.ObjectField(position, label, null, typeof(Object), false);
                if (asset != null)
                {
                    assetNameProp.stringValue = asset.name;
                }
            }
            else
            {
                EditorGUI.TextField(position, label, assetNameProp.stringValue);
                if (GUI.Button(new Rect(position.xMax - 32, position.yMin, 32, position.height), "X"))
                {
                    assetNameProp.stringValue = "";
                }
            }
        }
    }

}