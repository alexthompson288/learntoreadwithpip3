using UnityEngine;
using System.Collections;

public class SelectDifficultyButton : MonoBehaviour {

    [SerializeField]
    private int m_difficulty;
    [SerializeField]
    private GameMenu m_pictureGameMenu;
    [SerializeField]
    private GameObject m_lockedHierarchy;

    void OnClick()
    {
        m_pictureGameMenu.SelectDifficulty(m_difficulty);
    }
	
	void LateUpdate()
    {
        if (m_difficulty == 0 || (SessionInformation.Instance.GetHighestLevelCompletedForApp() >= m_difficulty))
        {
            collider.enabled = true;
            m_lockedHierarchy.SetActive(false);
        }
        else
        {
            collider.enabled = false;
        }
    }
	
	/*
    void LateUpdate()
    {
        if ((m_difficulty == 0)
            || (SessionInformation.Instance.GetHighestLevelCompletedForGame(
            SessionInformation.Instance.GetSelectedGame()) >= m_difficulty))
        {
            collider.enabled = true;
            m_lockedHierarchy.SetActive(false);
        }
        else
        {
            collider.enabled = false;
        }
    }
    */
}
