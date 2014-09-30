using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlusGameButton : MonoBehaviour 
{
    public delegate void PlusGameButtonEventHandler (PlusGameButton gameButton);
    public event PlusGameButtonEventHandler Unpressed;

    [SerializeField]
    private string m_gameName;
    [SerializeField]
    private int m_numPlayers = 1;
    [SerializeField]
    private UILabel m_gameLabel;
    [SerializeField]
    private GameObject m_scoreLabelParent;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private ColorBadge[] m_colorBadges;
    [SerializeField]
    private float m_rotateAmount = 20.02f;

    [System.Serializable]
    class ColorBadge
    {
        public UISprite m_sprite;
        public GameObject m_lockedSprite;
        public ColorInfo.PipColor m_pipColor;
    }

    void OnPress(bool isDown)
    {
        if (!isDown && Unpressed != null)
        {
            Unpressed(this);
        }
    }

    IEnumerator Start()
    {
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

        // Color Badges
        if (m_colorBadges != null && m_colorBadges.Length > 0)
        {
            List<ColorBadge> unlockedBadges = new List<ColorBadge>();

            if (isUnlocking && PlusScoreInfo.Instance.HasNewMaxColor())
            {
                unlockedBadges = System.Array.FindAll(m_colorBadges, x => (int)x.m_pipColor > PlusScoreInfo.Instance.GetOldMaxColor() 
                                                      && (int)x.m_pipColor <= PlusScoreInfo.Instance.GetNewMaxColor()).ToList(); 
            }

            ColorInfo.PipColor maxColor = (ColorInfo.PipColor)PlusScoreInfo.Instance.GetMaxColor(m_gameName, PlusGameMenuCoordinator.Instance.GetScoreType());

            for (int i = 0; i < m_colorBadges.Length; ++i)
            {
                if (unlockedBadges.Contains(m_colorBadges[i]))
                {
                    //////D.Log("Unlocking color badge");
                    StartCoroutine(UnlockColorBadge(m_colorBadges[i]));
                } 
                else if ((int)m_colorBadges[i].m_pipColor <= (int)maxColor)
                {
                    m_colorBadges[i].m_lockedSprite.SetActive(false);
                } 
            }
        }

        if (isUnlocking)
        {
            PlusScoreInfo.Instance.ClearUnlockTrackers();
        }

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        DataRow game = DataHelpers.GetGame(m_gameName);
        m_gameLabel.text = game != null ? DataHelpers.GetLabelText(game) : m_gameName;
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

#if UNITY_EDITOR
    void Update()
    {
        if (m_scoreLabelParent != null)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                StartCoroutine(UnlockHighScore());
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                m_scoreLabelParent.transform.eulerAngles = Vector3.zero;
            }
        }
    }
#endif

    IEnumerator UnlockColorBadge(ColorBadge colorBadge)
    {
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        GameObject go = colorBadge.m_sprite.gameObject;
        float tweenDuration = 0.8f;

        TweenAlpha.Begin(colorBadge.m_lockedSprite, tweenDuration, 0);

        iTween.RotateBy(go, new Vector3(0, 0, 255), tweenDuration);

        Vector3 originalScale = go.transform.localScale;
        iTween.ScaleTo(go, originalScale * 2f, tweenDuration / 2);
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
