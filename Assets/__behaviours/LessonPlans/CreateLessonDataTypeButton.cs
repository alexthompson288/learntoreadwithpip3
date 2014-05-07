using UnityEngine;
using System.Collections;

public class CreateLessonDataTypeButton : MonoBehaviour 
{
	[SerializeField]
	private string m_dataType;

	static UISprite m_currentButton; // I keep track of current button and color changing here instead of in content coordinator because the code was already done and this didn't need as much refactoring

	void Awake()
	{
		if(m_dataType == "phonemes")
		{
			MakeThisCurrentButton();
		}
	}

	void OnClick()
	{
		LessonContentCoordinator.Instance.ChangeDataType(m_dataType);
		MakeThisCurrentButton();
	}

	void MakeThisCurrentButton()
	{
		if(m_currentButton != null)
		{
			m_currentButton.color = LessonContentCoordinator.Instance.GetDeselectColor();
		}

		m_currentButton = GetComponentInChildren<UISprite>() as UISprite;

		m_currentButton.color = LessonContentCoordinator.Instance.GetSelectColor();
	}
}
