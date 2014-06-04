using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class StoryBrowserCoordinator : Singleton<StoryBrowserCoordinator> 
{
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private GameObject m_bookPrefab;
    [SerializeField]
    private UIGrid m_bookGrid;

    List<StoryBrowserBook> m_spawnedBooks = new List<StoryBrowserBook>();

    PipButton m_currentColorButton = null;

    public string GetCurrentColorName()
    {
        return m_currentColorButton != null ? ColorInfo.GetColorString(m_currentColorButton.pipColor).ToLower() : "pink";
    }

    void Start()
    {
        foreach (PipButton button in m_colorButtons)
        {
            button.Pressing += OnChooseColor;
        }

        StartCoroutine(ChangeColor(ColorInfo.PipColor.Pink));

        foreach (PipButton button in m_colorButtons)
        {
            if(button.pipColor == ColorInfo.PipColor.Pink)
            {
                m_currentColorButton = button;
                m_currentColorButton.ChangeSprite(true);
                break;
            }
        }
    }

    void OnChooseColor(PipButton button)
    {
        if (button != m_currentColorButton)
        {
            if (m_currentColorButton != null)
            {
                m_currentColorButton.ChangeSprite(false);
            }

            m_currentColorButton = button;

            StartCoroutine(ChangeColor(button.pipColor));
        }
    }

    IEnumerator ChangeColor(ColorInfo.PipColor pipColor)
    {
        Debug.Log("ChangeColor");

        CollectionHelpers.DestroyObjects(m_spawnedBooks, true);

        yield return new WaitForSeconds(0.3f);

        string colorName = ColorInfo.GetColorString(pipColor).ToLower();
        
        DataTable dataTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE " + colorName + "='t' ORDER BY fontsize, difficulty");

        int bookIndex = 0;

        foreach (DataRow row in dataTable.Rows)
        {
            if (row["publishable"] != null && row["publishable"].ToString() == "t")
            {
                GameObject bookInstance = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bookPrefab, m_bookGrid.transform);
                bookInstance.name = bookIndex.ToString() + "_Book";
                //NewStoryBrowserBookButton bookButton = bookInstance.GetComponent<NewStoryBrowserBookButton>();
                bookInstance.GetComponent<StoryBrowserBook>().SetUp(row, colorName);
                m_spawnedBooks.Add(bookInstance.GetComponent<StoryBrowserBook>() as StoryBrowserBook);
                //bookButton.SetUpWith(row);
                ++bookIndex;
            }
        }
        
        m_bookGrid.Reposition();
    }
}
