using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwatFliesPlayer : GamePlayer
{
    [SerializeField]
    private int m_playerIndex;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;

    int m_score;

    List<GameWidget> m_spawnedFlies = new List<GameWidget>();
}
