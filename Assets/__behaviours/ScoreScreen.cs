﻿using UnityEngine;
using System.Collections;

public class ScoreScreen : MonoBehaviour {

    [SerializeField]
    private GameObject m_onePlayerHierarchy;
    [SerializeField]
    private GameObject m_twoPlayerHierarchy;

    [SerializeField]
    private string[] m_videoFilenames;

	// Use this for initialization
	IEnumerator Start () 
    {
        m_onePlayerHierarchy.SetActive(true);

        int winningIndex = SessionInformation.Instance.GetWinningPlayerIndex();

        MobileMovieTexture[] moviePlayers = GetComponents<MobileMovieTexture>();
        moviePlayers[0]
            .SetFilename(m_videoFilenames[SessionInformation.Instance.GetPlayerIndexForPlayer(winningIndex)]);

        moviePlayers[0].Play();

        WingroveAudio.WingroveRoot.Instance.PostEvent
    ("VICTORY_DANCE_" + SessionInformation.Instance.GetPlayerIndexForPlayer(winningIndex));

        SessionInformation.Instance.SetHighestLevelCompletedForGame(
            SessionInformation.Instance.GetSelectedGame(),
            SessionInformation.Instance.GetDifficulty() + 1);
		
		SessionInformation.Instance.SetStarsEarnedForHighestLevel(SessionInformation.Instance.GetDifficulty(), SessionInformation.Instance.GetStarsEarnedForHighestLevel() + 1);
		SessionInformation.Instance.SetCoins(SessionInformation.Instance.GetDifficulty(), SessionInformation.Instance.GetCoins() + 1);
		SessionInformation.Instance.SetHasWonRecently(true);
		SessionInformation.Instance.SetHasEverWonCoin(true);

		TTInformation.Instance.SetGoldCoins(TTInformation.Instance.GetGoldCoins() + 1);

		Debug.Log("Just about to increment skill progress");
		SkillProgressInformation.Instance.IncrementCurrentStarSkillProgress();
		
        //yield return new WaitForSeconds(1.0f);

        //WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");

        yield return new WaitForSeconds(1.0f);

        if (SessionInformation.Instance.GetNumPlayers() == 2)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent
                ("PLAYER_" + SessionInformation.Instance.GetPlayerIndexForPlayer(winningIndex) + "_WINS");
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("WIN_SINGLE");            
        }

        yield return new WaitForSeconds(15.0f);

        TransitionScreen.Instance.GoBack();
	}
}
