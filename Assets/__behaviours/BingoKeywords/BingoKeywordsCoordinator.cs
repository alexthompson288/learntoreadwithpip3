using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BingoKeywordsCoordinator : Singleton<BingoKeywordsCoordinator> {
	[SerializeField]
	private int m_numSlots;
	[SerializeField]
	private BingoKeywordsPlayer[] m_gamePlayers;
	[SerializeField]
	private float m_newWordDelay;

	DataRow m_currentWordData;

	List<DataRow> m_wordsPool = new List<DataRow>();

	List<DataRow> m_slotWords = new List<DataRow>();
	Dictionary<string, DataRow> m_nonSlotWords = new Dictionary<string, DataRow>();

	int m_winningPlayerIndex = -1;

	public int GetNumPlayers()
	{
		return SessionInformation.Instance.GetNumPlayers();
	}

	public void CharacterSelected(int characterIndex)
	{
		for (int i = 0; i < GetNumPlayers(); ++i)
		{
			m_gamePlayers[i].HideCharacter(characterIndex);
		}
	}

	List<DataRow> GetWordsPool()
	{
		int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int difficulty = SessionInformation.Instance.GetDifficulty();
		Debug.Log("sectionIds[difficulty]: " + sectionIds[difficulty]);
		return GameDataBridge.Instance.GetSectionWords(sectionIds[difficulty]).Rows;
	}

	void SetWordsPool()
	{
		int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int difficulty = SessionInformation.Instance.GetDifficulty();
		Debug.Log("sectionIds[difficulty]: " + sectionIds[difficulty]);
		m_wordsPool = GameDataBridge.Instance.GetSectionWords(sectionIds[difficulty]).Rows;
	}

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("SELECT_CHARACTER");

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		SetWordsPool();

		//List<DataRow> wordsPool = GetWordsPool();



		StartCoroutine(PlayGame());
	}

	IEnumerator PlayGame ()
	{
		int numPlayers = GetNumPlayers();
		
		while (true)
		{
			bool allSelected = true;
			for(int i = 0; i < numPlayers; ++i)
			{
				if (!m_gamePlayers[i].HasSelectedCharacter())
				{
					allSelected = false;
				}
			}
			
			if (allSelected)
			{
				break;
			}
			
			yield return null;
		}
		
		yield return new WaitForSeconds(2.0f);
		
		for (int i = 0; i < numPlayers; ++i)
		{
			m_gamePlayers[i].HideAll();
		}
		
		yield return new WaitForSeconds(1.0f);

		for(int i = 0; i < numPlayers; ++i)
		{
			m_gamePlayers[i].SpawnSlots(m_numSlots, m_wordsPool);
		}

		yield return new WaitForSeconds(1.0f);

		while(m_winningPlayerIndex == -1)
		{
			int index = Random.Range(0, m_wordsPool.Count - 1);
			m_currentWordData = m_wordsPool[index];
			//m_wordsPool.RemoveAt(index);

			if(m_wordsPool.Count == 0)
			{
				SetWordsPool();
			}

			SpeakCurrentWord();

			yield return new WaitForSeconds(m_newWordDelay);
		}

		SessionInformation.Instance.SetWinner(m_winningPlayerIndex);
		yield return new WaitForSeconds(1.5f);
		TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
	}
	
	public void SetWinningPlayerIndex(int winningPlayerIndex)
	{
		m_winningPlayerIndex = winningPlayerIndex;
	}
	
	public void SpeakCurrentWord()
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWordData["word"].ToString());
		GetComponent<AudioSource>().clip = loadedAudio;
		GetComponent<AudioSource>().Play();
	}

	public string GetCurrentWord()
	{
		return m_currentWordData["word"].ToString();
	}
}
