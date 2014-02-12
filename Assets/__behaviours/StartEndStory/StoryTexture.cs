using UnityEngine;
using System.Collections;

public class StoryTexture : MonoBehaviour {
	[SerializeField]
	private UITexture m_texture;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id='" + SessionInformation.Instance.GetBookId() + "' and pageorder='" + 1 + "'");
		
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			
			string imageName = row["image"] == null ? "" : row["image"].ToString().Replace(".png", "");
			
			Texture2D image = LoaderHelpers.LoadObject<Texture2D>("Images/storypages/" + imageName);
			
			if(image != null)
			{
				m_texture.mainTexture = image;
			}
			else
			{
				m_texture.enabled = false;
			}
		}

		/*
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories where id='" + SessionInformation.Instance.GetBookId() + "'");
		
		if (dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			
			string imageName = row["storycoverartwork"] == null ? "" : row["storycoverartwork"].ToString().Replace(".png", "");

			Debug.Log("imageName: " + imageName);
		}
		*/
	}
}
