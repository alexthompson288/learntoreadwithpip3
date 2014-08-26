using UnityEngine;
using System.Collections;

public class ScoreHealth : PlusScoreKeeper
{
    [SerializeField]
    private int m_maxPixelsMovePerFrame;
    [SerializeField]
    private UISprite m_healthBar;
    [SerializeField]
    private Transform m_healthBarTargetLocation;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private GameObject m_multiplayerParent;
    [SerializeField]
    private UISprite m_multiplayerBar;
    [SerializeField]
    private string[] m_characterIconNames;
    [SerializeField]
    private UISprite m_characterSprite;
    [SerializeField]
    private UISprite m_opponentCharacterSprite;
    [SerializeField]
    private GameObject m_levelUpTweener;

    Transform m_levelUpOnLocation;

    int m_startHeight;

    ScoreHealth m_opponentScoreHealth;

    Vector3 m_levelUpOffLocalPosition
    {
        get
        {
            Vector3 pos = m_levelUpOnLocation.localPosition;
            pos.x += 500f;
            return pos;
        }
    }

    public int barHeight
    {
        get
        {
            return m_healthBar.height;
        }
    }

    public override void SetCharacterIcon(int characterIndex)
    {
        m_characterSprite.gameObject.SetActive(true);
        m_characterSprite.spriteName = m_characterIconNames [characterIndex];

        if (m_opponentScoreHealth != null)
        {
            m_opponentScoreHealth.SetOpponentCharacterIcon(characterIndex);
        }
    }

    protected override void SetOpponentCharacterIcon(int opponentCharacterIndex)
    {
        m_opponentCharacterSprite.gameObject.SetActive(true);
        m_opponentCharacterSprite.spriteName = m_characterIconNames [opponentCharacterIndex];
    }

    protected override void Awake()
    {
        base.Awake();

        if (m_opponentKeeper is ScoreHealth)
        {
            m_opponentScoreHealth = m_opponentKeeper as ScoreHealth;
        }

        m_levelUpOnLocation = new GameObject("LevelUpOnLocation").transform;
        m_levelUpOnLocation.parent = transform;
        m_levelUpOnLocation.position = m_levelUpTweener.transform.position;

        m_levelUpTweener.transform.localPosition = m_levelUpOffLocalPosition;
    }

    void Start ()
    {
        m_characterSprite.gameObject.SetActive(false);
        m_opponentCharacterSprite.gameObject.SetActive(false);

        int numPlayers = SessionInformation.Instance.GetNumPlayers();

        m_multiplayerParent.SetActive(numPlayers == 2);
        m_scoreLabel.gameObject.SetActive(numPlayers == 1);

        if (numPlayers == 1)
        {
            m_opponentKeeper = null;
        }

        m_health = m_startHealth;

        m_scoreLabel.text = m_score.ToString();

        RefreshHealthBar(false);

        m_startHeight = m_healthBar.height;
    }

    // TODO
    public override IEnumerator Celebrate()
    {
        yield return new WaitForSeconds(3f);
    }

    public override void UpdateScore(int delta = 1)
    {
        PlayAudio(delta);

        if (m_state == State.LevellingUp)
        {
            m_state = State.Timer;
        }

        ++ m_numAnswered;

        if (delta > 0)
        {
            m_score += delta;
        }

        m_scoreLabel.text = m_score.ToString();
        
        if (delta > 0)
        {
            m_health += m_healthGainedOnCorrect;
        }
        else if (delta < 0)
        {
            m_health -= m_healthLostOnIncorrect;
        }

        m_health = Mathf.Clamp(m_health, 0, m_maxHealth);

        if (Mathf.Approximately(m_health, m_maxHealth))
        {
            m_state = State.LevellingUp;
            StartCoroutine(LevelUp());
        }

        if (delta > 0 && m_opponentKeeper != null)
        {
            m_opponentKeeper.UpdateScore(-delta);
        }
    }

    IEnumerator LevelUp()
    {
        int numPlayers = SessionInformation.Instance.GetNumPlayers();

        if (numPlayers == 1)
        {
            m_healthLostPerSecond = StringHelpers.Evalf(System.String.Format(m_levelUpFormula, m_healthLostPerSecond));
        }
        
        InvokeLevelledUp();
        
        Hashtable onTweenArgs = new Hashtable();
        onTweenArgs.Add("position", m_levelUpOnLocation);
        onTweenArgs.Add("time", 0.5f);
        onTweenArgs.Add("easetype", iTween.EaseType.easeOutBounce);
        iTween.MoveTo(m_levelUpTweener, onTweenArgs);

        WingroveAudio.WingroveRoot.Instance.PostEvent("DING");
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");


        while (m_healthBar.height < Mathf.FloorToInt(m_healthBarTargetLocation.transform.localPosition.y))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        m_health = Mathf.Min(m_health, m_startHealth);

        while (m_healthBar.height > m_startHeight)
        {
            yield return null;
        }

        //D.Log("Reset");

        m_state = State.Timer;

        yield return new WaitForSeconds(0.5f);

        Hashtable offTweenArgs = new Hashtable();
        offTweenArgs.Add("position", m_levelUpOffLocalPosition);
        offTweenArgs.Add("time", 0.15f);
        offTweenArgs.Add("easetype", iTween.EaseType.linear);
        offTweenArgs.Add("islocal", true);
        iTween.MoveTo(m_levelUpTweener, offTweenArgs);
    }

    void Update()
    {
        if (m_state == State.Timer)
        {
            m_health -= Time.deltaTime * m_healthLostPerSecond;
            m_health = Mathf.Clamp(m_health, 0, m_maxHealth);
            
            if(m_health <= 0)
            {
                InvokeCompleted();
                m_state = State.Sleep;
            }
        }

        RefreshHealthBar();
    }

    void RefreshHealthBar(bool clampMovement = true)
    {
        int targetBarHeight = (int)(m_health * m_healthBarTargetLocation.localPosition.y / m_maxHealth);
        
        int barMoveAmount = targetBarHeight - m_healthBar.height;
        
        if (clampMovement && Mathf.Abs(barMoveAmount) > m_maxPixelsMovePerFrame)
        {
            barMoveAmount = m_maxPixelsMovePerFrame * barMoveAmount / Mathf.Abs(barMoveAmount);
        }
        
        m_healthBar.height += barMoveAmount;
        
        if (m_opponentScoreHealth != null)
        {
            m_multiplayerBar.height = m_opponentScoreHealth.barHeight;
        }
    }
}
