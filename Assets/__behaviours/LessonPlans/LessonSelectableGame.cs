using UnityEngine;
using System.Collections;

public class LessonSelectableGame : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_texture;
	[SerializeField]
	private string m_gameSceneName;
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
		LessonGameCoordinator.Instance.OnClickSelectable((Texture2D)m_texture.mainTexture, m_gameSceneName);
	}

	public string GetGameSceneName()
	{
		return m_gameSceneName;
	}
}
