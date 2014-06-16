using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;
using System.IO;

public class StoryReaderLogic : Singleton<StoryReaderLogic> 
{
    [SerializeField]
    private StoryPageDisplayer m_page0;
    [SerializeField]
    private StoryPageDisplayer m_page1;
    [SerializeField]
    private Transform[] m_textAnchors;
    [SerializeField]
    private GameObject m_audioPlayButton;
    [SerializeField]
    private GameObject m_hideWhenTurning;
    [SerializeField]
    private GameObject m_textPrefab;
	[SerializeField]
	private UILabel m_pageCount;
	[SerializeField]
	private float m_pageTurnDuration = 0.8f;
    [SerializeField]
    private UITexture m_storyPicture;

    static bool m_showWords = true;
    public static void SetShowWords(bool showWords)
    {
        m_showWords = showWords;
    }

#if UNITY_EDITOR
	[SerializeField]
	private bool m_useDebugStory;
#endif

    int m_currentPage;
    StoryPageDisplayer m_currentDisplayer;
    bool m_canTurn = false;
    List<GameObject> m_textObjects = new List<GameObject>();

	List<string> m_decodeList = new List<string>();

	int m_numPages;

	string m_currentLanguage = "text";

	int m_storyId = 85;

	bool m_hasStoryData = false;

	public static IEnumerator WaitForStoryData()
	{
		while (StoryReaderLogic.Instance == null) 
		{
			yield return null;
		}
		while (!StoryReaderLogic.Instance.m_hasStoryData) 
		{
			yield return null;
		}
	}

	// Use this for initialization
    IEnumerator Start() 
    {
        m_canTurn = false;
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataRow story = DataHelpers.GetStory();
        if (story != null)
        {
            m_storyId = System.Convert.ToInt32(story["id"]);
        }

		//UserStats.Activity.SetStoryId (m_storyId);

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "'");

		m_numPages = dt.Rows.Count;
		Debug.Log("There are " + m_numPages + " pages");

		m_hasStoryData = true;

        //yield return StartCoroutine(LoadAssetBundle());
        m_currentDisplayer = m_page0;
        StartCoroutine(NextPage(true));
	}

	public int GetStoryId()
	{
		return m_storyId;
	}

    IEnumerator LoadAssetBundle()
    {
        //DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery(
            //"select * from stories where id='"+SessionInformation.Instance.GetBookId()+
            //"' ");

		DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery(
			"select * from stories where id='"+ m_storyId +
			"' ");

        if ( dataTable.Rows.Count > 0 )
        {
            SavedAssetBundles.AssetVersionResult avr =  SavedAssetBundles.Instance.GetOnlineAssetVersions();
			Debug.Log("avr: " + avr);
			Debug.Log("avr.m_assetList: " + avr.m_assetList);
			Debug.Log("avr.m_assetList.Count: " + avr.m_assetList.Count);
            string expectedBundleName = "{PLATFORM}/" + dataTable.Rows[0]["title"].ToString().Replace(" ", "_").Replace("?", "_").Replace("!","_").ToLower() + ".unity3d";

            int latestVersion = 0;
            foreach (SavedAssetBundles.DownloadedAsset da in avr.m_assetList)
            {
                if (da.m_name == expectedBundleName)
                {
                    latestVersion = da.m_version;
                }
            }

            if ((!SavedAssetBundles.Instance.HasAny(expectedBundleName))
                || (!SavedAssetBundles.Instance.HasVersion(expectedBundleName, latestVersion)))
            {
                Debug.Log("Need new version of " + expectedBundleName);
                yield return StartCoroutine(AssetBundleLoader.Instance.DownloadWebAssetBundle(
                    Path.Combine(GameDataBridge.Instance.GetOnlineBaseURL(), expectedBundleName), SavedAssetBundles.Instance.GetSavedName(expectedBundleName), expectedBundleName, latestVersion));
            }
            else
            {
                // load it
                Debug.Log("Already have " + expectedBundleName);
                yield return StartCoroutine(
                    AssetBundleLoader.Instance.LoadPersistentDataAssetBundle(SavedAssetBundles.Instance.GetSavedName(expectedBundleName))
                );
            }

        yield break;
        }
    }

    public void TurnPage(bool back)
    {
        if (m_canTurn)
        {
            if (back)
            {
                if (m_currentPage > 0)
                {
                    StartCoroutine(PrevPage());
                }
            }
            else
            {
                StartCoroutine(NextPage(false));
            }
        }
    }

	public IEnumerator PrevPage()
	{
		Debug.Log("PrevPage()");
		
		ClearOld();
		
		m_hideWhenTurning.SetActive(false);
		m_canTurn = false;
		m_currentPage--;
		SwapActivePage();
		
		DataTable dt = UpdateCurrentDisplayer();
		
		StartCoroutine(TurnPrevPage());

		yield return new WaitForSeconds(0.3f);
		
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			SetText(row);
		}
	}

	IEnumerator TurnPrevPage()
	{
		yield return StartCoroutine(m_currentDisplayer.TurnPageBack((m_currentDisplayer == m_page0) ? m_page1.transform : m_page0.transform));

		m_hideWhenTurning.SetActive(true);
		m_canTurn = true;
	}

	IEnumerator TurnNextPage()
	{
		yield return StartCoroutine(m_currentDisplayer.TurnPage((m_currentDisplayer == m_page0) ? m_page1.transform : m_page0.transform));
		
		m_canTurn = true;
		m_hideWhenTurning.SetActive(true);
	}
	
	public IEnumerator NextPage(bool isFirstPage)
	{
		m_canTurn = false;
		m_hideWhenTurning.SetActive(false);
		if (!isFirstPage)
		{
			m_currentPage++;
			StartCoroutine(TurnNextPage());
			SwapActivePage();
		}
		
		ClearOld();
		DataTable dt = UpdateCurrentDisplayer();
		if (dt.Rows.Count == 0)
		{
			GameManager.Instance.CompleteGame();
		}
		
		if (!isFirstPage)
		{
			yield return new WaitForSeconds(0.3f);
		}
		
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			SetText(row);
		}
		
		if(isFirstPage)
		{
			yield return new WaitForSeconds(1f);
			m_canTurn = true;
			m_hideWhenTurning.SetActive(true);
		}
	}

    void SwapActivePage()
    {
        if (m_currentDisplayer == m_page0)
        {
            m_currentDisplayer = m_page1;
        }
        else
        {
            m_currentDisplayer = m_page0;
        }
    }

	private bool StringIsEmpty(string s)
	{
		return (s == "");
	}

	private bool StringIsSpace(string s)
	{
		return (s == " ");
	}

    DataTable UpdateCurrentDisplayer()
    {
		//Debug.Log("UpdateCurrentDisplayer()");

        int currentPageOneBased = m_currentPage + 1;

		if(currentPageOneBased <=  m_numPages)
		{
			m_pageCount.text = currentPageOneBased.ToString() + " \\ " + m_numPages.ToString();
		}

		
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "' and pageorder='" + currentPageOneBased + "'");

        if (dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];

            string imageName = row["image"] == null ? "" : row["image"].ToString().Replace(".png", "");
            string bgImageName = row["backgroundart"] == null ? "" : row["backgroundart"].ToString().Replace(".png", "");

            Texture2D image = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + imageName);
            Texture2D bgImage = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + bgImageName);

            m_currentDisplayer.Show(image, bgImage);

            if(m_storyPicture != null)
            {
                m_storyPicture.mainTexture = bgImage != null ? bgImage : image;
            }


			string storyType = SessionInformation.Instance.GetStoryType();

			if(storyType == "" || storyType == null)
			{
				storyType = "Pink";
			}

			m_decodeList.Clear();
			if(storyType == "Reception")
			{
				AddToDecodeList(row, "nondecodablereceptionwords");
				AddToDecodeList(row, "nondecodableyearonewords");
			}
			else if(storyType == "Year 1")
			{
				AddToDecodeList(row, "nondecodableyearonewords");
			}
			else if(storyType == "Classic")
			{
				string classicStoryLevel = SessionInformation.Instance.GetClassicStoryLevel();

				if(classicStoryLevel == "Reception" || classicStoryLevel == "Year 1")
				{
					AddToDecodeList(row, "receptionwords");
				}

				if(classicStoryLevel == "Year 1")
				{
					AddToDecodeList(row, "yearonenwords");
				}
			}

			m_decodeList.RemoveAll(StringIsEmpty);
			m_decodeList.RemoveAll(StringIsSpace);
        }

        return dt;
    }

	void AddToDecodeList(DataRow row, string column)
	{
		string unsplit = (string)row[column];
		if(unsplit != null)
		{
			string[] splitList = unsplit.Split(' ');

			foreach(string s in splitList)
			{
				m_decodeList.Add(s);
			}
		}
	}

    void ClearOld()
    {
        foreach (GameObject oldText in m_textObjects)
        {
            Destroy(oldText);
        }
        m_textObjects.Clear();
        m_audioPlayButton.SetActive(false);
    }

    void SetText(DataRow row)
    {
		if (m_showWords)
        {
            //textposition
            string textToDisplay = row [m_currentLanguage].ToString().Replace("\\n", "\n");

            string[] lines = textToDisplay.Split('\n');
            
            float length = 0;
            float height = 0;
            float maxWidth = 0;

            foreach (string line in lines)
            {
                string[] lineWords = line.Split(' ');
                bool hadValidWord = false;
                foreach (string newWord in lineWords)
                {
                    if (!string.IsNullOrEmpty(newWord) && newWord != " ")
                    {
                        hadValidWord = true;
                        GameObject newWordInstance = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
                            m_textPrefab, m_textAnchors [0]);

                        m_textObjects.Add(newWordInstance);

                        newWordInstance.GetComponent<UILabel>().text = newWord + " ";
                        newWordInstance.transform.localPosition = new Vector3(length, height, 0);
                        Vector3 wordSize = newWordInstance.GetComponent<UILabel>().font.CalculatePrintedSize(newWord + " ", false, UIFont.SymbolStyle.None);
                        length += wordSize.x;
                        maxWidth = Mathf.Max(maxWidth, length);

                        string storyType = SessionInformation.Instance.GetStoryType();
                        if (storyType == "" || storyType == null)
                        {
                            storyType = "Reception";
                        }

                        ShowPipPadForWord showPipPadForWord = newWordInstance.GetComponent<ShowPipPadForWord>() as ShowPipPadForWord;
                        bool isOnDecodeList = m_decodeList.Contains(newWord.ToLower().Replace(".", "").Replace(",", "").Replace(" ", "").Replace("?", ""));

                        showPipPadForWord.SetUp(newWord, wordSize, m_currentLanguage == "text");

                        // Highlight if word is on the decode list
                        if (isOnDecodeList)
                        {
                            showPipPadForWord.Highlight(storyType == "Classic"); // If "Classic" the word is decodeable, otherwise it is non-decodeable
                        }
                    }
                }
                if (hadValidWord)
                {
                    length = 0;
                    height -= 96;
                }
            }

            UpdateAudio(row);

            if (maxWidth > 850)
            {
                float scale = 850 / maxWidth;
                m_textAnchors [0].transform.localScale = scale * Vector3.one;
            } 
            else
            {
                m_textAnchors [0].transform.localScale = Vector3.one;
            }
        }
    }

	public void SetLanguage(string language)
	{
		m_currentLanguage = language;

		ClearOld();

		DataTable dt = UpdateCurrentDisplayer();
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			SetText(row);

			UpdateAudio(row, "_" + language.Replace("text", ""));
		}
	}

	void UpdateAudio(DataRow row, string modifier = "")
	{
		string audioSetting = row["audio"] == null ? null : row["audio"].ToString();
		if (!string.IsNullOrEmpty(audioSetting))
		{
			m_audioPlayButton.SetActive(true);
			m_audioPlayButton.GetComponent<StoryPlayLineButton>().SetLineAudio("audio/stories/" + row["audio"].ToString() + modifier);
		}
	}

	public float GetPageTurnDuration()
	{
		return m_pageTurnDuration;
	}
}
