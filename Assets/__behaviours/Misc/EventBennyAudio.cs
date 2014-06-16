using UnityEngine;
using System.Collections;

// TODO: Delete, this has been deprecated
public class EventBennyAudio : MonoBehaviour 
{
	public delegate void Click();
	public event Click OnClickEvent;

	void OnClick()
	{
		if(OnClickEvent != null)
		{
			OnClickEvent();
		}
	}
}
