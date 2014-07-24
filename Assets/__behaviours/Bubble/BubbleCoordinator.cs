using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class BubbleCoordinator : MonoBehaviour
{
	[SerializeField]
	private GameObject m_bubblePrefab;
	[SerializeField]
	private GameObject m_bubbleParentPrefab;
	[SerializeField]
	private Transform m_bubbleParentLocator;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private Transform m_bubbleDistanceLocator;
	[SerializeField]
	private Transform m_bubbleDestroyLocator;
	[SerializeField]
	private int m_maxLives = -1;
	[SerializeField]
	private float m_timeRemaining = -1f;
	[SerializeField]
	private string m_dataType;
	[SerializeField]
	private int m_expToLevelUp;
	[SerializeField]
	private float m_speed;
	[SerializeField]
	private float m_speedIncrease;
	[SerializeField]
	private int m_numSpawn;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private UILabel m_scoreCounter;
	[SerializeField]
	private float m_probabilityTarget;
	[SerializeField]
	private ClickEvent m_benny;
	[SerializeField]
	private int m_maxRowCount;
	[SerializeField]
	private float m_bubbleHeightRange = 50f;
	[SerializeField]
	private LivesDisplay m_livesDisplay;
	[SerializeField]
	private GameObject m_levelUp;
	[SerializeField]
	private GameObject m_newHighScore;
	[SerializeField]
	private Transform m_celebrationOff;

	Vector3 m_celebrationOn;

	int m_numRows = 0;
	
	int m_livesRemaining;
	
	List<DataRow> m_dataPool = new List<DataRow>();
	DataRow m_targetData = null;

	List<Transform> m_bubbleParents = new List<Transform>();

	Transform m_highestBubbleParent;
	float m_spawnThreshold;
	
	int m_exp = 0;

	int m_currentLevel;

	int m_score = 0;

	IEnumerator Start()
	{
		m_celebrationOn = m_levelUp.transform.position;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if (System.String.IsNullOrEmpty(m_dataType))
        {
            //m_dataType = DataHelpers.GetDataType();
            m_dataType = "words";
        }

		AddToDataPool(true);

		if(m_dataType == "words" || m_dataType == "keywords")
		{
			PipPadBehaviour.Instance.Hiding += OnPipPadHide;
		}

		m_benny.SingleClicked += OnBennyClick;

		m_spawnThreshold = m_bubbleDistanceLocator.position.y;
		
		if(m_maxLives > 0)
		{
			m_livesRemaining = m_maxLives;
			m_livesDisplay.SetLifeIcon(3);
			m_livesDisplay.SetLives(m_livesRemaining);
		}

		
		if(m_numSpawn > m_locators.Length)
		{
			m_numSpawn = m_locators.Length;
		}
		
		if(m_numSpawn > m_dataPool.Count)
		{
			m_numSpawn = m_dataPool.Count;
		}

		m_scoreCounter.text = m_score.ToString();
		
		if(m_dataPool.Count > 0)
		{
			//yield return StartCoroutine(SpawnBubbles());
			StartCoroutine(SpawnBubbles());

			yield return new WaitForSeconds(0.5f);

			WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_READY_STEADY_GO");

			yield return new WaitForSeconds(2f);

			StartCoroutine(NewTarget());

			if(m_timeRemaining > 0)
			{
				StartCoroutine(CountTime());
			}
		}
		else
		{
			StartCoroutine(OnGameFinish());
		}
	}

	IEnumerator SpawnBubbles()
	{
		List<DataRow> spawnData = new List<DataRow>();
		
		while(spawnData.Count < m_numSpawn)
		{
			spawnData.Add(m_dataPool[Random.Range(0, m_dataPool.Count)]);
		}

		if(m_targetData != null && !spawnData.Contains(m_targetData))
		{
			spawnData[Random.Range(0, spawnData.Count)] = m_targetData;
		}
		
		GameObject newBubbleParent = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bubbleParentPrefab, m_bubbleParentLocator);
		m_bubbleParents.Add(newBubbleParent.transform);
		m_highestBubbleParent = newBubbleParent.transform;
		
		PermaMove parentBehaviour = newBubbleParent.AddComponent<PermaMove>() as PermaMove;
		parentBehaviour.SetDirection(Vector3.down);
		parentBehaviour.SetSpeed(m_speed);
		
		int locatorIndex = 0;
		foreach(DataRow data in spawnData)
		{
			GameObject newBubble = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bubblePrefab, 
			                                                                              m_locators[locatorIndex]);
			
			newBubble.transform.parent = newBubbleParent.transform;
			
			Vector3 newLocalPos = newBubble.transform.localPosition;
			newLocalPos.y = Random.Range(-m_bubbleHeightRange, m_bubbleHeightRange);
			newBubble.transform.localPosition = newLocalPos;
			
			ClickableWidget bubbleBehaviour = newBubble.GetComponentInChildren<ClickableWidget>() as ClickableWidget;
			bubbleBehaviour.SetUp(data);
			bubbleBehaviour.OnWidgetClick += OnBubbleClick;
			
			++locatorIndex;
		}
		
		while(m_highestBubbleParent == null || m_highestBubbleParent.position.y > m_spawnThreshold)
		{
			yield return null;
		}
		
		++m_numRows;
		
		if(m_numRows < m_maxRowCount)
		{
			StartCoroutine(SpawnBubbles());
		}
		else
		{
			StartCoroutine(UpdateBubbles());
		}
	}
	
	IEnumerator UpdateBubbles()
	{
		Transform lowestBubbleParent = m_bubbleParents[0];
		
		foreach(Transform bubbleParent in m_bubbleParents)
		{
			if(bubbleParent.position.y < lowestBubbleParent.position.y)
			{
				lowestBubbleParent = bubbleParent;
			}
		}


		List<DataRow> newData = new List<DataRow>();
		ClickableWidget[] bubbles = lowestBubbleParent.GetComponentsInChildren<ClickableWidget>() as ClickableWidget[];

		for(int i = 0; i < bubbles.Length; ++i)
		{
			newData.Add(m_dataPool[Random.Range(0, m_dataPool.Count)]);
		}

		if(m_targetData != null && !newData.Contains(m_targetData))
		{
			newData[Random.Range(0, newData.Count)] = m_targetData;
		}


		for(int i = 0; i < bubbles.Length; ++i)
		{
			bubbles[i].collider.enabled = true;

			bubbles[i].Off();

			if(i < newData.Count)
			{
				bubbles[i].SetUp(newData[i]);
			}
			
			Vector3 newLocalPos = bubbles[i].transform.localPosition;
			newLocalPos.y = Random.Range(-m_bubbleHeightRange, m_bubbleHeightRange);
			bubbles[i].transform.localPosition = newLocalPos;
		}
		
		// Move bubble parent back to the top
		lowestBubbleParent.transform.position = m_bubbleParentLocator.position;
		m_highestBubbleParent = lowestBubbleParent.transform;
		
		while(m_highestBubbleParent == null || m_highestBubbleParent.position.y > m_spawnThreshold)
		{
			yield return null;
		}
		
		StartCoroutine(UpdateBubbles());
	}

	void OnPipPadHide()
	{
		foreach(Transform bubbleParent in m_bubbleParents)
		{
			bubbleParent.GetComponent<PermaMove>().Play();
		}
	}

	void OnBubbleClick(ClickableWidget bubbleBehaviour) 
	{
		bubbleBehaviour.collider.enabled = false;

		if(m_targetData != null)
		{
			DataRow data = bubbleBehaviour.GetData();
			
			bool isCorrect = (System.Convert.ToInt32(data["id"]) == System.Convert.ToInt32(m_targetData["id"]));

			if(m_dataType == "phonemes")
			{
				isCorrect = (m_targetData["grapheme"].ToString() == data["grapheme"].ToString());
			}

			bubbleBehaviour.On();
			
			if(isCorrect)
			{
				WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");
				
				++m_score;
				m_scoreCounter.text = m_score.ToString();
				iTween.PunchScale(m_scoreCounter.transform.parent.gameObject, Vector3.one * 1.5f, 0.8f);
				
				++m_exp;
				if(m_exp >= m_expToLevelUp)
				{
					LevelUp();
				}
				
				StartCoroutine(NewTarget(0.8f));
			}
			else
			{
				WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_FIREFLY");
				
				if(Random.Range(0,3) == 0) // 1/3 probability to play a burp
				{
					WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_BURP_RANDOM");
				}

				if(m_dataType == "words" || m_dataType == "keywords")
				{
					PipPadBehaviour.Instance.Show(m_targetData["word"].ToString());
					foreach(Transform bubbleParent in m_bubbleParents)
					{
						bubbleParent.GetComponent<PermaMove>().Pause();
					}
				}
				
				if(m_maxLives > 0)
				{
					--m_livesRemaining;
					m_livesDisplay.SetLives(m_livesRemaining);
					
					if(m_livesRemaining <= 0)
					{
						StartCoroutine(OnGameFinish());
					}
				}
			}
		}
	}
	
	void LevelUp()
	{
		//D.Log("LEVEL UP!");

		m_speed += m_speedIncrease;
		foreach(Transform bubbleParent in m_bubbleParents)
		{
			bubbleParent.GetComponent<PermaMove>().SetSpeed(m_speed);
		}
		
		m_exp = 0;
		++m_expToLevelUp;

		++m_currentLevel;

		StartCoroutine(CelebrationCoordinator.Instance.LevelUp(m_currentLevel));
	}

	void AddToDataPool(bool inclusive)
	{
		switch(m_dataType)
		{
		case "keywords":
			m_dataPool.AddRange(DataHelpers.GetKeywords());
			break;
		case "phonemes":
			m_dataPool.AddRange(DataHelpers.GetPhonemes());
			break;
		default:
			//D.Log("Getting word data");
			m_dataPool.AddRange(DataHelpers.GetWords());
			break;
		}
	}
	
	IEnumerator CountTime()
	{
		while(true)
		{
			m_timeRemaining -= Time.deltaTime;
			
			if(m_timeRemaining < 0)
			{
				StartCoroutine(OnGameFinish());
			}
			
			yield return null;
		}
	}
	
	IEnumerator NewTarget(float delay = 0f)
	{
		yield return new WaitForSeconds(delay);
		m_targetData = m_dataPool[Random.Range(0, m_dataPool.Count)];
		SayShortAudio();
	}
	
	IEnumerator OnGameFinish()
	{
		if(m_dataType == "words" || m_dataType == "keywords")
		{
			while(PipPadBehaviour.Instance.IsShowing())
			{
				yield return null;
			}

			yield return new WaitForSeconds(1f);
		}
		else
		{
			yield return null;
		}

        SessionInformation.SetDefaultPlayerVar();
        GameManager.Instance.CompleteGame();
	}
	
	void SayLongAudio()
	{
		if(m_dataType == "phonemes")
		{
			AudioClip clip = LoaderHelpers.LoadMnemonic(m_targetData);

			if(clip != null)
			{
				m_audioSource.clip = clip;
				m_audioSource.Play();
			}
		}
		else
		{
			SayShortAudio();
		}
	}
	
	void SayShortAudio()
	{
		AudioClip clip = null;
		
		if(m_dataType == "phonemes")
		{
			clip = AudioBankManager.Instance.GetAudioClip(m_targetData["grapheme"].ToString());
		}
		else
		{
			////D.Log("SayShortAudio()");
			////D.Log("m_targetData: " + m_targetData);
			////D.Log("id: " + m_targetData["id"].ToString());
			////D.Log("word: " + m_targetData["word"].ToString());
			clip = LoaderHelpers.LoadAudioForWord(m_targetData["word"].ToString());
		}
		
		if(clip != null)
		{
			m_audioSource.clip = clip;
			m_audioSource.Play();
		}
	}
	
	void OnBennyClick(ClickEvent clickBehaviour)
	{
		SayLongAudio();
	}
}