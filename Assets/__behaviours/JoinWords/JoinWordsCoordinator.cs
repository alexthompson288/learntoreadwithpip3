using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class JoinWordsCoordinator : Singleton<JoinWordsCoordinator>
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
    private int m_targetScore = 2;
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

    List<DataRow> m_wordSelection = new List<DataRow>();

    List<GameObject> m_spawnedObjects = new List<GameObject>();

    int m_score;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_wordSelection.AddRange(DataHelpers.GetWords());

        m_progressScoreBar.SetStarsTarget(m_targetScore);

        //yield return new WaitForSeconds(1.0f);
        //WingroveAudio.WingroveRoot.Instance.PostEvent("MATCH_INSTRUCTION");
        //yield return new WaitForSeconds(3.0f);

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
			if(tex == null)
			{
				m_wordSelection.Remove(m_wordSelection[i]);
			}
			
			Resources.UnloadUnusedAssets();
		}

		if(m_pairsToShowAtOnce > m_wordSelection.Count)
		{
			m_pairsToShowAtOnce = m_wordSelection.Count;
		}

		if(m_wordSelection.Count > 0)
		{
        	StartCoroutine(SetUpNext());
		}
		else
		{
			FinishGame();
		}
    }

    IEnumerator SetUpNext()
    {
        //HashSet<string> words = new HashSet<string>();
        HashSet<DataRow> words = new HashSet<DataRow>();

        while (words.Count < m_pairsToShowAtOnce)
        {
            //words.Add(m_wordSelection[Random.Range(0, m_wordSelection.Count)]["word"].ToString());
            words.Add(m_wordSelection[Random.Range(0, m_wordSelection.Count)]);
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

        foreach (DataRow selectedWord in words)
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

            //newText.GetComponent<WordPictureJoinableDrag>().SetUp(selectedWord);
            //newImage.GetComponent<WordPictureJoinableDrag>().SetUp(selectedWord);

            newText.GetComponent<JoinableLineDraw>().SetUp("words", selectedWord);
            newText.GetComponent<JoinableLineDraw>().JoinableJoinEventHandler += OnJoinableRelease;

            newImage.GetComponent<JoinableLineDraw>().SetUp("words", selectedWord);
            newImage.GetComponent<JoinableLineDraw>().JoinableJoinEventHandler += OnJoinableRelease;

            //newText.transform.localScale = Vector3.zero;
            //newImage.transform.localScale = Vector3.zero;

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
			FinishGame();
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(SetUpNext());
        }
    }

	void FinishGame()
	{
		SessionInformation.SetDefaultPlayerVar();
		GameManager.Instance.CompleteGame();
	}

    public void SpeakWord(string word)
    {
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(word);
        GetComponent<AudioSource>().clip = loadedAudio;
        GetComponent<AudioSource>().Play();
    }

    public IEnumerator SpeakWellDone(string word)
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
        Resources.UnloadUnusedAssets();
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(word);
        GetComponent<AudioSource>().clip = loadedAudio;
        GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(1.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");      
    }

    public void Connect(WordPictureJoinableDrag a, WordPictureJoinableDrag b)
    {
        if (a != b)
        {
            if (a.IsPicture() != b.IsPicture())
            {
                if (a.GetWord() == b.GetWord())
                {
                    StartCoroutine(SpeakWellDone(a.GetWord()));
                                  
                    a.Off(a.transform.position.x < b.transform.position.x ? m_leftOff : m_rightOff);
                    b.Off(a.transform.position.x < b.transform.position.x ? m_rightOff : m_leftOff);
                    m_spawnedObjects.Remove(a.gameObject);
                    m_spawnedObjects.Remove(b.gameObject);

                    if (m_spawnedObjects.Count == 0)
                    {                        
                        StartCoroutine(AddPoint());
                    }
                }
                else
                {
					WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
                    PipPadBehaviour.Instance.Show(a.IsPicture() ? b.GetWord() : a.GetWord());
                    PipPadBehaviour.Instance.SayAll(1.5f);
                }
            }
        }
    }

    void OnJoinableRelease(JoinableLineDraw a, JoinableLineDraw b)
    {
        if (a != b)
        {
            if (a.isPicture != b.isPicture)
            {
                if (a.data == b.data)
                {
                    StartCoroutine(SpeakWellDone(a.data["word"].ToString()));
                    
                    a.TransitionOff(a.transform.position.x < b.transform.position.x ? m_leftOff : m_rightOff);
                    b.TransitionOff(a.transform.position.x < b.transform.position.x ? m_rightOff : m_leftOff);
                    m_spawnedObjects.Remove(a.gameObject);
                    m_spawnedObjects.Remove(b.gameObject);
                    
                    if (m_spawnedObjects.Count == 0)
                    {                        
                        StartCoroutine(AddPoint());
                    }
                }
                else
                {
                    WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
                    PipPadBehaviour.Instance.Show(a.isPicture ? b.data["word"].ToString() : a.data["word"].ToString());
                    PipPadBehaviour.Instance.SayAll(1.5f);
                }
            }
        }
    }

}
