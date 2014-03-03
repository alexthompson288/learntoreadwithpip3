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
	private Transform m_callButtonRightPosition;
	[SerializeField]
	private GameObject m_navMenuGoParent;

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
		m_callButton.transform.position = m_callButtonRightPosition.position;
	}

	public void Call()
	{
		Debug.Log("isOn: " + m_mainMoveable.IsOn());
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
		CallMoveable(m_roomMoveable);
	}

	public void CallUserMoveable()
	{
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
