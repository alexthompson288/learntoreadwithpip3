using UnityEngine;
using System.Collections;

public class ChangeImageOnClick : MonoBehaviour 
{
	[SerializeField]
	private UISprite m_sprite;
	[SerializeField]
	private string m_spriteOffName;
	[SerializeField]
	private string m_spriteOnName;
	[SerializeField]
	private UITexture m_texture;
	[SerializeField]
	private Texture2D m_offTexture;
	[SerializeField]
	private Texture2D m_onTexture;
	[SerializeField]
	private float m_offDelay = 0.3f;
	
	void Awake () 
	{
		Off ();
	}

	void OnClick()
	{
		On();
		StopAllCoroutines();
		StartCoroutine(OffAfterDelay());
	}

	void Off()
	{
		if(m_sprite != null)
		{
			//Debug.Log("Changing spriteName: " + m_spriteOffName);
			m_sprite.spriteName = m_spriteOffName;
		}
		
		if(m_texture != null)
		{
			m_texture.mainTexture = m_offTexture;
		}
	}

	void On()
	{
		if(m_sprite != null)
		{
			//Debug.Log("Changing spriteName: " + m_spriteOnName);
			m_sprite.spriteName = m_spriteOnName;
		}
		
		if(m_texture != null)
		{
			m_texture.mainTexture = m_onTexture;
		}
	}

	IEnumerator OffAfterDelay()
	{
		yield return new WaitForSeconds(m_offDelay);
		Off();
	}

}
