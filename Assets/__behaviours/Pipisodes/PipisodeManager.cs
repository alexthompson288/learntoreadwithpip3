using UnityEngine;
using System.Collections;

public class PipisodeManager : Singleton<PipisodeManager> 
{
    [SerializeField]
    private string m_relativePathMp4 = "Pipisodes/mp4";

    public void PlayPipisode(int pipisodeId)
    {
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from pipisodes WHERE id=" + pipisodeId);
        
        if(dt.Rows.Count > 0)
        {
            PlayPipisode(dt.Rows[0]);
        }
    }
    
    public void PlayPipisode(DataRow pipisode)
    {
        string filename = pipisode["pipisode_title"].ToString().ToLower().Replace(" ", "_");
        Debug.Log("filename: " + filename);

#if UNITY_ANDROID || UNITY_IPHONE
        Handheld.PlayFullScreenMovie(System.String.Format("{0}/{1}.mp4", m_relativePathMp4, filename),
                                     Color.black, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFit);
#endif
    }
}
