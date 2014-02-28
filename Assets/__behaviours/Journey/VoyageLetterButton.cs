using UnityEngine;
using System.Collections;
using Wingrove;

public class VoyageLetterButton : MonoBehaviour 
{
	[SerializeField]
	private int m_id;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private UILabel m_graphemeLabel;
	[SerializeField]
	private UITexture m_mnemonicTexture;
	[SerializeField]
	private string m_mainColorString = "[333333]";
	[SerializeField]
	private string m_highlightColorString = "[FF0000]";

	DataRow m_data;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());	

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id=" + m_id);

		if(dt.Rows.Count > 0)
		{
			m_data = dt.Rows[0];

			string spriteName = AlphabetBookInformation.Instance.GetTexture(System.Convert.ToInt32(m_data["id"]));
			if(spriteName != null)
			{
				m_background.spriteName = spriteName;
			}
			else
			{
				m_mnemonicTexture.color = Color.black;
				m_graphemeLabel.color = Color.black;
			}

			string graphemeText = m_data["phoneme"].ToString();
			m_graphemeLabel.text = graphemeText;

			if(graphemeText.Length > 2)
			{
				m_graphemeLabel.transform.parent.localScale = Vector3.one * 1.5f;
			}
			else if(graphemeText.Length > 1)
			{
				m_graphemeLabel.transform.parent.localScale = Vector3.one * 2f;
			}

			string mnemonicText = m_data["mneumonic"].ToString();

			string imageFilename =
				string.Format("Images/mnemonics_images_png_250/{0}_{1}",
				              graphemeText,
				              mnemonicText.Replace(" ", "_"));
			
			Texture2D tex = (Texture2D)Resources.Load(imageFilename);
			if(tex != null)
			{
				m_mnemonicTexture.mainTexture = tex;
			}
			else
			{
				m_mnemonicTexture.enabled = false;
			}
		}
		else
		{
			gameObject.SetActive(false);
		}

		if(JourneyInformation.Instance.GetLastLetterUnlocked() == System.Convert.ToInt32(m_data["id"]))
		{
			yield return new WaitForSeconds(1f);
			Vector3 originalScale = transform.localScale;
			iTween.ScaleTo(gameObject, transform.localScale * 2, 0.8f);
			yield return new WaitForSeconds(0.8f);
			WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
			iTween.PunchPosition(gameObject, new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f), 1f);
			yield return new WaitForSeconds(1f);
			iTween.ScaleTo(gameObject, originalScale, 0.5f);

			JourneyInformation.Instance.SetLastLetterUnlocked(null);
		}
	}

	void OnClick()
	{
		StartCoroutine(OnClickCo());
	}

	IEnumerator OnClickCo()
	{
		yield return new WaitForSeconds(0.3f);
		JourneyCoordinator.Instance.OnClickLetter(m_data);
	}

	void OnDoubleClick()
	{
		StopAllCoroutines();
		JourneyCoordinator.Instance.OnDoubleClickLetter(m_data);
	}
	
	void OnDestroy()
	{
		Texture2D tex = (Texture2D)m_mnemonicTexture.mainTexture;
		m_mnemonicTexture.mainTexture = null;
		Resources.UnloadAsset(tex);
	}
}
