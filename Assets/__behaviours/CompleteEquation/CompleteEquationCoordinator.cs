using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompleteEquationCoordinator : GameCoordinator 
{
    [SerializeField]
    private Transform m_firstIntLocation;
    [SerializeField]
    private Transform m_operationLocation;
    [SerializeField]
    private Transform m_secondIntLocation;
    [SerializeField]
    private Transform m_answerLocation;
    [SerializeField]
    private float m_probabilityCurrentIsInteger;

    List<DataRow> m_operators = new List<DataRow>();

    IEnumerator Start()
    {
        m_probabilityCurrentIsInteger = Mathf.Clamp01(m_probabilityCurrentIsInteger);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetNumbers();

        m_operators = DataHelpers.GetArithmeticOperators();
    }

    void AskQuestion()
    {
        DataRow answer = GetRandomData();

        bool currentIsInteger = Random.Range(0f, 1f) < m_probabilityCurrentIsInteger;

        //m_currentData = currentIsInteger ? m_dataPool [Random.Range(0, m_dataPool.Count)] : m_operators [Random.Range(0, m_operators.Count)];
    }
}
