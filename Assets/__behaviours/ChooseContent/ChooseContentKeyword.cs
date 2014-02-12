using UnityEngine;
using System.Collections;

public class ChooseContentKeyword : ChooseContentButton 
{
	static ChooseContentKeyword m_targetKeyword;
	
	public void SetUp (DataRow data) 
	{
		m_data = data;
		
		m_label.text = m_data["word"].ToString();
		
		if(ContentInformation.Instance.IsTargetKeyword(m_data))
		{
			m_targetKeyword = this;
			//m_background.color = Color.red;
		}
		else if(ContentInformation.Instance.HasKeyword(m_data))
		{
			//m_background.color = Color.grey;
		}
	}
	
	void OnClick()
	{
		if(ContentInformation.Instance.IsTargetKeyword(m_data))
		{
			ContentInformation.Instance.RemoveKeyword(m_data);
			ContentInformation.Instance.SetTargetKeyword(null);
			m_targetKeyword = null;
			//m_background.color = Color.white;
		}
		if(ContentInformation.Instance.HasKeyword(m_data))
		{
			if(m_targetKeyword != null)
			{
				//m_targetKeyword.m_background.color = Color.gray;
			}
			m_targetKeyword = this;
			ContentInformation.Instance.SetTargetKeyword(m_data);
			//m_background.color = Color.red;
		}
		else
		{
			ContentInformation.Instance.AddKeyword(m_data, true);
			//m_background.color = Color.gray;
		}
		
		CallOnButtonClick();
	}
	
	public override bool CheckData()
	{
		return ContentInformation.Instance.HasKeyword(m_data);
	}
	
	public override void AddData()
	{
		if(!ContentInformation.Instance.IsTargetKeyword(m_data))
		{
			ContentInformation.Instance.AddKeyword(m_data);
			//m_background.color = Color.gray;
		}
	}
	
	public override void RemoveData()
	{
		if(ContentInformation.Instance.IsTargetKeyword(m_data))
		{
			ContentInformation.Instance.SetTargetKeyword(null);
		}
		
		ContentInformation.Instance.RemoveKeyword(m_data);
		//m_background.color = Color.white;
	}
}
