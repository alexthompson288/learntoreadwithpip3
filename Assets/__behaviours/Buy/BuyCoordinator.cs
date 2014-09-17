using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuyCoordinator<T> : Singleton<T> where T : Object
{
	[SerializeField]
	private UICamera m_myCam;

	protected void DisableUICams()
	{
        NGUIHelpers.EnableUICams(false, new UICamera[] { m_myCam, OldParentGate.Instance.GetUICam() });
	}

    public virtual void RefreshBuyButton() {}
}
