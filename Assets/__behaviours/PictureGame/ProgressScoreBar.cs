using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ProgressScoreBar : MonoBehaviour {

    [SerializeField]
    private UISlider m_slider;
    [SerializeField]
    private UILabel m_scoreLabel;
    [SerializeField]
    private GameObject m_starSprite;
    [SerializeField]
    private string m_starOffString;
    [SerializeField]
    private string m_starOnString;
    [SerializeField]
    private Transform m_lowTransform;
    [SerializeField]
    private Transform m_highTransform;
	[SerializeField]
	private float m_starOnTweenDuration;
	[SerializeField]
	private ParticleSystem m_particles; 

    List<GameObject> m_spawnedStars = new List<GameObject>();
	
	void Start ()
	{
		m_particles.enableEmission = false;
	}

    public void SetTimer(float amt)
    {
        m_slider.value = amt;
    }

    public void SetScore(int num)
    {
        m_scoreLabel.text = num.ToString();
    }

    public void SetStarsTarget(int numStars)
    {
        foreach (GameObject oldStar in m_spawnedStars)
        {
            Destroy(oldStar);
        }
        m_spawnedStars.Clear();

        Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition)
            / (numStars - 1);

        for (int index = 0; index < numStars; ++index)
        {
            GameObject newStar = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_starSprite,
                m_lowTransform);
            newStar.transform.localPosition = delta * index;
            m_spawnedStars.Add(newStar);
        }
    }

    public void SetStarsCompleted(int numStars)
    {
        for (int index = 0; index < numStars; ++index)
        {
            if (index < m_spawnedStars.Count)
            {
                if (m_spawnedStars[index].GetComponent<UISprite>().spriteName != m_starOnString)
                {
					WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
					m_particles.transform.position = m_spawnedStars[index].transform.position;
					m_particles.enableEmission = true;
					m_spawnedStars[index].GetComponent<UISprite>().depth += 2;
					float tweenDuration = 1f;
					iTween.PunchRotation(m_spawnedStars[index], new Vector3(0f, 0f, 360f), m_starOnTweenDuration);
					iTween.PunchScale(m_spawnedStars[index], Vector3.one * 5, m_starOnTweenDuration);
                    iTween.ShakePosition(m_spawnedStars[index], Vector3.one * 0.02f, m_starOnTweenDuration);
					StartCoroutine(OnStarTweenFinish(m_spawnedStars[index]));
                }
                m_spawnedStars[index].GetComponent<UISprite>().spriteName = m_starOnString;
            }
        }
        for (int index = numStars; index < m_spawnedStars.Count; ++index)
        {
            m_spawnedStars[index].GetComponent<UISprite>().spriteName = m_starOffString;
        }
    }
	
	IEnumerator OnStarTweenFinish(GameObject spawnedStar)
	{
		yield return new WaitForSeconds(m_starOnTweenDuration);
		spawnedStar.GetComponent<UISprite>().depth -=2;
		m_particles.enableEmission = false;
	}
}
