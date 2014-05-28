using UnityEngine;
using System.Collections;

public class CatapultCountingCoordinator : GameCoordinator 
{
    [SerializeField]
    private CountingTarget m_target;
    [SerializeField]
    private PictureDisplay m_display;

    int m_numHits = 0;

	// Use this for initialization
	IEnumerator Start () 
    {
        m_scoreKeeper.SetTargetScore(m_targetScore);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        m_dataPool = DataHelpers.OnlyLowNumbers(m_dataPool, m_target.locatorCount);

        if (m_dataPool.Count > 0)
        {
            AskQuestion();
        }
        else
        {
            StartCoroutine(CompleteGame());
        }
	}

    void AskQuestion()
    {
        m_currentData = GetRandomData();

        m_display.On("numbers", m_currentData);

        StartCoroutine(m_target.On(0));
    }

    void OnTargetHit(CountingTarget target, Collider other)
    {
        target.StoreAmmo(other);

        ++m_numHits;

        if (m_numHits >= System.Convert.ToInt32(m_currentData ["value"]))
        {
            ++m_score;
            m_scoreKeeper.UpdateScore();

            StartCoroutine(ClearQuestion());
        }
    }

    IEnumerator ClearQuestion()
    {
        m_target.Off();
        m_display.Off();
        
        yield return new WaitForSeconds(0.5f);

        if (m_score < m_targetScore)
        {
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
}
