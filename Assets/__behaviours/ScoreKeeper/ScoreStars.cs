using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ScoreStars : ScoreKeeper 
{
    [SerializeField]
    private GameObject m_starPrefab;
    [SerializeField]
    private Transform m_lowTransform;
    [SerializeField]
    private Transform m_highTransform;
    [SerializeField]
    private string m_starOffName;
    [SerializeField]
    private string m_starOnName;
    [SerializeField]
    private float m_starOnTweenDuration;
    [SerializeField]
    private ParticleSystem m_particles; 
    
    List<GameObject> m_spawnedStars = new List<GameObject>();

    int m_starIndex = 0;

    void Start ()
    {
        m_particles.enableEmission = false;
    }

    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);
        
        CollectionHelpers.DestroyObjects(m_spawnedStars);
        
        int divisor = m_targetScore > 1 ? m_targetScore - 1 : m_targetScore;
        Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition) / divisor;
        
        for (int index = 0; index < targetScore; ++index)
        {
            GameObject newMarker = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_starPrefab, m_lowTransform);
            newMarker.transform.localPosition = delta * index;
            m_spawnedStars.Add(newMarker);
        }
    }

    public override void UpdateScore(int delta)
    {
        if (delta > 0)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

            for(int i = 0; i < delta; ++i)
            {
                if(m_starIndex < m_spawnedStars.Count)
                {
                    UISprite starSprite = m_spawnedStars[m_starIndex].GetComponent<UISprite>() as UISprite;
                    starSprite.depth += 2;
                    starSprite.spriteName = m_starOnName;

                    m_particles.transform.position = m_spawnedStars[m_starIndex].transform.position;
                    m_particles.enableEmission = true;

                    float tweenDuration = 1f;
                    iTween.PunchRotation(m_spawnedStars[m_starIndex], new Vector3(0f, 0f, 360f), m_starOnTweenDuration);
                    iTween.PunchScale(m_spawnedStars[m_starIndex], Vector3.one * 5, m_starOnTweenDuration);
                    iTween.ShakePosition(m_spawnedStars[m_starIndex], Vector3.one * 0.02f, m_starOnTweenDuration);

                    StartCoroutine(CompleteStarTween(m_spawnedStars[m_starIndex]));

                    ++m_starIndex;
                }
            }
        }
        else if (delta < 0)
        {
            for(int i = 0; i < Mathf.Abs(delta); ++i)
            {
                --m_starIndex;

                if(m_starIndex > -1)
                {
                    m_spawnedStars[m_starIndex].GetComponent<UISprite>().spriteName = m_starOffName;
                }
            }
        }
    }


    IEnumerator CompleteStarTween(GameObject spawnedStar)
    {
        yield return new WaitForSeconds(m_starOnTweenDuration);
        spawnedStar.GetComponent<UISprite>().depth -=2;
        m_particles.enableEmission = false;
    }
}
