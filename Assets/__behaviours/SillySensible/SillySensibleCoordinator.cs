using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SillySensibleCoordinator : Singleton<SillySensibleCoordinator> 
{
    [SerializeField]
    private AnimManager m_trollAnimManager;
    [SerializeField]
    private Transform m_trollMouthLocation;
    [SerializeField]
    private AnimManager m_pipAnimManager;
    [SerializeField]
    private Transform m_pipBackpackLocation;
    [SerializeField]
    private ClickEvent m_trollCollider;
    [SerializeField]
    private ClickEvent m_pipCollider;
    [SerializeField]
    private Transform m_spawnLocation;
    [SerializeField]
    private GameObject m_wordPrefab;
    [SerializeField]
    private Transform m_trollBoundary;
    [SerializeField]
    private Transform m_pipBoundary;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
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
    [SerializeField]
    private AudioClip m_onCompleteAudio;
    [SerializeField]
    private Benny m_benny;
    [SerializeField]
    private AudioClip m_instructionAudio;

    List<DataRow> m_wordList = new List<DataRow>();
    List<DataRow> m_sillyWords = new List<DataRow>();

    GameWidget m_spawnedWidget;

    bool m_isSillyWord;

    string m_currentWord;

    int m_score = 0;

    int m_sillyWordsSoFar = 0;
    int m_sensibleWordsSoFar = 0;

    bool m_hasGotWrong = false;

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        m_benny.SetFirst(m_instructionAudio);
        StartCoroutine(m_benny.PlayAudio());

        m_trollCollider.SingleClicked += OnClickTroll;
        m_pipCollider.SingleClicked += OnClickPip;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        SessionInformation.Instance.SetNumPlayers(1);
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);

		m_wordList.AddRange(DataHelpers.GetWords());

		m_sillyWords.AddRange(DataHelpers.GetSillywords());

        m_scoreKeeper.SetTargetScore(m_targetScore);


		if(m_wordList.Count > 0)
		{
            /*
            while(m_benny.IsPlaying())
            {
                yield return null;
            }
            */
            yield return new WaitForSeconds(2f);

            ShowNextWord();
		}
		else
		{
			StartCoroutine(CompleteGame());
		}
	}

    void ShowNextWord()
    {
        m_hasGotWrong = false;

        if ((m_sillyWordsSoFar < m_targetScore / 2) && (m_sensibleWordsSoFar < m_targetScore / 2))
        {
            m_isSillyWord = Random.Range(0, 10) > 5;
        } 
        else
        {
            if (m_sillyWordsSoFar < m_sensibleWordsSoFar)
            {
                m_isSillyWord = true;
            } 
            else if (m_sillyWordsSoFar > m_sensibleWordsSoFar)
            {
                m_isSillyWord = false;
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

        m_currentWord  = m_isSillyWord ? m_sillyWords[Random.Range(0, m_sillyWords.Count)]["word"].ToString() : m_wordList[Random.Range(0, m_wordList.Count)]["word"].ToString();

        GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, m_spawnLocation);
        m_spawnedWidget = newGo.GetComponent<GameWidget>() as GameWidget;
        m_spawnedWidget.SetUp(m_currentWord, true);
        m_spawnedWidget.Unpressing += OnWidgetRelease;
    }

    void OnClickTroll(ClickEvent click)
    {
        StartCoroutine(OnAnswer(m_spawnedWidget, m_isSillyWord));
    }

    void OnClickPip(ClickEvent click)
    {
        StartCoroutine(OnAnswer(m_spawnedWidget, !m_isSillyWord));
    }

    void OnWidgetRelease(GameWidget widget)
    {   
        if (widget.transform.position.x < m_trollBoundary.position.x)
        {
            StartCoroutine(OnAnswer(widget, m_isSillyWord));
        }
        else if (widget.transform.position.x > m_pipBoundary.position.x)
        {
            StartCoroutine(OnAnswer(widget, !m_isSillyWord));
        } 
        else
        {
            widget.TweenToStartPos();
        }
    }

    IEnumerator OnAnswer(GameWidget widget, bool isCorrect)
    {
        if (widget != null)
        {
            if (isCorrect)
            {
                ++m_score;

                m_spawnedWidget = null;
                widget.EnableCollider(false);

                if (m_isSillyWord)
                {
                    yield return StartCoroutine(FeedTroll(widget.gameObject));
                } 
                else
                {
                    AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWord);
                    if (loadedAudio != null)
                    {
                        m_audioSource.clip = loadedAudio;
                        m_audioSource.Play();
                    }

                    //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

                    yield return StartCoroutine(FeedPip(widget.gameObject));
                }

                widget.Off();

                WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
                m_scoreKeeper.UpdateScore(1);

                if (m_score < m_targetScore)
                {
                    yield return new WaitForSeconds(2);
                    ShowNextWord();
                } 
                else
                {
                    StartCoroutine(CompleteGame());
                }
            } 
            else
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
                widget.TweenToStartPos();
                yield return null;
            }
        }

        yield break;
    }

    IEnumerator FeedTroll(GameObject go)
    {
        yield return new WaitForSeconds(0.25f);

        m_pipAnimManager.PlayAnimation("THUMBS_UP");
        
        float positionTweenDuration = 0.3f;

        iTween.MoveTo(go, m_trollMouthLocation.position, positionTweenDuration);
        
        yield return new WaitForSeconds(positionTweenDuration / 2f);
        
        iTween.ScaleTo(go, Vector3.one * 0.1f, positionTweenDuration / 2f);

        string eventString = "TROLL_GULP";
        WingroveAudio.WingroveRoot.Instance.PostEvent(eventString);
        
        m_trollAnimManager.PlayAnimation("EAT");
        
        yield return new WaitForSeconds(positionTweenDuration / 2f);
    }

    IEnumerator FeedPip(GameObject go)
    {
        yield return new WaitForSeconds(0.25f);
        
        float positionTweenDuration = 0.3f;
        
        iTween.MoveTo(go, m_pipBackpackLocation.position, positionTweenDuration);
        
        yield return new WaitForSeconds(positionTweenDuration / 2f);
        
        iTween.ScaleTo(go, Vector3.one * 0.1f, positionTweenDuration / 2f);

        //string animName = Random.Range(0, 2) == 0 ? "THUMBS_UP" : "JUMP";
        //m_pipAnimManager.PlayAnimation(animName);
        m_pipAnimManager.PlayAnimation("JUMP");
        
        yield return new WaitForSeconds(positionTweenDuration / 2f);
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

    IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(1.0f);
        iTween.MoveTo(m_moveHierarchy, m_offPosition.transform.position, 1.0f);
        yield return new WaitForSeconds(1.0f);
        //WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_BURP");
        m_audioSource.clip = m_onCompleteAudio;
        m_audioSource.Play();
        yield return new WaitForSeconds(0.1f);
        m_videoHierarchy.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        //WingroveAudio.WingroveRoot.Instance.PostEvent("SILLY_TROLL");
        yield return new WaitForSeconds(3.0f);
        
        GameManager.Instance.CompleteGame();
    }
}
