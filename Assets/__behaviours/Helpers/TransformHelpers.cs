using UnityEngine;
using System.Collections;

public static class TransformHelpers 
{
    public static float GetDuration(Transform tra, Vector3 targetPos, float speed)
    {
        return (targetPos - tra.position).magnitude / speed;
    }

	public static bool ApproxPos(GameObject a, GameObject b)
	{
		return ApproxPos(a.transform.position, b.transform.position);
	}

	public static bool ApproxPos(Transform a, Transform b)
	{
		return ApproxPos(a.position, b.position);
	}

	public static bool ApproxPos(Vector3 a, Vector3 b)
	{
		return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
	}

	public static void SetLocalPosY(Transform tra, float newPosY)
	{
		Vector3 localPos = tra.localPosition;
		localPos.y = newPosY;
		tra.localPosition = localPos;
	}

	public static void SetLocalPosY(GameObject go, float newPosY)
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
