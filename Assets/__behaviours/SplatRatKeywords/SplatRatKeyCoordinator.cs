using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplatRatKeyCoordinator : Singleton<SplatRatKeyCoordinator> 
{	
	[SerializeField]
	SplatRatKeyPlayer[] m_gamePlayers;
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
	
    Dictionary<DataRow, AudioClip> m_keywordAudio = new Dictionary<DataRow, AudioClip>();
	
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
		string enviro = EnviroManager.Instance.GetEnvironment().ToString();
		SplatRatEnviro ratEnviro = Resources.Load<SplatRatEnviro>(System.String.Format("SplatRat/{0}_SplatRat", enviro));
		
		if(ratEnviro != null)
		{
			Debug.Log("Found ratEnviro");

			Texture2D frontTex = ratEnviro.GetFrontTex();
			Texture2D rearTex = ratEnviro.GetRearTex();
			
			foreach(SplatRatKeyPlayer player in m_gamePlayers)
			{
				player.SetTextures(frontTex, rearTex);
			}
		}
		else
		{
			Debug.LogError("ratEnviro is null");
		}

		Debug.Log("SplatRatKeyCoordinator.Start()");
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
		
		List<DataRow> lettersPool = new List<DataRow>();


		lettersPool.AddRange(GameDataBridge.Instance.GetKeywords());
		//lettersPool = GameDataBridge.Instance.GetSectionWords(1414).Rows;

		
		Debug.Log("lettersPool.Count: " + lettersPool.Count);
		

        foreach (DataRow myPh in lettersPool)
        {
			Debug.Log("word: " + myPh["word"].ToString());
            string audioFilename = string.Format("{0}",
                myPh["word"]);
			
			m_keywordAudio[myPh] = LoaderHelpers.LoadAudioForWord(audioFilename);
        }
		
		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Session)
		{
			foreach(DataRow letter in lettersPool)
			{
				if(letter["is_target_word"] != null && letter["is_target_word"].ToString() == "t")
				{
					Debug.Log("Found target: " + letter["word"].ToString());
					m_currentLetterData = letter;
					break;
				}
			}
		}
		else if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Custom)
		{
			m_currentLetterData = LessonInfo.Instance.GetTargetData(LessonInfo.DataType.Keywords);
		}
		
		if(m_currentLetterData == null) // Even if we are in the Voyage, we might need to execute this if a database error means that there is no target phoneme
		{
			int selectedIndex = Random.Range(0, lettersPool.Count);
			m_currentLetterData = lettersPool[selectedIndex];
			Debug.Log("Random target: " + m_currentLetterData["word"].ToString());
		}

		m_currentLetter = m_currentLetterData["word"].ToString();
		

        AudioClip bennyChangeableAudio = m_keywordAudio[m_currentLetterData];
        

		
		for(int index = 0; index < m_bennyTheBooks.Length; ++index)
		{
			m_bennyTheBooks[index].SetUp("SPLAT_RAT_KEYWORDS_INSTRUCTION", 4.8f);
			m_bennyTheBooks[index].SetChangeableInstruction(bennyChangeableAudio);
		}

		if(lettersPool.Count > 0)
		{
			StartCoroutine(PlayGame(lettersPool));
		}
		else
		{
			Debug.Log("No words found");
			FinishGame(0);
		}
	}
	
	IEnumerator PlayGame(List<DataRow> lettersPool)
    {
        int numPlayers = GetNumPlayers();
		Debug.Log("numPlayers: " + numPlayers);
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
			StartCoroutine(m_gamePlayers[index].DisplayLargeBlackboard(m_currentLetter));
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_RAT_KEYWORDS_INSTRUCTION");
		
		yield return new WaitForSeconds(2.6f);
		
		StartCoroutine(PlayLetterSound());
		
		yield return new WaitForSeconds(1.5f);
		
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

		yield return new WaitForSeconds(1.5f);
        
		FinishGame(winningIndex);
    }

	void FinishGame(int winningIndex)
	{
		SessionInformation.Instance.SetWinner(winningIndex);

		PipHelpers.OnGameFinish();
	}
	
	void StopPlayer(int index)
	{
		m_gamePlayers[index].StopGame();
	}
	
	public void GiveHint(Blackboard blackBoard) // TODO: Move this function to SplatRatKeyPlayer
	{
		blackBoard.ShowImage(null, m_currentLetter, null, null);
		
        WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
			
        StartCoroutine(PlayLetterSound());
	}
	
	public IEnumerator PlayLetterSound()
    {
        m_audioSource.clip = m_keywordAudio[m_currentLetterData];
        m_audioSource.Play();

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
