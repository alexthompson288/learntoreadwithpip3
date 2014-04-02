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
	private TweenOnOffBehaviour m_userMoveable;
	[SerializeField]
	private GameObject m_callButton;
	[SerializeField]
	private GameObject m_callButtonRight;
	[SerializeField]
	private GameObject m_navMenuGoParent;
	[SerializeField]
	private UILabel m_buyAllBooksLabel;

	void Awake()
	{
		m_navMenuGoParent.SetActive(true);
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
			m_userMoveable.Off();
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

	public void CallUserMoveable()
	{
		Debug.Log("CallUserMoveable(): - isOn: " + m_userMoveable.IsOn());

		m_buyAllBooksLabel.text = System.String.Format("Unlock All {0} Books - £19.99", BuyManager.Instance.numBooks);

		CallMoveable(m_userMoveable);
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
