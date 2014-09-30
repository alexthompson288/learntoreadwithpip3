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

	void OnPress (bool pressed) 
	{
        if (!pressed)
        {
            ////////D.Log("CallNavMenu.OnPress(false)");
            PipGameBuildSettings gameSettings = (PipGameBuildSettings)(SettingsHolder.Instance.GetSettings());
    		
            if (gameSettings.m_disableNavMenu)
            {
                TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
                if (ts != null)
                {
                    ts.ChangeLevel(gameSettings.m_startingSceneName, false);
                }
            } 
            else
            {
                ////////D.Log("Calling");
                NavMenu.Instance.Call(m_menuType);
            }
        }
	}
}
