using UnityEngine;
using System.Collections;

public class StoryBanner : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_titleLabel;
	[SerializeField]
	private UILabel m_authorLabel;


	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories where id='" + SessionInformation.Instance.GetBookId() + "'");
		
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			
			m_titleLabel.text = row["title"].ToString();

			m_authorLabel.text = ("by " + row["author"].ToString());

			Debug.Log("width: " + m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x);

			if(m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 1400)
			{
				m_titleLabel.transform.localScale = Vector3.one * 0.35f;
			}
			else if(m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 1300)
			{
				m_titleLabel.transform.localScale = Vector3.one * 0.4f;
			}
			else if(m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 1000)
			{
				m_titleLabel.transform.localScale = Vector3.one * 0.5f;
			}
			else if(m_titleLabel.font.CalculatePrintedSize(m_titleLabel.text, false, UIFont.SymbolStyle.None).x > 500)
			{
				m_titleLabel.transform.localScale = Vector3.one * 0.75f;
			}


			Debug.Log("Title Scale: " + m_titleLabel.transform.localScale);
		}
	}
}
