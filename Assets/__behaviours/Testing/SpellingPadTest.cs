using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellingPadTest : MonoBehaviour {

	List<DataRow> m_wordsPool = new List<DataRow>();
	
	DataRow m_currentWordData;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		int[] sectionIds =((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int difficulty = SessionInformation.Instance.GetDifficulty();
		m_wordsPool = GameDataBridge.Instance.GetSectionWords(sectionIds[difficulty]).Rows;
	}

	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.O))
		{
			m_currentWordData = m_wordsPool[Random.Range(0, m_wordsPool.Count)];

			SpellingPadBehaviour.Instance.DisplayNewWord(m_currentWordData["word"].ToString());
		}
	}
}
