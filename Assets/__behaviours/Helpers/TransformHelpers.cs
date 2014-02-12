using UnityEngine;
using System.Collections;

public static class TransformHelpers 
{
	public static void SetLocalPosY(Transform tra, float newPosY)
	{
		Vector3 localPos = tra.localPosition;
		localPos.y = newPosY;
		tra.localPosition = localPos;
	}

	public static void SetLocaPosY(GameObject go, float newPosY)
	{
		Vector3 localPos = go.transform.localPosition;
		localPos.y = newPosY;
		go.transform.localPosition = localPos;
	}

	public static bool ApproxLocalScale2D(Transform tra, Vector3 vec)
	{
		return (Mathf.Approximately(tra.localScale.x, vec.x) && Mathf.Approximately(tra.localScale.y, vec.y));
	}
}
