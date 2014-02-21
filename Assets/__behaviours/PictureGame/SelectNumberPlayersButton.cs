using UnityEngine;
using System.Collections;

public class SelectNumberPlayersButton : MonoBehaviour {

    [SerializeField]
    private int m_numPlayers = 1;
    [SerializeField]
    private GameMenu m_pictureGameMenu = null;

	// TODO: Remove all references to m_pictureGameMenu when GameMenu has been completely deprecated
    void OnClick()
    {
		/*
		if(m_pictureGameMenu != null)
		{
        	m_pictureGameMenu.SelectPlayerCount(m_numPlayers);
		}

		if(LevelMenuCoordinator.Instance != null)
		{
			LevelMenuCoordinator.Instance.SelectPlayerCount(m_numPlayers);
		}
		*/
		GameMenuCoordinator.Instance.OnChooseNumPlayers(m_numPlayers);
    }
}
