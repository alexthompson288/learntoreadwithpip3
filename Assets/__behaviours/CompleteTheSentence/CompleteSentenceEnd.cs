using UnityEngine;
using System.Collections;

public class CompleteSentenceEnd : MonoBehaviour {


	[SerializeField]
	private string[] m_videoFilenames;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		// always pip, always winner
		SessionInformation.Instance.SetPlayerIndex(0, 3);
		SessionInformation.Instance.SetWinner(0);

		int winningIndex = SessionInformation.Instance.GetWinningPlayerIndex();

		MobileMovieTexture[] moviePlayers = GetComponents<MobileMovieTexture>();
		moviePlayers[0]
		.SetFilename(m_videoFilenames[SessionInformation.Instance.GetPlayerIndexForPlayer(winningIndex)]);
		
		moviePlayers[0].Play();
		
		WingroveAudio.WingroveRoot.Instance.PostEvent
			("VICTORY_DANCE_" + SessionInformation.Instance.GetPlayerIndexForPlayer(winningIndex));
		
		yield return new WaitForSeconds(1.0f);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
		
		yield return new WaitForSeconds(1.0f);
		

		WingroveAudio.WingroveRoot.Instance.PostEvent("WIN_SINGLE");            
		
		yield return new WaitForSeconds(15.0f);
		
		TransitionScreen.Instance.ChangeLevel("NewStoryBrowser", false);
	}
}
