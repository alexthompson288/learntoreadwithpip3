using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour 
{
	[SerializeField]
	private UISprite m_sprite;
	[SerializeField]
	private float m_duration = 0.3f;
	[SerializeField]
	private Vector3 m_offset = new Vector3(0, -30, 0);
	[SerializeField]
	private string m_audioEvent = "BUTTON_PRESS";
	[SerializeField]
	private VisualFeedback m_visualFeedback;
	[SerializeField]
	private Color m_pressedColor;
	
	Color m_unpressedColor;

	enum VisualFeedback
	{
		Spritename,
		Color
	}

	void Awake () 
	{
		m_unpressedColor = m_sprite.color;

		UIButtonOffset uiButton = GetComponent<UIButtonOffset>() as UIButtonOffset;

		if(uiButton == null)
		{
			uiButton = gameObject.AddComponent<UIButtonOffset>() as UIButtonOffset;
		}
	
		uiButton.pressed = m_offset;
		uiButton.duration = m_duration;
	}
	
	void OnClick()
	{
		switch(m_visualFeedback)
		{
		case VisualFeedback.Color:
			m_sprite.color = m_pressedColor;
			break;
		case VisualFeedback.Spritename:
			//string pressedSpriteName = m_sprite.spriteName;
			//pressedSpriteName[pressedSpriteName.Length - 1] = 'b';
			//m_sprite.spriteName = pressedSpriteName;
			break;
		default:
			break;
		}

		StartCoroutine(Unpress());
	}

	IEnumerator Unpress()
	{
		yield return new WaitForSeconds(m_duration);

		switch(m_visualFeedback)
		{
		case VisualFeedback.Color:
			m_sprite.color = m_unpressedColor;
			break;
		case VisualFeedback.Spritename:
			//string pressedSpriteName = m_sprite.spriteName;
			//pressedSpriteName[pressedSpriteName.Length - 1] = 'a';
			//m_sprite.spriteName = pressedSpriteName;
			break;
		default:
			break;
		}
	}
}
