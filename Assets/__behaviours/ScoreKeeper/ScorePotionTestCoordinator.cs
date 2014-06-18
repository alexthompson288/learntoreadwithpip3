using UnityEngine;
using System.Collections;
using Wingrove;

public class ScorePotionTestCoordinator : MonoBehaviour 
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private GameObject m_widgetPrefab;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;

	// Use this for initialization
	void Start () 
    {
        m_scoreKeeper.SetTargetScore(m_locators.Length);

	    foreach (Transform locator in m_locators)
        {
            GameObject newWidget = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_widgetPrefab, locator);
            GameWidget widget = newWidget.GetComponent<GameWidget>() as GameWidget;

            if(widget != null)
            {
                widget.AllReleaseInteractions += OnWidgetClick;
            }
        }
	}
	
	void OnWidgetClick(GameWidget widget)
    {
        Debug.Log("OnWidgetClick()");
        StartCoroutine(m_scoreKeeper.UpdateScore(widget.gameObject, 1));
    }
}
