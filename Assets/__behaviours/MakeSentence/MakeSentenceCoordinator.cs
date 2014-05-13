using UnityEngine;
using System.Collections;

public class MakeSentenceCoordinator : GameCoordinator 
{
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetSentences();

        ClampTargetScore();
    }
}
