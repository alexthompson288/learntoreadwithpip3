using UnityEngine;
using System.Collections;

public abstract class ChooseContentButton : MonoBehaviour 
{
	public delegate void ButtonClick();
	public event ButtonClick OnButtonClick;

	[SerializeField]
	protected UILabel m_label;
	[SerializeField]
	protected UISprite m_background;
	
	protected DataRow m_data;
	
	public abstract bool CheckData();
	public abstract void AddData();
	public abstract void RemoveData();

	protected void CallOnButtonClick()
	{
		if(OnButtonClick != null)
		{
			OnButtonClick();
		}
	}

}
