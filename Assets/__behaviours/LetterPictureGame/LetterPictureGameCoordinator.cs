using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LetterPictureGameCoordinator : Singleton<LetterPictureGameCoordinator> {

    [SerializeField]
    private LetterPictureGamePlayer[] m_gamePlayers;
	
	[SerializeField]
    int[] m_difficultySections;

    List<DataRow> m_dataRows = null;

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

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        UnityEngine.Random.seed = DateTime.Now.Millisecond;
		
		int sectionId = m_difficultySections[SessionInformation.Instance.GetDifficulty()];
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
		List<DataRow> lettersPool = dt.Rows;
		Dictionary<DataRow, Texture2D> phonemeImages = new Dictionary<DataRow, Texture2D>();

        foreach (DataRow myPh in lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));

            phonemeImages[myPh] = (Texture2D)Resources.Load(imageFilename);
        }
		
		int numPlayers = GetNumPlayers();
		for(int index = 0; index < numPlayers; ++index)
		{
			m_gamePlayers[index].SetUp(lettersPool, phonemeImages);
		}


        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_WORD_INSTRUCTION");
        yield return new WaitForSeconds(3.0f);

        StartCoroutine(PlayGame());
    }

    public List<DataRow> GetWordList()
    {
        return m_dataRows;
    }

    IEnumerator PlayGame()
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

        WingroveAudio.WingroveRoot.Instance.PostEvent("READY_STEADY_GO");

        yield return new WaitForSeconds(1.0f);

        for (int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].StartGame();
        }

        bool haveAllPlayersFinished = false;
        bool hasAnyFinished = false;
        while (!hasAnyFinished)
        {
            bool allPlayersFinished = true;
            for (int index = 0; index < numPlayers; ++index)
            {
                if (!m_gamePlayers[index].HasFinished())
                {
                    allPlayersFinished = false;
                }
                else
                {
                    hasAnyFinished = true;
                }
            }
            haveAllPlayersFinished = allPlayersFinished;
            yield return null;
        }     

        // if either has won, go to win screen
        int winningIndex = -1;
        for (int index = 0; index < numPlayers; ++index)
        {
            if (m_gamePlayers[index].HasWon())
            {
                winningIndex = index;
            }
        }

        // if neither has won, and in 2 player, it's not the loser that wins
        if ((winningIndex == -1)&&(numPlayers==2))
        {
            for (int index = 0; index < numPlayers; ++index)
            {
                if (m_gamePlayers[index].HasFinished() && !m_gamePlayers[index].HasWon())
                {
                    winningIndex = index == 0 ? 1 : 0;
                }
            }
        }

        if (winningIndex != -1)
        {
            SessionInformation.Instance.SetWinner(winningIndex);
            yield return new WaitForSeconds(1.5f);
            TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers[index].ShowRetryPrompt();
            }
        }
    }

    
}
