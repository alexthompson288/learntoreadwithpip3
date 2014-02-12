using UnityEngine;
using System.Collections;

public class BingoSlot : MonoBehaviour {
	public delegate void BingoClick(BingoSlot slot);
	public event BingoClick OnBingoClick;

	[SerializeField]
	private string m_offSpriteName;
	[SerializeField]
	private string m_onSpriteName;
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private UISprite m_sprite;

	string m_word;

	bool m_isOn;

	void Start()
	{
		m_sprite.spriteName = m_offSpriteName;
		m_isOn = false;
	}

	public void SetUp(string word)
	{
		m_word = word;
		m_label.text = m_word;
	}

	void OnClick()
	{
		Debug.Log("OnClick");
		if(OnBingoClick != null)
		{
			OnBingoClick(this);
		}
	}

	public void ChangeToOnSprite()
	{
		m_sprite.spriteName = m_onSpriteName;
		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEARS");
	}

	public string GetWord()
	{
		return m_word;
	}

	public bool GetIsOn()
	{
		return m_isOn;
	}
}
