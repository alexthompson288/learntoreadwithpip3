using UnityEngine;
using System.Collections;

public class AudioBankManager : Singleton<AudioBankManager> {

    [SerializeField]
    private AudioBank[] m_audioBanks;


    public AudioClip GetAudioClip(string name)
    {
        AudioClip found = null;
        foreach (AudioBank ab in m_audioBanks)
        {
            found = ab.GetClip(name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
