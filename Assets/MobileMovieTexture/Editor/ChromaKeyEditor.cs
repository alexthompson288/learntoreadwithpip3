using UnityEngine;
using UnityEditor;
using System.Collections;


public class ChromaKeyEditor : EditorWindow 
{	
	static Color YCrCbToRGB( Vector4 yCrCb )
	{
		//1.1643828125, 0, 1.59602734375, -.87078515625
		//1.1643828125, -.39176171875, -.81296875, .52959375
		//1.1643828125f, 2.017234375f, 0, -1.081390625
		//0,0,0,1
		
		yCrCb.w = 1.0f;
		
		Vector4 YCbCr2R = new Vector4(1.1643828125f, 0f, 1.59602734375f, -.87078515625f);
		Vector4 YCbCr2G = new Vector4(1.1643828125f, -.39176171875f, -.81296875f, .52959375f);
		Vector4 YCbCr2B = new Vector4(1.1643828125f, 2.017234375f, 0f, -1.081390625f);
		
		return new Color( Vector4.Dot(yCrCb, YCbCr2R), Vector4.Dot(yCrCb, YCbCr2G), Vector4.Dot(yCrCb, YCbCr2B), 1.0f);
	}
	
	static Vector4 RGBToYCrCb( Color rgb)
	{
		// 0.256788657  0.504129732  0.097905693  0.062498094
		//-0.148222885 -0.290992461  0.439215346  0.500000858
 		//0.439215346 -0.367788182 -0.071427164  0.499999860
 		//0.000000000  0.000000000  0.000000000  1.000000000
		
		rgb.a = 1.0f;
		
		Vector4 RGBToY = new Vector4(0.256788657f,  0.504129732f,  0.097905693f,  0.062498094f);
		Vector4 RGBToCr = new Vector4(-0.148222885f, -0.290992461f,  0.439215346f,  0.5f);
		Vector4 RGBToCb = new Vector4(0.439215346f, -0.367788182f, -0.071427164f,  0.5f);
		
		return new Vector4( Vector4.Dot(rgb, RGBToY), Vector4.Dot(rgb, RGBToCr), Vector4.Dot(rgb, RGBToCb), 1.0f);
	}
	
	[MenuItem ("Window/Chroma Key Editor")]
    static void Init () 
	{
        // Get existing open window or if none, make a new one:
        ChromaKeyEditor window = (ChromaKeyEditor)EditorWindow.GetWindow (typeof (ChromaKeyEditor));

        Debug.Log("Opening chroma key editor " +window.name); //Get rid of the warning
    }
	
	void OnGUI () 
	{
		Material material = Selection.activeObject as Material;
		
		if (material != null && material.shader.name == "Color Space/YCrCbtoRGB Chroma Key")
		{
			Vector4 keyYCrCbParam = material.GetVector("_KeyYCrCb");
			Vector4 keyScaleParam = material.GetVector("_KeyScale");
			
			float offsetBottom = -keyYCrCbParam.w;
			float scale = keyScaleParam.w;
			
			Color keyRGB = YCrCbToRGB(keyYCrCbParam);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Chroma Color");
			
			Color newKeyRGB = EditorGUILayout.ColorField(keyRGB, GUILayout.Width(200));
			
			EditorGUILayout.EndHorizontal();
			
			Vector3 keyScale = new Vector3(keyScaleParam.x,keyScaleParam.y,keyScaleParam.z);
			
			float maxDifferenceLength = keyScale.magnitude;
			
			float offsetBottomNormalized = offsetBottom / maxDifferenceLength;
			
			float topOffsetNormalized = scale / (maxDifferenceLength / offsetBottom);
			
			EditorGUILayout.LabelField("Threshold", "");
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("High");
			
			float newBottomOffsetNormalized = 1.0f - EditorGUILayout.Slider(1.0f - offsetBottomNormalized, float.Epsilon, 1.0f - float.Epsilon);
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Low");
			
			float newTopOffsetNormalised = 1.0f - EditorGUILayout.Slider(1.0f -topOffsetNormalized, 0.0f, 1.0f);
			
			EditorGUILayout.EndHorizontal();
			
			newTopOffsetNormalised = Mathf.Max(newTopOffsetNormalised, newBottomOffsetNormalized);
			newBottomOffsetNormalized = Mathf.Min(newBottomOffsetNormalized,newTopOffsetNormalised);
				
			
			float newBottomOffset = newBottomOffsetNormalized * maxDifferenceLength;
			
			
			if ( Vector4.Distance(keyRGB, newKeyRGB) > 1.0f/255.0f || !Mathf.Approximately(offsetBottom,newBottomOffset))
			{
				keyYCrCbParam = RGBToYCrCb(newKeyRGB);
				
				keyYCrCbParam.w = -newBottomOffset;
				
				material.SetVector("_KeyYCrCb", keyYCrCbParam );
			}
				
			//(length(deltaVec) - _KeyYCrCb.w) * _KeyScale.w;
			
			float newScale = (maxDifferenceLength / newBottomOffset) * newTopOffsetNormalised;
			
			if ( !Mathf.Approximately(scale,newScale) )
			{
				keyScaleParam.w = newScale;
				
				material.SetVector("_KeyScale", keyScaleParam );
			}
			
			Vector3 newKeyScale;
			
			EditorGUILayout.LabelField("Channel Priority", "");
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Y");
			
			newKeyScale.x = EditorGUILayout.Slider(keyScale.x, 0.0f, 1.0f);
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Cr");
			
			newKeyScale.y = EditorGUILayout.Slider(keyScale.y, 0.0f, 1.0f);
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Cb");
			
			newKeyScale.z = EditorGUILayout.Slider(keyScale.z, 0.0f, 1.0f);
			
			EditorGUILayout.EndHorizontal();
			
			float newMaxDifferenceLength = newKeyScale.magnitude;
			
			if ( !Mathf.Approximately(newMaxDifferenceLength,maxDifferenceLength) )
			{
				//Have to recalculate scale and offset
				newBottomOffset = newBottomOffsetNormalized * newMaxDifferenceLength;
				newScale = (newMaxDifferenceLength / newBottomOffset) * newTopOffsetNormalised;
				
				//Now upload everything
				keyYCrCbParam.w = -newBottomOffset;
				
				material.SetVector("_KeyYCrCb", keyYCrCbParam );
				
				keyScaleParam.x = newKeyScale.x;
				keyScaleParam.y = newKeyScale.y;
				keyScaleParam.z = newKeyScale.z;
				keyScaleParam.w = newScale;
				
				material.SetVector("_KeyScale", keyScaleParam );
			}
		}
		else
		{
			if (material != null )
			{
				GUILayout.TextArea ("Material selected does not use the Chroma Key shader");
			}
			else
			{
				GUILayout.TextArea ("Please select a material that uses the Chroma Key shader");
			}
		}
	}
}
