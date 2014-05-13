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
    private VoyageGameButton[] m_gameButtons;
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
        m_pipisodeButton.OnSingleClick += OnClickPipisodeButton;
        m_storyButton.OnSingleClick += OnClickStoryButton;
        m_backBlocker.OnSingleClick += OnClickBackBlocker;
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
                GameManager.Instance.ClearAllData();
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

        VoyageCoordinator.Instance.SetSessionId(Convert.ToInt32(m_session ["id"]));

        Color color = ColorInfo.GetColor(VoyageCoordinator.Instance.currentColor);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }

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

        bool enablePipisodeButton = m_session["pipisode_id"] != null && Convert.ToInt32(m_session["pipisode_id"]) != 0;
        m_pipisodeButton.gameObject.SetActive(enablePipisodeButton);

        bool enableStoryButton = m_session["story_id"] != null && Convert.ToInt32(m_session["story_id"]) != 0;
        m_storyButton.gameObject.SetActive(enableStoryButton);

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
