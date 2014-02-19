using UnityEngine;
using System.Collections;

public class CreateLessonDataTypeButton : MonoBehaviour 
{
	[SerializeField]
	private LessonInfo.DataType m_dataType;

	void OnClick()
	{
		LessonContentCoordinator.Instance.ChangeDataType(m_dataType);
	}
}
