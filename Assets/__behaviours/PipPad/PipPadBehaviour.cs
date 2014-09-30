using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class PipPadBehaviour : Singleton<PipPadBehaviour>
{
	public delegate void HideEventHandler();
    public event HideEventHandler Hiding;

	public delegate void BlackboardClickEventHandler(ImageBlackboard clickedBoard);
    public event BlackboardClickEventHandler BlackboardClicked;

	[SerializeField]
	private Transform m_mainHierarchyTransform;
	[SerializeField]
	private Transform m_padPopUpTargetOn = null;
	[SerializeField]
	private GameObject m_phonemeButton = null;
	[SerializeField]
	private Transform m_textPosition = null;
	[SerializeField]
	private PlayWordButton m_sayWholeWordButton = null;
	[SerializeField]
	private GameObject m_trickyStars = null;
	[SerializeField]
	private ImageBlackboard m_imageBlackboard = null;
	[SerializeField]
	private ImageBlackboard[] m_additionalImageBlackboards;
	[SerializeField]
	private bool m_dismissable = true;
	[SerializeField]
	private GameObject m_dismissButton = null;
	[SerializeField]
	private TweenOnOffBehaviour m_pipWordNotFound;
	[SerializeField]
	private float m_postPhonemeSpeakDelay = 0.4f;
    [SerializeField]
    private Color m_highFrequencyColor;
    [SerializeField]
    private Color m_trickyColor;
    [SerializeField]
    private GameObject m_wordNotFoundParent;
    [SerializeField]
    private GameObject m_downButtonParent;
	
	Vector3 m_offPosition;
	
	List<GameObject> m_createdPhonemeButtons = new List<GameObject>();
	
	bool m_showing;
	bool m_noPhonemeButtons;
	bool m_multipleBlackboardMode = false;
	
	string m_currentLanguage;

	string m_currentEnglishWord;
	
	// Use this for initialization
	void Awake()
	{
        m_wordNotFoundParent.SetActive(true);
		m_offPosition = m_mainHierarchyTransform.position;
	}

    public void HideDownButton()
    {
        if (m_downButtonParent != null)
        {
            m_downButtonParent.SetActive(false);
        }
    }

	IEnumerator Start()
	{
		HideAllBlackboards();
		
		m_mainHierarchyTransform.gameObject.SetActive(false);
		if (!m_dismissable)
		{
			m_dismissButton.SetActive(false);
		}
		yield break;
	}
	
	public void SetLanguage(string language)
	{
		////////D.Log("SetLanguage(" + language + ")");
		m_currentLanguage = language;
		DisplayWord(m_currentEnglishWord);
	}
	
	public class PhonemeBuildInfo
	{
		public string m_displayString;
		public int m_fullPhonemeId;
		public int m_positionIndex;
		public PhonemeBuildInfo m_linkedPhoneme;
		public string m_audioFilename;
		public string m_imageFilename;
		public string m_mnemonic;
		public string m_fullPhoneme;
		public bool m_isSecondInSplitDigraph;
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
	
	public bool IsShowing()
	{
		return m_showing;
	}

	public void Show(string word, bool postEvent = false)
	{
#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("Word", word);
		//FlurryBinding.logEventWithParameters("PipPad", ep, false);
#endif

		// EARLY EXIT
		if (m_showing)
		{
			return;
		}
		m_showing = true;
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

		m_currentLanguage = "word";

		string editedWord = StringHelpers.Edit(word);

		m_currentEnglishWord = editedWord;

		DisplayWord(editedWord, postEvent);

		m_mainHierarchyTransform.gameObject.SetActive(true);
		iTween.MoveTo(m_mainHierarchyTransform.gameObject, m_padPopUpTargetOn.position, 1.0f);
	}

	public void DisplayWord(string word, bool postEvent = false)
	{
        m_isSayingAll = false;

		foreach (GameObject go in m_createdPhonemeButtons)
		{
			Destroy(go);
		}
		m_createdPhonemeButtons.Clear();
		
		string editedWord = StringHelpers.Edit(word);

		//////////D.Log("editedWord: " + editedWord);

		try
		{
			SqliteDatabase database = GameDataBridge.Instance.GetDatabase();
			
			DataTable dt = database.ExecuteQuery("select * from words where word='" + editedWord + "'");
			
			if(dt.Rows.Count == 0)
			{
				m_pipWordNotFound.On();
			}

			////////D.Log("editedWord: " + editedWord);
			m_sayWholeWordButton.SetWordAudio(editedWord);
			
			if (dt.Rows.Count > 0)
			{
				DataRow row = dt[0];

                ////////D.Log("PipPadBehavior orderedphonemes: " + row["ordered_phonemes"]);


				if(postEvent)
				{
					//UserStats.Activity.AddPipPadCall(Convert.ToInt32(row["id"]));
				}
				
                string[] phonemes = DataHelpers.GetOrderedPhonemeIdStrings(row);

				bool hasPhonemes = false;

				foreach(string phoneme in phonemes)
				{
					if(!String.IsNullOrEmpty(phoneme))
					{
						hasPhonemes = true;
						break;
					}
				}

				if(hasPhonemes || (row["tricky"] != null && row["tricky"].ToString() == "t"))
				{
					List<PhonemeBuildInfo> pbiList = new List<PhonemeBuildInfo>();
					
                    bool areStarsActive = (((row["tricky"] != null && row["tricky"].ToString() == "t") || (row["highfrequencyword"] != null && row["highfrequencyword"].ToString() == "t"))
					                       && (row["nonsense"] == null || row["nonsense"].ToString() == "f"));
					
					if (areStarsActive || m_currentLanguage != "word")
					{
						m_noPhonemeButtons = true;
						PhonemeBuildInfo pbi = new PhonemeBuildInfo();

						pbi.m_displayString = row[m_currentLanguage].ToString(); // If the field is null then error thrown, catch executes, shows "Word Not Found" sign

						////////D.Log(m_currentLanguage + ": " + row[m_currentLanguage].ToString());
						pbi.m_positionIndex = 0;
						pbi.m_fullPhonemeId = -1;
						pbiList.Add(pbi);

                        m_trickyStars.SetActive(areStarsActive);

                        Color starColor = (row["tricky"] != null && row["tricky"].ToString() == "t") ? m_trickyColor : m_highFrequencyColor;

                        UISprite[] starSprites = m_trickyStars.GetComponentsInChildren<UISprite>(true) as UISprite[];

                        ////////D.Log("Found " + starSprites.Length + " star sprites");

                        foreach(UISprite star in starSprites)
                        {
                            star.color = starColor;
                        }

						m_trickyStars.SetActive(areStarsActive);
					}
					else
					{
						m_noPhonemeButtons = false;
						m_trickyStars.SetActive(false);
						int index = 0;
						int splitPhoneme = 0;
						foreach (string phoneme in phonemes)
						{
							if(splitPhoneme == 2)
							{
								++index;
								splitPhoneme = 0;
							}
							
							if(splitPhoneme == 1)
							{
								++splitPhoneme;
							}
							
							DataTable phT = database.ExecuteQuery("select * from phonemes where id='" + phoneme + "'");
							if (phT.Rows.Count > 0)
							{
								DataRow myPh = phT[0];
								string phonemeData = myPh["phoneme"].ToString();
								PhonemeBuildInfo pbi = new PhonemeBuildInfo();
								
								string audioFilename =
									string.Format("{0}",
									              myPh["grapheme"]);
								
								string imageFilename =
									string.Format("Images/mnemonics_images_png_250/{0}_{1}",
									              myPh["phoneme"],
									              myPh["mneumonic"].ToString().Replace(" ", "_"));
								
								pbi.m_audioFilename = audioFilename;
								pbi.m_imageFilename = imageFilename;
								pbi.m_mnemonic = myPh["mneumonic"].ToString();
								pbi.m_fullPhoneme = myPh["phoneme"].ToString();
								
								pbi.m_isSecondInSplitDigraph = false;
								
								if (phonemeData.Contains("-"))
								{
									splitPhoneme = 1;
									////////D.Log("INFO - phonemeData: " + phonemeData);
									pbi.m_displayString = phonemeData[0].ToString();
									PhonemeBuildInfo pbi2 = new PhonemeBuildInfo();
									pbi2.m_displayString = phonemeData[2].ToString();
									pbi2.m_positionIndex = index + 2;
									////////D.Log("INFO - index + 2: " + (index + 2));
									pbi.m_linkedPhoneme = pbi2;
									pbi2.m_linkedPhoneme = pbi;
									pbi.m_fullPhonemeId = Convert.ToInt32(phoneme);
									pbi2.m_fullPhonemeId = Convert.ToInt32(phoneme);
									pbi2.m_audioFilename = audioFilename;
									pbi2.m_imageFilename = imageFilename;
									pbi2.m_mnemonic = myPh["mneumonic"].ToString();
									pbi2.m_fullPhoneme = myPh["phoneme"].ToString();
									pbi2.m_isSecondInSplitDigraph = true;
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
					}
					
					pbiList.Sort(SortPhonemes);
					float width = 0;
					
					Dictionary<PhonemeBuildInfo, GameObject> createdInfos = new Dictionary<PhonemeBuildInfo, GameObject>();
					foreach (PhonemeBuildInfo pbi in pbiList)
					{
						GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_phonemeButton, m_textPosition);

						m_createdPhonemeButtons.Add(newPhoneme);

						newPhoneme.GetComponent<PipPadPhoneme>().SetUpPhoneme(pbi, m_noPhonemeButtons, pbi.m_isSecondInSplitDigraph);

						width += newPhoneme.GetComponent<PipPadPhoneme>().GetWidth() / 2.0f;
						newPhoneme.transform.localPosition = new Vector3(width, 0, 0);
						width += newPhoneme.GetComponent<PipPadPhoneme>().GetWidth() / 2.0f;

						createdInfos[pbi] = newPhoneme;
					}
					
					foreach (PhonemeBuildInfo pbi in pbiList)
					{
						if (pbi.m_linkedPhoneme != null)
						{
							createdInfos[pbi.m_linkedPhoneme].GetComponent<PipPadPhoneme>().Link(createdInfos[pbi]);
						}
					}
					
					if (width > 512)
					{
						////////D.Log("PipPad word width > 512: " + width);
						m_textPosition.transform.localScale = new Vector3(0.8f, 1, 1);
						m_textPosition.transform.localPosition = new Vector3(((-width / 2.0f) * 0.8f) + 60, m_textPosition.transform.localPosition.y, m_textPosition.localPosition.z);
					}
					else
					{
						////////D.Log("PipPad word width < 512: " + width);
						m_textPosition.transform.localPosition = new Vector3((-width / 2.0f) + 60, m_textPosition.transform.localPosition.y, m_textPosition.localPosition.z);
						m_textPosition.transform.localScale = Vector3.one;
					}

					m_pipWordNotFound.Off(); // Only executes if word was found, otherwise error thrown and control never reaches here
				}
				else
				{
					m_pipWordNotFound.On();
				}
			}
		}
		catch
		{
			m_pipWordNotFound.On();
		}
	}

	public void ShowMultipleBlackboards(string imageA, string imageB, string imageC)
	{
		m_multipleBlackboardMode = true;
		
		Texture2D imageATex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + imageA);
		m_imageBlackboard.EnableClick();
		m_imageBlackboard.ShowImage(imageATex, null, null, null);
		
		Texture2D imageBTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + imageB);
		m_additionalImageBlackboards[1].ShowImage(imageBTex, null, null, null);
		m_additionalImageBlackboards[1].EnableClick();
		
		if (imageC != null)
		{
			Texture2D imageCTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + imageC);
			m_additionalImageBlackboards[0].ShowImage(imageCTex, null, null, null);
			m_additionalImageBlackboards[0].EnableClick();
		}
	}

	public void ShowMultipleBlackboards(IList<string> list)
	{
		m_multipleBlackboardMode = true;

		for(int i = 0; i < 1 + m_additionalImageBlackboards.Length && i < list.Count; ++i)
		{
			ImageBlackboard blackboard = (i == 0 ? m_imageBlackboard : m_additionalImageBlackboards[i - 1]);

			Texture2D tex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + list[i]);
			blackboard.ShowImage(tex, null, null, null);
			blackboard.EnableClick();
		}
	}
	
	public void HideAllBlackboards()
	{
		m_imageBlackboard.Hide();
		foreach (ImageBlackboard extraIb in m_additionalImageBlackboards)
		{
			if(extraIb != null)
			{
				extraIb.Hide();
			}
		}
	}
	
	public void BlackBoardClicked(ImageBlackboard clickedBlackboard)
	{
		if (CorrectPictureGameCoordinator.Instance != null)
		{
			if (clickedBlackboard == m_imageBlackboard)
			{
				CorrectPictureGameCoordinator.Instance.WordClicked(0, clickedBlackboard);
			}
			else if (clickedBlackboard == m_additionalImageBlackboards[0] )
			{
				CorrectPictureGameCoordinator.Instance.WordClicked(1, clickedBlackboard);
			}
			else if (clickedBlackboard == m_additionalImageBlackboards[1])
			{
				CorrectPictureGameCoordinator.Instance.WordClicked(2, clickedBlackboard);
			}
		}

		if(BlackboardClicked != null)
		{
            BlackboardClicked(clickedBlackboard);
		}
	}
	
	public void Dismiss()
	{
		if (m_dismissable)
		{
			Hide();
		}
	}
	
	public void SetDismissable(bool dismissable)
	{
		m_dismissable = dismissable;
	}
	
	public void Hide()
	{
		if (m_showing)
		{
			if(Hiding != null)
			{
				Hiding();
			}

			StopAllCoroutines();
			m_pipWordNotFound.Off();
			m_multipleBlackboardMode = false;
			WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_PAD_DISAPPEAR");
			iTween.Stop(m_mainHierarchyTransform.gameObject);
			iTween.MoveTo(m_mainHierarchyTransform.gameObject, m_offPosition, 1.0f);
			m_imageBlackboard.Hide();
			foreach (ImageBlackboard extraIb in m_additionalImageBlackboards)
			{
				if(extraIb != null)
				{
					extraIb.Hide();
				}
			}
			m_showing = false;
		}
	}
	
	public void HighlightWholeWord()
	{
		foreach (GameObject createdPhonemes in m_createdPhonemeButtons)
		{
			PipPadPhoneme pipPadPhoneme = createdPhonemes.GetComponent<PipPadPhoneme>();
			if (pipPadPhoneme != null)
			{
				pipPadPhoneme.ActivateFinal();
			}
		}
	}
	
	public void ShowPhonemeImage(Texture2D image, string mnemonic, string phoneme)
	{
		if (m_multipleBlackboardMode == false)
		{
			m_imageBlackboard.ShowImage(image, mnemonic, phoneme, null);
		}
	}
	
	public void ReShowWordImage()
	{
        ////////D.Log("ReShowWordImage()");

		if (m_multipleBlackboardMode == false)
		{
			Texture2D currentWordImage = Resources.Load("Images/word_images_png_350/_" + m_currentEnglishWord) as Texture2D;

			if (currentWordImage != null)
			{
				m_imageBlackboard.ShowImage(currentWordImage, null, null, null);
			}
		}
	}

	public float GetTotalLength(float delay = 0)
	{
		return (GetWordLength() + GetCombinedPhonemeLength() + delay);
	}

	public float GetWordLength()
	{
		return m_sayWholeWordButton.GetClipLength();
	}

	public float GetCombinedPhonemeLength()
	{
		float combinedDuration = 0;

		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			combinedDuration += phoneme.GetComponent<PipPadPhoneme>().GetClipLength() + m_postPhonemeSpeakDelay;
		}

		return combinedDuration;
	}


	public void SayWholeWord()
	{
		m_sayWholeWordButton.Speak();
	}


	bool m_isSayingAll = false;
	
	public void SayAll(float delay)
	{
		if(m_isSayingAll)
		{
            ////////D.Log("Is already saying all. Return early");
			return;
		}

		m_isSayingAll = true;

		////////D.Log("PPB.SayAll()");
		StopAllCoroutines();
		StartCoroutine(SayAllCo(delay));
	}
	
	IEnumerator SayAllCo(float delay)
	{
		if (!m_noPhonemeButtons)
		{
			if (m_showing)
			{
				yield return new WaitForSeconds(delay);
				foreach (GameObject phonemeButton in m_createdPhonemeButtons)
				{
					if(phonemeButton.GetComponent<PipPadPhoneme>().GetIsSecondInSplitDigraph())
					{
						continue;
					}
					
					//phonemeButton.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
					phonemeButton.GetComponent<PipPadPhoneme>().Activate();

					yield return new WaitForSeconds(m_postPhonemeSpeakDelay);
				}
				yield return new WaitForSeconds(0.5f);

				//SayWholeWord();
				yield return StartCoroutine(m_sayWholeWordButton.PlayWord());
			}
		}
		else
		{
			//SayWholeWord();
			yield return StartCoroutine(m_sayWholeWordButton.PlayWord());
		}

		m_isSayingAll = false;
	}

	public List<GameObject> GetCreatedPhonemes()
	{
		return m_createdPhonemeButtons;
	}

	public void EnableSayWholeWordButton(bool enable)
	{
		StartCoroutine(EnableSayWholeWordButtonCo(enable));
	}

	IEnumerator EnableSayWholeWordButtonCo(bool enable)
	{
		if(enable)
		{
			m_sayWholeWordButton.gameObject.SetActive(enable);
		}
		
		Vector3 localScale = enable ? Vector3.one : Vector3.zero;
		float tweenDuration = 0.1f;

		iTween.ScaleTo(m_sayWholeWordButton.gameObject, localScale, tweenDuration);

		yield return new WaitForSeconds(tweenDuration);
		
		m_sayWholeWordButton.gameObject.SetActive(enable);
	}

	public void EnableButtons(bool active)
	{
		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			phoneme.GetComponent<PipPadPhoneme>().EnableButtons(active);
		}
	}

	public void EnableButton(bool active, string targetPhoneme)
	{
		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			if(phoneme.GetComponent<PipPadPhoneme>().GetText() == targetPhoneme)
			{
				phoneme.GetComponent<PipPadPhoneme>().EnableButtons(active);
			}
		}
	}

	public void EnableDragColliders(bool active)
	{
		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			phoneme.GetComponent<PipPadPhoneme>().EnableDragCollider(active);
		}
	}

	public void EnableDragCollider(bool active, string targetPhoneme)
	{
		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			if(phoneme.GetComponent<PipPadPhoneme>().GetText() == targetPhoneme)
			{
				phoneme.GetComponent<PipPadPhoneme>().EnableDragCollider(active);
			}
		}
	}

	public void EnableLabels(bool active)
	{
		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			phoneme.GetComponent<PipPadPhoneme>().EnableLabel(active);
		}
	}

	public List<GameObject> GetDraggablesTracking(GameObject go)
	{
		List<GameObject> trackers = new List<GameObject>();

		foreach(GameObject phoneme in m_createdPhonemeButtons)
		{
			TriggerTracker tracker = phoneme.GetComponentInChildren<TriggerTracker>() as TriggerTracker;
			if(tracker != null && tracker.IsTracking(go))
			{
				trackers.Add(phoneme);
			}
		}

		return trackers;
	}
}
