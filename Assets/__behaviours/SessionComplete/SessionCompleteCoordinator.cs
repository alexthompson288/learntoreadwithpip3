using UnityEngine;
using System.Collections;
using Wingrove;

public class SessionCompleteCoordinator : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_sessionCompleteButtonPrefab;
    [SerializeField]
    private UIGrid m_grid;
    [SerializeField]
    private string[] m_spriteNames;

    ThrobGUIElement m_currentThrobBehaviour = null;

	// Use this for initialization
	void Start () 
    {
        int sessionNum = VoyageInfo.Instance.hasBookmark ? VoyageInfo.Instance.currentSessionNum : -1;

        for (int i = 0; i < m_spriteNames.Length; ++i)
        {
            GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionCompleteButtonPrefab, m_grid.transform);
            newButton.GetComponent<ClickEvent>().OnSingleClick += OnButtonClick;
            newButton.GetComponent<ClickEvent>().SetString(m_spriteNames[i]);
            newButton.GetComponentInChildren<UISprite>().spriteName = m_spriteNames[i];

            if(VoyageInfo.Instance.GetSessionBackground(sessionNum) == m_spriteNames[i])
            {
                m_currentThrobBehaviour = newButton.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
                m_currentThrobBehaviour.On();
            }
        }

        m_grid.Reposition();
	}

    void OnButtonClick(ClickEvent click)
    {
        if (m_currentThrobBehaviour != null)
        {
            m_currentThrobBehaviour.Off();
        }

        m_currentThrobBehaviour = click.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
        m_currentThrobBehaviour.On();

        if (VoyageInfo.Instance.hasBookmark)
        {
            VoyageInfo.Instance.AddSessionBackground(VoyageInfo.Instance.currentSessionNum, click.GetString());
        }
    }
}
