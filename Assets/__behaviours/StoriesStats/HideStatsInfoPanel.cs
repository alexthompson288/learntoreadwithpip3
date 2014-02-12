using UnityEngine;
using System.Collections;

public class HideStatsInfoPanel : MonoBehaviour 
{
	[SerializeField]
	private TweenOnOffBehaviour m_tweenScript;
	
	// Use this for initialization
	void OnClick () 
	{
		m_tweenScript.Off();
	}
}
