using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SillySensibleLetterCoordinator : Singleton<SillySensibleLetterCoordinator> {

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
	private BennyAudio m_bennyTheBook;


    bool m_isSillyWord;
	
    GameObject m_currentLetterPrefab;
	DataRow m_currentLetterData;
	DataRow m_audioLetterData;


    int m_score = 0;

    int m_trollFatness;
    int m_trollSubFatness = 0;

    int m_sillyWordsSoFar = 0;
    int m_sensibleWordsSoFar = 0;
	
	[SerializeField]
    int[] m_difficultySections;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
    private ImageBlackboard m_blackBoard;

    List<DataRow> m_lettersPool = new List<DataRow>();
	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
	Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();

	// Use this for initialization
	IEnumerator Start () 
    {
		m_bennyTheBook.SetInstruction("SILLY_SENSIBLE_LETTERS_INSTRUCTION");
		
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		yield return new WaitForSeconds(1f);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SILLY_SENSIBLE_LETTERS_INSTRUCTION");

        SessionInformation.Instance.SetNumPlayers(1);
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);
		
		m_blackBoard.MoveWidgets();
		
		int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int difficulty = SessionInformation.Instance.GetDifficulty();
		int endIndex = sectionIds.Length - 2 + difficulty;
		
		for(int index = 0; index < endIndex; ++index)
		{
			int sectionId = sectionIds[index];
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
			m_lettersPool.AddRange(dt.Rows);
		}
		
		/*
		int difficulty = SessionInformation.Instance.GetDifficulty() + 1;
		
		for(int index = 0; index < difficulty; ++index)
		{
			int sectionId = m_difficultySections[index];
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
			m_lettersPool.AddRange(dt.Rows);
		}
		*/

        foreach (DataRow myPh in m_lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));
			
			Debug.Log(myPh["phoneme"]);
			
            m_phonemeImages[myPh] = (Texture2D)Resources.Load(imageFilename);
			
			string audioFilname = string.Format("{0}",
                myPh["grapheme"]);

            m_graphemeAudio[myPh] = AudioBankManager.Instance.GetAudioClip(audioFilname);
            m_longAudio[myPh] = LoaderHelpers.LoadMnemonic(myPh);
        }

        m_progressScoreBar.SetStarsTarget(m_targetScore);
		
		yield return new WaitForSeconds(6f);

        StartCoroutine(ShowNextWord());
	}
	
	public void PlayLetterSound(DataRow letterData, bool tryLong = true)
    {
        if (m_longAudio[letterData] && tryLong)
        {
            m_audioSource.clip = m_longAudio[letterData];
            m_audioSource.Play();
        }
        else
        {
            m_audioSource.clip = m_graphemeAudio[letterData];
            m_audioSource.Play();
        }
    }

    IEnumerator ShowNextWord()
    {
        yield return new WaitForSeconds(1.0f);

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
		
		m_currentLetterData = m_lettersPool[Random.Range(0, m_lettersPool.Count)];
		m_audioLetterData = m_currentLetterData;
		
		if(m_isSillyWord)
		{
			while(m_audioLetterData["phoneme"].ToString() == m_currentLetterData["phoneme"].ToString())
			{
				m_audioLetterData = m_lettersPool[Random.Range(0, m_lettersPool.Count)];
			}
		}
		
		PlayLetterSound(m_audioLetterData, true);

        m_currentLetterPrefab = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
            m_draggableWordPrefab, m_spawnLocation);
        transform.localScale = Vector3.zero;

        m_currentLetterPrefab.GetComponentInChildren<UILabel>().text = m_currentLetterData["phoneme"].ToString();
    }

    public void WordDropped()
    {
        StartCoroutine(WordDroppedFinalise());
    }

    public IEnumerator WordDroppedFinalise()
    {
        if (m_currentLetterPrefab.transform.position.y < m_topBoundary.position.y)
        {
            bool isDone = false;
            bool isCorrect = false;
            if (m_currentLetterPrefab.transform.position.x < m_trollBoundary.position.x)
            {
                isDone = true;

                if ( m_isSillyWord )
                {
					m_currentLetterPrefab.GetComponent<DraggableLetter>().Off();
                    WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_GULP");
					yield return new WaitForSeconds(0.5f);
					WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
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
            else if (m_currentLetterPrefab.transform.position.x > m_pipBoundary.position.x)
            {
                isDone = true;

                if ( !m_isSillyWord )
                {
                    m_blackBoard.ShowImage(m_phonemeImages[m_currentLetterData], m_currentLetterData["phoneme"].ToString(), m_currentLetterData["phoneme"].ToString(), null);
					PlayLetterSound(m_currentLetterData, true);
					
					m_currentLetterPrefab.GetComponent<DraggableLetter>().Off();
                    m_pipAnimation.PlayAnimation("ON");
                    yield return new WaitForSeconds(0.8f);
                    m_pipAnimation.PlayAnimation("OFF");
                    isCorrect = true;
					
					yield return new WaitForSeconds(2.5f);
					WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
                }
            }
			
            if (isCorrect)
            {
                m_score++;
                m_progressScoreBar.SetStarsCompleted(m_score);
                m_progressScoreBar.SetScore(m_score);
				
				
				
				m_blackBoard.Hide();
				
            }
            else
            {
				iTween.MoveTo(m_currentLetterPrefab, m_spawnLocation.position, 1.0f);

                if (!m_isSillyWord )
                {
					m_blackBoard.ShowImage(m_phonemeImages[m_currentLetterData], m_currentLetterData["phoneme"].ToString(), m_currentLetterData["phoneme"].ToString(), null);
                }
				
				PlayLetterSound(m_audioLetterData, true);
            }

            if (isDone)
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
                    TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
                }
				else if(isCorrect)
                {
                    StartCoroutine(ShowNextWord());
                }
            }
        }
    }
	
	

    public void ShowPadForWord()
    {
        //if (m_currentWord != null)
        //{
           // PipPadBehaviour.Instance.Show(m_currentWord);
        //}

    }
}
