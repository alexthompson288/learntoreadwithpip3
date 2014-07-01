using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplatRatGameCoordinator : Singleton<SplatRatGameCoordinator> 
{	
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
		string enviro = EnviroManager.Instance.GetEnvironment().ToString();
		SplatRatEnviro ratEnviro = Resources.Load<SplatRatEnviro>(System.String.Format("SplatRat/{0}_SplatRat", enviro));

		if(ratEnviro != null)
		{
			D.Log("Found ratEnviro");
			
			Texture2D frontTex = ratEnviro.GetFrontTex();
			Texture2D rearTex = ratEnviro.GetRearTex();
			
			foreach(SplatRatGamePlayer player in m_gamePlayers)
			{
				player.SetTextures(frontTex, rearTex);
			}
		}
		else
		{
			D.LogError("ratEnviro is null");
		}

        if (GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("INSTRUCTION_CHOOSE_CHARACTER");
        }

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

        if (GetNumPlayers() == 1)
        {
            CharacterSelectionParent.DisableAll();
            SessionInformation.SetDefaultPlayerVar();
        }

		List<DataRow> lettersPool = DataHelpers.GetPhonemes();

		D.Log("lettersPool.Count: " + lettersPool.Count);


        foreach (DataRow myPh in lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));
			
			D.Log("phoneme: " + myPh["phoneme"]);

            m_phonemeImages[myPh] = (Texture2D)Resources.Load(imageFilename);

            string audioFilname = string.Format("{0}",
                myPh["grapheme"]);

            m_graphemeAudio[myPh] = AudioBankManager.Instance.GetAudioClip(audioFilname);
            m_longAudio[myPh] = LoaderHelpers.LoadMnemonic(myPh);
        }
		

		if(lettersPool.Count > 0)
		{
            m_currentLetterData = DataHelpers.GetSingleTargetData("phonemes", lettersPool);

            //UserStats.Activity.AddPhoneme(m_currentLetterData);

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

        if (numPlayers == 2)
        {
            while (true)
            {
                bool allSelected = true;
                for (int index = 0; index < numPlayers; ++index)
                {
                    if (!m_gamePlayers [index].HasSelectedCharacter())
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

            yield return new WaitForSeconds(0.8f);
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_READY_STEADY_GO");
            
            yield return new WaitForSeconds(1.5f);

            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers [index].HideAll();
            }
    		
            yield return new WaitForSeconds(1.0f);
        }
		
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

		for(int index = 0; index < numPlayers; ++index)
		{
			D.Log("Looping to display");
            bool playBenny = index == 0;
            StartCoroutine(m_gamePlayers[index].SetUpBenny(m_currentLetterData, playBenny));
			StartCoroutine(m_gamePlayers[index].DisplayLargeBlackboard(m_phonemeImages[m_currentLetterData], m_currentLetter, m_currentLetter));
		}
		
		//WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_THE_RAT_INSTRUCTION");
		
		//yield return new WaitForSeconds(4.8f);
		
		yield return new WaitForSeconds(3.5f);
		
		for(int index = 0; index < numPlayers; ++index)
		{
			m_gamePlayers[index].HideLargeBlackboard();
		}
		
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

		GameManager.Instance.CompleteGame();		
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

    public void PostOnIncorrect()
    {
        //UserStats.Activity.AddIncorrectPhoneme(m_currentLetterData);
    }
	
	public int GetPercentageProbabilityLetterIsTarget()
	{
		return m_percentageProbabilityLetterIsTarget;
	}
}
