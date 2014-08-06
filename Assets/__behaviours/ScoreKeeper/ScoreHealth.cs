using UnityEngine;
using System.Collections;

public class ScoreHealth : ScoreKeeper
{
    public event ScoreKeeperEventHandler LevelledUp;

    [SerializeField]
    private int m_maxPixelsMovePerFrame;
    [SerializeField]
    private UISprite m_healthBar;
    [SerializeField]
    private Transform m_healthBarTargetLocation;
    [SerializeField]
    private int m_maxHealth;
    [SerializeField]
    private int m_startHealth;
    [SerializeField]
    private float m_healthGainedOnCorrect;
    [SerializeField]
    private float m_healthLostOnIncorrect;
    [SerializeField]
    private float m_healthLostPerSecond;
    [SerializeField]
    private string m_levelUpFormula;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private GameObject m_multiplayerParent;
    [SerializeField]
    private UISprite m_multiplayerBar;
    [SerializeField]
    private ScoreHealth m_opponentKeeper;
    [SerializeField]
    private GameObject m_debuggingLevelUpGo;

    float m_health;

    public int barHeight
    {
        get
        {
            return m_healthBar.height;
        }
    }

    void Start()
    {
        m_debuggingLevelUpGo.SetActive(false);

        int numPlayers = SessionInformation.Instance.GetNumPlayers();

        m_multiplayerParent.SetActive(numPlayers == 2);
        m_scoreLabel.gameObject.SetActive(numPlayers == 1);

        m_health = m_startHealth;

        m_scoreLabel.text = m_score.ToString();

        RefreshBar(false);
    }

    public override void UpdateScore(int delta = 1)
    {
        base.UpdateScore(delta);

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
            m_healthLostPerSecond = StringHelpers.Evalf(System.String.Format(m_levelUpFormula, m_healthLostPerSecond));
            if(LevelledUp != null)
            {
                LevelledUp(this);
            }

            m_debuggingLevelUpGo.SetActive(true);
            StartCoroutine(DisableDebugLevelUpGo());
        }

        RefreshBar(true);
    }

    IEnumerator DisableDebugLevelUpGo()
    {
        yield return new WaitForSeconds(0.5f);
        m_debuggingLevelUpGo.SetActive(false);
    }

    public void StartTimer()
    {
        StartCoroutine("StartTimerCo");
    }

    IEnumerator StartTimerCo()
    {
        while (true)
        {
            m_health -= Time.deltaTime * m_healthLostPerSecond;
            m_health = Mathf.Clamp(m_health, 0, m_maxHealth);

            RefreshBar(true);
            
            if(m_health <= 0)
            {
                InvokeCompleted();
                StopCoroutine("StartTimerCo");
            }
            
            yield return null;
        }
    }
    
    public void PauseTimer()
    {
        StopCoroutine("StartTimerCo");
    }

    void RefreshBar(bool clampMoveDistance)
    {
        int targetBarHeight = (int)(m_health * m_healthBarTargetLocation.localPosition.y / m_maxHealth);

        int barMoveAmount = targetBarHeight - m_healthBar.height;

        if (clampMoveDistance && Mathf.Abs(barMoveAmount) > m_maxPixelsMovePerFrame)
        {
            barMoveAmount = m_maxPixelsMovePerFrame * barMoveAmount / Mathf.Abs(barMoveAmount);
        }

        m_healthBar.height += barMoveAmount;

        if (m_opponentKeeper != null)
        {
            m_multiplayerBar.height = m_opponentKeeper.barHeight;
        }
    }
}
