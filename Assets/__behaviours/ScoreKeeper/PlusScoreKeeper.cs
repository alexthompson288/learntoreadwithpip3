﻿using UnityEngine;
using System.Collections;

public class PlusScoreKeeper : ScoreKeeper
{
    public event ScoreKeeperEventHandler LevelledUp;

    [SerializeField]
    protected int m_maxHealth;
    [SerializeField]
    protected int m_startHealth;
    [SerializeField]
    protected float m_healthGainedOnCorrect;
    [SerializeField]
    protected float m_healthLostOnIncorrect;
    [SerializeField]
    protected float m_healthLostPerSecond;
    [SerializeField]
    protected string m_levelUpFormula;

    protected PlusScoreKeeper m_opponentKeeper;

    protected enum State
    {
        Sleep,
        Timer,
        LevellingUp
    }


    protected State m_state;
    protected float m_health;


    protected virtual void SetOpponentCharacterIcon(int opponentCharacterIndex){}
    public virtual void SetCharacterIcon(int characterIndex){}

    protected virtual void Awake()
    {
        PlusScoreKeeper[] plusScoreKeepers = Object.FindObjectsOfType(typeof(PlusScoreKeeper)) as PlusScoreKeeper[];

        for (int i = 0; i < plusScoreKeepers.Length; ++i)
        {
            if(plusScoreKeepers[i] != this)
            {
                m_opponentKeeper = plusScoreKeepers[i];
                break;
            }
        }
    }

    public void SetLevelUpFormula(string myLevelUpFormula)
    {
        m_levelUpFormula = myLevelUpFormula;
    }
    
    public void SetHealthLostPerSecond(float myHealthLostPerSecond)
    {
        m_healthLostPerSecond = myHealthLostPerSecond;
    }
    
    public void StartTimer()
    {
        m_state = State.Timer;
    }

    protected void InvokeLevelledUp()
    {
        if (LevelledUp != null)
        {
            LevelledUp(this);
        }
    }
}
