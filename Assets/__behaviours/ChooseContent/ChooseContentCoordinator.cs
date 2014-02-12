using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ChooseContentCoordinator : Singleton<ChooseContentCoordinator> 
{
	[SerializeField]
	private Transform m_letterSetButtonParent;
	[SerializeField]
	private UIGrid m_letterGrid;
	[SerializeField]
	private UIDraggableCamera m_letterCamera;
	[SerializeField]
	private UIGrid m_wordGrid;
	[SerializeField]
	private UIDraggableCamera m_wordCamera;
	[SerializeField]
	private UIGrid m_keywordGrid;
	[SerializeField]
	private UIDraggableCamera m_keywordCamera;
	[SerializeField]
	private GameObject m_setPrefab;
	[SerializeField]
	private GameObject m_wordPrefab;
	[SerializeField]
	private GameObject m_letterPrefab;
	[SerializeField]
	private GameObject m_keywordPrefab;
	[SerializeField]
	private GameObject m_emptyPrefab;
	[SerializeField]
	private int[] m_sets;
	[SerializeField]
	private int[] m_letterSections;
	[SerializeField]
	private int[] m_wordSections;
	[SerializeField]
	private int m_markerRowCount = 6;
	
	List<GameObject> m_spawnedButtons = new List<GameObject>();
	
	Vector3 m_letterCamStartPos;
	Vector3 m_wordCamStartPos;

	Dictionary<GameObject, GameObject> m_letterMarkers = new Dictionary<GameObject, GameObject>();
	Dictionary<GameObject, GameObject> m_wordMarkers = new Dictionary<GameObject, GameObject>();
	Dictionary<GameObject, GameObject> m_keywordMarkers = new Dictionary<GameObject, GameObject>();

	
	// Use this for initialization
	IEnumerator Start () 
	{
		m_letterCamStartPos = m_letterCamera.transform.position;
		
		yield return StartCoroutine(ContentInformation.WaitForLoad());
		
		StartCoroutine(SpawnLetters());
		StartCoroutine(SpawnWords());
		StartCoroutine(SpawnKeywords());

		SetActiveWords(false);
		SetActiveKeywords(false);
		SetActiveLetters(true);
	}

	public void SetActiveLetters (bool isActive)
	{
		m_letterCamera.transform.position = m_letterCamStartPos;
		m_letterCamera.enabled = isActive;
		m_letterCamera.GetComponent<Camera>().enabled = isActive;
		m_letterGrid.gameObject.SetActive(isActive);
		m_letterGrid.Reposition();
		m_letterSetButtonParent.gameObject.SetActive(isActive);
	}

	public void SetActiveWords (bool isActive)
	{
		m_wordCamera.transform.position = m_letterCamStartPos;
		m_wordCamera.enabled = isActive;
		m_wordCamera.GetComponent<Camera>().enabled = isActive;
		m_wordGrid.gameObject.SetActive(isActive);
		m_wordGrid.Reposition();
	}

	public void SetActiveKeywords (bool isActive)
	{
		m_keywordCamera.transform.position = m_letterCamStartPos;
		m_keywordCamera.enabled = isActive;
		m_keywordCamera.GetComponent<Camera>().enabled = isActive;
		m_keywordGrid.gameObject.SetActive(isActive);
		m_keywordGrid.Reposition();
	}
	
	float ClearButtons()
	{
		if(m_spawnedButtons.Count > 0)
		{
			float spawnDelay = m_spawnedButtons[0].GetComponent<ScaleOnOffBehaviour>().GetTweenDuration();
			
			for(int i = 0; i < m_spawnedButtons.Count; ++i)
			{
				ScaleOnOffBehaviour buttonBehaviour = m_spawnedButtons[i].GetComponent<ScaleOnOffBehaviour>() as ScaleOnOffBehaviour;
				
				if(buttonBehaviour != null)
				{
					StartCoroutine(buttonBehaviour.Off());
				}
				else
				{
					Destroy(m_spawnedButtons[i]);
				}
			}
			
			m_spawnedButtons.Clear();
			
			return spawnDelay;
		}
		else
		{
			return 0;
		}
	}
	
	public IEnumerator SpawnLetters ()
	{
		m_letterCamera.transform.position = m_letterCamStartPos;

		//SetActiveLetters(true);
		//SetActiveWords(false);
		//SetActiveKeywords(false);
		
		yield return new WaitForSeconds(0.1f);
		
		Dictionary<Transform, Transform> positions = new Dictionary<Transform, Transform>();
		
		for(int setIndex = 0; setIndex < m_sets.Length; ++setIndex)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + m_sets[setIndex]);
			
			if(dt.Rows.Count > 0)
			{
				string[] phonemeIds = dt.Rows[0]["setphonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				List<DataRow> phonemes = new List<DataRow>();
				
				foreach(string id in phonemeIds)
				{
					DataTable phonemeTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
					
					if(phonemeTable.Rows.Count > 0)
					{
						phonemes.Add(phonemeTable.Rows[0]);
					}
				}
				
				ChooseContentSet setButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setPrefab, m_letterSetButtonParent).GetComponent<ChooseContentSet>() 
					as ChooseContentSet;

				setButton.GetComponent<UIDragCamera>().draggableCamera = m_letterCamera;
				
				m_spawnedButtons.Add(setButton.gameObject);
				
				Debug.Log("phonemes.Count: " + phonemes.Count);
				
				for(int i = 0; i < m_letterGrid.maxPerLine; ++i)
				{
					GameObject newLetter = null;
					if(i < phonemes.Count)
					{
						newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterPrefab, m_letterGrid.transform);
						newLetter.GetComponent<UIDragCamera>().draggableCamera = m_letterCamera;
						newLetter.GetComponent<ChooseContentLetter>().SetUp(phonemes[i]);
						setButton.AddButton((ChooseContentLetter)newLetter.GetComponent<ChooseContentLetter>());
					}
					else
					{
						newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_letterGrid.transform);
					}
					
					newLetter.name = "Button";
					m_spawnedButtons.Add(newLetter);
					
					if(i == 0)
					{
						positions.Add(setButton.transform, newLetter.transform);
					}
				}
				
				setButton.SetUp(m_sets[setIndex]);
			}
		}
		
		Debug.Log("m_spawnedButtons.Count: " + m_spawnedButtons.Count);
		
		m_letterGrid.Reposition();
		
		foreach(KeyValuePair<Transform, Transform> kvp in positions)
		{
			Vector3 newPosition = kvp.Key.position;
			newPosition.y = kvp.Value.position.y;
			kvp.Key.position = newPosition;
		}
	}
	
	public IEnumerator SpawnWords ()
	{
		ChooseContentCamera contentCam = m_wordCamera.GetComponent<ChooseContentCamera>() as ChooseContentCamera;

		m_wordCamera.transform.position = m_letterCamStartPos;

		//SetActiveLetters(false);
		//SetActiveWords(true);
		//SetActiveKeywords(false);
		
		yield return new WaitForSeconds(0.1f);
		
		//foreach(int set in m_sets)
		for(int setIndex = 0; setIndex < m_sets.Length; ++setIndex)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + m_sets[setIndex]);
			
			if(dt.Rows.Count > 0)
			{
				ChooseContentSet newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setPrefab, m_wordGrid.transform).GetComponent<ChooseContentSet>() 
					as ChooseContentSet;

				contentCam.AddSet(newSetButton);
				
				newSetButton.name = "Button";
				newSetButton.GetComponent<UIDragCamera>().draggableCamera = m_wordCamera;
				
				m_spawnedButtons.Add(newSetButton.gameObject);
				
				for(int i = 0; i < m_wordGrid.maxPerLine - 1; ++i)
				{
					GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_wordGrid.transform);
					newEmpty.name = "Button";
					m_spawnedButtons.Add(newEmpty);
				}
				
				string[] wordIds = dt.Rows[0]["setwords"].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				List<DataRow> words = new List<DataRow>();
				
				foreach(string id in wordIds)
				{
					DataTable wordTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
					
					if(wordTable.Rows.Count > 0)
					{
						words.Add(wordTable.Rows[0]);
					}
				}
				
				for(int i = 0; i < words.Count; ++i)
				{
					GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, m_wordGrid.transform);
					newWord.name = "Button";
					newWord.GetComponent<UIDragCamera>().draggableCamera = m_wordCamera;
					newWord.GetComponent<ChooseContentWord>().SetUp(words[i]);
					newSetButton.AddButton((ChooseContentWord)newWord.GetComponent<ChooseContentWord>());
					m_spawnedButtons.Add(newWord);
				}

				if(words.Count > 0)
				{
					int remainder = words.Count % m_wordGrid.maxPerLine;
					
					if(remainder > 0)
					{
						int emptySlots = m_wordGrid.maxPerLine - remainder;
						for(int i = 0; i < emptySlots; ++i)
						{
							GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_wordGrid.transform);
							newEmpty.name = "Button";
							m_spawnedButtons.Add(newEmpty);
						}
					}
					
					newSetButton.SetUp(m_sets[setIndex], m_wordGrid.gameObject.layer);
				}
			}
		}
		
		m_wordGrid.Reposition();
	}
	
	public IEnumerator SpawnKeywords ()
	{
		m_keywordCamera.transform.position = m_letterCamStartPos;

		//SetActiveLetters(false);
		//SetActiveWords(false);
		//SetActiveKeywords(true);
		
		yield return new WaitForSeconds(0.1f);
		
		//foreach(int set in m_sets)
		for(int setIndex = 0; setIndex < m_sets.Length; ++setIndex)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + m_sets[setIndex]);
			
			if(dt.Rows.Count > 0)
			{
				string[] keywordIds = dt.Rows[0]["setkeywords"].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				List<DataRow> keywords = new List<DataRow>();
				
				foreach(string id in keywordIds)
				{
					DataTable keywordTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
					
					if(keywordTable.Rows.Count > 0)
					{
						keywords.Add(keywordTable.Rows[0]);
					}
				}

				if(keywords.Count > 0)
				{
					ChooseContentSet newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setPrefab, m_keywordGrid.transform).GetComponent<ChooseContentSet>() 
						as ChooseContentSet;
					
					newSetButton.name = "Button";
					newSetButton.GetComponent<UIDragCamera>().draggableCamera = m_keywordCamera;
					
					m_spawnedButtons.Add(newSetButton.gameObject);
					
					for(int i = 0; i < m_keywordGrid.maxPerLine - 1; ++i)
					{
						GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_keywordGrid.transform);
						newEmpty.name = "Button";
						m_spawnedButtons.Add(newEmpty);
					}

					for(int i = 0; i < keywords.Count; ++i)
					{
						GameObject newKeyword = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_keywordPrefab, m_keywordGrid.transform);
						newKeyword.name = "Button";
						newKeyword.GetComponent<UIDragCamera>().draggableCamera = m_keywordCamera;
						newKeyword.GetComponent<ChooseContentKeyword>().SetUp(keywords[i]);
						newSetButton.AddButton((ChooseContentKeyword)newKeyword.GetComponent<ChooseContentKeyword>());
						m_spawnedButtons.Add(newKeyword);
					}

			
					int remainder = keywords.Count % m_keywordGrid.maxPerLine;
					
					if(remainder > 0)
					{
						int emptySlots = m_keywordGrid.maxPerLine - remainder;
						for(int i = 0; i < emptySlots; ++i)
						{
							GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_keywordGrid.transform);
							newEmpty.name = "Button";
							m_spawnedButtons.Add(newEmpty);
						}
					}
					
					newSetButton.SetUp(m_sets[setIndex], m_keywordGrid.gameObject.layer);
				}
			}
		}

		m_keywordGrid.Reposition();
	}
}
/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ChooseContentCoordinator : Singleton<ChooseContentCoordinator> 
{
	[SerializeField]
	private Transform m_letterSetButtonParent;
	[SerializeField]
	private UIGrid m_letterGrid;
	[SerializeField]
	private UIDraggableCamera m_letterCamera;
	[SerializeField]
	private UIGrid m_wordGrid;
	[SerializeField]
	private UIDraggableCamera m_wordCamera;
	[SerializeField]
	private GameObject m_setPrefab;
	[SerializeField]
	private GameObject m_wordPrefab;
	[SerializeField]
	private GameObject m_letterPrefab;
	[SerializeField]
	private GameObject m_emptyPrefab;
	[SerializeField]
	private int[] m_sets;
	[SerializeField]
	private int[] m_letterSections;
	[SerializeField]
	private int[] m_wordSections;

	List<GameObject> m_spawnedButtons = new List<GameObject>();

	Vector3 m_letterCamStartPos;
	Vector3 m_wordCamStartPos;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		m_letterCamStartPos = m_letterCamera.transform.position;

		yield return StartCoroutine(ContentInformation.WaitForLoad());

		StartCoroutine(DisplayLetters());
	}

	float ClearButtons()
	{
		if(m_spawnedButtons.Count > 0)
		{
			float spawnDelay = m_spawnedButtons[0].GetComponent<ScaleOnOffBehaviour>().GetTweenDuration();
			
			for(int i = 0; i < m_spawnedButtons.Count; ++i)
			{
				ScaleOnOffBehaviour buttonBehaviour = m_spawnedButtons[i].GetComponent<ScaleOnOffBehaviour>() as ScaleOnOffBehaviour;

				if(buttonBehaviour != null)
				{
					StartCoroutine(buttonBehaviour.Off());
				}
				else
				{
					Destroy(m_spawnedButtons[i]);
				}
			}
			
			m_spawnedButtons.Clear();

			return spawnDelay;
		}
		else
		{
			return 0;
		}
	}

	public IEnumerator DisplayLetters ()
	{
		yield return new WaitForSeconds(ClearButtons());

		m_letterCamera.transform.position = m_letterCamStartPos;

		yield return new WaitForSeconds(0.1f);

		Dictionary<Transform, Transform> positions = new Dictionary<Transform, Transform>();

		for(int setIndex = 0; setIndex < m_sets.Length; ++setIndex)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + m_sets[setIndex]);

			if(dt.Rows.Count > 0)
			{
				string[] phonemeIds = dt.Rows[0]["setphonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');

				List<DataRow> phonemes = new List<DataRow>();

				foreach(string id in phonemeIds)
				{
					DataTable phonemeTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");

					if(phonemeTable.Rows.Count > 0)
					{
						phonemes.Add(phonemeTable.Rows[0]);
					}
				}

				ChooseContentSet setButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setPrefab, m_letterSetButtonParent).GetComponent<ChooseContentSet>() 
												as ChooseContentSet;

				m_spawnedButtons.Add(setButton.gameObject);

				Debug.Log("phonemes.Count: " + phonemes.Count);

				for(int i = 0; i < m_letterGrid.maxPerLine; ++i)
				{
					GameObject newLetter = null;
					if(i < phonemes.Count)
					{
						newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterPrefab, m_letterGrid.transform);
						newLetter.GetComponent<UIDragCamera>().draggableCamera = m_letterCamera;
						newLetter.GetComponent<ChooseContentLetter>().SetUp(phonemes[i]);
						setButton.AddButton((ChooseContentLetter)newLetter.GetComponent<ChooseContentLetter>());
					}
					else
					{
						newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_letterGrid.transform);
					}

					newLetter.name = "Button";
					m_spawnedButtons.Add(newLetter);

					if(i == 0)
					{
						positions.Add(setButton.transform, newLetter.transform);
					}
				}

				setButton.SetUp(m_sets[setIndex]);
			}
		}

		Debug.Log("m_spawnedButtons.Count: " + m_spawnedButtons.Count);

		m_letterGrid.Reposition();

		foreach(KeyValuePair<Transform, Transform> kvp in positions)
		{
			Vector3 newPosition = kvp.Key.position;
			newPosition.y = kvp.Value.position.y;
			kvp.Key.position = newPosition;
		}
	}

	public IEnumerator DisplayWords ()
	{
		yield return new WaitForSeconds(ClearButtons());

		m_wordCamera.transform.position = m_letterCamStartPos;

		yield return new WaitForSeconds(0.1f);

		//foreach(int set in m_sets)
		for(int setIndex = 0; setIndex < m_sets.Length; ++setIndex)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + m_sets[setIndex]);
			
			if(dt.Rows.Count > 0)
			{
				ChooseContentSet newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setPrefab, m_wordGrid.transform).GetComponent<ChooseContentSet>() 
					as ChooseContentSet;
				
				newSetButton.name = "Button";
				newSetButton.GetComponent<UIDragCamera>().draggableCamera = m_wordCamera;
				
				m_spawnedButtons.Add(newSetButton.gameObject);
				
				for(int i = 0; i < m_wordGrid.maxPerLine - 1; ++i)
				{
					GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_wordGrid.transform);
					newEmpty.name = "Button";
					m_spawnedButtons.Add(newEmpty);
				}

				string[] wordIds = dt.Rows[0]["setwords"].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				List<DataRow> words = new List<DataRow>();
				
				foreach(string id in wordIds)
				{
					DataTable wordTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
					
					if(wordTable.Rows.Count > 0)
					{
						words.Add(wordTable.Rows[0]);
					}
				}

				for(int i = 0; i < words.Count; ++i)
				{
					GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, m_wordGrid.transform);
					newWord.name = "Button";
					newWord.GetComponent<UIDragCamera>().draggableCamera = m_wordCamera;
					newWord.GetComponent<ChooseContentWord>().SetUp(words[i]);
					newSetButton.AddButton((ChooseContentWord)newWord.GetComponent<ChooseContentWord>());
					m_spawnedButtons.Add(newWord);
				}

				int remainder = words.Count % m_wordGrid.maxPerLine;

				if(remainder > 0)
				{
					int emptySlots = m_wordGrid.maxPerLine - remainder;
					for(int i = 0; i < emptySlots; ++i)
					{
						GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_wordGrid.transform);
						newEmpty.name = "Button";
						m_spawnedButtons.Add(newEmpty);
					}
				}

				newSetButton.SetUp(m_sets[setIndex], m_wordGrid.gameObject.layer);
			}
		}

		m_wordGrid.Reposition();
	}

	public IEnumerator DisplayKeywords ()
	{
		yield return new WaitForSeconds(ClearButtons());
		
		m_wordCamera.transform.position = m_letterCamStartPos;
		
		yield return new WaitForSeconds(0.1f);
		
		//foreach(int set in m_sets)
		for(int setIndex = 0; setIndex < m_sets.Length; ++setIndex)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + m_sets[setIndex]);
			
			if(dt.Rows.Count > 0)
			{
				ChooseContentSet newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setPrefab, m_wordGrid.transform).GetComponent<ChooseContentSet>() 
					as ChooseContentSet;
				
				newSetButton.name = "Button";
				newSetButton.GetComponent<UIDragCamera>().draggableCamera = m_wordCamera;
				
				m_spawnedButtons.Add(newSetButton.gameObject);
				
				for(int i = 0; i < m_wordGrid.maxPerLine - 1; ++i)
				{
					GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_wordGrid.transform);
					newEmpty.name = "Button";
					m_spawnedButtons.Add(newEmpty);
				}
				
				string[] wordIds = dt.Rows[0]["setwords"].ToString().Replace("[", "").Replace("]", "").Split(',');
				
				List<DataRow> words = new List<DataRow>();
				
				foreach(string id in wordIds)
				{
					DataTable wordTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE id='" + id + "'");
					
					if(wordTable.Rows.Count > 0)
					{
						words.Add(wordTable.Rows[0]);
					}
				}
				
				for(int i = 0; i < words.Count; ++i)
				{
					GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, m_wordGrid.transform);
					newWord.name = "Button";
					newWord.GetComponent<UIDragCamera>().draggableCamera = m_wordCamera;
					newWord.GetComponent<ChooseContentWord>().SetUp(words[i]);
					newSetButton.AddButton((ChooseContentWord)newWord.GetComponent<ChooseContentWord>());
					m_spawnedButtons.Add(newWord);
				}
				
				int remainder = words.Count % m_wordGrid.maxPerLine;
				
				if(remainder > 0)
				{
					int emptySlots = m_wordGrid.maxPerLine - remainder;
					for(int i = 0; i < emptySlots; ++i)
					{
						GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_wordGrid.transform);
						newEmpty.name = "Button";
						m_spawnedButtons.Add(newEmpty);
					}
				}
				
				newSetButton.SetUp(m_sets[setIndex], m_wordGrid.gameObject.layer);
			}
		}
		
		m_wordGrid.Reposition();
	}
}
*/
