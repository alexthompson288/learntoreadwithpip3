using UnityEngine;
using System.Collections;

public class LessonSelectableGame : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_texture;
	[SerializeField]
	private string m_gameSceneName;

	void OnClick()
	{
		LessonGameCoordinator.Instance.OnClickSelectable((Texture2D)m_texture.mainTexture, m_gameSceneName);
	}	
}
