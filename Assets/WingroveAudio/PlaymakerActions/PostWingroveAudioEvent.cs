// to use WingroveAudio with Playmaker, uncomment the following line
//#define USE_PLAYMAKER
#if USE_PLAYMAKER
using UnityEngine;
using HutongGames.PlayMaker;
using WingroveAudio;

[ActionCategory("WingroveAudio")]
public class PostWingroveAudioEvent : FsmStateAction
{

    public FsmString m_audioEventToPost;
    public FsmGameObject m_gameObjectToLinkTo;

	// Code that runs on entering the state.
	public override void OnEnter()
	{
        if (WingroveRoot.Instance != null)
        {
            if (m_gameObjectToLinkTo.IsNone )
            {
                WingroveRoot.Instance.PostEvent(m_audioEventToPost.Value);
            }
            else
            {
                WingroveRoot.Instance.PostEventGO(m_audioEventToPost.Value, m_gameObjectToLinkTo.Value);
            }
            
        }
		Finish();
	}


}
#endif