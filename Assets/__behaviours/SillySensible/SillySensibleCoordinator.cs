using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SillySensibleCoordinator : Singleton<SillySensibleCoordinator> {

    [SerializeField]
    private SimpleSpriteAnim m_trollAnimation;
    [SerializeField]
    private SimpleSpriteAnim m_pipAnimation;
    [SerializeField]
    private string[] m_standAnimations;
    [SerializeField]
    private string[] m_idleLevelAnims;
    [SerializeField]
    private string[] m_growAnims;
    [SerializeField]
    private Transform m_spawnLocation;
    [SerializeField]
    private GameObject m_draggableWordPrefab;
    [SerializeField]
    private Transform m_topBoundary;
    [SerializeField]
    private Transform m_trollBoundary;
    [SerializeField]
    private Transform m_pipBoundary;
    [SerializeField]
    private ProgressScoreBar m_progressScoreBar;
    [SerializeField]
    private int m_targetScore = 6;
    [SerializeField]
    private Transform m_offPosition;
    [SerializeField]
    private GameObject m_moveHierarchy;
    [SerializeField]
    private GameObject m_videoHierarchy;
	[SerializeField]
	private Transform m_pipCorrectPosition;
	[SerializeField]
	private Transform m_trollCorrectPosition;
	[SerializeField]
	private AudioSource m_audioSource;

    List<DataRow> m_wordList = new List<DataRow>();
    List<DataRow> m_sillyWords = new List<DataRow>();

    bool m_isSillyWord;
    GameObject m_currentWordPrefab;
    string m_currentWord;

    int m_score = 0;

    int m_trollFatness;
    int m_trollSubFatness = 0;

    int m_sillyWordsSoFar = 0;
    int m_sensibleWordsSoFar = 0;

    bool m_hasGotWrong = false;

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        SessionInformation.Instance.SetNumPlayers(1);
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);

		m_wordList.AddRange(DataHelpers.GetWords());

		m_sillyWords.AddRange(DataHelpers.GetSillyWords());

        m_progressScoreBar.SetStarsTarget(m_targetScore);


        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SILLY_SENSIBLE_INSTRUCTION");

        yield return new WaitForSeconds(5.0f);

		if(m_wordList.Count > 0)
		{
        	StartCoroutine(ShowNextWord());
		}
		else
		{
			FinishGame();
		}
	}

    IEnumerator ShowNextWord()
    {
        yield return new WaitForSeconds(2.0f);

        m_hasGotWrong = false;

        if ((m_sillyWordsSoFar < m_targetScore / 2) &&
            (m_sensibleWordsSoFar < m_targetScore / 2))
        {
            m_isSillyWord = Random.Range(0, 10) > 5;
        }
        else
        {
            if (m_score == m_sensibleWordsSoFar + m_sillyWordsSoFar)
            {
                m_isSillyWord = m_sensibleWordsSoFar > m_sillyWordsSoFar;
            }
            else
            {
                m_isSillyWord = Random.Range(0, 10) > 5;
            }
        }

        if (m_isSillyWord)
        {
            m_sillyWordsSoFar++;
        }
        else
        {
            m_sensibleWordsSoFar++;
        }

        m_currentWord  = m_isSillyWord ? m_sillyWords[Random.Range(0, m_sillyWords.Count)]["word"].ToString() :
            m_wordList[Random.Range(0, m_wordList.Count)]["word"].ToString();

        m_currentWordPrefab = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
            m_draggableWordPrefab, m_spawnLocation);
        m_currentWordPrefab.transform.localScale = Vector3.zero;

        m_currentWordPrefab.GetComponentInChildren<UILabel>().text = m_currentWord;
    }

    public void WordDropped()
    {
        StartCoroutine(WordDroppedFinalise());
    }

    public IEnumerator WordDroppedFinalise()
    {
        if (m_currentWordPrefab.transform.position.y < m_topBoundary.position.y)
        {
            bool isDone = false;
            bool isCorrect = false;
            if (m_currentWordPrefab.transform.position.x < m_trollBoundary.position.x)
            {
                isDone = true;                
                if ( m_isSillyWord )
                {
					iTween.MoveTo(m_currentWordPrefab, m_trollCorrectPosition.position, 0.2f);
                    WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_GULP");
                    m_trollSubFatness++;
                    if (m_trollSubFatness == 2)
                    {
                        if (m_trollFatness < 2)
                        {
                            m_trollAnimation.PlayAnimation(m_growAnims[m_trollFatness]);
                            yield return new WaitForSeconds(0.9f);
                            m_trollFatness++;
                            m_trollAnimation.PlayAnimation(m_standAnimations[m_trollFatness]);
                        }
                        m_trollSubFatness = 0;
                    }
                    else
                    {
                        m_trollAnimation.PlayAnimation(m_idleLevelAnims[m_trollFatness]);
                        yield return new WaitForSeconds(0.4f);
                        m_trollAnimation.PlayAnimation(m_standAnimations[m_trollFatness]);
                        yield return new WaitForSeconds(0.5f);
                    }
                    isCorrect = true;
                }
            }
            else if (m_currentWordPrefab.transform.position.x > m_pipBoundary.position.x)
            {
                isDone = true;

                if ( !m_isSillyWord )
                {
					if(!m_hasGotWrong)
					{
						AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWord);
				        if (loadedAudio != null)
						{
				            m_audioSource.clip = loadedAudio;
				            m_audioSource.Play();
				        }
					}
					
					iTween.MoveTo(m_currentWordPrefab, m_pipCorrectPosition.position, 0.2f);
					int animIndex = Random.Range(0,2);
                    m_pipAnimation.PlayAnimation("ON_" + animIndex);
                    yield return new WaitForSeconds(0.8f);
                    m_pipAnimation.PlayAnimation("OFF_" + animIndex);
                    isCorrect = true;
                }
            }

            if (isCorrect)
            {
                m_currentWordPrefab.GetComponent<DraggableWord>().Off();

                if (!m_hasGotWrong)
                {
                    m_score++;
                    m_progressScoreBar.SetStarsCompleted(m_score);
                    m_progressScoreBar.SetScore(m_score);
                }
                else
                {
                    if (!m_isSillyWord )
                    {
                        PipPadBehaviour.Instance.Show(m_currentWord);
                        PipPadBehaviour.Instance.SayAll(3f);
                        while (PipPadBehaviour.Instance.IsShowing())
                        {
                            yield return null;
                        }
                    }
                }
				
				if (m_isSillyWord)
                {
                    WingroveAudio.WingroveRoot.Instance.PostEvent("WORD_IS_SILLY");
                }
                else
                {
                    WingroveAudio.WingroveRoot.Instance.PostEvent("WORD_IS_SENSIBLE");
				}
            }
            else
            {
				iTween.MoveTo(m_currentWordPrefab, m_spawnLocation.position, 0.2f);
                m_hasGotWrong = true;
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
            }

            if (isDone && isCorrect)
            {
                if (m_score == m_targetScore)
                {
                    yield return new WaitForSeconds(1.0f);
                    iTween.MoveTo(m_moveHierarchy, m_offPosition.transform.position, 1.0f);
                    yield return new WaitForSeconds(1.0f);
                    WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_BURP");
                    m_videoHierarchy.SetActive(true);
                    yield return new WaitForSeconds(3.0f);
                    WingroveAudio.WingroveRoot.Instance.PostEvent("SILLY_TROLL");
                    yield return new WaitForSeconds(3.0f);

					FinishGame();
				}
                else
                {
                    StartCoroutine(ShowNextWord());
                }
            }
        }
    }
	
	public void PlayWordAudio()
    {
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWord);
        if (loadedAudio != null)
		{
            m_audioSource.clip = loadedAudio;
            m_audioSource.Play();
        }
    }

    public void ShowPadForWord()
    {
        if (m_currentWord != null)
        {
            PipPadBehaviour.Instance.Show(m_currentWord);
        }

    }

	void FinishGame()
	{
		GameManager.Instance.CompleteGame();
	}
}
