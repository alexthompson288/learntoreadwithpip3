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
			letters.Add(i.ToString());
		}

		int index = 0;
		foreach(string letter in letters)
		{
			GameObject newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab,
			                                                                              transform);
			
			AudioClip graphemeAudio = AudioBankManager.Instance.GetAudioClip("lettername_" + letter);
			
			newLetter.GetComponent<DoubleAudioLabel>().SetUp(letter, graphemeAudio, null);
			newLetter.GetComponent<DoubleAudioLabel>().OnSingle += OnSingle;

			index.ToString();

			m_createdLetters.Add(newLetter);

			++index;
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