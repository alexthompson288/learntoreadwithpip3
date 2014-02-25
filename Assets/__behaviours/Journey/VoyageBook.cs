using UnityEngine;
using System.Collections;

public class VoyageBook : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_texture;

	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		int sessionNum = JourneyInformation.Instance.GetSessionsCompleted();
		//int sessionNum = 55;

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE number=" + sessionNum);

		if(dt.Rows.Count > 0 && dt.Rows[0]["story_id"] != null)
		{
			int storyId =System.Convert.ToInt32(dt.Rows[0]["story_id"]);

			dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=" + storyId);

			if(dt.Rows.Count > 0)
			{
				string imageName = dt.Rows[0]["storycoverartwork"] == null ? "" : dt.Rows[0]["storycoverartwork"].ToString().Replace(".png", "");
				Texture2D tex = LoaderHelpers.LoadObject<Texture2D>("Images/story_covers/" + imageName);
				if (tex != null)
				{
					m_texture.mainTexture = tex;
				}
			}
		}

		if(m_texture.mainTexture == null)
		{
			Debug.Log("Disabling story UITexture because tex is null");
			m_texture.enabled = false;
		}
	}
}
