using UnityEngine;
using System.Collections;

public class BingoButton : MonoBehaviour {
	[SerializeField]
	private BingoKeywordsPlayer m_gamePlayer;

	// Use this for initialization
	void Start () 
	{
		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	public void On () 
	{
		TweenScale.Begin(gameObject, 0.5f, Vector3.one);
	}

	void OnClick()
	{
		m_gamePlayer.CheckWin();
	}
}
