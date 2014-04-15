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

    DataRow m_currentSession;

    public int sessionId
    {
        get
        {
            return Convert.ToInt32(m_currentSession["id"]);
        }
    }

    public int sessionNum
    {
        get
        {
            return Convert.ToInt32(m_currentSession["number"]);
        }
    }

    void Awake()
    {
        m_pipisodeButton.OnSingleClick += OnClickPipisodeButton;
        m_storyButton.OnSingleClick += OnClickStoryButton;
        m_backBlocker.OnSingleClick += OnClickBackBlocker;
    }

    void OnClickPipisodeButton(ClickEvent click)
    {

    }

    void OnClickStoryButton(ClickEvent click)
    {

    }

    void OnClickBackBlocker(ClickEvent click)
    {
        Off();
    }

	public void On(ColorInfo.PipColor pipColor, int sessionNum)
    {
        Debug.Log("VoyageSessionBoard.On()");
        Debug.Log("sessionNum: " + sessionNum);

        Color color = ColorInfo.GetColor(pipColor);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE number=" + sessionNum);

        Debug.Log(String.Format("Found {0} sessions", dt.Rows.Count));

        if(dt.Rows.Count > 0)
        {
            m_currentSession = dt.Rows[0];

            int sessionId = Convert.ToInt32(m_currentSession["id"]);
            dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + sessionId);
            Debug.Log(String.Format("Found {0} sections for id {1}", dt.Rows.Count, sessionId));
            if(dt.Rows.Count > 0)
            {
                for(int i = 0; i < dt.Rows.Count && i < m_gameButtons.Length; ++i)
                {
                    m_gameButtons[i].On(dt.Rows[i], color);
                }
            }

            bool enablePipisodeButton = m_currentSession["pipisode_id"] != null && Convert.ToInt32(m_currentSession["pipisode_id"]) != 0;
            m_pipisodeButton.gameObject.SetActive(enablePipisodeButton);

            bool enableStoryButton = m_currentSession["story_id"] != null && Convert.ToInt32(m_currentSession["story_id"]) != 0;
            m_storyButton.gameObject.SetActive(enableStoryButton);

            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
            Hashtable tweenArgs = new Hashtable();
            tweenArgs.Add("position", m_moveableOn);
            tweenArgs.Add("time", m_tweenDuration);
            tweenArgs.Add("easetype", iTween.EaseType.easeOutBounce);
            iTween.MoveTo(m_moveable, tweenArgs);
        }
    }

    public void Off()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_moveableOff);
        tweenArgs.Add("time", m_tweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        iTween.MoveTo(m_moveable, tweenArgs);

        m_currentSession = null;
    }
}
