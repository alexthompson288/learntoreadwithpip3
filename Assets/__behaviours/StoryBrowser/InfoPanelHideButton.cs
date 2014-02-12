using UnityEngine;
using System.Collections;

public class InfoPanelHideButton : MonoBehaviour {

	void OnClick()
	{
		InfoPanelBox.Instance.Hide();
	}
	
	void OnPress(bool press)
	{
	}
	
	void OnDrag(Vector2 drag)
	{
		
	}
}
