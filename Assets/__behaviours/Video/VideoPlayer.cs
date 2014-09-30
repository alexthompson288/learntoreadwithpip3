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
    [SerializeField]
    private GameObject m_errorLabelParent;

    WWW m_www;

    float m_progressTweenDuration = 0.25f;

#if UNITY_EDITOR
    [SerializeField]
    private bool m_sliderStayActiveOnAwake;
#endif

    void Awake()
    {
        if (m_progressBar != null && m_progressBarParent != null)
        {
            m_progressBarParent.SetActive(true);
            m_progressBar.value = 0;

            Vector3 progressBarScale = Vector3.zero;

#if UNITY_EDITOR
            if(m_sliderStayActiveOnAwake)
            {
                progressBarScale = Vector3.one;
            }
#endif

            m_progressBarParent.transform.localScale = progressBarScale;
        }

        ////////D.Log("StreamingAssets - " + m_filename + ": " + HasStreamingAssetsCopy(m_filename));

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
        ////////D.Log("OnPressPlayCo()");
        if (!HasLocalCopy())
        {
            ////////D.Log("No local copy found, downloading");
            yield return StartCoroutine("DownloadVideo");
        } 
        else
        {
            ////////D.Log("Found local copy");
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
        ////////D.Log("OnPressDownload()");
        if (!HasLocalCopy())
        {
            ////////D.Log("No local copy found, downloading");
            StartCoroutine("DownloadVideo");
        }
        else
        {
            ////////D.Log("Found local copy, no download");
        }
    }

    void OnPressDelete(PipButton button)
    {
        ////////D.Log("OnPressDelete()");
        if (File.Exists(GetFullPath(m_filename))) // Don't call HasLocalCopy(), we need to ignore StreamingAssets because the directory is not writable
        {
            ////////D.Log("Found local copy, deleting");
            File.Delete(GetFullPath(m_filename));

            if(Deleted != null)
            {
                Deleted(this);
            }
        } 
        else
        {
            ////////D.Log("No local copy found");
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
        m_errorLabelParent.transform.localScale = Vector3.zero;

        ////////D.Log("VideoPlayer.DownloadVideo(): " + m_filename);

        ////////D.Log("DISABLE UI");
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

        ////////D.Log("WAIT");
        ////////D.Log("Download url: " + m_url + m_filename);
        m_www = new WWW(m_url + m_filename);

        StartCoroutine("UpdateProgressBar");

        yield return m_www;
        
        ////////D.Log("DOWNLOADED");
        
        if (m_www.isDone && m_www.error == null)
        {
            ////////D.Log("WRITING");
            FileStream stream = new FileStream(GetFullPath(m_filename), FileMode.Create);
            stream.Write(m_www.bytes, 0, m_www.bytes.Length);
            stream.Close();
        } 
        else
        {
            ////////D.Log("DOWNLOAD FAILED");
            ////////D.Log("www.isDone: " + m_www.isDone);
            ////////D.Log("www.error == null: " + m_www.error == null);
            
            if(m_www.error != null)
            {
                ////////D.Log("error: " + m_www.error);

                string errorMessage = "Oops!\nSomething went wrong\nTry again later";

                if(m_www.error.Contains("Could not resolve host"))
                {
                    errorMessage = "Check your internet connection";
                }
                else if(m_www.error.Contains("403"))
                {
                    errorMessage = "Oops!\nFile not found";
                }

                m_errorLabelParent.GetComponentInChildren<UILabel>().text = errorMessage;
            }

            StopCoroutine("UpdateProgressBar");

            m_progressBar.value = 0;
            iTween.ScaleTo(m_errorLabelParent, Vector3.one, 0.3f);

            yield return new WaitForSeconds(3f);
        }

        StopCoroutine("UpdateProgressBar");

        yield return(StartCoroutine(ResetProgressBar()));

        ////////D.Log("ENABLE UI");
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
