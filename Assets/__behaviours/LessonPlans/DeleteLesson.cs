using UnityEngine;
using System.Collections;

public class DeleteLesson : MonoBehaviour 
{
	void OnClick()
	{
		LessonInfo.Instance.DeleteCurrentLesson();
	}
}
