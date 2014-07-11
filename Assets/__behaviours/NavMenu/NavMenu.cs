using UnityEngine;
using System.Collections;
using Wingrove;

public class NavMenu : Singleton<NavMenu> 
{
	[SerializeField]
	private TweenOnOffBehaviour m_mainMoveable;
	[SerializeField]
	private TweenOnOffBehaviour m_roomMoveable;
	[SerializeField]
	private TweenOnOffBehaviour m_buyMoveable;
	[SerializeField]
	private GameObject m_callButton;
	[SerializeField]
	private GameObject m_callButtonRight;
	[SerializeField]
	private GameObject m_navMenuGoParent;
	[SerializeField]
	private UILabel m_buyAllBooksLabel;
    [SerializeField]
    private GameObject m_parentGate;
    [SerializeField]
    private GameObject m_pipisodesButton;
    [SerializeField]
    private Transform m_gamesButton;
    [SerializeField]
    private Transform m_storiesButton;
    [SerializeField]
    private Collector m_pipAnim;
    [SerializeField]
    private TweenOnOffBehaviour m_infoMoveable;
    [SerializeField]
    private PipButton m_callInfoMoveable;
    [SerializeField]
    private ClickEvent m_dismissInfoMoveable;

	void Awake()
	{
        m_callInfoMoveable.Unpressing += CallInfoMoveable;
        m_dismissInfoMoveable.SingleClicked += DismissInfoMoveable;

		m_navMenuGoParent.SetActive(true);
        m_mainMoveable.gameObject.SetActive(true);
        m_buyMoveable.gameObject.SetActive(true);
        m_roomMoveable.gameObject.SetActive(true);
        m_parentGate.SetActive(true);

#if UNITY_STANDALONE
        if(m_pipisodesButton != null)
        {
            m_pipisodesButton.SetActive(false);
            m_gamesButton.localPosition = new Vector3(-120, m_gamesButton.localPosition.y, m_gamesButton.localPosition.z);
            m_storiesButton.localPosition = new Vector3(120, m_storiesButton.localPosition.y, m_storiesButton.localPosition.z);
        }
#endif
	}

	void Start()
	{
		if(SessionInformation.Instance.GetNumPlayers() == 2) 
		{
			m_callButton.SetActive(false);
		}
	}

	public void HideCallButton()
	{
		m_callButton.SetActive(false);
	}

	public void MoveCallButtonToRight()
	{
		m_callButton.SetActive(false);
		m_callButtonRight.SetActive(true);
	}

    public void CallInfoMoveable(PipButton button)
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        m_infoMoveable.On();
    }

    public void DismissInfoMoveable(ClickEvent click)
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        m_infoMoveable.Off();
    }

	public void Call()
	{
		if(m_mainMoveable.IsOn())
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
			m_mainMoveable.Off();
			m_roomMoveable.Off();
			m_buyMoveable.Off();

            m_infoMoveable.Off();

            m_pipAnim.StopAnim();
		}
		else
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			m_mainMoveable.On();

            m_pipAnim.StartAnim();
		}
	}

	public void CallRoomMoveable()
	{
		CallMoveable(m_roomMoveable);
	}

	public void CallBuyMoveable()
	{
		m_buyAllBooksLabel.text = System.String.Format("Unlock All {0} Books - £19.99", BuyManager.Instance.numBooks);

		CallMoveable(m_buyMoveable);
	}

	public void CallMoveable(TweenOnOffBehaviour moveable)
	{
		if(moveable.IsOn())
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
			moveable.Off();
		}
		else
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			moveable.On();
		}
	}

}
