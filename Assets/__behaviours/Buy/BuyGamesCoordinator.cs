using UnityEngine;
using System.Collections;

public class BuyGamesCoordinator : BuyCoordinator<BuyGamesCoordinator> 
{
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private ClickEvent m_backCollider;

	void Awake()
	{
		m_backCollider.OnSingleClick += OnClickBackCollider;
	}

	public void Show()
	{
		DisableUICams();
		m_tweenBehaviour.On(false);
	}

	public void OnClickBackCollider(ClickEvent click)
	{
		m_tweenBehaviour.Off(false);
		EnableUICams();
	}
}
