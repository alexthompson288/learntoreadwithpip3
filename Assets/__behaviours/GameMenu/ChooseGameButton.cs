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
    private int m_numPlayers = 1;

    DataRow m_game = null;

    public int GetNumPlayers()
    {
        return m_numPlayers;
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
        else if(m_game != null)
        {
            ScoreInfo.RefreshStars(m_starSprites, m_game ["name"].ToString(), colorName);
        }
    }

    public void TweenScoreStars(GameObject starPrefab, UIGrid starSpawnGrid)
    {
        Debug.Log("TweenScoreStars()");
        int numNewStars = ScoreInfo.Instance.GetNewHighScoreStars() - ScoreInfo.Instance.GetPreviousHighScoreStars();
        int startIndex = ScoreInfo.Instance.GetPreviousHighScoreStars();

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");

        List<GameObject> newStars = new List<GameObject>();

        for(int i = startIndex; i < startIndex + numNewStars && i < m_starSprites.Length; ++i)
        {
            newStars.Add(Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(starPrefab, starSpawnGrid.transform, true));
        }

        starSpawnGrid.Reposition();

        NGUIHelpers.PositionGridHorizontal(starSpawnGrid);

        for (int i = 0; i < newStars.Count && i < m_starSprites.Length; ++i)
        {
            newStars[i].transform.parent = m_starSprites[i].transform.parent;
            StartCoroutine(TweenStar(newStars[i], m_starSprites[i].transform, i == startIndex));
        }
    }

    IEnumerator TweenStar(GameObject star, Transform target, bool playAudio)
    {
        float spawnTweenDuration = 0.5f;

        iTween.ScaleTo(star, Vector3.one * 1.5f, spawnTweenDuration);

        yield return new WaitForSeconds(spawnTweenDuration + 0.2f);

        float moveTweenDuration = 1f;
        
        iTween.MoveTo(star, target.position, moveTweenDuration);
        TweenAlpha.Begin(target.gameObject, moveTweenDuration, 0);
        iTween.ShakeRotation(star, new Vector3(0, 0, 360f), moveTweenDuration);
        iTween.ScaleTo(star, Vector3.one, moveTweenDuration);

        if (playAudio)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
        }
    }
}
