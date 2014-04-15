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

	// Use this for initialization
	void Start () 
    {
        for (int i = 0; i < m_spriteNames.Length; ++i)
        {
            GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionCompleteButtonPrefab, m_grid.transform);
            newButton.GetComponent<ClickEvent>().SetString(m_spriteNames[i]);
            newButton.GetComponentInChildren<UISprite>().spriteName = m_spriteNames[i];
        }

        m_grid.Reposition();
	}

    void OnButtonClick(ClickEvent click)
    {
        if (VoyageInfo.Instance.hasBookmark)
        {
            VoyageInfo.Instance.AddSessionBackground(VoyageInfo.Instance.currentSessionNum, click.GetString());
        }
    }
}
