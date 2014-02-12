using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SplatGameCoordinator : Singleton<SplatGameCoordinator>
{
    [SerializeField]
    private ProgressScoreBar m_progressScore;
    [SerializeField]
    private Transform m_playArea;
    [SerializeField]
    private Transform m_playAreaMin;
    [SerializeField]
    private Transform m_playAreaMax;
    [SerializeField]
    private Blackboard m_blackBoard;
    [SerializeField]
    private int m_targetScore = 6;
    [SerializeField]
    private string[] m_skinResourceNames;
    [SerializeField]
    private GameObject m_splatPrefab;
    [SerializeField]
    private int[] m_difficultySections;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private SimpleSpriteAnim m_spriteAnim;
    [SerializeField]
    private UITexture m_worldTexture;
    [SerializeField]
    private TweenOnOffBehaviour[] m_selectSkinButtons;
	[SerializeField]
	private ChangeableBennyAudio m_bennyTheBook;
	
	string m_splatSound;
	
    int m_score = 0;
    int m_lives = 0;

    List<DataRow> m_lettersPool = new List<DataRow>();
    SplatGameSkin m_loadedSkin;
    Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
    Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();

    DataRow m_currentLetterData;
    List<GameObject> m_spawnedObjects = new List<GameObject>();

    string m_currentLetter;

    // Use this for initialization
    IEnumerator Start()
    {
		m_blackBoard.MoveWidgets();
		//m_bennyTheBook.SetUp("SPLAT_INSTRUCTION", 3.5f);
		m_bennyTheBook.SetUp(null, 0f);
	
		
        // always pip, always winner
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);

        m_progressScore.SetStarsTarget(m_targetScore);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_WHAT_WANT");

		m_lettersPool.AddRange(GameDataBridge.Instance.GetLetters());

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

        foreach (TweenOnOffBehaviour two in m_selectSkinButtons)
        {
            two.On();
        }
    }

    public void SelectSkin(int skinIndex)
    {
        foreach (TweenOnOffBehaviour two in m_selectSkinButtons)
        {
            two.Off();
        }

        GameObject skinPrefab = (GameObject)Resources.Load(m_skinResourceNames[skinIndex]);
        m_loadedSkin = skinPrefab.GetComponent<SplatGameSkin>();

        m_worldTexture.mainTexture = m_loadedSkin.m_backgroundTexture;
        iTween.MoveTo(m_worldTexture.gameObject, Vector3.zero, 2.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent(m_loadedSkin.m_ambienceAudioEvent);
		
		switch(skinIndex)
		{
		case 0:
			WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_CHICKEN");
			m_splatSound = "SPLAT_CHICKEN";
			break;
		case 1:
			WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_NUT");
			m_splatSound = "SPLAT_NUT";
			break;
		case 2:
			WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_MUSHROOM");
			m_splatSound = "SPLAT_MUSHROOM";
			break;
		case 3:
			WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_FIREFLY");
			m_splatSound = "SPLAT_FIREFLY";
			break;
		}
        
        StartCoroutine(StartGame());
    }
	
	IEnumerator StartGame()
	{
		if(m_lettersPool.Count > 0)
		{
			yield return new WaitForSeconds(1.5f);
			
			WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_INSTRUCTION");
			
			yield return new WaitForSeconds(3f);
			
			StartCoroutine(SpawnQuestion());
		}
		else
		{
			FinishGame();
		}
	}

    void OnDestroy()
    {
        if (WingroveAudio.WingroveRoot.Instance != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("AMBIENCE_STOP");
        }
        Resources.UnloadUnusedAssets();
    }

    public void LetterClicked(string letter, GameObject letterOb)
    {
		WingroveAudio.WingroveRoot.Instance.PostEvent(m_splatSound);
		
        if (letter == m_currentLetter)
        {
            foreach (GameObject go in m_spawnedObjects)
            {
                go.GetComponent<SplattableLetter>().SplatDestroy();
            }
            m_spawnedObjects.Clear();
            m_score++;
            m_progressScore.SetStarsCompleted(m_score);
            m_progressScore.SetScore(m_score);
            m_blackBoard.Hide();
            m_spriteAnim.PlayAnimation("ON");
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");

			if (m_score < m_targetScore)
            {
                StartCoroutine(SpawnQuestion());
            }
            else
            {
				FinishGame();
			}
        }
        else
        {
            m_blackBoard.ShowImage(m_phonemeImages[m_currentLetterData],
                m_currentLetterData["phoneme"].ToString(),
                m_currentLetter);
            PlayHint();
        }
    }

	void FinishGame()
	{
		StartCoroutine(FinishGameCo());
	}

	IEnumerator FinishGameCo()
	{
		yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage)
		{
			JourneyInformation.Instance.OnGameFinish();
		}
		else
		{
			TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
		}
	}
	
	AudioClip GetCurrentAudio()
	{
		if (m_longAudio[m_currentLetterData] != null)
        {
            return m_longAudio[m_currentLetterData];
        }
        else
        {
            return m_graphemeAudio[m_currentLetterData];
        }
	}

    void PlayHint()
    {
		m_audioSource.clip = GetCurrentAudio();
		m_audioSource.Play();
    }

    public IEnumerator SpawnQuestion()
    {
        yield return new WaitForSeconds(1.0f);

        int selectedIndex = Random.Range(0, m_lettersPool.Count);

        m_currentLetterData = m_lettersPool[selectedIndex];
        m_currentLetter = m_currentLetterData["phoneme"].ToString();

        int letters = 5;
        for (int index = 0; index < 5; ++index)
        {
            string letter = m_currentLetter;
            if (index != 0)
            {
                letter = m_lettersPool[Random.Range(0, m_lettersPool.Count)]["phoneme"].ToString();
            }
            GameObject newSplat = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_splatPrefab, m_playArea);
            newSplat.GetComponent<SplattableLetter>().SetUp(letter, m_playAreaMin, m_playAreaMax, m_loadedSkin.GetComponent<SplatGameSkin>());
            m_spawnedObjects.Add(newSplat);
        }
		
		m_bennyTheBook.SetChangeableInstruction(GetCurrentAudio());
		
		PlayHint();

        m_spriteAnim.PlayAnimation("OFF");

    }
}
