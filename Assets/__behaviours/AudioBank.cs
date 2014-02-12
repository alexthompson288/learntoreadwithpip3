using UnityEngine;
using System.Collections;

public class AudioBank : MonoBehaviour {

    [SerializeField]
    private AudioClip[] m_audioBankClips;

    public AudioClip GetClip(string name)
    {
        foreach (AudioClip ac in m_audioBankClips)
        {
            if (ac.name == name)
            {
                return ac;
            }
        }
        return null;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
