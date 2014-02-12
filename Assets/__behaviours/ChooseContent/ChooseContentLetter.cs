using UnityEngine;
using System.Collections;

public class ChooseContentLetter : ChooseContentButton 
{	
	static ChooseContentLetter m_targetLetter = null;

	public void SetUp (DataRow data) 
	{
		m_data = data;
		
		m_label.text = m_data["phoneme"].ToString();

		if(ContentInformation.Instance.IsTargetLetter(m_data))
		{
			m_targetLetter = this;
			m_background.color = Color.red;
		}
		else if(ContentInformation.Instance.HasLetter(m_data))
		{
			m_background.color = Color.grey;
		}
	}

	void OnClick()
	{
		if(ContentInformation.Instance.IsTargetLetter(m_data))
		{
			ContentInformation.Instance.RemoveLetter(m_data, true);
			ContentInformation.Instance.SetTargetLetter(null);
			m_targetLetter = null;
			m_background.color = Color.white;
		}
		else if(ContentInformation.Instance.HasLetter(m_data))
		{
			if(m_targetLetter != null)
			{
				m_targetLetter.m_background.color = Color.gray;
			}
			m_targetLetter = this;
			ContentInformation.Instance.SetTargetLetter(m_data);
			m_background.color = Color.red;
		}
		else
		{
			ContentInformation.Instance.AddLetter(m_data, true);
			m_background.color = Color.gray;
		}

		CallOnButtonClick();
	}

	public override bool CheckData()
	{
		return ContentInformation.Instance.HasLetter(m_data);
	}

	public override void AddData()
	{
		if(!ContentInformation.Instance.IsTargetLetter(m_data))
		{
			ContentInformation.Instance.AddLetter(m_data);
			m_background.color = Color.gray;
		}
	}
	
	public override void RemoveData()
	{
		if(ContentInformation.Instance.IsTargetLetter(m_data))
		{
			ContentInformation.Instance.SetTargetLetter(null);
		}

		ContentInformation.Instance.RemoveLetter(m_data);
		m_background.color = Color.white;
	}
}
