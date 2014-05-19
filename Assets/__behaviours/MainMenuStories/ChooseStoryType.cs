using UnityEngine;
using System.Collections;

public class ChooseStoryType : MonoBehaviour 
{
    [SerializeField]
    ColorInfo.PipColor m_storyType;
    [SerializeField]
    private UISprite m_sprite;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private PipButton m_button;

    void Start()
    {
        m_button.SetPipColor(m_storyType, true);
        //m_sprite.color = ColorInfo.GetColor(m_storyType);
        m_label.text = ColorInfo.GetColorString(m_storyType);

        m_button.Pressing += OnButtonClick;
    }

    void OnButtonClick(PipButton button)
    {
        SessionInformation.Instance.SetStoryType(ColorInfo.GetColorString(m_storyType)); // Use strings for story type just in case we want to add non-color types (eg. Classics)
        
        Debug.Log("Set Story Type: " + SessionInformation.Instance.GetStoryType());
        
        #if UNITY_IPHONE
        //      System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
        //      ep.Add("StoryType", m_storyType);
        //      FlurryBinding.logEventWithParameters("ChooseStoryType", ep, false);
        #endif

        TransitionScreen.Instance.ChangeLevel("NewStoryBrowser", true);
    }
}
