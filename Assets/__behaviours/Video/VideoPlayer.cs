using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class VideoPlayer : MonoBehaviour 
{
    public delegate void VideoPlayerEventHandler(VideoPlayer videoPlayer);
    public event VideoPlayerEventHandler Downloaded;
    public event VideoPlayerEventHandler Deleted;

    [SerializeField]
    private string m_filename;
    [SerializeField]
    private string m_streamingAssetsRelativePath = "/mp4/";
    [SerializeField]
    private string m_url = "https://s3-eu-west-1.amazonaws.com/pipisodes/";
    //private string m_url = "https://s3-eu-west-1.amazonaws.com/pipvideotest";
    [SerializeField]
    private Color m_bgColor = Color.black;
#if UNITY_IPHONE || UNITY_ANROID
    [SerializeField]
    private FullScreenMovieControlMode m_controlMode = FullScreenMovieControlMode.Full;
    [SerializeField]
    private FullScreenMovieScalingMode m_scalingMode = FullScreenMovieScalingMode.AspectFit;
#endif
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
    [SerializeField]
    private UICamera m_loadingBarCamera;
    [SerializeField]
    private PipButton m_cancelButton;

    WWW m_www;

    float m_progressTweenDuration = 0.25f;


    void Awake()
    {
        if (m_progressBar != null && m_progressBarParent != null)
        {
            m_progressBarParent.SetActive(true);
            m_progressBar.value = 0;
            m_progressBarParent.transform.localScale = Vector3.zero;
        }

        Debug.Log("StreamingAssets - " + m_filename + ": " + HasStreamingAssetsCopy(m_filename));

        m_playButton.Unpressing += OnPressPlay;
        m_deleteButton.Unpressing += OnPressDelete;
        m_downloadButton.Unpressing += OnPressDownload;
        m_cancelButton.Unpressing += OnPressCancel;
    }

    string GetFullPath(string filename)
    {
        return (Application.persistentDataPath + "/" + filename);
    }

    bool HasStreamingAssetsCopy(string filename)
    {
        return File.Exists(Application.dataPath + "/StreamingAssets" + m_streamingAssetsRelativePath + filename);
    }

    public bool HasLocalCopy(string filename = "")
    {
        if (String.IsNullOrEmpty(filename))
        {
            filename = m_filename;
        }

        return HasStreamingAssetsCopy(filename) || File.Exists(GetFullPath(filename));
    }

    public void SetFilename(string newFilename)
    {
        m_filename = newFilename;
    }

    public string GetPipisodeFilename(DataRow pipisode)
    {
        return pipisode ["image_filename"] != null ? pipisode ["image_filename"].ToString() + ".mp4" : "";
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
            yield return StartCoroutine("DownloadVideo");
        } 
        else
        {
            Debug.Log("Found local copy");
        }

        string playPath = HasStreamingAssetsCopy(m_filename) ? m_streamingAssetsRelativePath + m_filename : GetFullPath(m_filename);

#if UNITY_EDITOR
#elif UNITY_IPHONE
        if(!HasStreamingAssetsCopy(m_filename))
        {
            playPath = "file://" + GetFullPath(m_filename);
        }
#endif

#if UNITY_IPHONE || UNITY_ANDROID
        Handheld.PlayFullScreenMovie(playPath, m_bgColor, m_controlMode, m_scalingMode);
#endif
    }

    void OnPressDownload(PipButton button)
    {
        Debug.Log("OnPressDownload()");
        if (!HasLocalCopy())
        {
            Debug.Log("No local copy found, downloading");
            StartCoroutine("DownloadVideo");
        }
        else
        {
            Debug.Log("Found local copy, no download");
        }
    }

    void OnPressDelete(PipButton button)
    {
        Debug.Log("OnPressDelete()");
        if (File.Exists(GetFullPath(m_filename))) // Don't call HasLocalCopy(), we need to ignore StreamingAssets because the directory is not writable
        {
            Debug.Log("Found local copy, deleting");
            File.Delete(GetFullPath(m_filename));

            if(Deleted != null)
            {
                Deleted(this);
            }
        } 
        else
        {
            Debug.Log("No local copy found");
        }
    }

    void OnPressCancel(PipButton button)
    {
        StopCoroutine("DownloadVideo");
        StopCoroutine("UpdateProgressBar");

        StartCoroutine(ResetProgressBar());

        UICamera[] uiCams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];
        foreach (UICamera cam in uiCams)
        {
            cam.enabled = true;
        }
    }

    IEnumerator DownloadVideo()
    {
        Debug.Log("VideoPlayer.DownloadVideo(): " + m_filename);

        Debug.Log("DISABLE UI");
        UICamera[] uiCams = UnityEngine.Object.FindObjectsOfType(typeof(UICamera)) as UICamera[];
        
        foreach (UICamera cam in uiCams)
        {
            if(cam != m_loadingBarCamera)
            {
                cam.enabled = false;
            }
        }

        if (m_progressBarParent != null)
        {
            iTween.ScaleTo(m_progressBarParent, Vector3.one, m_progressTweenDuration);
            yield return new WaitForSeconds(m_progressTweenDuration);
        }

        Debug.Log("WAIT");
        Debug.Log("Download url: " + m_url + m_filename);
        m_www = new WWW(m_url + m_filename);

        StartCoroutine("UpdateProgressBar");

        yield return m_www;
        
        Debug.Log("DOWNLOADED");
        
        if (m_www.isDone && m_www.error == null)
        {
            Debug.Log("WRITING");
            FileStream stream = new FileStream(GetFullPath(m_filename), FileMode.Create);
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

        yield return(StartCoroutine(ResetProgressBar()));

        Debug.Log("ENABLE UI");
        foreach (UICamera cam in uiCams)
        {
            cam.enabled = true;
        }

        yield return null;

        if (Downloaded != null)
        {
            Downloaded(this);
        }
    }

    IEnumerator ResetProgressBar()
    {
        if (m_progressBarParent != null)
        {
            iTween.ScaleTo(m_progressBarParent, Vector3.zero, m_progressTweenDuration);
            yield return new WaitForSeconds(m_progressTweenDuration);
            
            if(m_progressBar != null)
            {
                m_progressBar.value = 0;
            }
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
