using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class DragToContainerCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_draggablePrefab;
	[SerializeField]
	private List<Transform> m_draggableLocators = new List<Transform>();
	[SerializeField]
	private GameObject m_crocodilePrefab;
	[SerializeField]
	private List<Transform> m_crocodileLocators = new List<Transform>();
	[SerializeField]
	private int m_numSpawn;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private DataType m_dataType;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private Blackboard m_blackboard;

	public enum DataType
	{
		words,
		phonemes
	}

	AudioClip m_currentShortAudio;
	AudioClip m_currentLongAudio;

	List<DraggableLabel> m_spawnedDraggables = new List<DraggableLabel>();

	List<DragToCrocodile> m_spawnedCrocodiles = new List<DragToCrocodile>();

	List<DataRow> m_targetData = new List<DataRow>();
	List<DataRow> m_dummyData = new List<DataRow>();

	DataRow m_currentTargetData = null;

	string m_textAttribute;

	int SortByPosX(DragToCrocodile dragA, DragToCrocodile dragB)
	{
		float a = dragA.transform.position.x;
		float b = dragB.transform.position.x;

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

	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		yield return new WaitForSeconds(1f);

		List<DataRow> data;
		string targetAttribute;


		if(m_dataType == DataType.phonemes)
		{
			data = DataHelpers.GetPhonemes();
			m_textAttribute = "phoneme";
			targetAttribute = "is_target_phoneme";
		}
		else
		{
			//data = DataHelpers.GetSectionWords(446).Rows;
			data = DataHelpers.GetWords();
			m_textAttribute = "word";
			targetAttribute = "is_target_word";
		}

		foreach(DataRow row in data)
		{
			if(row[targetAttribute] != null && row[targetAttribute].ToString() == "t")
			{
				m_targetData.Add(row);
			}
			else
			{
				m_dummyData.Add(row);
			}
		}

		// We cannot ask more questions than there are target questions to ask
		if(m_targetScore > m_targetData.Count) 
		{
			m_targetScore = m_targetData.Count;
		}

		// We cannot ask more questions than there are crocodiles to fit them in
		if(m_targetScore > m_crocodileLocators.Count)
		{
			m_targetScore = m_crocodileLocators.Count;
		}

		if(m_targetScore == 1)
		{
			m_crocodileLocators.RemoveAt(m_crocodileLocators.Count - 1); // Remove the last crocodile
			m_crocodileLocators.RemoveAt(0); // Remove the first crocodile
		}

		for(int i = 0; i < m_targetScore; ++i)
		{
			GameObject newCrocodile = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_crocodilePrefab, m_crocodileLocators[i]);
			m_spawnedCrocodiles.Add(newCrocodile.GetComponent<DragToCrocodile>() as DragToCrocodile);
			newCrocodile.GetComponent<DragToCrocodile>().EnableTrigger(false);
		}

		m_crocodileLocators = null; // Free up the memory 

		m_spawnedCrocodiles.Sort(SortByPosX);

		// In a single question, we cannot spawn more than all the dummies and the target
		if(m_numSpawn > m_dummyData.Count + 1) 
		{
			m_numSpawn = m_dummyData.Count + 1;
		}

		// In a single question, we cannot spawn more than the number of draggable locators
		if(m_numSpawn > m_draggableLocators.Count) 
		{
			m_numSpawn = m_draggableLocators.Count;
		}

		if(m_targetData.Count > 0)
		{
			SpawnQuestion();
		}
		else
		{
			StartCoroutine(OnGameFinish());
		}
	}

	void SpawnQuestion()
	{
		m_spawnedCrocodiles[0].EnableTrigger(true);
		m_spawnedCrocodiles[0].PlayOpen();

		m_currentTargetData = m_targetData[Random.Range(0, m_targetData.Count)];
		m_targetData.Remove(m_currentTargetData);

		if(m_dataType == DataType.phonemes)
		{
			m_currentShortAudio = AudioBankManager.Instance.GetAudioClip(m_currentTargetData["grapheme"].ToString());
			m_currentLongAudio = LoaderHelpers.LoadMnemonic(m_currentTargetData);
		}
		else
		{
			m_currentShortAudio = LoaderHelpers.LoadAudioForWord(m_currentTargetData[m_textAttribute].ToString());
			m_currentLongAudio = m_currentShortAudio;
		}

		HashSet<DataRow> spawnData = new HashSet<DataRow>();

		spawnData.Add(m_currentTargetData);

		int safetyCounter = 0;
		while(spawnData.Count < m_numSpawn && safetyCounter < 100)
		{
			spawnData.Add(m_dummyData[Random.Range(0, m_dummyData.Count)]);
			++safetyCounter;
		}

		List<Transform> draggableLocators = new List<Transform>();
		draggableLocators.AddRange(m_draggableLocators);

		foreach(DataRow data in spawnData)
		{
			int locatorIndex = Random.Range(0, draggableLocators.Count);
			GameObject newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, draggableLocators[locatorIndex]);
			draggableLocators.RemoveAt(locatorIndex);

			DraggableLabel draggable = newDraggable.GetComponent<DraggableLabel>() as DraggableLabel;
			draggable.SetUp(data[m_textAttribute].ToString(), null, true, data);
			m_spawnedDraggables.Add(draggable);
			draggable.OnRelease += OnDraggableRelease;
		}

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

		PlayShortAudio();

		ChangeableBennyAudio.Instance.SetChangeableInstruction(m_currentLongAudio);
	}

	void OnDraggableRelease(DraggableLabel draggableBehaviour)
	{
		if(m_spawnedCrocodiles[0].IsTracking(draggableBehaviour.gameObject))
		{
			if(m_currentTargetData == draggableBehaviour.GetData())
			{
				StartCoroutine(OnCorrect(draggableBehaviour));
			}
			else
			{
				draggableBehaviour.TweenToStartPos();
				PlayLongAudio();
				
				if(m_dataType == DataType.phonemes)
				{
					string imageFilename =
						string.Format("Images/mnemonics_images_png_250/{0}_{1}",
						              m_currentTargetData["phoneme"],
						              m_currentTargetData["mneumonic"].ToString().Replace(" ", "_"));
					
					
					Texture2D image = (Texture2D)Resources.Load(imageFilename);
					string grapheme = m_currentTargetData["phoneme"].ToString();
					m_blackboard.ShowImage(image, grapheme, grapheme);
				}
				else
				{
					PipPadBehaviour.Instance.Show(m_currentTargetData[m_textAttribute].ToString());
				}
			}
		}
		else
		{
			draggableBehaviour.TweenToStartPos();
			PlayLongAudio();
		}
	}

	IEnumerator OnCorrect (DraggableLabel draggableBehaviour)
	{
		yield return new WaitForSeconds(PlayShortAudio() + 0.15f);

		iTween.MoveTo(draggableBehaviour.gameObject, m_spawnedCrocodiles[0].GetMouthPos(), 0.1f);

		yield return new WaitForSeconds(0.1f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_GULP");
		iTween.ScaleTo(draggableBehaviour.gameObject, Vector3.zero, 0.25f);
		m_spawnedCrocodiles[0].PlayClose();
		m_spawnedCrocodiles.RemoveAt(0);

		yield return new WaitForSeconds(0.5f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

		for(int i = m_spawnedDraggables.Count - 1; i > -1; --i)
		{
			m_spawnedDraggables[i].Off();
		}
		
		m_spawnedDraggables.Clear ();

		yield return new WaitForSeconds(1f);
		
		if(m_spawnedCrocodiles.Count > 0 && m_targetData.Count > 0)
		{
			SpawnQuestion();
		}
		else
		{
			StartCoroutine(OnGameFinish());
		}
	}
	
	void OnNoDragClick(DraggableLabel draggableBehaviour)
	{

	}

	IEnumerator OnGameFinish()
	{
		yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
		GameManager.Instance.CompleteGame();
	}

	float PlayShortAudio()
	{
		if(m_currentShortAudio != null)
		{
			m_audioSource.clip = m_currentShortAudio;
			m_audioSource.Play();
			return m_audioSource.clip.length;
		}
		else
		{
			return 0;
		}
	}

	float PlayLongAudio()
	{
		if(m_currentLongAudio != null)
		{
			m_audioSource.clip = m_currentLongAudio;
			m_audioSource.Play();
			return m_audioSource.clip.length;
		}
		else
		{
			return 0;
		}
	}
}
