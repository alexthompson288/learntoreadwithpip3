using UnityEngine;
using System.Collections;

public class ChooseContentWord : ChooseContentButton 
{
	static ChooseContentWord m_targetWord;

	public void SetUp (DataRow data) 
	{
		m_data = data;
		
		m_label.text = m_data["word"].ToString();

		if(m_label.text.Length > 5)
		{
			m_label.transform.localScale *= 0.75f;
		}

		if(ContentInformation.Instance.IsTargetWord(m_data))
		{
			m_targetWord = this;
			m_background.color = Color.red;
		}
		else if(ContentInformation.Instance.HasWord(m_data))
		{
			m_background.color = Color.grey;
		}
	}

	void OnClick()
	{
		if(ContentInformation.Instance.IsTargetWord(m_data))
		{
			ContentInformation.Instance.RemoveWord(m_data, true);
			ContentInformation.Instance.SetTargetWord(null);
			m_targetWord = null;
			m_background.color = Color.white;
		}
		else if(ContentInformation.Instance.HasWord(m_data))
		{
			if(m_targetWord != null)
			{
				m_targetWord.m_background.color = Color.gray;
			}
			m_targetWord = this;
			ContentInformation.Instance.SetTargetWord(m_data);
			m_background.color = Color.red;
		}
		else
		{
			ContentInformation.Instance.AddWord(m_data, true);
			m_background.color = Color.gray;
		}

		CallOnButtonClick();
	}

	public override bool CheckData()
	{
		return ContentInformation.Instance.HasWord(m_data);
	}

	public override void AddData()
	{
		if(!ContentInformation.Instance.IsTargetWord(m_data))
		{
			ContentInformation.Instance.AddWord(m_data);
			m_background.color = Color.gray;
		}
	}

	public override void RemoveData()
	{
		if(ContentInformation.Instance.IsTargetWord(m_data))
		{
			ContentInformation.Instance.SetTargetWord(null);
		}

		ContentInformation.Instance.RemoveWord(m_data);
		m_background.color = Color.white;
	}
}
