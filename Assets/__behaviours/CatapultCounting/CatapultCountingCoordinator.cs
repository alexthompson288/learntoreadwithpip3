using UnityEngine;
using System.Collections;

public class CatapultCountingCoordinator : GameCoordinator 
{
    [SerializeField]
    private CatapultBehaviour m_catapult;
    [SerializeField]
    private CountingTarget m_target;
    [SerializeField]
    private DataDisplay m_display;

	// Use this for initialization
	IEnumerator Start () 
    {
        m_catapult.MoveToMultiplayerLocation(0);
        m_scoreKeeper.SetTargetScore(m_targetScore);
        m_target.OnTargetHit += OnTargetHit;

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

    void OnTargetHit(Target target, Collider other)
    {
        m_target.StoreAmmo(other);

        AudioClip clip = LoaderHelpers.LoadAudioForNumber(m_target.storedAmmoCount);

        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }

        Resources.UnloadUnusedAssets();

        if (m_target.storedAmmoCount >= System.Convert.ToInt32(m_currentData ["value"]))
        {
            ++m_score;
            m_scoreKeeper.UpdateScore();

            StartCoroutine(ClearQuestion());
        }
    }

    IEnumerator ClearQuestion()
    {
        yield return new WaitForSeconds(0.8f);

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
