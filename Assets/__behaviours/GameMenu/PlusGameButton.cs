using UnityEngine;
using System.Collections;

public class PlusGameButton : MonoBehaviour 
{
    public delegate void PlusGameButtonEventHandler (PlusGameButton gameButton);
    public event PlusGameButtonEventHandler Clicked;

    [SerializeField]
    private string m_gameName;
    [SerializeField]
    private int m_numPlayers;
    [SerializeField]
    private GameObject m_scoreLabelParent;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private ColorBadge[] m_colorBadges; 

    [System.Serializable]
    class ColorBadge
    {
        public UISprite m_sprite;
        public ColorInfo.PipColor m_pipColor;
    }

    void OnClick()
    {
        if (Clicked != null)
        {
            Clicked(this);
        }
    }

    void Start()
    {
        bool isUnlocking = PlusScoreInfo.Instance.HasNewHighScoreTracker() && m_gameName == PlusScoreInfo.Instance.GetNewTrackerGame();

        // High Scores
        if (isUnlocking && PlusScoreInfo.Instance.HasNewHighScore())
        {
            StartCoroutine(UnlockHighScore());
        }

        m_scoreLabel.text = PlusScoreInfo.Instance.GetScore(m_gameName, PlusGameMenuCoordinator.Instance.GetScoreType()).ToString();

        // Color Badges
        ColorBadge newMaxColorBadge = null;

        if (isUnlocking && PlusScoreInfo.Instance.HasNewMaxColor())
        {
            newMaxColorBadge = System.Array.Find(m_colorBadges, x => (int)x.m_pipColor == PlusScoreInfo.Instance.GetNewTrackerMaxColor());
        }

        ColorInfo.PipColor maxColor = (ColorInfo.PipColor)PlusScoreInfo.Instance.GetMaxColor(m_gameName, PlusGameMenuCoordinator.Instance.GetScoreType());

        foreach (ColorBadge colorBadge in m_colorBadges)
        {
            if(colorBadge == newMaxColorBadge)
            {
                StartCoroutine(UnlockColorBadge(colorBadge));
            }
            else if(colorBadge.m_pipColor < maxColor)
            {
                colorBadge.m_sprite.color = Color.white;
            }
        }
    }

    // TODO
    IEnumerator UnlockHighScore()
    {
        yield return null;
    }

    IEnumerator UnlockColorBadge(ColorBadge colorBadge)
    {
        GameObject go = colorBadge.m_sprite.gameObject;
        float tweenDuration = 0.3f;

        TweenColor.Begin(go, tweenDuration, Color.white);

        iTween.RotateBy(go, new Vector3(0, 0, 360), tweenDuration);

        Vector3 originalScale = go.transform.localScale;
        iTween.ScaleTo(go, originalScale * 1.25f, tweenDuration / 2);
        yield return new WaitForSeconds(tweenDuration / 2);
        iTween.ScaleTo(go, originalScale, tweenDuration / 2);
    }

    public int GetNumPlayers()
    {
        return m_numPlayers;
    }

    public string GetGameName()
    {
        return m_gameName;
    }
}
