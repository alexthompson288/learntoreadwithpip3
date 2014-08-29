using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformHelpers 
{
    public static Transform FindClosest<T>(IList<T> list, Transform target)
    {
        Transform closest = null;

        if (list.Count > 0)
        {
            bool isGameObject = list [0] is GameObject;

            if (isGameObject || list [0] is Component)
            {
                closest = isGameObject ? (list [0] as GameObject).transform : (list [0] as Component).transform;

                float closestDistance = Vector3.Distance(closest.position, target.position);

                foreach (T t in list)
                {
                    Transform tra = isGameObject ? (t as GameObject).transform : (t as Component).transform;

                    if (Vector3.Distance(tra.position, target.position) < closestDistance)
                    {
                        closest = tra;
                        closestDistance = Vector3.Distance(closest.position, target.position);
                    }
                }
            }
        }

        return closest;
    }

    public static void DestroyChildren(Transform parent, string destroyMessage = "")
    {
        int childCount = parent.childCount;

        bool sendMessage = !string.IsNullOrEmpty(destroyMessage);

        for (int i = childCount - 1; i > -1; --i)
        {
            if(sendMessage)
            {
                parent.GetChild(i).SendMessage(destroyMessage);
            }
            else
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }
    }

    public static float GetDuration(Transform tra, Vector3 targetPos, float speed, bool useLocal = false)
    {
        if (useLocal)
        {
            return (targetPos - tra.localPosition).magnitude / speed;
        } 
        else
        {
            return (targetPos - tra.position).magnitude / speed;
        }
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
