using UnityEngine;
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
    [SerializeField]
    private UILabel m_scaleLabel;
    [SerializeField]
    private UITexture m_background;
    [SerializeField]
    private float m_maxWidth = 125;
    [SerializeField]
    private float m_maxHeight = 36;

    int m_storyId = 85;

    int m_numPages;
    int m_currentPage;

    string m_currentTextAttribute
    {
        get
        {
            return "text_" + ColorInfo.GetColorString(GameManager.Instance.currentColor).ToLower();
        }
    }

    List<DataRow> m_storyPages = new List<DataRow>();

    List<GameObject> m_textObjects = new List<GameObject>();
    
    List<string> m_decodeList = new List<string>();

    PipButton m_currentColorButton = null;

	// Use this for initialization
	IEnumerator Start () 
    {
        System.Array.Sort(m_pageTurnButtons, CollectionHelpers.LeftToRight);

        for (int i = 0; i < m_pageTurnButtons.Length; ++i)
        {
            int buttonInt = i == 0 ? -1 : 1;
            m_pageTurnButtons [i].SetInt(buttonInt);

            m_pageTurnButtons [i].Unpressing += TurnPage;
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        ColorInfo.PipColor startPipColor = GameManager.Instance.currentColor;

        DataRow story = DataHelpers.GetStory();
        if (story != null)
        {
            m_storyId = System.Convert.ToInt32(story ["id"]);
        } 

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "'");
        m_storyPages = dt.Rows;
        m_numPages = m_storyPages.Count;

        UpdatePage();
	}

    void TurnPage(PipButton button)
    {
        m_currentPage += button.GetInt();

        if (m_currentPage > m_numPages)
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

    void OnClickColorButton(PipButton button)
    {
        if (button != m_currentColorButton)
        {
            if (m_currentColorButton != null)
            {
                m_currentColorButton.ChangeSprite(false);
            }

            m_currentColorButton = button;

            GameManager.Instance.SetCurrentColor(m_currentColorButton.pipColor);
 

            ClearText();
            UpdatePage();
        }
    }

    /*
    public void SetTextAttribute(string textAttribute)
    {
        StartCoroutine(SetLanguageCo(textAttribute));
    }

    IEnumerator SetLanguageCo(string textAttribute)
    {
        DataRow storyPage = FindStoryPage();

        if (storyPage != null)
        {
            m_currentTextAttribute = textAttribute;
            EnableButtons(false);

            TweenAlpha.Begin(m_textPanel, m_fadeDuration, 0);

            yield return new WaitForSeconds(m_fadeDuration);

            ClearText();
            UpdateText(storyPage);
            UpdateAudio(storyPage);

            TweenAlpha.Begin(m_textPanel, m_fadeDuration, 0);
        }
    }
    */

    void UpdateAudio(DataRow storyPage)
    {
        if (storyPage != null && storyPage [m_currentTextAttribute] != null)
        {
            string audioFileName = storyPage ["audio"] != null ? 
                System.String.Format("{0}_{1}", storyPage ["audio"].ToString(), ColorInfo.GetColorString(GameManager.Instance.currentColor)) 
                    : null;

            m_audioPlayButton.SetActive(!System.String.IsNullOrEmpty(audioFileName));
            m_audioPlayButton.GetComponent<StoryPlayLineButton>().SetLineAudio("audio/stories/" + audioFileName);
        } 
        else
        {
            m_audioPlayButton.SetActive(false);
        }
    }

    void UpdatePicture(DataRow storyPage)
    {
        m_storyPicture.mainTexture = DataHelpers.GetPicture(storyPage);
    }

    void UpdatePicture()
    {
        Texture2D tex = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/the_end");
        m_storyPicture.mainTexture = tex;
    }

    void UpdateText(DataRow storyPage)
    {
        m_scaleLabel.transform.localScale = Vector3.one;

        string originalText = storyPage [m_currentTextAttribute].ToString().Replace("\\n", "\n").Replace("\n", "").Replace("  ", " ");

#if UNITY_EDITOR
        //originalText = "salkdrje lqirjli awlekjtq lwierjtie rjgliqej alkrjgq elrjgeorfg jorjgeorjt liawejrp ierjgiej lkwJEFLKSJDF ASLDKEFLASDJF JKSDFLdlkfjgeli  kjr wlekrfjwl slkjgrells slijfwli slrkjgle sdlhgliew slijgwl lkrjg lrgijo wleirjow wliejr wliej welitje proyjs";
#endif

        string textToDisplay = originalText;

        ColorInfo.PipColor currentColor = GameManager.Instance.currentColor;
        bool isPipPadAllowed = (currentColor != ColorInfo.PipColor.Green && currentColor != ColorInfo.PipColor.Orange);

        while (true)
        {
            int lineStartIndex = 0;
            int checkLength = 1;
            for (int i = 0; i < textToDisplay.Length && lineStartIndex + checkLength < textToDisplay.Length; ++i)
            {
                if (NGUIHelpers.GetLabelWidth(m_scaleLabel, textToDisplay.Substring(lineStartIndex, checkLength)) > m_maxWidth)
                {
                    // Find empty char before lineStartIndex + checkLength
                    while (textToDisplay[lineStartIndex + checkLength] != ' ')
                    {
                        --checkLength;
                    }

                    while(checkLength > -1)
                    {
                        if(textToDisplay[lineStartIndex + checkLength] == ' ')
                        {
                            ++checkLength; // Increment checkLength so that the empty character is at the end of the current line instead of the start of the new one
                            break;
                        }
                        else
                        {
                            --checkLength;
                        }
                    }

                    textToDisplay = textToDisplay.Insert(lineStartIndex + checkLength, "\n");
                    lineStartIndex = lineStartIndex + checkLength;

                    checkLength = 1;
                } 
                else
                {
                    ++checkLength;
                }
            }

            if(NGUIHelpers.GetLabelHeight(m_scaleLabel, textToDisplay) < m_maxHeight)
            {
                break;
            }
            else
            {
                m_scaleLabel.transform.localScale *= 0.99f;
                textToDisplay = originalText;
            }
        }

        m_textAnchors [0].transform.localScale = m_scaleLabel.transform.localScale;

        m_scaleLabel.text = textToDisplay;

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

                    UILabel label = newWordInstance.GetComponent<UILabel>() as UILabel;
                    label.text = newWord + " ";
                    newWordInstance.transform.localPosition = new Vector3(length, height, 0);
                    Vector3 wordSize = NGUIHelpers.GetLabelSize3(label);
                    length += wordSize.x;
                    widestLineWidth = Mathf.Max(widestLineWidth, length);
                    
                    string storyType = SessionInformation.Instance.GetStoryType();
                    if (storyType == "" || storyType == null)
                    {
                        storyType = "Reception";
                    }
                    
                    ShowPipPadForWord showPipPadForWord = newWordInstance.GetComponent<ShowPipPadForWord>() as ShowPipPadForWord;

                    if(isPipPadAllowed)
                    {
                        bool isOnDecodeList = m_decodeList.Contains(newWord.ToLower().Replace(".", "").Replace(",", "").Replace(" ", "").Replace("?", ""));
                        
                        showPipPadForWord.SetUp(newWord, wordSize, true);
                        
                        // Highlight if word is on the decode list
                        if (isOnDecodeList)
                        {
                            showPipPadForWord.Highlight(storyType == "Classic"); // If "Classic" the word is decodeable, otherwise it is non-decodeable
                        }
                    }
                    else
                    {
                        Destroy(showPipPadForWord);
                    }
                }
            }
            if (hadValidWord)
            {
                length = 0;
                //height -= 96;
                height -= 100;
            }
        }
    }

    void UpdatePage()
    {
        if (m_currentPage < m_numPages)
        {
            m_pageCountLabel.text = System.String.Format("{0} \\ {1}", m_currentPage + 1, m_numPages);

            DataRow storyPage = FindStoryPage();
            
            if (storyPage != null)
            {
                UpdatePicture(storyPage);
                UpdateAudio(storyPage);
                UpdateText(storyPage);
            }
        }
        else
        {
            UpdatePicture();
        }
    }

    DataRow FindStoryPage()
    {
        /*
        int oneBasedCurrentPage = m_currentPage + 1;
        
        if(oneBasedCurrentPage <=  m_numPages)
        {
            m_pageCountLabel.text = oneBasedCurrentPage.ToString() + " \\ " + m_numPages.ToString();
        }

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + m_storyId + "' and pageorder='" + oneBasedCurrentPage + "'");

        return dt.Rows.Count > 0 ? dt.Rows [0] : null;
        */
        return m_currentPage < m_storyPages.Count ? m_storyPages [m_currentPage] : null;
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
