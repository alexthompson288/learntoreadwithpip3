using UnityEngine;
using System.Collections;

// TODO: Make an integrated PathFollower class. It needs to have multiple orientation options
public class ladybug : PathFollower 
{
	//public Spline Path;
	public float TotalTime = 20.0f;
	private float CurrTime = 0.0f;

	[SerializeField]
	private Vector3 m_RotationModifier = Vector3.zero;

	// Use this for initialization
	protected override void Start () 
	{
        //CurrTime = Random.Range(0, TotalTime);
		CurrTime = 0;

		base.Start();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Path != null)
		{
			//CurrTime += Time.deltaTime;
			CurrTime += Time.deltaTime * m_speedModifier;
			transform.position = Path.GetPositionOnSpline(CurrTime / TotalTime);
			transform.right = -Path.GetTangentToSpline(CurrTime / TotalTime);

			Vector3 rot = transform.right;
			rot.x += m_RotationModifier.x;
			rot.y += m_RotationModifier.y;
			rot.z += m_RotationModifier.z;
			transform.right = rot;
			
			if(CurrTime > TotalTime)
				CurrTime = 0.0f;
		}
	}


}
