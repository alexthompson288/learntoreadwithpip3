using UnityEngine;
using System.Collections;

public class LocalVideoPlayer : MonoBehaviour 
{
    [SerializeField]
    private string m_path;
    [SerializeField]
    private string m_filename;

	void OnClick()
    {
        PlayVideo();
    }

    public void PlayVideo()
    {
        if (m_path.Length > 0 && m_filename.Length > 0)
        {
            char pathLast = m_path [m_path.Length - 1];
            char filenameFirst = m_filename [0];
            
            if(pathLast != '/'  && pathLast != '\\' && filenameFirst != '/'  && filenameFirst != '\\')
            {
                m_path += "/";
            }
        }
        
        Handheld.PlayFullScreenMovie(m_path + m_filename);
    }
}
