using UnityEngine;
using System.Collections;

public class CallNavMenu : MonoBehaviour 
{
	[SerializeField]
	private bool m_multiplayerButton = false;
	[SerializeField]
	private NavMenu.MenuType m_menuType = NavMenu.MenuType.Main;

    void Start()
	{
		if(m_multiplayerButton && SessionInformation.Instance.GetNumPlayers() < 2)
		{
			gameObject.SetActive(false);
		}
	}

	void OnClick () 
	{
        Menu.Instance.On();
	}
}
