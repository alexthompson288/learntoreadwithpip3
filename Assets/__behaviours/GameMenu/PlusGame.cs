using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlusGame : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_scoreLabelParent;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private UISprite m_gameIcon;
    [SerializeField]
    private UILabel m_gameLabel;
    [SerializeField]
    private float m_rotateAmount = 20.02f;
    [SerializeField]
    private ColorStar[] m_colorStars;

    string m_gameName;

    [System.Serializable]
    class ColorStar
    {
        public UISprite m_sprite;
        public ColorInfo.PipColor m_pipColor;
    }

    void Awake()
    {
        foreach (ColorStar star in m_colorStars)
        {
            star.m_sprite.transform.localScale = Vector3.zero;
            star.m_sprite.color = ColorInfo.GetColor(star.m_pipColor);
        }
    }

    IEnumerator UnlockHighScore()
    {
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
        
        float tweenDuration = 0.8f;
        
        iTween.RotateBy(m_scoreLabelParent, new Vector3(0, 0, m_rotateAmount), tweenDuration);
        
        Vector3 originalScale = m_scoreLabelParent.transform.localScale;
        iTween.ScaleTo(m_scoreLabelParent, originalScale * 2f, tweenDuration / 2);
        yield return new WaitForSeconds(tweenDuration / 2);
        m_scoreLabel.text = PlusScoreInfo.Instance.GetNewScore().ToString();
        iTween.ScaleTo(m_scoreLabelParent, originalScale, tweenDuration / 2);
    }

    IEnumerator UnlockColorStar(ColorStar colorStar)
    {
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        GameObject go = colorStar.m_sprite.gameObject;
        float tweenDuration = 0.8f;

        Vector3 scoreLabelParentPos = m_scoreLabelParent.transform.position;
        scoreLabelParentPos.x = colorStar.m_sprite.transform.position.x;
        iTween.MoveTo(m_scoreLabelParent, scoreLabelParentPos, tweenDuration);

        iTween.RotateBy(go, new Vector3(0, 0, 255), tweenDuration);

        iTween.ScaleTo(go, Vector3.one * 1.5f, tweenDuration / 2);

        yield return new WaitForSeconds(tweenDuration / 2);

        iTween.ScaleTo(go, Vector3.one, tweenDuration / 2);
    }

    public void SetUp(string myGameName)
    {
        m_gameName = myGameName;

        bool isUnlocking = PlusScoreInfo.Instance.HasUnlockTrackers() && m_gameName == PlusScoreInfo.Instance.GetUnlockGame();
        
        // High Scores
        if (m_scoreLabel != null)
        {
            if (isUnlocking && PlusScoreInfo.Instance.HasNewHighScore())
            {
                m_scoreLabel.text = PlusScoreInfo.Instance.GetOldHighScore().ToString();
                StartCoroutine(UnlockHighScore());
            }
            else
            {
                m_scoreLabel.text = PlusScoreInfo.Instance.GetScore(m_gameName, PlusGameMenuCoordinator.Instance.GetScoreType()).ToString();
            }
        }
        
        // Color Stars
        if (m_colorStars != null && m_colorStars.Length > 0)
        {
            List<ColorStar> unlockedStars = new List<ColorStar>();
            
            if (isUnlocking && PlusScoreInfo.Instance.HasNewMaxColor())
            {
                unlockedStars = System.Array.FindAll(m_colorStars, x => (int)x.m_pipColor > PlusScoreInfo.Instance.GetOldMaxColor() 
                                                      && (int)x.m_pipColor <= PlusScoreInfo.Instance.GetNewMaxColor()).ToList(); 
            }
            
            ColorInfo.PipColor maxColor = (ColorInfo.PipColor)PlusScoreInfo.Instance.GetMaxColor(m_gameName, PlusGameMenuCoordinator.Instance.GetScoreType());
            
            for (int i = 0; i < m_colorStars.Length; ++i)
            {
                if (unlockedStars.Contains(m_colorStars[i]))
                {
                    StartCoroutine(UnlockColorStar(m_colorStars[i]));
                } 
            }
        }
        
        if (isUnlocking)
        {
            PlusScoreInfo.Instance.ClearUnlockTrackers();
        }

        DataRow game = DataHelpers.GetGame(m_gameName);
        m_gameLabel.text = game != null ? DataHelpers.GetLabelText(game) : m_gameName;
    }

    public string GetGameName()
    {
        return m_gameName;
    }
}
