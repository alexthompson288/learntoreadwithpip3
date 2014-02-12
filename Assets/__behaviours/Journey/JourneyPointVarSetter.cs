using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class JourneyPointVarSetter : MonoBehaviour 
{
	void Start () 
	{
		Debug.Log("JourneyPointVarSetter.Start()");
		/*
		JourneyPoint[] points = Object.FindObjectsOfType(typeof(JourneyPoint)) as JourneyPoint[];
		Debug.Log("There are " + points.Length + " journey points");

		int highestSessionNum = 0;

		foreach(JourneyPoint point in points)
		{
			if(point.GetSessionNum() > highestSessionNum)
			{
				highestSessionNum = point.GetSessionNum();
			}
		}

		Debug.Log("Highest sessionNum is " + highestSessionNum);

		int mySessionNum = highestSessionNum + 1;
		GetComponent<JourneyPoint>().SetSessionNum(highestSessionNum + 1);
		name = "JourneyPoint_" + mySessionNum.ToString();
		*/
	}
}
