using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChooseContentSet : MonoBehaviour 
{
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private UILabel m_label;

	List<ChooseContentButton> m_buttons = new List<ChooseContentButton>();

	public void AddButton (ChooseContentButton newButton) 
	{
		m_buttons.Add(newButton);
		newButton.OnButtonClick += OnButtonClick;
	}

	void OnButtonClick ()
	{
		if(CheckSet())
		{
			m_background.color = Color.gray;
		}
		else
		{
			m_background.color = Color.white;
		}
	}

	void OnClick ()
	{
		if(CheckSet())
		{
			DeselectButtons();
			m_background.color = Color.white;
		}
		else
		{
			SelectButtons();
			m_background.color = Color.gray;
		}
	}

	void SelectButtons()
	{
		foreach(ChooseContentButton button in m_buttons)
		{
			button.AddData();
		}
		ContentInformation.Instance.Save();

		m_background.color = Color.gray;
	}

	void DeselectButtons()
	{
		foreach(ChooseContentButton button in m_buttons)
		{
			button.RemoveData();
		}
		ContentInformation.Instance.Save();

		m_background.color = Color.white;
	}

	public void SetUp(int setId, int layer = -1)
	{
		m_label.text = "Set " + setId.ToString();

		if(layer != -1)
		{
			gameObject.layer = layer;
			m_label.gameObject.layer = layer;
			m_label.transform.parent.gameObject.layer = layer;
			m_background.gameObject.layer = layer;
		}

		if(CheckSet())
		{
			m_background.color = Color.grey;
		}
	}

	bool CheckSet()
	{
		bool allSelected = true;
		
		foreach(ChooseContentButton button in m_buttons)
		{
			if(!button.CheckData())
			{
				allSelected = false;
			}
		}

		return allSelected;
	}

	public void SetButtonsActive(bool isActive)
	{
		if(gameObject.activeInHierarchy != isActive)
		{
			gameObject.SetActive(isActive);

			foreach(ChooseContentButton button in m_buttons)
			{
				button.gameObject.SetActive(isActive);
			}
		}
	}
}
