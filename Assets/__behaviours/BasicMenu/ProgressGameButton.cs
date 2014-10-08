using UnityEngine;
using System.Collections;

public class ProgressGameButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_icon;
    [SerializeField]
    private UISprite m_button;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite[] m_stars;

    string m_gameName;

    void Awake()
    {
        System.Array.Sort(m_stars, CollectionHelpers.LeftToRight);
    }

    public void SetUp(string myGameName, ColorInfo.PipColor pipColor)
    {
        m_gameName = myGameName;
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE name='" + m_gameName + "'");
        DataRow game = dt.Rows.Count > 0 ? dt.Rows[0] : null;

        m_label.text = game != null && game["labeltext"] != null ? game ["labeltext"].ToString() : m_gameName;

        m_button.color = ColorInfo.GetColor(pipColor);

        // Stars
        ScoreInfo.RefreshStars(m_stars, m_gameName, pipColor.ToString());

        if (ScoreInfo.Instance.HasNewHighScore() && ScoreInfo.Instance.GetNewHighScoreGame() == m_gameName)
        {
            StartCoroutine(TweenNewStars());
        }
    }

    IEnumerator TweenNewStars()
    {
        int totalStars = ScoreInfo.Instance.GetNewHighScoreStars();
        int newStars = totalStars - ScoreInfo.Instance.GetPreviousHighScoreStars();
        int newStarIndex = totalStars - newStars;

        ScoreInfo.Instance.DestroyNewHighScore();

        // Set the color of the newly unlocked stars back to white beforehand, so we can set their color to gold during the tween
        for (int i = newStarIndex; i < totalStars; ++i)
        {
            m_stars[i].color = Color.white;
        }

        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
        yield return new WaitForSeconds(0.5f);

        float tweenDuration = 1f;
        for(int i = newStarIndex; i < totalStars; ++i)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
            
            m_stars[i].color = ColorInfo.GetColor(ColorInfo.PipColor.Gold);
            
            iTween.RotateBy(m_stars[i].gameObject, new Vector3(0, 0, 20.02f), tweenDuration);
            
            Vector3 originalScale = m_stars[i].transform.localScale;
            iTween.ScaleTo(m_stars[i].gameObject, originalScale * 2.5f, tweenDuration / 2);
            yield return new WaitForSeconds(tweenDuration / 2);
            iTween.ScaleTo(m_stars[i].gameObject, originalScale, tweenDuration / 2);
        }

//        WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
//
//        float tweenDuration = 1f;
//        for(int i = newStarIndex; i < totalStars; ++i)
//        {
//            iTween.PunchScale(m_stars[i].gameObject, Vector3.one * 2f, tweenDuration);
//            iTween.PunchRotation(m_stars[i].gameObject, new Vector3(0f, 0f, 360f), tweenDuration);
//            m_stars[i].color = ColorInfo.GetColor(ColorInfo.PipColor.Gold);
//        }
    }

    void OnClick()
    {
        BasicGameMenuCoordinator.Instance.OnClickProgressGame(this);
    }

    public string GetGameName()
    {
        return m_gameName;
    }
}
