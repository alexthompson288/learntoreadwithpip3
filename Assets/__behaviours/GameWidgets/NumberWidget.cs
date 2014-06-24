using UnityEngine;
using System.Collections;

public class NumberWidget : GameWidget
{
	[SerializeField]
	private bool m_showDots;
	[SerializeField]
	private DigitDisplay m_digitDisplay;
	[SerializeField]
	private Transform m_digitSpritesParent;
	[SerializeField]
	private UISprite[] m_digitSprites;

	void Awake()
	{
		System.Array.Sort (m_digitSprites, CollectionHelpers.LeftToRight);
	}

	enum DigitDisplay
	{
		None,
		Sprite,
		Label
	}

	public override void SetUp(DataRow number)
	{
		m_data = number;
		
		int value = System.Convert.ToInt32(number ["value"]);
		string valueString = value.ToString ();

		if (m_icon != null) 
		{
			m_icon.gameObject.SetActive(m_showDots);

			UISprite iconSprite = m_icon as UISprite;
			if(iconSprite != null)
			{
				iconSprite.spriteName = valueString + "_dots";
			}
		}

		foreach (UISprite sprite in m_digitSprites) 
		{
			sprite.gameObject.SetActive (m_digitDisplay == DigitDisplay.Sprite);
		}

		m_label.gameObject.SetActive (m_digitDisplay == DigitDisplay.Label);

		if (m_digitDisplay == DigitDisplay.Sprite) 
		{
			string[] spriteNames = DataHelpers.GetNumberSpriteNames (m_data).ToArray ();

			for (int i = 0; i < m_digitSprites.Length; ++i) 
			{
				if (i < spriteNames.Length) 
				{
					m_digitSprites [i].spriteName = spriteNames [i];
				} 
				else 
				{
					m_digitSprites [i].gameObject.SetActive (false);
				}
			}

			if (spriteNames.Length == 1) 
			{
				m_digitSprites [0].transform.localPosition = new Vector3 (0, m_digitSprites [0].transform.localPosition.y);
			}
			else if (spriteNames.Length == 2) 
			{
				float delta = m_digitSprites [1].transform.localPosition.x - m_digitSprites [0].transform.localPosition.x;
				m_digitSprites [0].transform.localPosition = new Vector3 (-delta / 2, m_digitSprites [0].transform.localPosition.y);
				m_digitSprites [1].transform.localPosition = new Vector3 (delta / 2, m_digitSprites [0].transform.localPosition.y);
			}
		} 
		else if (m_digitDisplay == DigitDisplay.Label) 
		{
			m_label.text = DataHelpers.GetLabelText (m_data);
		} 
		else if (m_digitDisplay == DigitDisplay.None) 
		{
			m_icon.transform.localPosition = Vector3.zero;
		}

		if (!m_showDots || m_icon == null) 
		{
			m_digitSpritesParent.localPosition = Vector3.zero;
			m_label.transform.localPosition = Vector3.zero;
		}

		SetUpBackground();
	}
}
