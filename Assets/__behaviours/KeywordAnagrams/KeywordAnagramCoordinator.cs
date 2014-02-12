using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class KeywordAnagramCoordinator : MonoBehaviour {

	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private Blackboard m_blackboard;
	[SerializeField]
	private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private Transform[] m_locators;

	int m_score;

	List<DataRow> m_wordsPool = new List<DataRow>();

	DataRow m_currentWord;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int difficulty = SessionInformation.Instance.GetDifficulty();
		m_wordsPool = GameDataBridge.Instance.GetSectionWords(sectionIds[difficulty]).Rows;
	}

	IEnumerator PlayGame ()
	{
		while(m_score == m_targetScore)
		{

		}

		yield return null;
	}
}
