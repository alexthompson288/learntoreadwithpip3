using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class FeedTrollLettersCoordinator : MonoBehaviour 
{	
    [SerializeField]
    private AnimManager m_trollAnimManager;
	[SerializeField]
    private SimpleSpriteAnim m_trollAnimation;
	[SerializeField]
    private string[] m_standAnimations;
    [SerializeField]
    private string[] m_idleLevelAnims;
    [SerializeField]
    private string[] m_growAnims;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
    private GameObject m_draggableLabelPrefab;
    [SerializeField]
    private Transform m_trollBoundary;
	[SerializeField]
    private ProgressScoreBar m_progressScoreBar;
    [SerializeField]
    private int m_targetScore = 6;
	[SerializeField]
	private Transform m_mouthLocation;
	[SerializeField]
    private Transform m_offPosition;
	[SerializeField]
    private GameObject m_moveHierarchy;
    [SerializeField]
    private GameObject m_videoHierarchy;
	[SerializeField]
	private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_instructionAudio;
    [SerializeField]
    private AudioClip m_onCompleteAudio;
	[SerializeField]
    private Blackboard m_blackBoard;
	[SerializeField]
	private int[] m_difficultyNumSpawn;
    [SerializeField]
    private Benny m_benny;
	
	int m_numSpawn;
	
	List<DraggableLabel> m_spawnedDraggables = new List<DraggableLabel>();
	
	int m_score = 0;

    int m_trollFatness;
    int m_trollSubFatness = 0;
	
	DataRow m_currentLetterData;

    List<DataRow> m_lettersPool = new List<DataRow>();
	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
	Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();
	

	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		// Always Pip, Always winner
		SessionInformation.Instance.SetNumPlayers(1);
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);
		
		m_blackBoard.MoveWidgets();

		m_lettersPool = DataHelpers.GetPhonemes();

		m_numSpawn = m_lettersPool.Count;

		if(m_numSpawn > m_locators.Length)
		{
			m_numSpawn = m_locators.Length;
		}

		Debug.Log("m_lettersPool.Count: " + m_lettersPool.Count);

        foreach (DataRow myPh in m_lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));
			
            m_phonemeImages[myPh] = (Texture2D)Resources.Load(imageFilename);
			
			string audioFilname = string.Format("{0}",
                myPh["grapheme"]);

            m_graphemeAudio[myPh] = AudioBankManager.Instance.GetAudioClip(audioFilname);
            m_longAudio[myPh] = LoaderHelpers.LoadMnemonic(myPh);
        }

		if(m_lettersPool.Count == 1)
		{
			m_targetScore = 3;
		}

        m_progressScoreBar.SetStarsTarget(m_targetScore);
		
		//yield return new WaitForSeconds(3f);

        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        m_audioSource.clip = m_instructionAudio;
        m_audioSource.Play();

        yield return new WaitForSeconds(2f);

		if(m_lettersPool.Count > 0)
		{
        	AskQuestion();
		}
		else
		{
			StartCoroutine(FinishGame());
		}
	}
	
	void AskQuestion()
	{
		m_currentLetterData = m_lettersPool[Random.Range(0, m_lettersPool.Count)];

        //UserStats.Activity.AddPhoneme(m_currentLetterData);
		
		m_benny.SetFirst(m_graphemeAudio[m_currentLetterData]);
		
		PlayLetterSound(m_currentLetterData);
		
		Dictionary<string, DataRow> lettersToSpawn = new Dictionary<string, DataRow>();
		lettersToSpawn.Add(m_currentLetterData["phoneme"].ToString(), m_currentLetterData);
		
		while(lettersToSpawn.Count < m_numSpawn)
		{
			DataRow letterData = m_lettersPool[Random.Range(0, m_lettersPool.Count)];
			lettersToSpawn[letterData["phoneme"].ToString()] = letterData;
		}
		
		List<Transform> locators = new List<Transform>(m_locators);
		while(locators.Count > m_numSpawn)
		{
			int removeIndex = Random.Range(0, locators.Count);
			//Debug.Log("removeIndex: " + removeIndex);
			//Debug.Log("name: " + locators[removeIndex].name);
			locators.RemoveAt(removeIndex);
		}
		
		int i = 0;
		foreach(KeyValuePair<string, DataRow> kvp in lettersToSpawn)
		{
			GameObject newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggableLabelPrefab, locators[i]);
			++i;
			
			DraggableLabel draggable = newLetter.GetComponent<DraggableLabel>() as DraggableLabel;
			draggable.SetUp(kvp.Key, m_graphemeAudio[kvp.Value]);
			//draggable.OnRelease += OnDraggableRelease;
            draggable.SetCanDrag(false);
            draggable.OnNoDragClick += OnDraggableRelease;
			m_spawnedDraggables.Add(draggable);
		}
	}
	
	void OnDraggableRelease(DraggableLabel draggable)
	{
		//if(draggable.transform.position.x < m_trollBoundary.position.x)
		//{
            //UserStats.Activity.IncrementNumAnswers();

			if(draggable.GetText() == m_currentLetterData["phoneme"].ToString())
			{
				StartCoroutine(OnCorrectAnswer(draggable));
			}
			else
			{
                //UserStats.Activity.AddIncorrectPhoneme(m_currentLetterData);

				PlayLetterSound(m_currentLetterData);
				
				m_blackBoard.ShowImage(m_phonemeImages[m_currentLetterData], 
					m_currentLetterData["phoneme"].ToString(), m_currentLetterData["phoneme"].ToString());
			
				draggable.TweenToStartPos();
			}
		//}
		//else
		//{
			//draggable.TweenToStartPos(); 
		//}
	}
	
	IEnumerator OnCorrectAnswer(DraggableLabel draggable)
	{
		PlayLetterSound(m_currentLetterData, false);
		
		m_blackBoard.ShowImage(m_phonemeImages[m_currentLetterData], 
			m_currentLetterData["phoneme"].ToString(), m_currentLetterData["phoneme"].ToString());
		
		yield return new WaitForSeconds(0.25f);

        float positionTweenDuration = 0.3f;

        iTween.MoveTo(draggable.gameObject, m_mouthLocation.position, positionTweenDuration);
		
        yield return new WaitForSeconds(positionTweenDuration / 2f);

        iTween.ScaleTo(draggable.gameObject, Vector3.one * 0.1f, positionTweenDuration / 2f);


        //string eventString = (Random.Range(0, 3) == 0) ? "TROLL_BURP_RANDOM" : "TROLL_GULP";
        string eventString = "TROLL_GULP";
        WingroveAudio.WingroveRoot.Instance.PostEvent(eventString);
        
        m_trollAnimManager.PlayAnimation("EAT");

        yield return new WaitForSeconds(positionTweenDuration / 2f);
		
		m_spawnedDraggables.Remove(draggable);
		draggable.Off();
		

        /*
        int probability = Random.Range(0, 10);
        string eventString = "TROLL_GULP";
        if(probability > 7)
        {
            eventString = "TROLL_FART_RANDOM";
        }
        else if(probability > 5)
        {
            eventString = "TROLL_BURP_RANDOM";
        }
        
		// TODO: Put troll animation calls in methods. Remove code duplication below
        m_trollSubFatness++;
        if (m_trollSubFatness == 2)
        {
            if (m_trollFatness < 2)
            {
                m_trollAnimation.PlayAnimation(m_growAnims[m_trollFatness]);
                //yield return new WaitForSeconds(0.9f);
				yield return new WaitForSeconds(0.5f);
                m_trollFatness++;
                m_trollAnimation.PlayAnimation(m_standAnimations[m_trollFatness]);
            }
			else
			{
				m_trollAnimation.PlayAnimation(m_idleLevelAnims[m_trollFatness]);
				//yield return new WaitForSeconds(0.4f);
				yield return new WaitForSeconds(0.2f);
				m_trollAnimation.PlayAnimation(m_standAnimations[m_trollFatness]);
				//yield return new WaitForSeconds(0.5f);
				yield return new WaitForSeconds(0.25f);
			}
            m_trollSubFatness = 0;
        }
        else
        {
            m_trollAnimation.PlayAnimation(m_idleLevelAnims[m_trollFatness]);
            //yield return new WaitForSeconds(0.4f);
			yield return new WaitForSeconds(0.2f);
            m_trollAnimation.PlayAnimation(m_standAnimations[m_trollFatness]);
            //yield return new WaitForSeconds(0.5f);
			yield return new WaitForSeconds(0.25f);
        }
        */


		
		
		
				
		for(int index = 0; index < m_spawnedDraggables.Count; ++index)
		{
			m_spawnedDraggables[index].Off();
		}
		
		m_spawnedDraggables.Clear();
		
		yield return new WaitForSeconds(1f);
		
		m_blackBoard.Hide();
		
		++m_score;
		m_progressScoreBar.SetStarsCompleted(m_score);
        m_progressScoreBar.SetScore(m_score);
		
		yield return new WaitForSeconds(0.5f);
		
		if(m_score < m_targetScore)
		{
			AskQuestion();
		}
		else
		{
			StartCoroutine(FinishGame());
		}
	}

	IEnumerator FinishGame()
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
		yield return new WaitForSeconds(6.0f);

		GameManager.Instance.CompleteGame();
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
}
