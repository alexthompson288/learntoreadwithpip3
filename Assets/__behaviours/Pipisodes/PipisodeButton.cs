using UnityEngine;
using System.Collections;

public class PipisodeButton : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_pipisodeImage;
    [SerializeField]
    private UISprite m_downloadSprite;
    [SerializeField]
    UIDragPanelContents m_dragPanelContents;

    DataRow m_pipisode;

	// Use this for initialization
	public void SetUp (DataRow pipisode, UIDraggablePanel draggablePanel, VideoPlayer videoPlayer) 
    {
        m_pipisode = pipisode;

        m_pipisodeImage.mainTexture = DataHelpers.GetPicture(m_pipisode);

        m_dragPanelContents.draggablePanel = draggablePanel;

        Refresh(videoPlayer);

        videoPlayer.Downloaded += OnVideoDownloadOrDelete;
        videoPlayer.Deleted += OnVideoDownloadOrDelete;
	}

    void OnVideoDownloadOrDelete(VideoPlayer videoPlayer)
    {
        Refresh(videoPlayer);
    }

    public void Refresh(VideoPlayer videoPlayer)
    {
        m_downloadSprite.gameObject.SetActive(videoPlayer.HasLocalCopy(videoPlayer.GetPipisodeFilename(m_pipisode)));
    }
}
