using UnityEngine;
using System.Collections;

public class LessonSelectedGame : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_texture;
	[SerializeField]
	private Texture2D m_defaultTexture;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private Color m_onColor;
	[SerializeField]
	private Color m_offColor;

	bool m_isOn;

	void Start()
	{
		m_texture.mainTexture = m_defaultTexture;
	}

	void OnClick()
	{
		LessonGameCoordinator.Instance.OnClickSelected(this);
	}

	public void SetTexture(Texture2D tex)
	{
		if(tex != null)
		{
			m_texture.mainTexture = tex;
		}
		else
		{
			m_texture.mainTexture = m_defaultTexture;
		}
	}

	public void On(bool isOn)
	{
		//////////D.Log(name + " On(" + isOn + ")");
		m_isOn = isOn;

		if(m_isOn)
		{
			m_background.color = m_onColor;
		}
		else
		{
			m_background.color = m_offColor;
		}
	}

	public bool IsOn()
	{
		return m_isOn;
	}

	public bool HasGame()
	{
		return m_texture.mainTexture != m_defaultTexture;
	}
}
