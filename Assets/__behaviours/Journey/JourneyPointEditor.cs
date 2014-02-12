using UnityEngine;
//using UnityEditor;

//[CanEditMultipleObjects]
//[CustomEditor(typeof(JourneyPoint))]
//public class JourneyPointEditor : Editor 
public class JourneyPointEditor : MonoBehaviour
{
	/*
	SerializedProperty m_sessionNum;

	void OnEnable ()
	{
		m_sessionNum = serializedObject.FindProperty("m_sessionNum");
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		serializedObject.Update();

		if(GUILayout.Button("Set All Session Nums"))
		{
			JourneyPoint[] journeyPoints = Object.FindObjectsOfType(typeof(JourneyPoint)) as JourneyPoint[];
			foreach(JourneyPoint journeyPoint in journeyPoints)
			{
				string name = journeyPoint.name;
				name = name.Replace("JourneyPoint_", "");
				journeyPoint.SetSessionNum(System.Convert.ToInt32(name));
			}
		}
	}
	*/
}

/*
if(GUILayout.Button("Rename JourneyPoint"))
{
	//serializedObject.targetObject.name = serializedObject.targetObject.name + "_" + m_sessionNum.intValue.ToString();

	Object[] targetObjs = serializedObject.targetObjects;
	int i = 0;
	foreach(Object obj in targetObjs)
	{
		//Debug.Log(i + " sessionNum: " + sessionNum.intValue);
		obj.name =  "JourneyPoint_" + m_sessionNum.intValue.ToString();
		++i;
	}
}



if(GUILayout.Button("Rename all JourneyPoints"))
{
	JourneyPoint[] journeyPoints = Object.FindObjectsOfType(typeof(JourneyPoint)) as JourneyPoint[];
	foreach(JourneyPoint journeyPoint in journeyPoints)
	{
		journeyPoint.name = "JourneyPoint_" + journeyPoint.GetSessionNum().ToString();
	}
}
 */