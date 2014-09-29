using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlusGame : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_scoreLabelMoveable;
    [SerializeField]
    private GameObject m_scoreLabelSpinnable;
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
    [SerializeField]
    private GameObject m_padlock;

    float m_scoreLabelOffset = 0.122f;

    DataRow m_game;

    ColorInfo.PipColor m_maxColor;

    public ColorInfo.PipColor GetMaxColor()
    {
        return m_maxColor;
    }

    [System.Serializable]
    class ColorStar
    {
        public UISprite m_colorStar;
        public ColorInfo.PipColor m_pipColor;
    }

    void Start()
    {
        System.Array.Sort(m_colorStars, (a, b) => ((int)a.m_pipColor).CompareTo((int)b.m_pipColor)); 

        foreach (ColorStar star in m_colorStars)
        {
            star.m_colorStar.transform.localScale = Vector3.zero;
            star.m_colorStar.color = ColorInfo.GetColor(star.m_pipColor);
        }
    }

    IEnumerator UnlockHighScore(int newHighScore)
    {
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
        
        float tweenDuration = 0.8f;
        
        iTween.RotateBy(m_scoreLabelSpinnable, new Vector3(0, 0, m_rotateAmount), tweenDuration);
        
        Vector3 originalScale = m_scoreLabelSpinnable.transform.localScale;
        iTween.ScaleTo(m_scoreLabelSpinnable, originalScale * 2f, tweenDuration / 2);
        yield return new WaitForSeconds(tweenDuration / 2);
        m_scoreLabel.text = newHighScore.ToString();
        iTween.ScaleTo(m_scoreLabelSpinnable, originalScale, tweenDuration / 2);
    }

    IEnumerator UnlockColorStar(ColorStar colorStar)
    {
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        GameObject go = colorStar.m_colorStar.gameObject;
        float tweenDuration = 0.8f;

        Vector3 scoreLabelParentPos = m_scoreLabelSpinnable.transform.position;
        scoreLabelParentPos.x = colorStar.m_colorStar.transform.position.x  + m_scoreLabelOffset;
        iTween.MoveTo(m_scoreLabelMoveable, scoreLabelParentPos, tweenDuration);

        iTween.RotateBy(go, new Vector3(0, 0, m_rotateAmount), tweenDuration);


        iTween.ScaleTo(go, Vector3.one * 1.5f, tweenDuration / 2);

        yield return new WaitForSeconds(tweenDuration / 2);

        iTween.ScaleTo(go, Vector3.one, tweenDuration / 2);
    }

    Transform FindStarTransform(ColorInfo.PipColor pipColor)
    {
        ColorStar targetStar = System.Array.Find(m_colorStars, x => x.m_pipColor == pipColor);
        return targetStar != null ? targetStar.m_colorStar.transform : null;
    }

    public void SetUp(string gameName, int characterIndex)
    {
        m_game = DataHelpers.GetGame(gameName);

        if (m_game != null)
        {
            string characterName = "pip";

            switch(characterIndex)
            {
                case 1:
                    characterName = "pop";
                    break;
                case 2:
                    characterName = "lala";
                    break;
                case 3:
                    characterName = "sam";
                    break;
                default:
                    break;
            }

            m_gameIcon.spriteName = string.Format("{0}_state_a", characterName);

            m_gameLabel.text = DataHelpers.GetLabelText(m_game);

            bool isUnlocking = PlusScoreInfo.Instance.HasUnlockTrackers() && gameName == PlusScoreInfo.Instance.GetUnlockGame();
            if (isUnlocking)
            {
                PlusGameMenuCoordinator.Instance.MakeAllPipsJump();
            }
            
            // High Scores
            if (m_scoreLabel != null)
            {
                if (isUnlocking && PlusScoreInfo.Instance.HasNewHighScore())
                {
                    m_scoreLabel.text = PlusScoreInfo.Instance.GetOldHighScore().ToString();
                    StartCoroutine(UnlockHighScore(PlusScoreInfo.Instance.GetNewScore()));
                } else
                {
                    m_scoreLabel.text = PlusScoreInfo.Instance.GetScore(gameName, PlusGameMenuCoordinator.Instance.GetScoreType()).ToString();
                }
            }
            
            // Color Stars
            if (m_colorStars != null && m_colorStars.Length > 0)
            {
                List<ColorStar> unlockedStars = new List<ColorStar>();

                m_maxColor = (ColorInfo.PipColor)PlusScoreInfo.Instance.GetMaxColor(gameName, PlusGameMenuCoordinator.Instance.GetScoreType());

                Transform targetTransform = FindStarTransform(m_maxColor);

                if (isUnlocking && PlusScoreInfo.Instance.HasNewMaxColor())
                {
                    int newMaxColor = PlusScoreInfo.Instance.GetNewMaxColor();

                    unlockedStars = System.Array.FindAll(m_colorStars, x => (int)x.m_pipColor > PlusScoreInfo.Instance.GetOldMaxColor() && (int)x.m_pipColor <= newMaxColor).ToList();

                    if (unlockedStars.Count > 0)
                    {
                        ColorStar lowestUnlocked = unlockedStars [0];
                        int index = System.Array.IndexOf(m_colorStars, lowestUnlocked);
                        --index;

                        if (index > -1)
                        {
                            targetTransform = FindStarTransform(m_colorStars [index].m_pipColor);
                        }
                    }
                }

                if (targetTransform != null)
                {
                    Vector3 pos = m_scoreLabelMoveable.transform.position;
                    pos.x = targetTransform.position.x + m_scoreLabelOffset;
                    m_scoreLabelMoveable.transform.position = pos;
                }

                for (int i = 0; i < m_colorStars.Length; ++i)
                {
                    if (unlockedStars.Contains(m_colorStars [i]))
                    {
                        StartCoroutine(UnlockColorStar(m_colorStars [i]));
                    } else if (m_colorStars [i].m_pipColor <= m_maxColor)
                    {
                        m_colorStars [i].m_colorStar.transform.localScale = Vector3.one;
                    }
                }
            }
            
            if (isUnlocking)
            {
                PlusScoreInfo.Instance.ClearUnlockTrackers();
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public DataRow GetGame()
    {
        return m_game;
    }

    public void RefreshPadlock()
    {
        m_padlock.SetActive(!IsGameUnlocked());
    }

    public bool IsGameUnlocked()
    {
        return ContentLock.Instance.IsPlusGameUnlocked(m_game.GetId());
    }
}
