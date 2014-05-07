using UnityEngine;
using System.Collections;

public class InstantiateLesson : MonoBehaviour 
{
	void OnClick()
	{
		LessonInfo.Instance.InstantiateLesson();
	}
}
