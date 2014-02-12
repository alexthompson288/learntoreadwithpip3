using UnityEngine;
using System.Collections;

public class StoryMenuBackground : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_bgTexture;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		//DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + SessionInformation.Instance.GetBookId() + "' and pageorder='" + 1 + "'");
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories where id='" + SessionInformation.Instance.GetBookId() + "'");

		Debug.Log("bookId: " + SessionInformation.Instance.GetBookId());

		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];

			Debug.Log("attribute: " + row["backgroundart"]);

			string bgImageName = row["backgroundart"] == null ? "" : row["backgroundart"].ToString().Replace(".png", "");

			Debug.Log("bgImageName: " + bgImageName);

			Texture2D bgImage = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + bgImageName);

			if(bgImage != null)
			{
				m_bgTexture.mainTexture = bgImage;
			}
		}
		else
		{
			Debug.Log("No story");
		}
	}
}
