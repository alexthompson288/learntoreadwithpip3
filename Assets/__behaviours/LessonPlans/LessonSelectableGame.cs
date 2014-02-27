using UnityEngine;
using System.Collections;

public class LessonSelectableGame : BuyableGame 
{
	[SerializeField]
	private string m_gameUserName;

	void Start()
	{
		if(System.String.IsNullOrEmpty(m_gameUserName))
		{
			m_gameUserName = m_gameSceneName.Replace("New", "");
		}

		UILabel label = GetComponentInChildren<UILabel>() as UILabel;

		if(label != null)
		{
			label.text = m_gameUserName;
		}

		float labelScale = m_gameUserName.Length >= 16 ? 0.14f : 0.2f;
		label.transform.localScale = Vector3.one * labelScale;
	}

	void OnClick()
	{
		if(m_isUnlocked)
		{
			LessonGameCoordinator.Instance.OnClickSelectable((Texture2D)m_gameIcon.mainTexture, m_gameSceneName);
		}
		else
		{
			BuyGamesCoordinator.Instance.Show();
		}
	}
}
