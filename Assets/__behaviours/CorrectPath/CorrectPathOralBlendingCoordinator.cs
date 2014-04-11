using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CorrectPathOralBlendingCoordinator : MonoBehaviour 
{
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private GameObject m_clickablePrefab;
	[SerializeField]
	private List<GameObject> m_bgs = new List<GameObject>();
	[SerializeField]
	private List<Transform> m_locators;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private int m_numSpawn = 2;
	[SerializeField]
	private ClickEvent m_benny;
	[SerializeField]
	private float m_bgTweenDuration;
	[SerializeField]
	private UITexture m_platform;

	int m_score = 0;

	Vector3 m_bgTweenDir;
	Vector3 m_offScreenPos;

	List<DataRow> m_targetWords = new List<DataRow>();
	List<DataRow> m_dummyWords = new List<DataRow>();

	Dictionary<DataRow, AudioClip[]> m_phonemeAudio = new Dictionary<DataRow, AudioClip[]>();

	DataRow m_currentTargetWord = null;

	List<DraggableLabel> m_spawnedClickables = new List<DraggableLabel>();

	AudioClip m_currentTargetAudio = null;

	CorrectPathEnviro m_settings;

	bool m_canClick = true;

	bool m_isPlayingPhoneme = false;
	
	IEnumerator Start () 
	{
		//GameObject goSettings = (GameObject)Resources.Load("CorrectPath_Forest"); // TODO: Read Castle from a datasaver class
		//m_settings = goSettings.GetComponent<CorrectPathEnviro>() as CorrectPathEnviro;

		EnviroManager.Environment enviro = EnviroManager.Instance.GetEnvironment();
		m_settings = Resources.Load<CorrectPathEnviro>("CorrectPath/" + enviro + "_CorrectPath");

		//m_settings = Resources.Load<CorrectPathEnviro>("CorrectPath/Castle_CorrectPath");

		m_platform.mainTexture = m_settings.m_platform;

		for(int i = 0; i < m_bgs.Count; ++i)
		{
			m_bgs[i].GetComponentInChildren<UITexture>().mainTexture = m_settings.m_backgrounds[Random.Range(0, m_settings.m_backgrounds.Length)];
		}

		// always pip, always winner
		SessionInformation.Instance.SetPlayerIndex(0, 3);
		SessionInformation.Instance.SetWinner(0);

		m_bgs.Sort(SortGoByPosY);

		Debug.Log("m_bgs[0]: " + m_bgs[0].name);

		m_bgTweenDir = m_bgs[0].transform.position - m_bgs[1].transform.position;
		m_offScreenPos = m_bgs[1].transform.position;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		List<DataRow> words = DataHelpers.GetWords();

		/*
		List<DataRow> words = new List<DataRow>();

		if(Game.session == Game.Session.Premade)
		{
			words = DataHelpers.GetWords();
			//words = DataHelpers.GetSectionWords(1455).Rows;
		}
		else
		{
			words = DataHelpers.GetSectionWords(1455).Rows;
		}
		*/

		//Debug.Log("preCount: " + words.Count);
		/*
		for(int i = words.Count - 1; i > -1; --i)
		{
			Texture2D image = null;
			if ((words[i]["image"] != null) && (!string.IsNullOrEmpty(words[i]["image"].ToString())))
			{
				image = (Texture2D)Resources.Load("Images/word_images_png_350/_" + words[i]["image"].ToString());
			}

			if(image == null)
			{
				image = (Texture2D)Resources.Load("Images/word_images_png_350/_" + words[i]["word"].ToString());
			}

			if(image == null)
			{
				words.RemoveAt(i);
			}

			Resources.UnloadUnusedAssets();
		}

		Debug.Log("postCount: " + words.Count);
		*/

		foreach(DataRow word in words)
		{
			if(word["is_dummy_word"] != null && word["is_dummy_word"].ToString() == "t")
			{
				m_dummyWords.Add(word);
			}
			else
			{
				m_targetWords.Add(word);
			}

			yield return null; // Return next frame to reduce time taken for frame to execute
		}

		Debug.Log("m_targetWords.Count: " + m_targetWords.Count);
		Debug.Log("m_dummyWords.Count: " + m_dummyWords.Count);

		if(m_targetScore > m_targetWords.Count)
		{
			m_targetScore = m_targetWords.Count;
		}

		if(m_numSpawn > m_dummyWords.Count + 1)
		{
			m_numSpawn = m_dummyWords.Count + 1;
		}
	
		for(int i = 0; i < m_targetWords.Count; ++i)
		{
			string[] phonemeIds = m_targetWords[i]["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			
			List<AudioClip> phonemeAudioList = new List<AudioClip>();
			
			foreach(string id in phonemeIds)
			{
				DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
				if(dt.Rows.Count > 0)
				{
					phonemeAudioList.Add(AudioBankManager.Instance.GetAudioClip(dt.Rows[0]["grapheme"].ToString()));
				}
			}
			
			m_phonemeAudio[m_targetWords[i]] = phonemeAudioList.ToArray();

			yield return null; // Return next frame to reduce time taken for frame to execute
		}

		Resources.UnloadUnusedAssets();

		if(m_targetWords.Count > 0)
		{
			m_benny.OnSingleClick += OnBennyClick;
			//StartCoroutine(SpawnQuestion());
			SpawnQuestion();
		}
		else
		{
			StartCoroutine(OnGameFinish());
		}
	}

	void SpawnQuestion()
	{
		m_currentTargetWord = m_targetWords[Random.Range(0, m_targetWords.Count)];
		m_targetWords.Remove(m_currentTargetWord);

        UserStats.Activity.AddWord(m_currentTargetWord);

		m_currentTargetAudio = LoaderHelpers.LoadAudioForWord(m_currentTargetWord["word"].ToString());

		List<DataRow> answers = new List<DataRow>();
		answers.Add(m_currentTargetWord);

		int linkingIndex = -1;

		if(m_currentTargetWord["linking_index"] != null)
		{
			linkingIndex = System.Convert.ToInt32(m_currentTargetWord["linking_index"]);
		}

		foreach(DataRow dummyWord in m_dummyWords)
		{
			if(dummyWord["linking_index"] != null && System.Convert.ToInt32(dummyWord["linking_index"]) == linkingIndex && answers.Count < m_numSpawn)
			{
				answers.Add(dummyWord);
			}
		}

		if(answers.Count < 2)
		{
			List<DataRow> dummyWords = new List<DataRow>();
			dummyWords.AddRange(m_dummyWords);
			while(answers.Count < m_numSpawn && dummyWords.Count > 0)
			{
				int dummyIndex = Random.Range(0, dummyWords.Count);
				answers.Add(dummyWords[dummyIndex]);
				dummyWords.RemoveAt(dummyIndex);
			}
		}

		while(answers.Count < 2)
		{
			DataRow dummyWord = m_targetWords[Random.Range(0, m_targetWords.Count)];

			if(m_currentTargetWord != dummyWord)
			{
				answers.Add(dummyWord);
			}
		}

		m_locators.Sort(ShuffleTransformList);

		for(int i = 0; i < answers.Count && i < m_locators.Count; ++i)
		{
			GameObject newClickable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_clickablePrefab, m_locators[i]);

			Texture2D image = null;
			if ((answers[i]["image"] != null) && (!string.IsNullOrEmpty(answers[i]["image"].ToString())))
			{
				image = (Texture2D)Resources.Load("Images/word_images_png_350/_" + answers[i]["image"].ToString());
			}
			else
			{
				image = (Texture2D)Resources.Load("Images/word_images_png_350/_" + answers[i]["word"].ToString());
			}

			DraggableLabel clickableBehaviour = newClickable.GetComponent<DraggableLabel>() as DraggableLabel;
			m_spawnedClickables.Add(clickableBehaviour);
			clickableBehaviour.SetUp("", null, false, answers[i], image);
			clickableBehaviour.OnNoDragClick += OnNoDragClick;
		}

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

		//StartCoroutine(PlayPhonemeAudio(0.8f));
		StartCoroutine(PlayPhonemeAudio());

		m_canClick = true;
	}

	IEnumerator TweenBGs()
	{
		for(int i = 0; i < m_bgs.Count; ++i)
		{
			iTween.MoveBy(m_bgs[i].gameObject, m_bgTweenDir, m_bgTweenDuration);
		}

		yield return new WaitForSeconds(m_bgTweenDuration);

		if(m_score < m_targetScore - 2) // -2 because we have not yet incremented score
		{
			m_bgs[0].GetComponentInChildren<UITexture>().mainTexture = m_settings.m_backgrounds[Random.Range(0, m_settings.m_backgrounds.Length)];
		}
		else
		{
			m_bgs[0].GetComponentInChildren<UITexture>().mainTexture = m_settings.m_finalBackground;
		}


		m_bgs[0].transform.position = m_offScreenPos;

		m_bgs.Reverse();
	}

	void OnNoDragClick(DraggableLabel draggable)
	{
		if(m_canClick)
		{
			/*
			StopCoroutine("PlayAllAudio");
			StopCoroutine("PlayPhonemeAudio");
			StopCoroutine("PlayWordAudio");
			*/

            UserStats.Activity.IncrementNumAnswers();

			StopAllCoroutines();

			if(draggable.GetData() == m_currentTargetWord)
			{
				m_canClick = false;
				StartCoroutine(OnCorrect());
			}
			else
			{
                UserStats.Activity.AddIncorrectWord(m_currentTargetWord);
				StartCoroutine(PlayAllAudio());
			}
		}
	}

	IEnumerator OnCorrect()
	{
		if(m_isPlayingPhoneme)
		{
			yield return StartCoroutine(PlayWordAudio());
		}
		else
		{
			yield return StartCoroutine(PlayAllAudio());
		}

		yield return new WaitForSeconds(0.2f);

		foreach(DraggableLabel clickable in m_spawnedClickables)
		{
			clickable.Off();
		}

		m_spawnedClickables.Clear();

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

		yield return new WaitForSeconds(0.3f);

		yield return StartCoroutine(TweenBGs());

		++m_score;
		if(m_score < m_targetScore)
		{
			//StartCoroutine(SpawnQuestion());
			SpawnQuestion();
		}
		else
		{
			StartCoroutine(OnGameFinish());
		}

		yield return null;
	}

	IEnumerator OnGameFinish()
	{
		yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
		if(Game.session == Game.Session.Premade)
		{
			GameManager.Instance.CompleteGame();
		}
		else
		{
			TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
		}
	}

	void OnBennyClick(ClickEvent behaviour)
	{
		StartCoroutine(PlayAllAudio());
	}

	IEnumerator PlayAllAudio(float initialDelay = 0)
	{
		yield return new WaitForSeconds(initialDelay);
		
		yield return StartCoroutine(PlayPhonemeAudio());
		
		yield return StartCoroutine(PlayWordAudio());
	}

	IEnumerator PlayWordAudio()
	{
		if(m_currentTargetAudio != null)
		{
			m_audioSource.clip = m_currentTargetAudio;
			m_audioSource.Play();
			yield return new WaitForSeconds(m_audioSource.clip.length);
		}
		yield return null;
	}

	IEnumerator PlayPhonemeAudio(float initialDelay = 0)
	{
		m_isPlayingPhoneme = true;

		yield return new WaitForSeconds(initialDelay);

		if(m_phonemeAudio.ContainsKey(m_currentTargetWord))
		{
			AudioClip[] phonemeAudio = m_phonemeAudio[m_currentTargetWord];
			
			foreach(AudioClip clip in phonemeAudio)
			{
				m_audioSource.clip = clip;
				if(m_audioSource.clip != null)
				{
					m_audioSource.Play();
					yield return new WaitForSeconds(m_audioSource.clip.length + 0.2f);
				}
			}
		}

		while(m_audioSource.isPlaying)
		{
			yield return null;
		}

		m_isPlayingPhoneme = false;
		
		//yield break;
	}

	int ShuffleTransformList(Transform a, Transform b)
	{
		int i = 0;

		while(i == 0)
		{
			i = Random.Range(-1, 2);
		}

		return i;
	}

	int SortGoByPosY(GameObject goA, GameObject goB)
	{
		float a = goA.transform.position.y;
		float b = goB.transform.position.y;

		if(a < b)
		{
			return -1;
		}
		else if(a > b)
		{
			return 1;
		}
		else
		{
			return 0;
		}

	}
}
