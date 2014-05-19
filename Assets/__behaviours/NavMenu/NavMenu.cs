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

	void Awake()
	{
		m_navMenuGoParent.SetActive(true);
        m_mainMoveable.gameObject.SetActive(true);
        m_buyMoveable.gameObject.SetActive(true);
        m_roomMoveable.gameObject.SetActive(true);
        m_parentGate.SetActive(true);
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
		/*
		m_callButton.transform.position = m_callButtonRightPosition.position;

		Transform callButtonOff = m_callButton.GetComponent<TweenOnOffBehaviour>().GetOffLocation();
		Vector3 newOffPosition = callButtonOff.position;
		newOffPosition.x = m_callButtonRightPosition.position.x;
		callButtonOff.position = newOffPosition;
		*/
	}

	public void Call()
	{
		Debug.Log("NavMenu.Call() - isOn: " + m_mainMoveable.IsOn());
		if(m_mainMoveable.IsOn())
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
			m_mainMoveable.Off();
			m_roomMoveable.Off();
			m_buyMoveable.Off();
		}
		else
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			m_mainMoveable.On();
		}
	}

	public void CallRoomMoveable()
	{
		Debug.Log("CallRoomMoveable(): - isOn: " + m_roomMoveable.IsOn());
		CallMoveable(m_roomMoveable);
	}

	public void CallBuyMoveable()
	{
		Debug.Log("CallUserMoveable(): - isOn: " + m_buyMoveable.IsOn());

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
