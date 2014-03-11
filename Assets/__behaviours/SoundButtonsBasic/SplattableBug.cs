﻿using UnityEngine;
using System.Collections;
using Wingrove;

public class SplattableBug : MonoBehaviour 
{
	[SerializeField]
	private UISprite m_sprite;
	[SerializeField]
	private string m_normalSpriteName;
	[SerializeField]
	private string m_splatSpriteName;
	[SerializeField]
	private ThrobGUIElement m_throbBehaviour;

	public void Splat()
	{
		Debug.Log("Unsplat");
		m_sprite.spriteName = m_splatSpriteName;
		WingroveAudio.WingroveRoot.Instance.PostEvent("HAPPY_GAWP");

		if(m_throbBehaviour != null)
		{
			//m_throbBehaviour.Off(false);
		}
	}

	public void Unsplat()
	{
		Debug.Log("Unsplat");
		m_sprite.spriteName = m_normalSpriteName;
		if(m_throbBehaviour != null)
		{
			//m_throbBehaviour.On();
		}
	}
}