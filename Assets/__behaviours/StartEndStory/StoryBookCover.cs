using UnityEngine;
using System.Collections;

public class StoryBookCover : MonoBehaviour 
{
	[SerializeField]
	private UITexture m_texture;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories where id=" + SessionInformation.Instance.GetBookId());

		if(dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			string artworkFile = row["storycoverartwork"].ToString();
			
			Texture2D texture = LoaderHelpers.LoadObject<Texture2D>("Images/story_covers/" + artworkFile);
			if (texture != null)
			{
				m_texture.mainTexture = texture;
			}
		}
	
	}
}
