using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LessonGameCoordinator : Singleton<LessonGameCoordinator> 
{
	[SerializeField]
	private List<LessonSelectedGame> m_selectedGames = new List<LessonSelectedGame>();

	int m_currentIndex = 0;

	// Use this for initialization
	void Start () 
	{
		m_selectedGames.Sort(CollectionHelpers.ComparePosX);

		m_selectedGames[m_currentIndex].On(true);

		//List<string> gameNames = LessonInfo.Instance.GetGames();

		//LessonSelectableGame[] selectableGames = Object.FindObjectsOfType(typeof(LessonSelectableGame)) as LessonSelectableGame[];
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Q))
		{
			Debug.Log("GAME NAMES");
			List<string> gameNames = LessonInfo.Instance.GetGames();
			for(int i = 0; i < gameNames.Count; ++i)
			{
				Debug.Log(i + ": " + gameNames[i]);
			}
		}
	}

	public void OnClickSelectable(Texture2D tex, string gameSceneName)
	{
		Debug.Log("Clicked selectable");
		if(m_currentIndex < m_selectedGames.Count)
		{
			m_selectedGames[m_currentIndex].SetTexture(tex);
			m_selectedGames[m_currentIndex].On(false);

			LessonInfo.Instance.AddGame(gameSceneName, m_currentIndex);

			while(m_currentIndex < m_selectedGames.Count && m_selectedGames[m_currentIndex].HasGame())
			{
				Debug.Log("Incrementing through already selected");
				++m_currentIndex;
			}

			if(m_currentIndex < m_selectedGames.Count)
			{
				m_selectedGames[m_currentIndex].On(true);
			}
		}
	}

	public void OnClickSelected(LessonSelectedGame selected)
	{
		int index = m_selectedGames.IndexOf(selected);

		int maxIndex = m_selectedGames.Count - 1;
		for(; maxIndex > -1; --maxIndex)
		{
			if(m_selectedGames[maxIndex].HasGame())
			{
				break;
			}
		}

		if(index <= maxIndex)
		{
			if(m_currentIndex < m_selectedGames.Count)
			{
				m_selectedGames[m_currentIndex].On(false);
			}

			m_currentIndex = index;
			m_selectedGames[m_currentIndex].On(true);
			m_selectedGames[m_currentIndex].SetTexture(null);
			LessonInfo.Instance.RemoveGame(m_currentIndex);
		}
	}
}
