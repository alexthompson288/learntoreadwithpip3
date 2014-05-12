using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCoordinator : Singleton<GameCoordinator> 
{
    [SerializeField]
    protected ScoreKeeper m_scoreKeeper;
    [SerializeField]
    protected int m_targetScore;
    
    protected int m_score = 0;
    
    protected List<DataRow> m_dataPool = new List<DataRow>();

    protected DataRow m_currentData = null;

    protected void ClampTargetScore()
    {
        m_targetScore = Mathf.Clamp(m_targetScore, 0, m_dataPool.Count);
    }

    protected virtual IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(2f);

        GameManager.Instance.CompleteGame();
    }

    protected DataRow GetRandomData()
    {
        return m_dataPool.Count > 0 ? m_dataPool[Random.Range(0, m_dataPool.Count)] : null;
    }
}
