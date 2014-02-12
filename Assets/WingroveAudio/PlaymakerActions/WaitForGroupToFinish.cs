// to use WingroveAudio with Playmaker, uncomment the following line
//#define USE_PLAYMAKER
#if USE_PLAYMAKER
using UnityEngine;
using HutongGames.PlayMaker;
using WingroveAudio;

[ActionCategory("WingroveAudio")]
public class WaitForGroupToFinish : FsmStateAction
{

    public WingroveGroupInformation m_informationGroup;

    public override void OnUpdate()
    {
        if (WingroveRoot.Instance != null)
        {            
            if (!m_informationGroup.IsAnyPlaying())
            {
                Finish();
            }
            
        }
    }

}
#endif