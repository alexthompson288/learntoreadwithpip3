using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Wingrove Resources Audio Source")]
    public class WingroveResourcesAudioSource : BaseWingroveAudioSource
    {
        [SerializeField]
        private string m_audioClipResourceName = "";
        [SerializeField]
        private float m_holdResource = 0.0f;
        [SerializeField]
        private bool m_onlyUnloadOnLevelChange = false;
        [SerializeField]
        private bool m_useStreamLoader = false;

        void Update()
        {
            if (m_users == 0)
            {
                m_timer -= WingroveRoot.GetDeltaTime();
                if (m_timer < 0)
                {
                    if (!m_onlyUnloadOnLevelChange)
                    {
                        Unload();
                    }
                }
            }
            UpdateInternal();
        }

        private AudioClip m_audioClip;

        private int m_users;
        private float m_timer;

        public override AudioClip GetAudioClip()
        {
            return m_audioClip;
        }

        void OnDestroy()
        {
            Unload();
        }

        void Load()
        {
            if (m_useStreamLoader)
            {
                GameObject streamLoader = (GameObject)Resources.Load(m_audioClipResourceName + "_SL");
                StreamLoader slComp = streamLoader.GetComponent<StreamLoader>();
                m_audioClip = slComp.m_referencedAudioClip;
            }
            else
            {
                m_audioClip = (AudioClip)Resources.Load(m_audioClipResourceName);
            }
        }

        void Unload()
        {
            Resources.UnloadAsset(m_audioClip);
            m_audioClip = null;
        }

        void OnLevelWasLoaded(int level)
        {
            if (m_onlyUnloadOnLevelChange)
            {
                Unload();
            }
        }

        public override void RemoveUsage()
        {
            m_users--;
            if (m_users <= 0)
            {
                m_users = 0;
                if (m_holdResource == 0.0f)
                {
                    if (!m_onlyUnloadOnLevelChange)
                    {
                        Unload();
                    }
                }
                else
                {
                    m_timer = m_holdResource;
                }
            }
        }

        public override void AddUsage()
        {
            if (m_audioClip == null)
            {
                Load();
            }
            m_users++;
        }

    }

}