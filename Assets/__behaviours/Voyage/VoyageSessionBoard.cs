using UnityEngine;
using System.Collections;
using System;
using Wingrove;

public class VoyageSessionBoard : Singleton<VoyageSessionBoard> 
{
    [SerializeField]
    private GameObject m_moveable;
    [SerializeField]
    private Transform m_moveableOn;
    [SerializeField]
    private Transform m_moveableOff;
    [SerializeField]
    private float m_tweenDuration;
    [SerializeField]
    private Transform m_gameButtonParent;
    [SerializeField]
    private PipButton m_gameButton;
    [SerializeField]
    private ClickEvent m_pipisodeButton;
    [SerializeField]
    private ClickEvent m_storyButton;

    [SerializeField]
    private ClickEvent m_backBlocker;
    [SerializeField]
    private UIWidget[] m_widgetsToColor;

    DataRow m_session = null;

    void Awake()
    {
        m_gameButton.Unpressing += OnClickGameButton;
        m_pipisodeButton.OnSingleClick += OnClickPipisodeButton;
        m_storyButton.OnSingleClick += OnClickStoryButton;
        m_backBlocker.OnSingleClick += OnClickBackBlocker;
    }

    void OnClickGameButton(PipButton button)
    {
        VoyageCoordinator.Instance.StartGames(Convert.ToInt32(m_session ["id"]));
    }

    void OnClickPipisodeButton(ClickEvent click)
    {
        if (m_session ["pipisode_id"] != null)
        {
            int pipisodeId = Convert.ToInt32(m_session["pipisode_id"]);
            PipisodeManager.Instance.PlayPipisode(pipisodeId);
        }
    }

    void OnClickStoryButton(ClickEvent click)
    {
        if (m_session ["story_id"] != null)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=" + Convert.ToInt32(m_session["story_id"]));

            if(dt.Rows.Count > 0)
            {
                GameManager.Instance.AddData("stories", dt.Rows);
                TransitionScreen.Instance.ChangeLevel("NewStoryMenu", false);
            }
        }
    }

    void OnClickBackBlocker(ClickEvent click)
    {
        Off();
    }

	public void On(DataRow session)
    {
        m_session = session;


        Color color = ColorInfo.GetColor(VoyageCoordinator.Instance.currentColor);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }

        /*
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + Convert.ToInt32(m_session["id"]));
        Debug.Log(String.Format("Found {0} sections for id {1}", dt.Rows.Count, Convert.ToInt32(m_session["id"])));

        for(int i = 0; i < m_gameButtons.Length; ++i)
        {
            bool hasData = (i < dt.Rows.Count);

            m_gameButtons[i].gameObject.SetActive(hasData);

            if(hasData)
            {
                m_gameButtons[i].On(dt.Rows[i], color);
            }
        }
        */

        bool foundPipisode = m_session ["pipisode_id"] != null && Convert.ToInt32(m_session ["pipisode_id"]) > 0;
        bool foundStory = m_session ["story_id"] != null && Convert.ToInt32(m_session ["story_id"]) > 0;

        m_pipisodeButton.gameObject.SetActive(foundPipisode);
        m_storyButton.gameObject.SetActive(foundStory);

        int numButtons = 1;
        numButtons += foundPipisode ? 1 : 0;
        numButtons += foundStory ? 1 : 0;

        Debug.Log("Num Buttons: " + numButtons);

        float horizontalOffset = 255;

        switch (numButtons)
        {
            case 1:
                m_gameButtonParent.localPosition = Vector3.zero;
                break;
            case 2:
                m_gameButtonParent.localPosition = new Vector3(-horizontalOffset / 2, 0, 0);

                Transform activeButton = foundPipisode ? m_pipisodeButton.transform : m_storyButton.transform;
                activeButton.localPosition = new Vector3(horizontalOffset / 2, 0, 0);
                break;
            case 3:
                m_gameButtonParent.localPosition = new Vector3(-horizontalOffset, 0, 0);
                m_pipisodeButton.transform.localPosition = Vector3.zero;
                m_storyButton.transform.localPosition = new Vector3(horizontalOffset, 0, 0);
                break;
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_moveableOn);
        tweenArgs.Add("time", m_tweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.easeOutBounce);
        iTween.MoveTo(m_moveable, tweenArgs);
    }

    public void Off()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_moveableOff);
        tweenArgs.Add("time", m_tweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        iTween.MoveTo(m_moveable, tweenArgs);
    }
}
