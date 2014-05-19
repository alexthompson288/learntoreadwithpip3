﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StoryCoordinator : Singleton<StoryCoordinator>
{
    [SerializeField]
    private GameObject m_audioPlayButton;
    [SerializeField]
    private UILabel m_pageCountLabel;
    [SerializeField]
    private GameObject m_picturePanel;
    [SerializeField]
    private GameObject m_textPanel;
    [SerializeField]
    private float m_fadeDuration = 0.5f;
    [SerializeField]
    private Transform[] m_textAnchors;
    [SerializeField]
    private GameObject m_textPrefab;
    [SerializeField]
    private UITexture m_storyPicture;
    [SerializeField]
    private PipButton[] m_pageTurnButtons;
    [SerializeField]
    private PipButton[] m_languageButtons;


    static bool m_showWords = true;
    public static void SetShowWords(bool showWords)
    {
        m_showWords = showWords;
    }

    int m_storyId = 85;

    int m_numPages;
    int m_currentPage;

    string m_currentLanguage = "text";

    List<GameObject> m_textObjects = new List<GameObject>();
    
    List<string> m_decodeList = new List<string>();

	// Use this for initialization
	IEnumerator Start () 
    {
        System.Array.Sort(m_pageTurnButtons, CollectionHelpers.ComparePosX);

        for (int i = 0; i < m_pageTurnButtons.Length; ++i)
        {
            int buttonInt = i == 0 ? -1 : 1;
            m_pageTurnButtons [i].SetInt(buttonInt);

            m_pageTurnButtons [i].Unpressing += TurnPage;
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataRow story = DataHelpers.GetStory();
        if (story != null)
        {
            m_storyId = System.Convert.ToInt32(story["id"]);
        }

        UserStats.Activity.SetStoryId (m_storyId);
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "'");
        
        m_numPages = dt.Rows.Count;
        Debug.Log("There are " + m_numPages + " pages");

        UpdatePage();
	}

    void TurnPage(PipButton button)
    {
        m_currentPage += button.GetInt();

        if (m_currentPage >= m_numPages)
        {
            GameManager.Instance.CompleteGame();
        } 
        else if (m_currentPage > -1)
        {
            StartCoroutine(TurnPageCo());
        } 
        else
        {
            m_currentPage = 0;
        }
    }

    IEnumerator TurnPageCo()
    {
        EnableButtons(false);

        TweenAlpha.Begin(m_textPanel, m_fadeDuration, 0);
        TweenAlpha.Begin(m_picturePanel, m_fadeDuration, 0);
        
        yield return new WaitForSeconds(m_fadeDuration);

        ClearText();
       
        UpdatePage();

        TweenAlpha.Begin(m_textPanel, m_fadeDuration, 1);
        TweenAlpha.Begin(m_picturePanel, m_fadeDuration, 1);

        yield return new WaitForSeconds(m_fadeDuration);

        EnableButtons(true);
    }

    void ClearText()
    {
        for (int i = m_textObjects.Count - 1; i > -1; --i)
        {
            Destroy(m_textObjects[i]);
        }
        m_textObjects.Clear();
    }

    public void SetLanguage(string language)
    {
        StartCoroutine(SetLanguageCo(language));
    }

    IEnumerator SetLanguageCo(string language)
    {
        DataRow storyPage = FindStoryPage();

        if (storyPage != null)
        {
            m_currentLanguage = language;
            EnableButtons(false);

            TweenAlpha.Begin(m_textPanel, m_fadeDuration, 0);

            yield return new WaitForSeconds(m_fadeDuration);

            ClearText();
            UpdateText(storyPage);
            UpdateAudio(storyPage, "_" + language.Replace("text", ""));

            TweenAlpha.Begin(m_textPanel, m_fadeDuration, 0);
        }
    }

    void UpdateAudio(DataRow storyPage, string modifier = "")
    {
        string audioSetting = storyPage["audio"] == null ? null : storyPage["audio"].ToString();
        if (!string.IsNullOrEmpty(audioSetting))
        {
            m_audioPlayButton.SetActive(true);
            m_audioPlayButton.GetComponent<StoryPlayLineButton>().SetLineAudio("audio/stories/" + storyPage["audio"].ToString() + modifier);
        }
    }

    void UpdatePicture(DataRow storyPage)
    {
        string imageName = storyPage["image"] == null ? "" : storyPage["image"].ToString().Replace(".png", "");
        string bgImageName = storyPage["backgroundart"] == null ? "" : storyPage["backgroundart"].ToString().Replace(".png", "");
        
        Texture2D image = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + imageName);
        Texture2D bgImage = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + bgImageName);
        
        m_storyPicture.mainTexture = bgImage != null ? bgImage : image;
    }

    void UpdateText(DataRow storyPage)
    {
        if (m_showWords)
        {
            //textposition
            string textToDisplay = storyPage [m_currentLanguage].ToString().Replace("\\n", "\n");
            
            string[] lines = textToDisplay.Split('\n');
            
            float length = 0;
            float height = 0;
            float widestLineWidth = 0;
            
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
                        widestLineWidth = Mathf.Max(widestLineWidth, length);
                        
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

            float maxWidth = 850;
            if (widestLineWidth > maxWidth)
            {
                float scale = maxWidth / widestLineWidth;
                m_textAnchors [0].transform.localScale = scale * Vector3.one;
            } else
            {
                m_textAnchors [0].transform.localScale = Vector3.one;
            }
        }
    }

    void UpdatePage()
    {
        DataRow storyPage = FindStoryPage();
        
        if (storyPage != null)
        {
            UpdateText(storyPage);
            UpdatePicture(storyPage);
            UpdateAudio(storyPage);
        }
    }

    DataRow FindStoryPage()
    {
        int oneBasedCurrentPage = m_currentPage + 1;
        
        if(oneBasedCurrentPage <=  m_numPages)
        {
            m_pageCountLabel.text = oneBasedCurrentPage.ToString() + " \\ " + m_numPages.ToString();
        }
        
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "' and pageorder='" + oneBasedCurrentPage + "'");

        return dt.Rows.Count > 0 ? dt.Rows [0] : null;
    }

    void EnableButtons(bool enable)
    {
        foreach (PipButton button in m_pageTurnButtons)
        {
            button.EnableCollider(enable);
        }

        foreach (PipButton button in m_languageButtons)
        {
            button.EnableCollider(enable);
        }
    }
}