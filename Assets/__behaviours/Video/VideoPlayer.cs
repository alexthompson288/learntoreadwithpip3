using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class VideoPlayer : MonoBehaviour 
{
    [SerializeField]
    private string m_filename;
    [SerializeField]
    private string m_streamingAssetsRelativePath = "/mp4/";
    [SerializeField]
    private string m_url = "https://s3-eu-west-1.amazonaws.com/pipvideotest";
    [SerializeField]
    private Color m_bgColor = Color.black;
    [SerializeField]
    private FullScreenMovieControlMode m_controlMode = FullScreenMovieControlMode.Full;
    [SerializeField]
    private FullScreenMovieScalingMode m_scalingMode = FullScreenMovieScalingMode.AspectFit;
    [SerializeField]
    private PipButton m_playButton;
    [SerializeField]
    private PipButton m_downloadButton;
    [SerializeField]
    private PipButton m_deleteButton;
    [SerializeField]
    private UISlider m_progressBar;
    [SerializeField]
    private GameObject m_progressBarParent;

    WWW m_www;

    void Awake()
    {
        if (m_progressBar != null && m_progressBarParent != null)
        {
            m_progressBar.value = 0;
            m_progressBarParent.transform.localScale = Vector3.zero;
        }

        Debug.Log("StreamingAssets - " + m_filename + ": " + HasStreamingAssetsCopy());

        m_playButton.Unpressing += OnPressPlay;
        m_deleteButton.Unpressing += OnPressDelete;
        m_downloadButton.Unpressing += OnPressDownload;
    }

    bool HasStreamingAssetsCopy()
    {
        return File.Exists(Application.dataPath + "/StreamingAssets" + m_streamingAssetsRelativePath + m_filename);
    }

    bool HasLocalCopy()
    {
        return HasStreamingAssetsCopy() || File.Exists(GetFullPath());
    }

    public void SetFilename(string newFilename)
    {
        m_filename = newFilename;
    }

    string GetFullPath()
    {
        return (Application.persistentDataPath + "/" + m_filename);
    }

    void OnPressPlay(PipButton button)
    {
        StartCoroutine(OnPressPlayCo());
    }

    IEnumerator OnPressPlayCo()
    {
        Debug.Log("OnPressPlayCo()");
        if (!HasLocalCopy())
        {
            Debug.Log("No local copy found, downloading");
            yield return StartCoroutine(DownloadVideo());
        } 
        else
        {
            Debug.Log("Found local copy");
        }

        string playPath = HasStreamingAssetsCopy() ? m_streamingAssetsRelativePath + m_filename : GetFullPath();

#if UNITY_EDITOR
#elif UNITY_IPHONE
        if(!HasStreamingAssetsCopy())
        {
            playPath = "file://" + GetFullPath();
        }
#endif

        Handheld.PlayFullScreenMovie(playPath, m_bgColor, m_controlMode, m_scalingMode);
    }

    void OnPressDownload(PipButton button)
    {
        Debug.Log("OnPressDownload()");
        if (!HasLocalCopy())
        {
            Debug.Log("No local copy found, downloading");
            StartCoroutine(DownloadVideo());
        }
        else
        {
            Debug.Log("Found local copy, no download");
        }
    }

    void OnPressDelete(PipButton button)
    {
        Debug.Log("OnPressDelete()");
        if (File.Exists(GetFullPath())) // Don't call HasLocalCopy(), we need to ignore StreamingAssets because the directory is not writable
        {
            Debug.Log("Found local copy, deleting");
            File.Delete(GetFullPath());
        } else
        {
            Debug.Log("No local copy found");
        }
    }

    IEnumerator DownloadVideo()
    {
        Debug.Log("VideoPlayer.DownloadVideo(): " + m_filename);

        Debug.Log("DISABLE UI");
        UICamera[] uiCams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];
        
        foreach (UICamera cam in uiCams)
        {
            cam.enabled = false;
        }

        float progressTweenDuration = 0.25f; 

        if (m_progressBarParent != null)
        {
            iTween.ScaleTo(m_progressBarParent, Vector3.one, progressTweenDuration);
            yield return new WaitForSeconds(progressTweenDuration);
        }

        Debug.Log("WAIT");
        Debug.Log("Download url: " + m_url + "/" + m_filename);
        m_www = new WWW(m_url + "/" + m_filename);

        StartCoroutine("UpdateProgressBar");

        yield return m_www;
        
        Debug.Log("DOWNLOADED");
        
        if (m_www.isDone && m_www.error == null)
        {
            Debug.Log("WRITING");
            FileStream stream = new FileStream(GetFullPath(), FileMode.Create);
            stream.Write(m_www.bytes, 0, m_www.bytes.Length);
            stream.Close();
        } 
        else
        {
            Debug.Log("DOWNLOAD FAILED");
            Debug.Log("www.isDone: " + m_www.isDone);
            Debug.Log("www.error == null: " + m_www.error == null);
            
            if(m_www.error != null)
            {
                Debug.Log("error: " + m_www.error);
            }
        }

        StopCoroutine("UpdateProgressBar");

        if (m_progressBarParent != null)
        {
            iTween.ScaleTo(m_progressBarParent, Vector3.zero, progressTweenDuration);
            yield return new WaitForSeconds(progressTweenDuration);

            if(m_progressBar != null)
            {
                m_progressBar.value = 0;
            }
        }

        Debug.Log("ENABLE UI");
        foreach (UICamera cam in uiCams)
        {
            cam.enabled = true;
        }

        yield return null;
    }

    IEnumerator UpdateProgressBar()
    {
        if (m_progressBar != null)
        {
            m_progressBar.value = m_www.progress;
            yield return null;
            StartCoroutine("UpdateProgressBar");
        } 
        else
        {
            yield break;
        }
    }
}
