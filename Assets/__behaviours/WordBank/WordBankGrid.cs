using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;

public class WordBankGrid : Singleton<WordBankGrid>
{

    [SerializeField]
    private GameObject m_wordButtonPrefab;

    List<DataRow> m_data = new List<DataRow>();
    List<GameObject> m_createdWords = new List<GameObject>();
    int m_correctWordIndex = 0;
    string m_currentWord = null;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        foreach (int sectionId in ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds)
        {
            DataTable dt = DataHelpers.GetSectionWords(sectionId);

            m_data.AddRange(dt.Rows);

        }

        int index = 0;
        foreach (DataRow row in m_data)
        {
            GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordButtonPrefab,
                transform);
            newWord.GetComponentInChildren<WordBankWord>().SetUp(row);
            newWord.name = string.Format("WORD_{0:000}_BOX", index);
            index++;
            m_createdWords.Add(newWord);
        }

        GetComponent<UIGrid>().Reposition();

        yield return new WaitForSeconds(1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("WORD_BANK_1");
        yield return new WaitForSeconds(1.5f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("WORD_BANK_2");
    }


    public void ShowThreeWords(string word)
    {
        m_currentWord = word;
        HashSet<string> wordList = new HashSet<string>();

        wordList.Add(word);
        while (wordList.Count < 3)
        {
            wordList.Add(m_data[Random.Range(0, m_data.Count)]["word"].ToString());
        }

        List<string> asList = new List<string>();
        asList.AddRange(wordList);

        List<string> reorderedList = new List<string>();

        int index = 0;
        while (asList.Count > 0)
        {
            int selectedIndex = Random.Range(0, asList.Count);

            string selected = asList[selectedIndex];

            reorderedList.Add(selected);
            if (selected == word)
            {
                m_correctWordIndex = index;
            }

            asList.RemoveAt(selectedIndex);
            ++index;
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_PICTURE_INSTRUCTION");
        PipPadBehaviour.Instance.ShowMultipleBlackboards(reorderedList[0], reorderedList[1], reorderedList[2]);
    }


    public void WordClicked(int wordIndex, ImageBlackboard fromBlackboard)
    {
        if (wordIndex == m_correctWordIndex)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
            SessionInformation.Instance.UnlockWord(m_currentWord);
            StartCoroutine(YouUnlockedItAudio());
        }
        else
        {
            fromBlackboard.ShakeFade();
            StartCoroutine(NotQuiteBlue());
        }
    }

    IEnumerator NotQuiteBlue()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
        yield return new WaitForSeconds(2.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_BLUE_BUTTONS");
    }

    IEnumerator YouUnlockedItAudio()
    {
        PipPadBehaviour.Instance.SetDismissable(false);
        PipPadBehaviour.Instance.HideAllBlackboards();
        PipPadBehaviour.Instance.SayAll(1.5f);
        yield return new WaitForSeconds(6f);
        PipPadBehaviour.Instance.Hide();
        yield return new WaitForSeconds(0.5f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("YOU_UNLOCKED_IT");
        PipPadBehaviour.Instance.SetDismissable(true);
        Refresh();
    }

    void Refresh()  
    {
        foreach(GameObject addedObject in m_createdWords)
        {
            addedObject.GetComponentInChildren<WordBankWord>().Refresh(false);
        }
    }

}
