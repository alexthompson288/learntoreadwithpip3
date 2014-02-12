using UnityEngine;
using System.Collections;

public class PermaTween : MonoBehaviour 
{
	[SerializeField]
	private GameObject tweeningObject;
	[SerializeField]
	private float tweenMinSpeed;
	[SerializeField]
	private float tweenMaxSpeed;
	[SerializeField]
	private Transform start;
	[SerializeField]
	private Transform end;
	
	Hashtable tweenVariables = new Hashtable();

	void Start()
	{
		tweenVariables.Add("position", end.position);
		tweenVariables.Add("speed", Random.Range(tweenMinSpeed, tweenMaxSpeed));
		tweenVariables.Add("oncomplete", "OnComplete");
		tweenVariables.Add("oncompletetarget", gameObject);
		tweenVariables.Add("easetype", iTween.EaseType.linear);
		
		iTween.MoveTo(tweeningObject, tweenVariables);
	}
	
	void OnComplete()
	{
		tweeningObject.transform.position = start.position;
			iTween.MoveTo(tweeningObject, tweenVariables);
	}
}
