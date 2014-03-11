using UnityEngine;
using System.Collections;
using System;
using Wingrove;

public class PipButton : MonoBehaviour 
{
	[SerializeField]
	private bool m_changeColor;
	[SerializeField]
	private Color m_pressedColor = Color.black;
	[SerializeField]
	private bool m_changeSprite;
	[SerializeField]
	private string m_pressedSpriteName;
	[SerializeField]
	private UISprite m_sprite;
	//[SerializeField]
	//private bool m_useOffset = true;
	[SerializeField]
	private float m_minDuration = 0.15f;
	[SerializeField]
	private Vector3 m_offset = new Vector3(0, -15, 0);
	[SerializeField]
	private string m_pressedAudio = "BUTTON_PRESS";
	[SerializeField]
	private string m_unpressedAudio = "BUTTON_UNPRESS";


	Color m_unpressedColor;
	string m_unpressedSpriteName;

	bool m_setLocalPos;
	Vector3 m_defaultLocalPos;

	float m_timeElapsed = 0;

	void Awake () 
	{
		if(m_changeColor)
		{
			m_unpressedColor = m_sprite.color;

			if(Mathf.Approximately(m_pressedColor.r, m_unpressedColor.r) && Mathf.Approximately(m_pressedColor.g, m_unpressedColor.g) && Mathf.Approximately(m_pressedColor.b, m_unpressedColor.b))
			{
				m_pressedColor = m_unpressedColor;

				for(int i = 0; i < 3; ++i) // Only change 0(r), 1(g), 2(b). 3 is alpha.
				{
					m_pressedColor[i] = Mathf.Clamp01(m_pressedColor[i] - 0.2f);
				}
			}
		}

		if(m_changeSprite)
		{
			m_unpressedSpriteName = m_sprite.spriteName;

			if(String.IsNullOrEmpty(m_pressedSpriteName))
			{
				m_pressedSpriteName = m_unpressedSpriteName.Substring(0, m_unpressedSpriteName.Length - 1) + "b";

				Debug.Log("m_pressedSpriteName: " + m_pressedSpriteName);
				Debug.Log("m_unpressedSpriteName: " + m_unpressedSpriteName);
			}
		}
	}

	// V1: Doesn't care if you keep button held down
	/*
	void OnClick()
	{
		m_defaultLocalPos = transform.localPosition;

		if(m_changeColor)
		{
			m_sprite.color = m_pressedColor;
		}

		if(m_changeSprite)
		{
			m_sprite.spriteName = m_pressedSpriteName;
		}

		WingroveAudio.WingroveRoot.Instance.PostEvent(m_audioEvent);

		StartCoroutine(Unpress());
	}

	IEnumerator Unpress()
	{
		yield return new WaitForSeconds(m_minDuration);
		
		if(m_changeColor)
		{
			m_sprite.color = m_unpressedColor;
		}
		
		if(m_changeSprite)
		{
			m_sprite.spriteName = m_unpressedSpriteName;
		}
	}
	*/

	// V2: Button doesn't stay pressed long enough if released quickly
	/*
	void OnPress(bool pressed)
	{
		if(pressed && !m_setLocalPos)
		{
			m_defaultLocalPos = transform.localPosition;
			m_setLocalPos = true;
		}

		TweenPosition.Begin(gameObject, m_minDuration, pressed ? m_defaultLocalPos + m_offset : m_defaultLocalPos).method = UITweener.Method.EaseInOut;

		if(m_changeColor)
		{
			m_sprite.color = pressed ? m_pressedColor : m_unpressedColor;
		}

		if(m_changeSprite)
		{
			m_sprite.spriteName = pressed ? m_pressedSpriteName : m_unpressedSpriteName;
		}

		WingroveAudio.WingroveRoot.Instance.PostEvent(pressed ? m_pressedAudio : m_unpressedAudio);
	}
	*/

	void OnPress(bool pressed)
	{
		if(pressed)
		{
			if(!m_setLocalPos) // We set the default local pos on first press instead of Awake/Start so that we can use this on grid items (ie. We don't want m_defaultLocalPos to be set before the grid repositions)
			{
				m_defaultLocalPos = transform.localPosition;
				m_setLocalPos = true;
			}

			m_timeElapsed = 0;
			StartCoroutine(StartTimer());
		}

		StartCoroutine(OnPressCo(pressed));
	}

	IEnumerator StartTimer()
	{
		while(m_timeElapsed < m_minDuration)
		{
			m_timeElapsed += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator OnPressCo(bool pressed)
	{
		while(!pressed && m_timeElapsed < m_minDuration)
		{
			yield return null;
		}
		
		TweenPosition.Begin(gameObject, m_minDuration, pressed ? m_defaultLocalPos + m_offset : m_defaultLocalPos).method = UITweener.Method.EaseInOut;
		
		if(m_changeColor)
		{
			m_sprite.color = pressed ? m_pressedColor : m_unpressedColor;
		}
		
		if(m_changeSprite)
		{
			m_sprite.spriteName = pressed ? m_pressedSpriteName : m_unpressedSpriteName;
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent(pressed ? m_pressedAudio : m_unpressedAudio);
	}
}
