using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobileMovieTexture))]
public class TestMobileTexture : MonoBehaviour 
{
    private MobileMovieTexture m_movieTexture;

    void Awake()
    {
        m_movieTexture = GetComponent<MobileMovieTexture>();

        m_movieTexture.onFinished += OnFinished;
    }

    void OnFinished(MobileMovieTexture sender)
    {
        Debug.Log(sender.Path + " has finished ");
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), m_movieTexture.isPlaying ? "Pause" : "Play" ))
        {
            if (m_movieTexture.isPlaying)
            {
                m_movieTexture.pause = true;
            }
            else
            {
                if (m_movieTexture.pause)
                {
                    m_movieTexture.pause = false;
                }
                else
                {
                    m_movieTexture.Play();
                }
            }
        }

    }
}
