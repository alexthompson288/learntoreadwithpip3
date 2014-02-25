using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplatRatGameCoordinator : Singleton<SplatRatGameCoordinator> {
	
	[SerializeField]
	SplatRatGamePlayer[] m_gamePlayers;
	[SerializeField]
	private AudioSource m_audioSource;
	
	[SerializeField]
    private int m_targetScore = 10;

	[SerializeField]
	private int m_percentageProbabilityLetterIsTarget;
	
	[SerializeField]
    private int[] m_difficultySections;
	
	[SerializeField]
	private bool m_waitForBoth;
	int m_numWaitForPlayers;
	
	[SerializeField]
	private ChangeableBennyAudio[] m_bennyTheBooks;
	
	string m_currentLetter = null;
	DataRow m_currentLetterData = null;
	
	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
    Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();
	
	public int GetNumPlayers()
    {
        return SessionInformation.Instance.GetNumPlayers();
    }
	
	public void CharacterSelected(int characterIndex)
    {
        for (int index = 0; index < GetNumPlayers(); ++index)
        {
            m_gamePlayers[index].HideCharacter(characterIndex);
        }
    }

	// Use this for initialization
	IEnumerator Start () 
	{	
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("SELECT_CHARACTER");
		
		m_percentageProbabilityLetterIsTarget = Mathf.Clamp(m_percentageProbabilityLetterIsTarget, 0, 100);
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		if(m_waitForBoth && GetNumPlayers() == 2)
		{
			m_numWaitForPlayers = 2;
		}
		else
		{
			m_numWaitForPlayers = 1;
		}

		List<DataRow> lettersPool = GameDataBridge.Instance.GetLetters();
		//List<DataRow> lettersPool = GameDataBridge.Instance.GetSectionLetters(1405);

		Debug.Log("lettersPool.Count: " + lettersPool.Count);

		/*
			int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
			int difficulty = SessionInformation.Instance.GetDifficulty();
			int endIndex = sectionIds.Length - 2 + difficulty;
			
			for(int index = 0; index < endIndex; ++index)
			{
				int sectionId = sectionIds[index];
				DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
				lettersPool.AddRange(dt.Rows);
			}
			*/

		/*
		List<DataRow> lettersPool = new List<DataRow>();

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Session)
		{
			int sectionId = SessionManager.Instance.GetCurrentSectionId();
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
			lettersPool.AddRange(dt.Rows);
		}
		else if(ContentInformation.Instance.UseCustom())
		{
			lettersPool = ContentInformation.Instance.GetLetters();
		}
		else
		{


			int setNum = SkillProgressInformation.Instance.GetCurrentLevel();

			lettersPool.AddRange(GameDataBridge.Instance.GetInclusiveSetData(setNum, "setphonemes", "phonemes"));
		}
		*/

        foreach (DataRow myPh in lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));
			
			Debug.Log("phoneme: " + myPh["phoneme"]);

            m_phonemeImages[myPh] = (Texture2D)Resources.Load(imageFilename);

            string audioFilname = string.Format("{0}",
                myPh["grapheme"]);

            m_graphemeAudio[myPh] = AudioBankManager.Instance.GetAudioClip(audioFilname);
            m_longAudio[myPh] = LoaderHelpers.LoadMnemonic(myPh);
        }
		

		if(lettersPool.Count > 0)
		{
			if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Session)
			{
				foreach(DataRow letter in lettersPool)
				{
					if(letter["is_target_phoneme"] != null && letter["is_target_phoneme"].ToString() == "t")
					{
						m_currentLetterData = letter;
						break;
					}
				}
			}
			else if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Custom)
			{
				m_currentLetterData = LessonInfo.Instance.GetTargetData(LessonInfo.DataType.Letters);
			}

			if(m_currentLetterData == null) // Even if we are in the Voyage, we might need to execute this if a database error means that there is no target phoneme
			{
				Debug.Log("m_currentLetterData was null so getting random");
				int selectedIndex = Random.Range(0, lettersPool.Count);
				m_currentLetterData = lettersPool[selectedIndex];
			}

			m_currentLetter = m_currentLetterData["phoneme"].ToString();
			
			AudioClip bennyChangeableAudio;
			if (m_longAudio[m_currentLetterData] != null)
	        {
	            bennyChangeableAudio = m_longAudio[m_currentLetterData];
	        }
	        else
	        {
	            bennyChangeableAudio = m_graphemeAudio[m_currentLetterData];
	        }
			
			Debug.Log("change: " + bennyChangeableAudio);
			
			for(int index = 0; index < m_bennyTheBooks.Length; ++index)
			{
				m_bennyTheBooks[index].SetUp("SPLAT_THE_RAT_INSTRUCTION", 4.8f);
				m_bennyTheBooks[index].SetChangeableInstruction(bennyChangeableAudio);
			}
			
			StartCoroutine(PlayGame(lettersPool));
		}
		else
		{
			StartCoroutine(OnFinish());
		}
	}
	
	IEnumerator PlayGame(List<DataRow> lettersPool)
    {
        int numPlayers = GetNumPlayers();

        while (true)
        {
            bool allSelected = true;
            for(int index = 0; index < numPlayers; ++index)
            {
                if (!m_gamePlayers[index].HasSelectedCharacter())
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

        for (int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].HideAll();
        }
		
		yield return new WaitForSeconds(1.0f);
		
		for(int index = 0; index < numPlayers; ++index)
		{
			Debug.Log("Looping to display");
			StartCoroutine(m_gamePlayers[index].DisplayLargeBlackboard(m_phonemeImages[m_currentLetterData], m_currentLetter, m_currentLetter));
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_THE_RAT_INSTRUCTION");
		
		yield return new WaitForSeconds(4.8f);
		
		StartCoroutine(PlayLetterSound());
		
		yield return new WaitForSeconds(3.5f);
		
		for(int index = 0; index < numPlayers; ++index)
		{
			m_gamePlayers[index].HideLargeBlackboard();
		}

        WingroveAudio.WingroveRoot.Instance.PostEvent("READY_STEADY_GO");

        yield return new WaitForSeconds(1.0f);
		
		for(int index = 0; index < numPlayers; ++index)
		{
			m_gamePlayers[index].SetScoreBar(m_targetScore);
			m_gamePlayers[index].SpawnSplattables(lettersPool);
		}
		
		int winningIndex = -1;
		
		while(true)
        {
			int numFinished = 0;
			for(int index = 0; index < numPlayers; ++index)
			{
				if(m_gamePlayers[index].GetScore() >= m_targetScore)
				{
					winningIndex = index;
					StopPlayer(index);
					++numFinished;
				}
			}
			
			if(numFinished >= m_numWaitForPlayers)
			{
				for(int index = 0; index < numPlayers; ++index)
				{
					m_gamePlayers[index].StopGame();
				}
				break;
			}

            yield return null;
        } 
        
		StartCoroutine(OnFinish(winningIndex));
    }

	IEnumerator OnFinish(int winningIndex = 0)
	{
		SessionInformation.Instance.SetWinner(winningIndex);

		yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());

		PipHelpers.OnGameFinish();		
	}
	
	void StopPlayer(int index)
	{
		m_gamePlayers[index].StopGame();
	}
	
	public void GiveHint(Blackboard blackBoard) 
	{
		blackBoard.ShowImage(m_phonemeImages[m_currentLetterData],
			         		 m_currentLetterData["phoneme"].ToString(),
                             m_currentLetter);
		
        WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
			
        StartCoroutine(PlayLetterSound());
	}
	
	public IEnumerator PlayLetterSound(bool tryLong = true)
    {
        if (m_longAudio[m_currentLetterData] != null && tryLong)
        {
            m_audioSource.clip = m_longAudio[m_currentLetterData];
            m_audioSource.Play();
        }
        else
        {
            m_audioSource.clip = m_graphemeAudio[m_currentLetterData];
            m_audioSource.Play();
        }

        yield break;
    }
	
	public string GetCurrentLetter()
	{
		return m_currentLetter;
	}
	
	public int GetPercentageProbabilityLetterIsTarget()
	{
		return m_percentageProbabilityLetterIsTarget;
	}
}
