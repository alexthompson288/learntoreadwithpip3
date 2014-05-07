using UnityEngine;
using System.Collections;

// TODO: Delete PathFollower
public abstract class PathFollower : MonoBehaviour 
{
	public Spline Path;
	protected float m_speedModifier = 1;

	public void SetSpline(Spline path)
	{
		Path = path;
	}
}
