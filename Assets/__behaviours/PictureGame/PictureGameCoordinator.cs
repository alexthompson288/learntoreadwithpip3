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
		D.Log("PictureGameCoordinator.Start()");

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        UnityEngine.Random.seed = DateTime.Now.Millisecond;

		m_dataRows = DataHelpers.GetWords();

		D.Log("There are " + m_dataRows.Count + " words");
        foreach (DataRow word in m_dataRows)
        {
            D.Log(word["word"].ToString());
        }

        if (GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("INSTRUCTION_CHOOSE_CHARACTER");
        } 
        else
        {
            SessionInformation.SetDefaultPlayerVar();
            CharacterSelectionParent.DisableAll();
        }

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

            for (int index = 0; index < numPlayers; ++index)
            {
                m_gamePlayers[index].HideAll();
            }

			WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_READY_STEADY_GO");
			yield return new WaitForSeconds(3.5f);
		}

		D.Log("m_gamePlayers.Length: " + m_gamePlayers.Length);
		D.Log("numPlayers: " + numPlayers);

        for (int index = 0; index < numPlayers; ++index)
        {
			D.Log("m_gamePlayers[" + index + "]" + m_gamePlayers[index]);
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
			SessionInformation.SetDefaultPlayerVar();
			GameManager.Instance.CompleteGame();
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
