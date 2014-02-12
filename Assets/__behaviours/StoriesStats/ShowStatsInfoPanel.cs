using UnityEngine;
using System.Collections;

public class ShowStatsInfoPanel : MonoBehaviour 
{
	[SerializeField]
	private TweenOnOffBehaviour m_tweenScript;

	// Use this for initialization
	void OnClick () 
	{
		Debug.Log("Clicked");
		m_tweenScript.On();
	}
}
