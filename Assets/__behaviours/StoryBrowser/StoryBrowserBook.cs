using UnityEngine;
using System.Collections;
using System;

public class StoryBrowserBook : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_storyPicture;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private PipButton m_infoButton;
    [SerializeField]
    private DataRow m_storyData;


    void Awake()
    {
        m_infoButton.Unpressing += OnClickInfo;
    }

    void OnClickInfo(PipButton button)
    {
        BuyBooksCoordinator.Instance.Show(m_storyData);
    }

    public void SetUp(DataRow dataRow, string colorName)
    {
        m_storyData = dataRow;
        
        m_background.spriteName = "storycover_" + colorName;

        m_label.text = m_storyData["title"].ToString();

        NGUIHelpers.MaxLabelWidth(m_label, 350f);

        m_storyPicture.mainTexture = DataHelpers.GetPicture("stories", m_storyData);

        m_storyPicture.gameObject.SetActive(m_storyPicture.mainTexture != null);
    }

    void OnClick()
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=" + Convert.ToInt32(m_storyData["id"]));
        if(dt.Rows.Count > 0)
        {
            StoryMenuCoordinator.SetStartColor(StoryBrowserCoordinator.Instance.GetCurrentColorName());
            StoryMenuCoordinator.SetStory(dt.Rows[0]);  
            TransitionScreen.Instance.ChangeLevel("NewStoryMenu", false);
        }
    }

    public IEnumerator Off()
    {
        float tweenDuration = 0.25f;
        iTween.ScaleTo(gameObject, Vector3.zero, tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration);
        
        Destroy(gameObject);
    }
}
