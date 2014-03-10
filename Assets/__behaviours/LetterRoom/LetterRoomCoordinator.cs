using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LetterRoomCoordinator : Singleton<LetterRoomCoordinator> 
{
	[SerializeField]
	private GameObject m_letterButtonPrefab;
	[SerializeField]
	private GameObject m_emptyPrefab;
	[SerializeField]
	private UIGrid m_setGrid;
	[SerializeField]
	private UIDraggablePanel m_setDraggablePanel;
	[SerializeField]
	private UIGrid m_graphemeGrid; // ["phoneme"]
	[SerializeField]
	private UIDraggablePanel m_graphemeDraggablePanel;
	[SerializeField]
	private UIGrid m_phonemeGrid; // ["grapheme"]
	[SerializeField]
	private UIDraggablePanel m_phonemeDraggablePanel;
	[SerializeField]
	private ClickEvent m_leftArrow;
	[SerializeField]
	private ClickEvent m_rightArrow;
	[SerializeField]
	private UILabel m_organizationLabel;
	[SerializeField]
	private UITexture m_loadingTexture;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private UIDragPanelContents m_backCollider;

	
	//List<GameObject> m_spawnedObjects = new List<GameObject>();

	enum OrganizationState
	{
		none,
		sets,
		phonemes,
		graphemes,
		end
	}

	OrganizationState m_organizationState = OrganizationState.sets;
	
	IEnumerator Start()
	{
		EnableToggleButtons(false);
		m_leftArrow.OnSingleClick += OnLeftToggle;
		m_rightArrow.OnSingleClick += OnRightToggle;

		m_phonemeDraggablePanel.gameObject.SetActive(false);
		m_graphemeDraggablePanel.gameObject.SetActive(false);

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_setDraggablePanel.GetComponent<UIPanel>().enabled = false;
		m_loadingTexture.enabled = true;

		StartCoroutine(SpawnSetGrid());
	}

	void EnableToggleButtons(bool enable)
	{
		m_leftArrow.EnableCollider(enable);
		m_rightArrow.EnableCollider(enable);
	}

	void OnLeftToggle(ClickEvent clickBehaviour)
	{
		OnOrganizationToggle(-1);
	}

	void OnRightToggle(ClickEvent clickBehaviour)
	{
		OnOrganizationToggle(1);
	}

	void OnOrganizationToggle(int direction)
	{
		m_organizationState += direction;

		if(m_organizationState == OrganizationState.none)
		{
			m_organizationState = OrganizationState.end - 1;
		}
		else if(m_organizationState == OrganizationState.end)
		{
			m_organizationState = OrganizationState.none + 1;
		}

		switch(m_organizationState)
		{
		case OrganizationState.phonemes:
			m_organizationLabel.text = "Phonemes";
			m_setDraggablePanel.gameObject.SetActive(false);
			m_graphemeDraggablePanel.gameObject.SetActive(false);
			m_phonemeDraggablePanel.gameObject.SetActive(true);
			m_backCollider.draggablePanel = m_phonemeDraggablePanel;
			
			if(m_phonemeGrid.transform.childCount == 0)
			{
				EnableToggleButtons(false);
				m_phonemeDraggablePanel.GetComponent<UIPanel>().enabled = false;
				m_loadingTexture.enabled = true;
				StartCoroutine(SpawnPhonemeGrid());
			}
			
			m_organizationState = OrganizationState.phonemes;
			break;
			
		case OrganizationState.graphemes:
			m_organizationLabel.text = "Graphemes";
			m_phonemeDraggablePanel.gameObject.SetActive(false);
			m_setDraggablePanel.gameObject.SetActive(false);
			m_graphemeDraggablePanel.gameObject.SetActive(true);
			m_backCollider.draggablePanel = m_graphemeDraggablePanel;
			
			if(m_graphemeGrid.transform.childCount == 0)
			{
				EnableToggleButtons(false);
				m_graphemeDraggablePanel.GetComponent<UIPanel>().enabled = false;
				m_loadingTexture.enabled = true;
				StartCoroutine(SpawnGraphemeGrid());
			}
			
			m_organizationState = OrganizationState.graphemes;
			break;
			
		case OrganizationState.sets:
			m_organizationLabel.text = "Sets";
			m_phonemeDraggablePanel.gameObject.SetActive(false);
			m_graphemeDraggablePanel.gameObject.SetActive(false);
			m_setDraggablePanel.gameObject.SetActive(true);
			m_backCollider.draggablePanel = m_setDraggablePanel;
			
			m_organizationState = OrganizationState.sets;
			break;
		}
	}

	public void OnLetterSingleClick(LetterButton letterBehaviour)
	{
		AudioClip phonemeAudio = AudioBankManager.Instance.GetAudioClip(letterBehaviour.GetData()["grapheme"].ToString());
		if(phonemeAudio != null)
		{
			m_audioSource.clip = phonemeAudio;
			m_audioSource.Play();
		}
		Resources.UnloadUnusedAssets();
	}

	public void OnLetterDoubleClick(LetterButton letterBehaviour)
	{
		AudioClip mnemonicAudio = LoaderHelpers.LoadMnemonic(letterBehaviour.GetData());
		if(mnemonicAudio != null)
		{
			m_audioSource.clip = mnemonicAudio;
			m_audioSource.Play();
		}
		Resources.UnloadUnusedAssets();
	}

	IEnumerator SpawnSetGrid()
	{
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets ORDER BY number");
		
		List<DataRow> setData = dt.Rows;
		
		if(setData.Count > 0)
		{
			int objName = 0;
			foreach(DataRow set in setData)
			{
				List<DataRow> phonemes = GameDataBridge.Instance.GetSetPhonemes(set);

				int spawnedPhonemes = 0;
				
				foreach(DataRow phoneme in phonemes)
				{
					if(phoneme["completed"] != null && phoneme["completed"].ToString() == "t")
					{
						++spawnedPhonemes;

						GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, m_setGrid.transform);
						
						newPhoneme.name = objName.ToString();
						++objName;
						
						//m_spawnedObjects.Add(newPhoneme);
						
						LetterButton letterBehaviour = newPhoneme.GetComponent<LetterButton>() as LetterButton;
						letterBehaviour.SetUp(phoneme, true, false);
						//letterBehaviour.SetDoubleMethod(letterBehaviour.TweenLarge);
						letterBehaviour.OnSingle += OnLetterSingleClick;
						letterBehaviour.OnDouble += OnLetterDoubleClick;


						UIDragPanelContents dragBehaviour = newPhoneme.AddComponent<UIDragPanelContents>() as UIDragPanelContents;
						dragBehaviour.draggablePanel = m_setDraggablePanel;
					}
				}
				
				if(spawnedPhonemes > 0)
				{
					int remainder = phonemes.Count % m_setGrid.maxPerLine;
					
					if(remainder > 0)
					{
						int numEmpty = m_setGrid.maxPerLine - remainder;
						for(int i = 0; i < numEmpty; ++i)
						{
							GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_setGrid.transform);
							
							newEmpty.name = objName.ToString();
							++objName;
							
							//m_spawnedObjects.Add(newEmpty);
						}
					}
				}

				//yield return null;
			}
		}
		
		yield return new WaitForSeconds(0.1f);
		
		m_setGrid.Reposition();

		yield return new WaitForSeconds(0.1f);

		m_setDraggablePanel.GetComponent<UIPanel>().enabled = true;
		m_loadingTexture.enabled = false;

		EnableToggleButtons(true);
	}


	IEnumerator SpawnGraphemeGrid()
	{
		yield return new WaitForSeconds(0.1f);

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes ORDER BY phoneme");
		
		List<DataRow> phonemes = dt.Rows;
		
		if(phonemes.Count > 0)
		{
			string lastGrapheme = phonemes[0]["phoneme"].ToString();
			int maxPerLine = m_graphemeGrid.maxPerLine;
			int objectName = 0;

			foreach(DataRow phoneme in phonemes)
			{
				if(phoneme["completed"] != null && phoneme["completed"].ToString() == "t")
				{
					if(phoneme["phoneme"].ToString() != lastGrapheme && objectName % maxPerLine != 0)
					{
						int numEmpty = maxPerLine - (objectName % maxPerLine);

						for(int i = 0; i < numEmpty; ++i)
						{
							GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_graphemeGrid.transform);
							newEmpty.name = objectName.ToString();
							++objectName;
						}
					}

					GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, m_graphemeGrid.transform);
						
					newPhoneme.name = objectName.ToString();
					++objectName;
						
					LetterButton letterBehaviour = newPhoneme.GetComponent<LetterButton>() as LetterButton;
					letterBehaviour.SetUp(phoneme, true, false);
					//letterBehaviour.SetDoubleMethod(letterBehaviour.TweenLarge);
					letterBehaviour.OnSingle += OnLetterSingleClick;
					letterBehaviour.OnDouble += OnLetterDoubleClick;

					UIDragPanelContents dragBehaviour = newPhoneme.AddComponent<UIDragPanelContents>() as UIDragPanelContents;
					dragBehaviour.draggablePanel = m_graphemeDraggablePanel;

					lastGrapheme = phoneme["phoneme"].ToString();
				}
			}

			yield return new WaitForSeconds(0.1f);

			m_graphemeGrid.Reposition();

			yield return new WaitForSeconds(0.1f);
			
			m_graphemeDraggablePanel.GetComponent<UIPanel>().enabled = true;
			m_loadingTexture.enabled = false;

			EnableToggleButtons(true);
		}
	}

	IEnumerator SpawnPhonemeGrid()
	{
		yield return new WaitForSeconds(0.1f);

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes ORDER BY grapheme");
		
		List<DataRow> phonemes = dt.Rows;
		
		if(phonemes.Count > 0)
		{
			string lastPhoneme = phonemes[0]["grapheme"].ToString();
			int maxPerLine = m_phonemeGrid.maxPerLine;
			int objectName = 0;

			foreach(DataRow phoneme in phonemes)
			{
				if(phoneme["completed"] != null && phoneme["completed"].ToString() == "t")
				{
					if(phoneme["grapheme"].ToString() != lastPhoneme && objectName % maxPerLine != 0)
					{
						int numEmpty = maxPerLine - (objectName % maxPerLine);
						
						for(int i = 0; i < numEmpty; ++i)
						{
							GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_phonemeGrid.transform);
							newEmpty.name = objectName.ToString();
							++objectName;
						}
					}

					GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, m_phonemeGrid.transform);
					
					newPhoneme.name = objectName.ToString();
					++objectName;
					
					LetterButton letterBehaviour = newPhoneme.GetComponent<LetterButton>() as LetterButton;
					letterBehaviour.SetUp(phoneme, true, false);
					//letterBehaviour.SetDoubleMethod(letterBehaviour.TweenLarge);
					letterBehaviour.OnSingle += OnLetterSingleClick;
					letterBehaviour.OnDouble += OnLetterDoubleClick;

					UIDragPanelContents dragBehaviour = newPhoneme.AddComponent<UIDragPanelContents>() as UIDragPanelContents;
					dragBehaviour.draggablePanel = m_phonemeDraggablePanel;

					lastPhoneme = phoneme["grapheme"].ToString();
				}
			}
			
			yield return new WaitForSeconds(0.1f);
			
			m_phonemeGrid.Reposition();

			yield return new WaitForSeconds(0.1f);
			
			m_phonemeDraggablePanel.GetComponent<UIPanel>().enabled = true;
			m_loadingTexture.enabled = false;
			
			EnableToggleButtons(true);
		}
	}

}
