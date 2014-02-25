using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class JoinLettersCoordinator : Singleton<JoinLettersCoordinator>
{
	[SerializeField]
	private JoinLettersPlayer[] m_gamePlayers;
    [SerializeField]
    private int m_targetScore = 6;
    [SerializeField]
    private int m_pairsToShowAtOnce = 3;
	[SerializeField]
    int[] m_difficultySections;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	bool m_waitForBoth;
	int m_numWaitForPlayers;
	
	int m_numFinishedPlayers;

    List<DataRow> m_lettersPool = new List<DataRow>();
	Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();


    int m_score;
	
	int GetNumPlayers()
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
    IEnumerator Start()
    {
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("SELECT_CHARACTER");
		
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		List<DataRow> lettersPool = GameDataBridge.Instance.GetLetters();
		
		foreach (DataRow myPh in lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));

			
			string audioFilname = string.Format("{0}",
                myPh["grapheme"]);

            m_graphemeAudio[myPh] = AudioBankManager.Instance.GetAudioClip(audioFilname);
            m_longAudio[myPh] = LoaderHelpers.LoadMnemonic(myPh);
        }
		
		int numPlayers = GetNumPlayers();
		
		for(int index = 0; index < numPlayers; ++index)
		{
			m_gamePlayers[index].SetUp(lettersPool); 
		}
		
		if(m_waitForBoth && numPlayers == 2)
		{
			m_numWaitForPlayers = 2;
		}
		else
		{
			m_numWaitForPlayers = 1;
		}
        
		if(m_lettersPool.Count > 0)
		{
			StartCoroutine(PlayGame());
		}
		else
		{
			FinishGame();
		}
    }

    IEnumerator PlayGame()
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
				Debug.Log("All Selected");
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        for (int index = 0; index < numPlayers; ++index)
        {
            m_gamePlayers[index].HideAll();
        }
		
        WingroveAudio.WingroveRoot.Instance.PostEvent("MATCH_LETTERS_INSTRUCTION");
		yield return new WaitForSeconds(4.0f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("READY_STEADY_GO");
		
		for(int index = 0; index < numPlayers; ++index)
		{
			StartCoroutine(m_gamePlayers[index].SetUpNext());
		}
		
		while(m_numFinishedPlayers < m_numWaitForPlayers)
		{
			yield return null;
		}
		
		for(int index = 0; index < numPlayers; ++index)
		{
			m_gamePlayers[index].DestroyJoinables();
		}
		
		int winningIndex = -1;
		
		for(int index = 0; index < numPlayers; ++index)
		{
			if(m_gamePlayers[index].GetScore() >= m_targetScore)
			{
				winningIndex = index;
				break;
			}
		}
		
		SessionInformation.Instance.SetWinner(winningIndex);
        yield return new WaitForSeconds(1.5f);
        
		FinishGame();
	}

	void FinishGame()
	{
		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Session)
		{
			SessionManager.Instance.OnGameFinish();
		}
		else
		{
			TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
		}
	}
	
	public void PlayLetterSound(DataRow letterData, bool tryLong = true)
    {
        if (m_longAudio[letterData] != null && tryLong)
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
	
	public int GetTargetScore()
	{
		return m_targetScore;
	}
	
	public int GetPairsToShowAtOnce()
	{
		return m_pairsToShowAtOnce;
	}
	
	public void IncrementNumFinishedPlayers()
	{
		++m_numFinishedPlayers;
	}
	
	public void LetterClicked(LetterPictureJoinableDrag joinable)
	{
		PlayLetterSound(joinable.GetLetterData(), joinable.IsPicture());
	}
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class JoinLettersCoordinator : Singleton<JoinLettersCoordinator>
{

    [SerializeField]
    private GameObject m_pictureBoxPrefab;
    [SerializeField]
    private GameObject m_wordBoxPrefab;
    [SerializeField]
    private Transform m_lowCorner;
    [SerializeField]
    private Transform m_highCorner;
    [SerializeField]
    private ProgressScoreBar m_progressScoreBar;
    [SerializeField]
    private int m_targetScore = 6;
    [SerializeField]
    private int m_pairsToShowAtOnce = 3;
    [SerializeField]
    private Transform m_spawnTransform;
    [SerializeField]
    private Transform m_leftOff;
    [SerializeField]
    private Transform m_rightOff;
    [SerializeField]
    private CharacterPopper m_characterPopper;
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

    List<GameObject> m_spawnedObjects = new List<GameObject>();

    int m_score;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		int sectionId = m_difficultySections[SessionInformation.Instance.GetDifficulty()];
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
		
		m_lettersPool = dt.Rows;

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

        m_progressScoreBar.SetStarsTarget(m_targetScore);

        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("MATCH_INSTRUCTION");
        yield return new WaitForSeconds(3.0f);
		
		m_blackBoard.MoveWidgets();

        StartCoroutine(SetUpNext());
    }

    IEnumerator SetUpNext()
    {
		Dictionary<string, DataRow> letters = new Dictionary<string, DataRow>();

        while (letters.Count < m_pairsToShowAtOnce)
        {
			DataRow letterData = m_lettersPool[Random.Range(0, m_lettersPool.Count)];
			letters[letterData["phoneme"].ToString()] = letterData;
            yield return null;
        }
        

        List<Vector3> positions = new List<Vector3>();
        Vector3 delta = m_highCorner.transform.localPosition - m_lowCorner.transform.localPosition;
        
        for (int index = 0; index < m_pairsToShowAtOnce * 2; ++index)
        {
            int x = index % m_pairsToShowAtOnce;
            int y = index / m_pairsToShowAtOnce;
            positions.Add(
                m_lowCorner.transform.localPosition +
                new Vector3((delta.x / m_pairsToShowAtOnce) * (x + 0.5f),
                    (delta.y / 2) * (y + 0.5f), 0)
                    + new Vector3(Random.Range(-delta.x / (m_pairsToShowAtOnce*20.0f), delta.x / (m_pairsToShowAtOnce*20.0f)),
                        Random.Range(-delta.y / 5, delta.y / 5),
                        0)
                        );
        }

		foreach(KeyValuePair<string, DataRow> letter in letters)
        {
            GameObject newText = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordBoxPrefab,
                m_spawnTransform);
            GameObject newImage = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pictureBoxPrefab,
    m_spawnTransform);

            int posIndex = Random.Range(0, positions.Count);
            Vector3 posA = positions[posIndex];
            positions.RemoveAt(posIndex);

            newText.transform.localPosition = posA;

            posIndex = Random.Range(0, positions.Count);
            Vector3 posB = positions[posIndex];
            positions.RemoveAt(posIndex);

            newImage.transform.localPosition = posB;
			
			Texture2D texture = m_phonemeImages[letter.Value];
			
			newText.GetComponent<LetterPictureJoinableDrag>().SetUp(letter.Value, texture);
            newImage.GetComponent<LetterPictureJoinableDrag>().SetUp(letter.Value, texture);

            newText.transform.localScale = Vector3.zero;
            newImage.transform.localScale = Vector3.zero;

            m_spawnedObjects.Add(newText);
            m_spawnedObjects.Add(newImage);
        }


        yield break;
    }

    IEnumerator AddPoint()
    {
        m_characterPopper.PopCharacter();

        yield return new WaitForSeconds(2.0f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

        m_score++;
        m_progressScoreBar.SetScore(m_score);
        m_progressScoreBar.SetStarsCompleted(m_score);

        if (m_score == m_targetScore)
        {
            SessionInformation.Instance.SetNumPlayers(1);
            SessionInformation.Instance.SetWinner(0);
            SessionInformation.Instance.SetPlayerIndex(0, 3);
            TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(SetUpNext());
        }
    }

    public void Connect(LetterPictureJoinableDrag a, LetterPictureJoinableDrag b)
    {
        if (a != b)
        {
            if (a.IsPicture() != b.IsPicture())
            {
				LetterPictureJoinableDrag letterObject;
				
				if(!a.IsPicture())
				{
					letterObject = a;
				}
				else
				{
					letterObject = b;
				}
				
                if (a.GetLetterData() == b.GetLetterData())
                {                    
					PlayLetterSound(letterObject.GetLetterData());
					m_blackBoard.Hide();
					StartCoroutine(movePictures(a, b));
                }
                else
                {
					PlayLetterSound(letterObject.GetLetterData());
					DisplayHint(m_phonemeImages[letterObject.GetLetterData()], letterObject.GetWord(), letterObject.GetWord());
                }
            }
        }
    }
	
	void DisplayHint(Texture2D texture, string word, string colorReplace)
	{
		m_blackBoard.ShowImage(texture, word, colorReplace);
	}
	
	IEnumerator movePictures(LetterPictureJoinableDrag a, LetterPictureJoinableDrag b)
	{
		a.Off(a.transform.position.x < b.transform.position.x ? m_leftOff : m_rightOff);
        b.Off(a.transform.position.x < b.transform.position.x ? m_rightOff : m_leftOff);
        m_spawnedObjects.Remove(a.gameObject);
        m_spawnedObjects.Remove(b.gameObject);

        if (m_spawnedObjects.Count == 0)
        {                        
            StartCoroutine(AddPoint());
        }
		
		yield return new WaitForSeconds(2.5f);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
	}
	
	public void PlayLetterSound(DataRow letterData)
    {
        if (m_longAudio[letterData] != null)
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
*/