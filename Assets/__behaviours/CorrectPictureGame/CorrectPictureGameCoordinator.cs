using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorrectPictureGameCoordinator : Singleton<CorrectPictureGameCoordinator>
{

    [SerializeField]
    private int m_sectionId;
    [SerializeField]
    private int m_scoreTarget = 6;
    [SerializeField]
    private ProgressScoreBar m_progressScoreBar;
    [SerializeField]
    private CharacterPopper m_characterToPop;
    [SerializeField]
    private CharacterPopper m_characterToPopTroll;
    [SerializeField]
    private int[] m_wordsForDifficulty;

    private List<DataRow> m_wordSelection;

    private List<string> m_remainingWords = new List<string>();
    private List<string> m_allWords = new List<string>();
    int m_correctIndex = 0;
    int m_score = 0;
    bool m_gotIncorrect = false;

	int m_maxSpawn = 0;

    DataRow m_currentWordData = null;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_wordSelection = DataHelpers.GetWords();

        m_progressScoreBar.SetStarsTarget(m_scoreTarget);

		for(int i = m_wordSelection.Count - 1; i > -1; --i)
		{
			Debug.Log(m_wordSelection[i]["word"].ToString());
			Texture2D tex = null;
			if(m_wordSelection[i]["image"] != null)
			{
				tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + m_wordSelection[i]["image"].ToString());
			}
			if(tex == null)
			{
				tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + m_wordSelection[i]["word"].ToString());
			}
			Debug.Log("tex: " + tex);
			if(tex != null)
			{
				m_remainingWords.Add(m_wordSelection[i]["word"].ToString());
				m_allWords.Add(m_wordSelection[i]["word"].ToString());
				++m_maxSpawn;
			}
			else
			{
				m_wordSelection.Remove(m_wordSelection[i]);
			}
			
			Resources.UnloadUnusedAssets();
		}

		Debug.Log("Difficulty Spawn: " + m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()]);
		if(m_maxSpawn > m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()])
		{
			m_maxSpawn = m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()];
		}

		if(m_maxSpawn < 2)
		{
			m_maxSpawn = 2;
		}

		Debug.Log("m_maxSpawn: " + m_maxSpawn);

		Debug.Log("m_wordSelect.Count: " + m_wordSelection.Count);
		Debug.Log("m_allWords.Count: " + m_allWords.Count);
		Debug.Log("m_remainingWords.Count: " + m_remainingWords.Count);


        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_PICTURE_INSTRUCTION");
        yield return new WaitForSeconds(3.0f);

		Resources.UnloadUnusedAssets();

		Debug.Log("m_maxSpawn: " + m_maxSpawn);

		if(m_wordSelection.Count >= m_maxSpawn)
		{
        	StartCoroutine(ShowNextQuestion());
		}
		else
		{
			FinishGame();
		}
    }

    IEnumerator ShowNextQuestion()
    {
        yield return new WaitForSeconds(1.5f);

        //if (m_remainingWords.Count >= m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()])
        //{
            m_gotIncorrect = false;
			string selectedWord = null;
			Texture2D targetTex = null;

			Debug.Log("ShowNextQuestion()");

			while(targetTex == null)
			{
				selectedWord = m_remainingWords[Random.Range(0, m_remainingWords.Count)];
				Debug.Log("selectedWord: " + selectedWord);
				targetTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + selectedWord);
			}

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE word='" + selectedWord + "'");
        if (dt.Rows.Count > 0)
        {
            m_currentWordData = dt.Rows[0];
            UserStats.Activity.AddWord(m_currentWordData);
        }

			Debug.Log("Found target text");

            //m_remainingWords.Remove(selectedWord);

            List<string> otherWordList = new List<string>();

            //while (otherWordList.Count < m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()])
			while (otherWordList.Count < m_maxSpawn - 1)
			{
			Debug.Log("Adding to otherWordList: " + otherWordList.Count);
				string otherWord = m_allWords[Random.Range(0, m_allWords.Count)];
				Texture2D tex = null;
                while (otherWord == selectedWord || tex == null)
                {
                    otherWord = m_allWords[Random.Range(0, m_allWords.Count)];
					Debug.Log("otherWord: " + otherWord);
					tex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + otherWord);
                }
                if ( !otherWordList.Contains(otherWord) )
                {
                    otherWordList.Add(otherWord);
                }
            }

			Debug.Log("otherWordList.Count: " + otherWordList.Count);

            PipPadBehaviour.Instance.Show(selectedWord);
            //if (m_wordsForDifficulty[SessionInformation.Instance.GetDifficulty()] == 2)

		Debug.Log("m_maxSpawn: " + m_maxSpawn);
			if (m_maxSpawn == 2)
            {
                if (Random.Range(0, 10) > 5)
                {
                    PipPadBehaviour.Instance.ShowMultipleBlackboards(selectedWord, otherWordList[0], null);
                    m_correctIndex = 0;
                }
                else
                {
                    PipPadBehaviour.Instance.ShowMultipleBlackboards(otherWordList[0], selectedWord, null);
                    m_correctIndex = 1;
                }
            }
            else
            {
                m_correctIndex = Random.Range(0, 2);
                if (m_correctIndex == 0)
                {
                    PipPadBehaviour.Instance.ShowMultipleBlackboards(selectedWord, otherWordList[0], otherWordList[1]);
                }
                else if (m_correctIndex == 1)
                {
                    PipPadBehaviour.Instance.ShowMultipleBlackboards(otherWordList[0], selectedWord, otherWordList[1]);
                }
                else
                {
                    PipPadBehaviour.Instance.ShowMultipleBlackboards(otherWordList[0], otherWordList[1], selectedWord);

                }
            }
        //}

        yield break;
    }

    IEnumerator OnCorrectClick()
    {
        if (!m_gotIncorrect)
        {
            m_score++;
            PipPadBehaviour.Instance.SayWholeWord();
            yield return new WaitForSeconds(1.5f);
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
            m_characterToPop.PopCharacter();
        }
        else
        {
            PipPadBehaviour.Instance.SayAll(0.5f);
            yield return new WaitForSeconds(4.0f);
        }
        PipPadBehaviour.Instance.Hide();

        m_progressScoreBar.SetStarsCompleted(m_score);
        m_progressScoreBar.SetScore(m_score);
        if (m_score < m_scoreTarget)
        {
            StartCoroutine(ShowNextQuestion());
        }
        else
        {
			FinishGame();
        }
    }

	void FinishGame()
	{
		GameHelpers.SetDefaultPlayerVar();
		GameManager.Instance.CompleteGame();
	}

    public void WordClicked(int index, ImageBlackboard clickedBlackboard)
    {
        UserStats.Activity.IncrementNumAnswers();

        if (index == m_correctIndex)
        {
            StopAllCoroutines();
            StartCoroutine(OnCorrectClick());
        }
        else
        {
            if(m_currentWordData != null)
            {
                UserStats.Activity.AddIncorrectWord(m_currentWordData);
            }

            m_gotIncorrect = true;
            m_characterToPopTroll.PopCharacter();
            clickedBlackboard.ShakeFade();
            StopAllCoroutines();
            StartCoroutine(StartIncorrect());
        }
    }

    IEnumerator StartIncorrect()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_MINOR");
        yield return new WaitForSeconds(0.5f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("GOOD_TRY");

        yield return new WaitForSeconds(1.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_BLUE_BUTTONS");

        yield return new WaitForSeconds(1.0f);
    }
}
