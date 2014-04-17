using UnityEngine;
using System.Collections;

public class CallNavMenu : MonoBehaviour 
{
	[SerializeField]
	private bool m_multiplayerButton = false;
	[SerializeField]
	private MenuType m_menuType = MenuType.Main;

	enum MenuType
	{
		Main,
		Room,
		Buy
	}

	void Start()
	{
		if(m_multiplayerButton && SessionInformation.Instance.GetNumPlayers() < 2)
		{
			gameObject.SetActive(false);
		}
	}

	void OnClick () 
	{
		Debug.Log(name + " - CallNavMenu.OnClick()");
		PipGameBuildSettings gameSettings = (PipGameBuildSettings)(SettingsHolder.Instance.GetSettings());
		
		if(gameSettings.m_disableNavMenu)
		{
			TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
			if (ts != null)
			{
				ts.ChangeLevel(gameSettings.m_startingSceneName, false);
			}
		}
		else
		{
			switch(m_menuType)
			{
			case MenuType.Main:
				NavMenu.Instance.Call();
				break;

			case MenuType.Room:
				NavMenu.Instance.CallRoomMoveable();
				break;

			case MenuType.Buy:
				NavMenu.Instance.CallBuyMoveable();
				break;
			}
		}

	}
}
