using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class BankLettersCoordinator : MonoBehaviour 
{
    [SerializeField]
    private ClickEvent m_correctButton;
    [SerializeField]
    private ClickEvent m_incorrectButton;
    [SerializeField]
    private ClickEvent m_leftArrow;
    [SerializeField]
    private ClickEvent m_rightArrow;
    [SerializeField]
    private ClickEvent m_backToIndexButton;
    [SerializeField]
    private ClickEvent m_soundButton;
    [SerializeField]
    private UILabel m_graphemeLabel;
    [SerializeField]
    private UITexture m_mnemonicTexture;
    [SerializeField]
    private UILabel m_mnemonicLabel;
    [SerializeField]
    private string m_mainColorString;
    [SerializeField]
    private string m_highlightColorString;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private GameObject m_allCorrectNotice;
    
    List<DataRow> m_phonemePool = new List<DataRow>();
    List<string> m_letterPool = new List<string>();
    
    int m_currentIndex = 0;
    
    bool m_pictureActive = false;
    bool m_buttonsActive = false;

    bool m_isAlphabet;

    
    IEnumerator Start()
    {
        yield return StartCoroutine(BankIndexCoordinator.WaitForSetDataType());

        m_isAlphabet = BankIndexCoordinator.Instance.GetDataType() == "alphabet";

        BankIndexCoordinator.Instance.OnMoveToShow += Refresh;

        if (m_isAlphabet)
        {
            for (char i = 'a'; i <= 'z'; ++i)
            {
                m_letterPool.Add(i.ToString());
            }
        } 

        m_mnemonicTexture.gameObject.SetActive(!m_isAlphabet);

        m_soundButton.SingleClicked += OnClickSoundButton;
        m_backToIndexButton.SingleClicked += OnClickBackToIndex;
        m_correctButton.SingleClicked += OnClickCorrect;
        m_incorrectButton.SingleClicked += OnClickIncorrect;
        m_leftArrow.SingleClicked += OnLeftArrowClick;
        m_rightArrow.SingleClicked += OnRightArrowClick;
    }
    
    void Refresh(DataRow phoneme, string s)
    {
        bool passedTarget = false;

        if (m_isAlphabet)
        {
            m_currentIndex = System.String.IsNullOrEmpty(s) ? 0 : m_letterPool.IndexOf(s);

            passedTarget = System.String.IsNullOrEmpty(s);
        } 
        else
        {
            m_phonemePool = DataHelpers.GetPhonemes();
            m_currentIndex = phoneme == null ? 0 : m_phonemePool.IndexOf(phoneme);

            passedTarget = phoneme != null;
        }

        if (passedTarget || !AllAnswersCorrect())
        {
            ToggleAllCorrectNotice(false);
            ShowNew();
        } 
        else
        {
            ToggleAllCorrectNotice(true);
        }
    }

    void ToggleAllCorrectNotice(bool allCorrect)
    {
        m_allCorrectNotice.SetActive(allCorrect);
        m_mnemonicTexture.gameObject.SetActive(!m_isAlphabet && !allCorrect);
        m_mnemonicLabel.gameObject.SetActive(!m_isAlphabet && !allCorrect);
        m_graphemeLabel.gameObject.SetActive(!allCorrect);
        m_soundButton.gameObject.SetActive(!allCorrect);
        EnableClickEventColliders(!allCorrect);
    }

    void ShowNew()
    {
        D.Log("ShowNew()");

        D.Log("count: " + m_phonemePool.Count);
        D.Log("currentIndex: " + m_currentIndex);

        string labelText = m_isAlphabet ? m_letterPool [m_currentIndex] : m_phonemePool [m_currentIndex] ["phoneme"].ToString();
        m_graphemeLabel.text = labelText;

        // Mnemonic Picture
        if (!m_isAlphabet)
        {
            DataRow phoneme = m_phonemePool[m_currentIndex];

            Texture2D tex = Resources.Load<Texture2D>(System.String.Format("Images/mnemonics_images_png_250/{0}_{1}",
                                                                           phoneme["phoneme"],
                                                                           phoneme["mneumonic"].ToString().Replace(" ", "_")));

            m_mnemonicTexture.gameObject.SetActive(tex != null);
            m_mnemonicTexture.mainTexture = tex;

            // mnemonic label
            string colorReplace = phoneme["phoneme"].ToString();
            string mnemonic = phoneme["mneumonic"].ToString();

            string finalletter = m_mainColorString + mnemonic.Replace(colorReplace, m_highlightColorString + colorReplace + m_mainColorString);
            // do some mad logic to replace splits
            if (colorReplace.Contains("-") && colorReplace.Length == 3)
            {
                int startIndex = 0;
                while (finalletter.IndexOf(colorReplace[0], startIndex) != -1)
                {
                    startIndex = finalletter.IndexOf(colorReplace[0], startIndex);
                    if (startIndex < finalletter.Length + 2)
                    {
                        if (finalletter[startIndex + 2] == colorReplace[2])
                        {
                            finalletter = finalletter.Insert(startIndex + 3, m_mainColorString);
                            finalletter = finalletter.Insert(startIndex + 2, m_highlightColorString);
                            finalletter = finalletter.Insert(startIndex + 1, m_mainColorString);
                            finalletter = finalletter.Insert(startIndex + 0, m_highlightColorString);
                            startIndex += m_mainColorString.Length * 2 + m_highlightColorString.Length * 2;
                        }
                    }
                    
                    startIndex++;
                }
            }

            m_mnemonicLabel.text = finalletter;
        }
    }

    void OnClickSoundButton(ClickEvent click)
    {
        //AudioClip clip = m_isAlphabet ? AudioBankManager.Instance.GetAudioClip("lettername_" + m_letterPool[m_currentIndex]) : LoaderHelpers.LoadMnemonic(m_phonemePool[m_currentIndex]);
        AudioClip clip = m_isAlphabet ? AudioBankManager.Instance.GetAudioClip("lettername_" + m_letterPool[m_currentIndex]) : 
                                        AudioBankManager.Instance.GetAudioClip(m_phonemePool[m_currentIndex]["grapheme"].ToString());

        if(clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }
    
    void OnClickBackToIndex(ClickEvent click)
    {
        BankIndexCoordinator.Instance.RefreshButtons();
    }
    
    void OnClickCorrect(ClickEvent click)
    {
        if (m_isAlphabet)
        {
            BankInfo.Instance.NewAnswer(m_letterPool[m_currentIndex], true);
        } 
        else
        {
            BankInfo.Instance.NewAnswer(m_phonemePool [m_currentIndex].GetId(), BankIndexCoordinator.Instance.GetDataType() , true);
        }

        OnArrowClick(1);
    }
    
    void OnClickIncorrect(ClickEvent click)
    {
        if (m_isAlphabet)
        {
            BankInfo.Instance.NewAnswer(m_letterPool[m_currentIndex], false);
        } 
        else
        {
            BankInfo.Instance.NewAnswer(m_phonemePool [m_currentIndex].GetId(), BankIndexCoordinator.Instance.GetDataType(), false);
        }

        OnArrowClick(1);
    }
    
    void OnLeftArrowClick(ClickEvent clickBehaviour)
    {
        OnArrowClick(-1);
    }
    
    void OnRightArrowClick(ClickEvent clickBehaviour)
    {
        OnArrowClick(1);
    }
    
    void OnArrowClick(int direction)
    {
        if (AllAnswersCorrect())
        {
            ToggleAllCorrectNotice(true);
        } 
        else
        {
            m_currentIndex += direction;
            ClampCurrentIndex();

            //int collectionCount = m_isAlphabet ? m_letterPool.Count : m_phonemePool.Count;

            if(m_isAlphabet)
            {
                while(BankInfo.Instance.IsCorrect(m_letterPool[m_currentIndex]))
                {
                    m_currentIndex += direction;
                    ClampCurrentIndex();
                }
            }
            else
            {
                while(BankInfo.Instance.IsCorrect(m_phonemePool[m_currentIndex].GetId(), BankIndexCoordinator.Instance.GetDataType()))
                {
                    m_currentIndex += direction;
                    ClampCurrentIndex();
                }
            }

            //EnableClickEventColliders(false);
            
            ShowNew();
        }
    }

    void ClampCurrentIndex()
    {
        int collectionCount = m_isAlphabet ? m_letterPool.Count : m_phonemePool.Count;

        if (m_currentIndex < 0)
        {
            m_currentIndex = collectionCount - 1;
        } 
        else if (m_currentIndex >= collectionCount)
        {
            m_currentIndex = 0;
        }
    }
    
    void EnableClickEventColliders(bool enable)
    {
        m_correctButton.EnableCollider(enable);
        m_incorrectButton.EnableCollider(enable);
        m_leftArrow.EnableCollider(enable);
        m_rightArrow.EnableCollider(enable);
        m_soundButton.EnableCollider(enable);
    }

    bool AllAnswersCorrect()
    {
        bool allCorrect = true;

        foreach (DataRow phoneme in m_phonemePool)
        {
            if(!BankInfo.Instance.IsCorrect(phoneme.GetId(), BankIndexCoordinator.Instance.GetDataType()))
            {
                allCorrect = false;
            }
        }

        foreach (string letter in m_letterPool)
        {
            if(!BankInfo.Instance.IsCorrect(letter))
            {
                allCorrect = false;
            }
        }

        return allCorrect;
    }
}
