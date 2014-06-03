using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class SpellingPadBehaviour : Singleton<SpellingPadBehaviour> 
{
	[SerializeField]
	private GameObject m_phonemePrefab;
    [SerializeField]
    private Transform m_phonemeParent;
    [SerializeField]
    private GameObject m_letterPrefab;
	[SerializeField]
	private Transform m_letterParent;
	[SerializeField]
	private PlayWordSpellingButton m_sayWholeWordButton;
	[SerializeField]
	private GameObject m_trickyStars;

    // TODO: Possibly deprecate
    [SerializeField]
    private GameObject m_printedWordPrefab;
    [SerializeField]
    private GameObject m_emptyPrefab;


    List<PadLetter> m_spawnedLetters = new List<PadLetter>();
    List<PadPhoneme> m_spawnedPhonemes = new List<PadPhoneme>();


	string m_editedWord;

	public class PhonemeBuildInfo
	{
        public DataRow m_phonemeData;
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
        CollectionHelpers.DestroyObjects(m_spawnedLetters);
        CollectionHelpers.DestroyObjects(m_spawnedPhonemes);
		
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

				
            m_trickyStars.SetActive(starsActive);
			
            int index = 0;
			
            foreach (string phoneme in phonemes) // phoneme is an: int id.ToString
            {
                DataTable phT = database.ExecuteQuery("select * from phonemes where id='" + phoneme + "'");			
                if (phT.Rows.Count > 0)			
                {	
                    PhonemeBuildInfo pbi = new PhonemeBuildInfo();

                    DataRow myPh = phT[0];
                    pbi.m_phonemeData = myPh;

                    string phonemeString = myPh["phoneme"].ToString();    
                    pbi.m_fullPhoneme = phonemeString;
							
                    string audioFilename = string.Format("{0}", myPh["grapheme"]);
				    if(starsActive)
					{
						audioFilename = "lettername_" + audioFilename;
					}	
                    pbi.m_audioFilename = audioFilename;


                    if (phonemeString.Contains("-"))
					{
						//Debug.Log(myPh["phoneme"].ToString() + " contains \"-\"");
                        pbi.m_displayString = phonemeString[0].ToString();
						PhonemeBuildInfo pbi2 = new PhonemeBuildInfo();
                        pbi2.m_phonemeData = myPh;
                        pbi2.m_displayString = phonemeString[2].ToString();
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
                        pbi.m_displayString = phonemeString;
					}
					pbi.m_positionIndex = index;
					pbiList.Add(pbi);
				}
				index++;
			}
			
			pbiList.Sort(SortPhonemes);
			float totalWidth = 0;
			
            Dictionary<PhonemeBuildInfo, PadPhoneme> createdInfos = new Dictionary<PhonemeBuildInfo, PadPhoneme>();
			foreach (PhonemeBuildInfo pbi in pbiList)
			{
				GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_phonemePrefab, m_phonemeParent);
				
                PadPhoneme phonemeBehaviour = newPhoneme.GetComponent<PadPhoneme>() as PadPhoneme;
                phonemeBehaviour.SetUp(pbi);
				
                totalWidth += phonemeBehaviour.GetWidth() / 2.0f;
                newPhoneme.transform.localPosition = new Vector3(totalWidth, 0, 0);
                totalWidth += phonemeBehaviour.GetWidth() / 2.0f;
				
                m_spawnedPhonemes.Add(phonemeBehaviour);
                createdInfos[pbi] = phonemeBehaviour;
			}
			
            int letterIndex = 0;
            float letterPosX = 0;
			foreach (PhonemeBuildInfo pbi in pbiList)
			{
				if (pbi.m_linkedPhoneme != null)
				{
					createdInfos[pbi.m_linkedPhoneme].Link(createdInfos[pbi]);
				}

                string displayString = pbi.m_displayString;
                foreach(char c in displayString)
                {
                    GameObject newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterPrefab, m_letterParent);

                    PadLetter letterBehaviour = newLetter.GetComponent<PadLetter>() as PadLetter;
                    letterBehaviour.SetUp(c.ToString(), letterIndex);
                    m_spawnedLetters.Add(letterBehaviour);
                    createdInfos[pbi].AddPadLetter(letterBehaviour);

                    ++letterIndex;

                    float halfLetterWidth = createdInfos[pbi].GetWidth() / (float)displayString.Length / 2.0f;

                    letterPosX += halfLetterWidth;
                    newLetter.transform.localPosition = new Vector3(letterPosX, 0);
                    letterPosX += halfLetterWidth;
                }
			}
			
            float maxWidth = 600;

            Vector3 parentScale = totalWidth < maxWidth ? Vector3.one : Vector3.one * maxWidth / totalWidth;
            m_letterParent.transform.localScale = parentScale;
            m_phonemeParent.transform.localScale = parentScale;

            float parentWidth = -totalWidth / 2.0f * parentScale.x;
            m_letterParent.transform.localPosition = new Vector3(parentWidth, m_letterParent.transform.localPosition.y, m_letterParent.localPosition.z);
            m_phonemeParent.transform.localPosition = new Vector3(parentWidth, m_phonemeParent.transform.localPosition.y, m_phonemeParent.localPosition.z);
		}
	}

	public void HighlightWholeWord()
	{
		foreach (PadPhoneme phoneme in m_spawnedPhonemes)
		{
            phoneme.ActivateFinal();
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
		foreach (PadLetter letter in m_spawnedLetters)
		{
            letter.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			yield return new WaitForSeconds(0.4f);
		}
	}

	public void SayShowAll(bool temporary)
	{
		SayAll();
        foreach(PadLetter letter in m_spawnedLetters)
		{
            if(letter.state != PadLetter.State.Answered)
			{
                letter.ChangeState(PadLetter.State.Hint);
			}
		}

		if(temporary)
		{
			StartCoroutine(HideAll(1.5f));
		}
	}

	IEnumerator HideAll(float delay)
	{
		yield return new WaitForSeconds(delay);
        foreach(PadLetter letter in m_spawnedLetters)
		{
            if(letter.state != PadLetter.State.Answered)
			{
                letter.ChangeState(PadLetter.State.Unanswered);
			}
		}
	}

	public void SayShowSequential()
	{
		Debug.Log("SpellingPadBehaviour.SayShowSequential()");
        foreach(PadLetter letter in m_spawnedLetters)
		{
            if(letter.state == PadLetter.State.Unanswered)
			{
                letter.ChangeState(PadLetter.State.Hint);
				//phonemeBehaviour.PlayAudio();
				break;
			}
		}
	}

	public void ChangeStateAll(PadLetter.State newState, string exceptionLetter = "", bool singleException = true)
	{
        // Don't change the state of the exceptionPhoneme
        // if singleException is true, after the first exceptionPhoneme ALL subsequent phonemes will change state

        foreach(PadLetter padLetter in m_spawnedLetters)
		{
            if(exceptionLetter != padLetter.GetLetter())
			{
                padLetter.ChangeState(newState);
			}
            else if(singleException)
            {
                exceptionLetter = null;
            }
		}
	}

    public void DisableTriggersAll(string exceptionPhoneme, bool singleException)
	{
        // Don't disable the collider of the exceptionPhoneme
        // if singleException is true, after the first exceptionPhoneme ALL subsequent phonemes will disable their collider
        foreach(PadLetter padLetter in m_spawnedLetters)
		{
            if(exceptionPhoneme != padLetter.GetLetter())
            {
                padLetter.EnableTrigger(false);
            }
            else if(singleException)
            {
                exceptionPhoneme = null;
            }
		}    
	}

	public PadLetter CheckLetters(string letter, Collider draggable)
	{
        //Debug.Log("Checking for: " + phoneme);

		foreach(PadLetter padLetter in m_spawnedLetters)
		{
            if(draggable == padLetter.GetOther())
            {
                //Debug.Log("Collider is inside " + padLetter.GetLetter());
            }
            else if(padLetter.GetOther() != null)
            {
                //Debug.Log("PadPhoneme has different: " + padLetter.GetLetter());
            }

            if(draggable == padLetter.GetOther() && letter == padLetter.GetLetter())
			{
                return padLetter;
			}
		}

		return null;
	}

    public PadLetter GetFirstUnansweredPhoneme()
    {
        PadLetter unansweredPadLetter = null;

        foreach(PadLetter padLetter in m_spawnedLetters)
        {
            if(padLetter.state != PadLetter.State.Answered)
            {
                unansweredPadLetter = padLetter;
                break;
            }
        }

        return unansweredPadLetter;
    }

	public GameObject DetachWord()
	{
		GameObject detached = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_letterParent);
        /*
		foreach(GameObject button in m_spawnedLetters)
		{
			GameObject label = button.GetComponent<SpellingPadPhoneme>().GetLabelGo();
			label.transform.parent = detached.transform;
		}
        */
		return detached;
	}

	public GameObject PrintWord()
	{
		GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_printedWordPrefab, m_letterParent);

		DraggableLabel draggableLabel = newWord.GetComponent<DraggableLabel>() as DraggableLabel;

		draggableLabel.SetUp(m_editedWord, null, true);
		draggableLabel.SetCanDrag(false);

		return newWord;
	}
}
 