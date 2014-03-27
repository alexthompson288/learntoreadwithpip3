using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PictureGameCoordinator : Singleton<PictureGameCoordinator> 
{
    [SerializeField]
    private PictureGamePlayer[] m_gamePlayers;

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
		Debug.Log("PictureGameCoordinator.Start()");

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        UnityEngine.Random.seed = DateTime.Now.Millisecond;

		m_dataRows = GameDataBridge.Instance.GetWords();
		//m_dataRows = GameDataBridge.Instance.GetSectionWords(1394).Rows;

		Debug.Log("There are " + m_dataRows.Count + " rows");

        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SELECT_CHARACTER");

		PipPadBehaviour.Instance.SetDismissable(true);

		if(m_dataRows.Count > 0)
		{
        	StartCoroutine(PlayGame());
		}
		else
		{

			StartCoroutine(FinishGame(0));
		}
    }

    public void SpeakWord(string word)
    {
        Resources.UnloadUnusedAssets();
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(word);
        GetComponent<AudioSource>().clip = loadedAudio;
        GetComponent<AudioSource>().Play();
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

        yield return new WaitForSeconds(1.0f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_WORD_INSTRUCTION");
        yield return new WaitForSeconds(4.0f);
		
		if ( numPlayers == 2 )
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_WORD_INSTRUCTION_MULTI");
			yield return new WaitForSeconds(3.0f);
		}

        for (int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].HideAll();
        }

        yield return new WaitForSeconds(1.0f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("READY_STEADY_GO");

        yield return new WaitForSeconds(1.0f);

		Debug.Log("m_gamePlayers.Length: " + m_gamePlayers.Length);
		Debug.Log("numPlayers: " + numPlayers);

        for (int index = 0; index < numPlayers; ++index)
        {
			Debug.Log("m_gamePlayers[" + index + "]" + m_gamePlayers[index]);
            m_gamePlayers[index].StartGame();
        }

        //float time = 0;
        //while (time < 60.0f)
        //{
        //    time += Time.deltaTime;
        //    for (int index = 0; index < numPlayers; ++index)
        //    {
        //        m_gamePlayers[index].SetTimer((60 - time) / 60.0f);
        //    }
        //    yield return null;
        //}

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

		StartCoroutine(FinishGame(winningIndex));
    }

	IEnumerator FinishGame(int winningIndex)
	{
		yield return null;

		if (winningIndex != -1)
		{
			PipHelpers.SetDefaultPlayerVar();
			PipHelpers.OnGameFinish();
		}
		else
		{
			if(Game.session == Game.Session.Premade)
			{
				SessionManager.Instance.OnGameFinish(false);
			}
			else
			{
				yield return new WaitForSeconds(1.5f);
				int numPlayers = SessionInformation.Instance.GetNumPlayers();
				for (int index = 0; index < numPlayers; ++index)
				{
					m_gamePlayers[index].ShowRetryPrompt();
				}
			}
		}
	}

    
}
