using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuyCoordinator<T> : Singleton<T> where T : Object
{
	[SerializeField]
	private UICamera m_myCam;

	UICamera m_parentGateCam = null;

	List<UICamera> m_disabledUICams = new List<UICamera>();

	protected void DisableUICams()
	{
		if(m_parentGateCam == null)
		{
			m_parentGateCam = ParentGate.Instance.GetUICam();
		}

		UICamera[] uiCams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];
		foreach(UICamera cam in uiCams)
		{
			if(cam != m_myCam && cam != m_parentGateCam && cam.enabled)
			{
				cam.enabled = false;
				m_disabledUICams.Add(cam);
			}
		}
	}
	
	protected void EnableUICams()
	{
		foreach(UICamera cam in m_disabledUICams)
		{
			cam.enabled = true;
		}
		
		m_disabledUICams.Clear();
	}
}
