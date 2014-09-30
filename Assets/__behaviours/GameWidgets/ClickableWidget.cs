using UnityEngine;
using System.Collections;

public class ClickableWidget : MonoBehaviour 
{
	public delegate void WidgetClick (ClickableWidget behaviour);
	public event WidgetClick OnWidgetClick;

	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private string m_dataType;
	[SerializeField]
	private string[] m_offSpriteNames;
	[SerializeField]
	private string [] m_onSpriteNames;
	[SerializeField]
	private bool m_linkOnOffIndex = true;
	
	DataRow m_data = null;

	void Start()
	{
		m_background.spriteName = m_offSpriteNames[Random.Range(0, m_offSpriteNames.Length)];
	}
	
	public void SetUp (DataRow data) 
	{
		m_data = data;

		if(m_data != null)
		{
			string labelData = "word";
			if(m_dataType == "phonemes")
			{
				labelData = "phoneme";	
			}

			try
			{
				m_label.text = m_data[labelData].ToString();
			}
			catch
			{
				////////D.LogError("SetUp failed");
				////////D.Log("data: " + m_data);
				////////D.Log("id: " + m_data["id"].ToString());
				if(m_dataType == "phonemes")
				{
					////////D.Log("phoneme: " + m_data["phoneme"].ToString());
				}
				else
				{
					////////D.Log("word: " + m_data["word"].ToString());
				}
			}
		}
		else
		{
			////////D.LogError("NO DATA!");
		}
	}
	
	void OnClick()
	{
		if(OnWidgetClick != null)
		{
			OnWidgetClick(this);
		}
	}

	public void On()
	{
		OnOff(m_offSpriteNames, m_onSpriteNames);
	}

	public void Off()
	{
		OnOff(m_onSpriteNames, m_offSpriteNames);
	}

	void OnOff(string[] from, string[] to)
	{
		if(m_linkOnOffIndex)
		{
			int spriteIndex = System.Array.IndexOf(from, m_background.spriteName);
			if(spriteIndex != -1)
			{
				m_background.spriteName = to[spriteIndex];
			}
		}
		else
		{
			m_background.spriteName = to[Random.Range(0, to.Length)];
		}
	}
	
	public DataRow GetData()
	{
		return m_data;
	}

	public void EnableWidgets(bool enable)
	{
		m_label.enabled = enable;
		m_background.enabled = enable;
	}
}
