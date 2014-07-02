using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChooseGameButton : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_icon;
    [SerializeField]
    private UISprite[] m_starSprites;
    [SerializeField]
    private string m_blackboardSpriteName;
    [SerializeField]
    private int m_numPlayers = 1;

    DataRow m_game = null;

    public int GetNumPlayers()
    {
        return m_numPlayers;
    }

    public string GetBlackboardSpriteName()
    {
        return m_blackboardSpriteName;
    }

    public UIAtlas GetSpriteAtlas()
    {
        return m_icon.atlas;
    }

    void Awake()
    {
        System.Array.Sort(m_starSprites, CollectionHelpers.LocalLeftToRight);
    }

	public void SetUp(DataRow game)
    {
        m_game = game;

        if (m_game != null)
        {
            m_label.text = m_game ["labeltext"] != null ? m_game ["labeltext"].ToString() : m_game ["name"].ToString();
        }
    }

    public string GetIconName()
    {
        return m_icon.spriteName;
    }

    public void Refresh(string colorName)
    {
        // If we have recently achieved a new high score, then don't show the new stars yet
        if (ScoreInfo.Instance.HasNewHighScore() && ScoreInfo.Instance.GetNewHighScoreGame() == m_game ["name"].ToString())
        {
            int numStars = ScoreInfo.Instance.GetPreviousHighScoreStars();

            System.Array.Sort(m_starSprites, CollectionHelpers.LocalLeftToRight);
            
            for (int i = 0; i < m_starSprites.Length; ++i)
            {
                string spriteName = i < numStars ? "star_active_512" : "star_inactive_512";
                m_starSprites[i].spriteName = spriteName;
            }
        } 
        else
        {
            ScoreInfo.RefreshStars(m_starSprites, m_game ["name"].ToString(), colorName);
        }
    }

    public void TweenScoreStars(GameObject starPrefab, Transform m_starSpawnLocation)
    {
        Debug.Log("TweenScoreStars()");
        int numNewStars = ScoreInfo.Instance.GetNewHighScoreStars() - ScoreInfo.Instance.GetPreviousHighScoreStars();
        int startIndex = ScoreInfo.Instance.GetPreviousHighScoreStars();

        //int numNewStars = 2;
        //int startIndex = 0;

        D.Log("TotalStars: " + ScoreInfo.Instance.GetNewHighScoreStars());
        D.Log("NewStars: " + numNewStars);
        D.Log("startIndex: " + startIndex);

        for(int i = startIndex; i < startIndex + numNewStars && i < m_starSprites.Length; ++i)
        {
            GameObject newStar = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(starPrefab, m_starSpawnLocation);
            newStar.transform.parent = m_starSprites[i].transform.parent;
            newStar.transform.localScale = Vector3.one * 3;

            Debug.Log("Tweening");

            float tweenDuration = 1f;

            iTween.ScaleTo(newStar, Vector3.one, tweenDuration);
            iTween.MoveTo(newStar, m_starSprites[i].transform.position, tweenDuration);
            TweenAlpha.Begin(m_starSprites[i].gameObject, tweenDuration, 0);
            //iTween.PunchRotation(newStar, new Vector3(0, 0, 360f), tweenDuration);
            iTween.ShakeRotation(newStar, new Vector3(0, 0, 360f), tweenDuration);
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
    }
}
