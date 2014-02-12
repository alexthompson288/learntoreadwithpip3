using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;
using System;

public class AlphabetRoomGrid : Singleton<AlphabetRoomGrid>
{
	[SerializeField]
	private GameObject m_letterButtonPrefab;
	[SerializeField]
	private int[] m_sectionIds;
	[SerializeField]
	private Blackboard m_blackboard;

	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();
	Dictionary<DataRow, AudioClip> m_graphemeAudio = new Dictionary<DataRow, AudioClip>();
	Dictionary<DataRow, AudioClip> m_longAudio = new Dictionary<DataRow, AudioClip>();
	
	List<GameObject> m_createdLetters = new List<GameObject>();
	
	DataRow m_currentLetterData = null;

	bool m_isUpperCase = false;
	
	// Use this for initialization
	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		List<string> letters = new List<string>();

		for (char i = 'a'; i <= 'z'; ++i)
		{
			string result = i == 'q' ? "qu" : i.ToString();
			letters.Add(result);
		}

		List<DataRow> lettersPool = new List<DataRow>();
		foreach(string letter in letters)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where phoneme='" + letter + "'");
			if ( dt.Rows.Count > 0 )
			{
				lettersPool.Add(dt.Rows[0]);
			}
		}
		
		int index = 0;
		foreach (DataRow row in lettersPool)
		{
			Debug.Log(row["phoneme"].ToString());
			GameObject newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab,
			                                                                              transform);

			AudioClip graphemeAudio = AudioBankManager.Instance.GetAudioClip("lettername_" + row["phoneme"].ToString());
			AudioClip mnemonicAudio = LoaderHelpers.LoadMnemonic(row);

			newLetter.GetComponent<DoubleAudioLabel>().SetUp(row["phoneme"].ToString(), graphemeAudio, null);
			newLetter.GetComponent<DoubleAudioLabel>().OnSingle += OnSingle;

			newLetter.name = index.ToString();

			index++;
			m_createdLetters.Add(newLetter);
		}

		yield return new WaitForSeconds(0.5f);
		
		GetComponent<UIGrid>().Reposition();
		
		yield return new WaitForSeconds(1.0f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("word_BANK_1");
	}

	void OnSingle(DoubleAudioLabel labelBehaviour)
	{
		m_blackboard.Hide();
	}

	void OnDouble(DoubleAudioLabel labelBehaviour)
	{
		string text = labelBehaviour.GetText();

		//DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE phoneme='" + text + "'");

		List<DataRow> phonemes = new List<DataRow>();


		for(int i = 0; i < m_sectionIds.Length; ++i)
		{
			int sectionId = m_sectionIds[i];
			DataTable dt = GameDataBridge.Instance.GetDatabase().
				ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId 
				             + " AND phonemes.phoneme='" + text + "'");

			phonemes.AddRange(dt.Rows);		
		}

		if(phonemes.Count > 0)
		{
			DataRow myPh = phonemes[0];

			string imageFilename =
				string.Format("Images/mnemonics_images_png_250/{0}_{1}",
				              myPh["phoneme"],
				              myPh["mneumonic"].ToString().Replace(" ", "_"));

			Texture2D tex = (Texture2D)Resources.Load(imageFilename);

			m_blackboard.ShowImage(tex, text, text);
		}

		Resources.UnloadUnusedAssets();
	}

	public void ToggleCase()
	{
		if(m_isUpperCase)
		{
			foreach(GameObject letter in m_createdLetters)
			{
				letter.GetComponent<DoubleAudioLabel>().ToLower();
			}
		}
		else
		{
			foreach(GameObject letter in m_createdLetters)
			{
				letter.GetComponent<DoubleAudioLabel>().ToUpper();
			}		
		}

		m_isUpperCase = !m_isUpperCase;
	}
}