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

        DataRow story = StoryMenuCoordinator.story;

        if(story != null)
        {
            Debug.Log("bgImageName: " + story["backgroundart"]);

			string bgImageName = story["backgroundart"] == null ? "" : story["backgroundart"].ToString().Replace(".png", "");

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
