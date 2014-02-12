using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StoriesStatsCoordinator : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_comprehensionStatsLabel;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private GameObject m_draggablePanelLabelPrefab;
	[SerializeField]
	private GameObject m_comprehensionParent;
	[SerializeField]
	private GameObject m_pipPadLabel;

	// Use this for initialization
	void Start () 
	{
		string correctCount = UserStoriesStats.Instance.GetCorrectCount().ToString();
		string incorrectCount = (UserStoriesStats.Instance.GetCorrectCount() 
		                         + UserStoriesStats.Instance.GetIncorrectCount()).ToString();

		if(incorrectCount == "0")
		{
			m_comprehensionParent.SetActive(false);
		}
		else
		{
			//m_comprehensionStatsLabel.text = correctCount + " / " + incorrectCount;
			m_comprehensionStatsLabel.text = correctCount + " \\ " + incorrectCount;
		}

		/*
		Dictionary<string, int> pipPadCalls = new Dictionary<string, int>();
		pipPadCalls.Add("hello", 3);
		pipPadCalls.Add("world", 2);
		pipPadCalls.Add("dog", 0);
		pipPadCalls.Add("cat", 0);
		pipPadCalls.Add("brown", 1);
		pipPadCalls.Add("sat", 0);
		pipPadCalls.Add("hat", 0);
		pipPadCalls.Add("cow", 1);
		*/


		Dictionary<string, int> pipPadCalls = UserStoriesStats.Instance.GetPipPadCalls();

		if(pipPadCalls.Count == 0)
		{
			m_grid.gameObject.SetActive(false);
			m_pipPadLabel.SetActive(false);

			m_comprehensionParent.transform.position = m_pipPadLabel.transform.position;
		}
		else
		{
			foreach(KeyValuePair<string, int> kvp in pipPadCalls)
			{
				GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePanelLabelPrefab, m_grid.transform);
				newWord.GetComponent<DraggablePanelLabel>().SetUp(kvp.Key);
				newWord.transform.localScale *= 0.5f;

				GameObject newNumber = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePanelLabelPrefab, m_grid.transform);
				newNumber.GetComponent<DraggablePanelLabel>().SetUp(kvp.Value.ToString());
				newNumber.transform.localScale *= 0.5f;
			}

			m_grid.Reposition();
		}
	}
}
