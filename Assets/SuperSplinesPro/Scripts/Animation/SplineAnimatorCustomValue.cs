using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using SplineUtilities;

//This class animates a gameobject along the spline at a specific speed. 
//Also it demonstrates how to use the 'custom value feature'. In this example it alters
//the color of an attached mesh renderer. The custom values for the nodes can be set via script
//or in the Inspector - simply click on a gameObject that has the SplineNode-component 
//attached to it.
[RequireComponent( typeof( MeshRenderer ) )]
public class SplineAnimatorCustomValue : MonoBehaviour
{
	public Spline spline;
	
	public WrapMode wrapMode = WrapMode.Clamp;
	
	public float speed = 1f;
	public float offSet = 0f;
	
	public float passedTime = 0f;
	
	void Update( ) 
	{
		passedTime += Time.deltaTime * speed;
		
		float clampedParam = SplineUtils.WrapValue( passedTime + offSet, 0f, 1f, wrapMode );
		
		//Interpolate a custom value according to the interpolation type and spline settings
		//The custom values can be set in the SplineNode scripts or in the inspector
		float customValue = spline.GetCustomValueOnSpline( clampedParam );
		
		transform.position = spline.GetPositionOnSpline( clampedParam );
		transform.rotation = spline.GetOrientationOnSpline( clampedParam );
		
		renderer.material.color = Color.red * (1f-customValue) + Color.blue * (customValue);
	}
}
