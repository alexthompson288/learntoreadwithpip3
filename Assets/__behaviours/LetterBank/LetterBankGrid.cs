using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;
using System;

public class LetterBankGrid : Singleton<LetterBankGrid>
{
    [SerializeField]
    private GameObject m_letterButtonPrefab;
	[SerializeField]
	private Blackboard[] m_questionBlackboards;
	[SerializeField]
	private Blackboard m_letterBlackboard;
	[SerializeField]
	private AudioSource m_audioSource;
	
    List<DataRow> m_lettersPool = new List<DataRow>();
	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
	Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
    Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();
	
    List<GameObject> m_createdLetters = new List<GameObject>();

    DataRow m_currentLetterData = null;
	
	int CompareDataRow(DataRow a, DataRow b)
	{
		return String.Compare(a["phoneme"].ToString(), b["phoneme"].ToString());
	}
	
    // Use this for initialization
    IEnumerator Start()
    {
		for(int i = 0; i < m_questionBlackboards.Length; ++i)
		{
			m_questionBlackboards[i].OnBoardClick += OnQuestionBlackboardClick;
		}
		
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;

		Dictionary<string, DataRow> noDuplicatesDictionary = new Dictionary<string, DataRow>();
		
		for(int i = 0; i < sectionIds.Length; ++i)
		{
			int sectionId = sectionIds[i];
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);

			foreach(DataRow row in dt.Rows)
			{
				noDuplicatesDictionary[row["phoneme"].ToString()] = row;
			}
		}
		
		foreach(KeyValuePair<string, DataRow> kvp in noDuplicatesDictionary)
		{
			m_lettersPool.Add(kvp.Value);
		}
		
		m_lettersPool.Sort(CompareDataRow);
		
		foreach(DataRow myPh in m_lettersPool)
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
		
        int index = 0;
        foreach (DataRow row in m_lettersPool)
        {
            GameObject newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab,
                transform);
            newLetter.GetComponentInChildren<LetterBankLetter>().SetUp(row);
            newLetter.name = string.Format("letter_{0:000}_BOX", index);
            index++;
            m_createdLetters.Add(newLetter);
        }

        GetComponent<UIGrid>().Reposition();

        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("word_BANK_1");
        yield return new WaitForSeconds(1.5f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("word_BANK_2");
    }
	
	public IEnumerator ShowOneLetter(DataRow currentLetterData)
	{
		m_currentLetterData = currentLetterData;
		
		HideQuestionBlackboards();
		m_letterBlackboard.ShowImage(m_phonemeImages[m_currentLetterData], m_currentLetterData["phoneme"].ToString(), m_currentLetterData["phoneme"].ToString());

		PlayerLetterAudio(m_currentLetterData);
		
		yield break;
	}
	
	public IEnumerator ShowThreeLetters(DataRow currentLetterData)
    {
		m_currentLetterData = currentLetterData;
		
		//WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_PICTURE_INSTRUCTION");
		
		if(m_letterBlackboard.GetIsShowing())
		{
			m_letterBlackboard.Hide();
			yield return new WaitForSeconds(1f);
		}
		
		DisableCollectionRoomCollider.Instance.DisableCollider(); // TODO: Deprecate the DisableCollectionRoomCollider class
		
		
		string currentLetter = m_currentLetterData["phoneme"].ToString();
		
		Dictionary<string, DataRow> dataDictionary = new Dictionary<string, DataRow>();
		dataDictionary.Add(currentLetter, m_currentLetterData);
		
		while(dataDictionary.Count < m_questionBlackboards.Length)
		{
			DataRow letterData = m_lettersPool[UnityEngine.Random.Range(0, m_lettersPool.Count)];
			dataDictionary[letterData["phoneme"].ToString()] = letterData;
		}
		
		List<DataRow> dataList = new List<DataRow>();
		dataList.AddRange(dataDictionary.Values);
		
		List<DataRow> reorderedList = new List<DataRow>();
		while(dataList.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, dataList.Count);
			DataRow letterData = dataList[index];
			
			reorderedList.Add(letterData);
			dataList.RemoveAt(index);
		}
		
		for(int index = 0; index < m_questionBlackboards.Length; ++index)
		{
			DataRow letterData = reorderedList[index];
			m_questionBlackboards[index].ShowImage(m_phonemeImages[letterData], letterData["mneumonic"].ToString(), letterData["phoneme"].ToString(), null);
		}
		
		PlayerLetterAudio(m_currentLetterData);
		
		Resources.UnloadUnusedAssets();
    }
	
	public void PlayerLetterAudio(DataRow letterData)
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
	
	public void OnQuestionBlackboardClick(Blackboard clickedBlackboard)
	{
		string currentLetter = m_currentLetterData["phoneme"].ToString();
	    if (clickedBlackboard.GetLetter() == currentLetter)
	    {
	        SessionInformation.Instance.UnlockLetter(currentLetter);
			StartCoroutine(GoldCoinBar.Instance.SpendCoin());
			StartCoroutine(UnlockItem.Instance.ShowItems());
				
	        HideQuestionBlackboards();
				
	        Refresh();
	        //StartCoroutine(YouUnlockedItAudio());
			WingroveAudio.WingroveRoot.Instance.PostEvent("YOU_UNLOCKED_IT");
	    }
		else
		{
			PlayerLetterAudio(m_currentLetterData);
			clickedBlackboard.ShakeFade();
		}
    }

    IEnumerator YouUnlockedItAudio()
    {
        yield return new WaitForSeconds(3f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("YOU_UNLOCKED_IT");
    }

    void Refresh()  
    {
        foreach(GameObject addedObject in m_createdLetters)
        {
            addedObject.GetComponentInChildren<LetterBankLetter>().Refresh(false);
        }
    }
	
	void HideQuestionBlackboards()
	{
		DisableCollectionRoomCollider.Instance.EnableCollider();
		
		for(int index = 0; index < m_questionBlackboards.Length; ++index)
		{
			m_questionBlackboards[index].Hide();
		}
		
		
	}

}