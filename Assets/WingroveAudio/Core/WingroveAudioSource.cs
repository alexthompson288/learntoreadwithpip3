using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Wingrove Audio Source")]
    public class WingroveAudioSource : BaseWingroveAudioSource
    {
        [SerializeField]
        private AudioClip m_audioClip = null;

        public override AudioClip GetAudioClip()
        {
            return m_audioClip;
        }        

        public override void RemoveUsage()
        {
            // do nothing
        }

        public override void AddUsage()
        {
            // do nothing
        }


    }

}