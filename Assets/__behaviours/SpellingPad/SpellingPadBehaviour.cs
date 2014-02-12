using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class SpellingPadBehaviour : Singleton<SpellingPadBehaviour> 
{
	[SerializeField]
	private GameObject m_printedWordPrefab;
	[SerializeField]
	private GameObject m_emptyPrefab;
	[SerializeField]
	private GameObject m_phonemeButton = null;
	[SerializeField]
	private Transform m_textPosition = null;
	[SerializeField]
	private PlayWordSpellingButton m_sayWholeWordButton = null;
	[SerializeField]
	private GameObject m_trickyStars = null;

	List<GameObject> m_createdPhonemeButtons = new List<GameObject>();

	string m_editedWord;

	public class PhonemeBuildInfo
	{
		public string m_displayString;
		public int m_fullPhonemeId;
		public int m_positionIndex;
		public PhonemeBuildInfo m_linkedPhoneme;
		public string m_audioFilename;
		public string m_fullPhoneme;
	}

	int SortPhonemes(PhonemeBuildInfo a, PhonemeBuildInfo b)
	{
		if (a.m_positionIndex > b.m_positionIndex)
		{
			return 1;
		}
		else if (a.m_positionIndex < b.m_positionIndex)
		{
			return -1;
		}
		else
		{
			return 0;
		}
	}

	public void DisplayNewWord(string word)
	{
		//WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_PAD_APPEAR");
		foreach (GameObject go in m_createdPhonemeButtons)
		{
			Destroy(go);
		}
		m_createdPhonemeButtons.Clear();
		
		m_editedWord = word.Replace("!", "").Replace(".", "").Replace(",", "").Replace(";", "").ToLower();
		
		SqliteDatabase database = GameDataBridge.Instance.GetDatabase();
		DataTable dt = database.ExecuteQuery("select * from words where word='" + m_editedWord + "'");
		
		m_sayWholeWordButton.SetWordAudio(m_editedWord);
		
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt[0];
			
			string[] phonemes = row["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			
			List<PhonemeBuildInfo> pbiList = new List<PhonemeBuildInfo>();
			
			bool starsActive = (((row["tricky"] != null && row["tricky"].ToString() == "t")
			                    || (row["nondecodable"] != null && row["nondecodable"].ToString() == "t"))
			                    && (row["nonsense"] == null || row["nonsense"].ToString() == "f"));

			//if (((row["tricky"] != null && row["tricky"].ToString() == "t")
			    // || (row["nondecodable"] != null && row["nondecodable"].ToString() == "t"))
			    //&& (row["nonsense"] == null || row["nonsense"].ToString() == "f"))
			//if(starsActive)
			//{
				//PhonemeBuildInfo pbi = new PhonemeBuildInfo();
				//pbi.m_displayString = row["word"].ToString();
				//pbi.m_positionIndex = 0;
				//pbi.m_fullPhonemeId = -1;
				//pbiList.Add(pbi);
				//m_trickyStars.SetActive(true);
			//}
			//else
			//{
				//m_trickyStars.SetActive(false);
				m_trickyStars.SetActive(starsActive);
				int index = 0;
				foreach (string phoneme in phonemes) // phoneme is an: int id.ToString
				{
					DataTable phT = database.ExecuteQuery("select * from phonemes where id='" + phoneme + "'");
					if (phT.Rows.Count > 0)
					{
						DataRow myPh = phT[0];
						string phonemeData = myPh["phoneme"].ToString();
						PhonemeBuildInfo pbi = new PhonemeBuildInfo();
						
						string audioFilename =
							string.Format("{0}",
							              myPh["grapheme"]);

						if(starsActive)
						{
							audioFilename = "lettername_" + audioFilename;
						}

						
						pbi.m_audioFilename = audioFilename;
						pbi.m_fullPhoneme = myPh["phoneme"].ToString();

						if (phonemeData.Contains("-"))
						{
							//Debug.Log(myPh["phoneme"].ToString() + " contains \"-\"");
							pbi.m_displayString = phonemeData[0].ToString();
							PhonemeBuildInfo pbi2 = new PhonemeBuildInfo();
							pbi2.m_displayString = phonemeData[2].ToString();
							pbi2.m_positionIndex = index + 2;
							pbi.m_linkedPhoneme = pbi2;
							pbi2.m_linkedPhoneme = pbi;
							pbi.m_fullPhonemeId = Convert.ToInt32(phoneme);
							pbi2.m_fullPhonemeId = Convert.ToInt32(phoneme);
							pbi2.m_audioFilename = audioFilename;
							pbi2.m_fullPhoneme = myPh["phoneme"].ToString();
							pbiList.Add(pbi2);
						}
						else
						{
							pbi.m_fullPhonemeId = Convert.ToInt32(phoneme);
							pbi.m_displayString = phonemeData;
						}
						pbi.m_positionIndex = index;
						pbiList.Add(pbi);
					}
					index++;
				}
			//}
			
			pbiList.Sort(SortPhonemes);
			float width = 0;
			
			Dictionary<PhonemeBuildInfo, GameObject> createdInfos = new Dictionary<PhonemeBuildInfo, GameObject>();
			foreach (PhonemeBuildInfo pbi in pbiList)
			{
				GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_phonemeButton, m_textPosition);
				
				newPhoneme.GetComponent<SpellingPadPhoneme>().SetUpPhoneme(pbi);
				
				width += newPhoneme.GetComponent<SpellingPadPhoneme>().GetWidth() / 2.0f;
				newPhoneme.transform.localPosition = new Vector3(width, 0, 0);
				width += newPhoneme.GetComponent<SpellingPadPhoneme>().GetWidth() / 2.0f;

				/*
				if(starsActive)
				{
					newPhoneme.GetComponent<SpellingPadPhoneme>().EnableButtons(false);
				}
				*/
				
				m_createdPhonemeButtons.Add(newPhoneme);
				createdInfos[pbi] = newPhoneme;
			}
			
			foreach (PhonemeBuildInfo pbi in pbiList)
			{
				if (pbi.m_linkedPhoneme != null)
				{
					createdInfos[pbi.m_linkedPhoneme].GetComponent<SpellingPadPhoneme>().Link(createdInfos[pbi]);
				}
			}
			
			
			//width /= pbiList.Count;
			//width *= (pbiList.Count - 1);
			
			if (width > 512)
			{
				m_textPosition.transform.localScale = new Vector3(0.8f, 1, 1);
				m_textPosition.transform.localPosition = new Vector3((-width / 2.0f) * 0.8f, m_textPosition.transform.localPosition.y, m_textPosition.localPosition.z);
			}
			else
			{
				m_textPosition.transform.localPosition = new Vector3(-width / 2.0f, m_textPosition.transform.localPosition.y, m_textPosition.localPosition.z);
				m_textPosition.transform.localScale = Vector3.one;
			}
		}
	}

	public void HighlightWholeWord()
	{
		foreach (GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			SpellingPadPhoneme spellingPadPhoneme = createdPhoneme.GetComponent<SpellingPadPhoneme>();
			if (spellingPadPhoneme != null)
			{
				spellingPadPhoneme.ActivateFinal();
			}
		}
	}

	public void SayWholeWord()
	{
		m_sayWholeWordButton.Speak();
	}
	
	public void SayAll()
	{
		StopAllCoroutines();
		SayWholeWord();
		StartCoroutine(Segment(m_sayWholeWordButton.GetClipLength()));
	}
	
	public IEnumerator Segment(float delay)
	{
		yield return new WaitForSeconds(delay);
		foreach (GameObject phonemeButton in m_createdPhonemeButtons)
		{
			phonemeButton.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			yield return new WaitForSeconds(0.4f);
		}
	}

	public void SayShowAll(bool temporary)
	{
		SayAll();
		foreach(GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			createdPhoneme.GetComponent<SpellingPadPhoneme>().MakeLabelTransparent();
		}

		if(temporary)
		{
			StartCoroutine(HideAll(1.5f));
		}
	}

	IEnumerator HideAll(float delay)
	{
		yield return new WaitForSeconds(delay);
		foreach(GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			SpellingPadPhoneme phonemeBehaviour = createdPhoneme.GetComponent<SpellingPadPhoneme>() as SpellingPadPhoneme;

			if(!Mathf.Approximately(phonemeBehaviour.GetLabelTransparency(), 1))
			{
				phonemeBehaviour.MakeLabelInvisible();
			}
		}
	}

	public void SayShowSequential()
	{
		foreach(GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			SpellingPadPhoneme phonemeBehaviour = createdPhoneme.GetComponent<SpellingPadPhoneme>() as SpellingPadPhoneme;
			if(Mathf.Approximately(phonemeBehaviour.GetLabelTransparency(), 0))
			{
				phonemeBehaviour.MakeLabelTransparent();
				phonemeBehaviour.PlayAudio();
				break;
			}
		}
	}

	public void MakeAllVisibleExceptTarget(string targetPhoneme)
	{
		foreach(GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			SpellingPadPhoneme spellingPadPhoneme = createdPhoneme.GetComponent<SpellingPadPhoneme>();

			if(targetPhoneme != spellingPadPhoneme.GetPhoneme())
			{
				spellingPadPhoneme.MakeLabelTransparent();
			}
		}
	}

	public void DisableAllCollidersExceptTarget(string targetPhoneme)
	{
		foreach(GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			SpellingPadPhoneme spellingPadPhoneme = createdPhoneme.GetComponent<SpellingPadPhoneme>();
			
			if(targetPhoneme != spellingPadPhoneme.GetPhoneme())
			{
				spellingPadPhoneme.collider.enabled = false;
			}
		}
	}

	public SpellingPadPhoneme CheckLetters(string phoneme, Collider draggable)
	{
		foreach(GameObject createdPhoneme in m_createdPhonemeButtons)
		{
			SpellingPadPhoneme spellingPadPhoneme = createdPhoneme.GetComponent<SpellingPadPhoneme>();

			if(draggable == spellingPadPhoneme.GetOther() && phoneme == spellingPadPhoneme.GetPhoneme())
			{
				return spellingPadPhoneme;
			}
			else
			{
				Debug.Log("SpellingPadPhoneme.CheckLetters()");
				Debug.Log("1-Fail: " + spellingPadPhoneme.GetPhoneme());
				Debug.Log("2-isOther: " + (draggable == spellingPadPhoneme.GetOther()));
				Debug.Log("3-isPhoneme: " + (phoneme == spellingPadPhoneme.GetPhoneme()));
				Debug.Log("padPhoneme: " + spellingPadPhoneme.GetPhoneme());
				Debug.Log("draggablePhoneme: " + phoneme);
			}
		}

		return null;
	}

	public GameObject DetachWord()
	{
		GameObject detached = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_textPosition);
		foreach(GameObject button in m_createdPhonemeButtons)
		{
			GameObject label = button.GetComponent<SpellingPadPhoneme>().GetLabelGo();
			label.transform.parent = detached.transform;
		}

		return detached;
	}

	public GameObject PrintWord()
	{
		GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_printedWordPrefab, m_textPosition);

		DraggableLabel draggableLabel = newWord.GetComponent<DraggableLabel>() as DraggableLabel;

		draggableLabel.SetUp(m_editedWord, null, true);
		draggableLabel.SetCanDrag(false);

		return newWord;
	}

	public float TweenAllBackgroundsAlpha(float newAlpha)
	{
		if(m_createdPhonemeButtons.Count > 0)
		{
			float tweenInvisibleDuration = 0;
			foreach(GameObject button in m_createdPhonemeButtons)
			{
				tweenInvisibleDuration = button.GetComponent<SpellingPadPhoneme>().TweenBackgroundAlpha(newAlpha);
			}
			return tweenInvisibleDuration;
		}
		else
		{
			return 0;
		}
	}

	public void MakeAllLabelsVisible()
	{
		foreach(GameObject button in m_createdPhonemeButtons)
		{
			button.GetComponent<SpellingPadPhoneme>().MakeLabelVisible();
		}
	}
}
 