using UnityEngine;
using System.Collections;

public class ladybug : PathFollower {
	
	//public Spline Path;
	public float TotalTime = 20.0f;
	private float CurrTime = 0.0f;

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
			
			if(CurrTime > TotalTime)
				CurrTime = 0.0f;
		}
	}


}
