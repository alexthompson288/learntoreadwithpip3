using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LettersStarBar : MonoBehaviour {

    [SerializeField]
    private GameObject m_starPrefab;
    [SerializeField]
    private string m_starOffString;
    [SerializeField]
    private string m_starOnString;
	[SerializeField]
	private GameObject m_goldCoinPrefab;
    [SerializeField]
    private Transform m_lowTransform;
    [SerializeField]
    private Transform m_highTransform;
	[SerializeField]
	private float m_starOnTweenDuration;
	[SerializeField]
	private ParticleSystem m_particles;
	[SerializeField]
	private Transform m_unlockLettersBoardOnPosition;
	[SerializeField]
	private Transform m_starTweenFrom;

    List<GameObject> m_spawnedStars = new List<GameObject>();
	
	bool m_secondSpawn = false;
	
	IEnumerator Start ()
	{
		m_particles.enableEmission = false;
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		SetUp();
	}
	
	void SetUp()
	{
		int sectionId = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds[SessionInformation.Instance.GetDifficulty()];
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
		
		int starsEarned = SessionInformation.Instance.GetStarsEarnedForHighestLevel();
		int starsTotal = dt.Rows.Count;
		
		
		//if(starsEarned >= starsTotal)
		//{
			//SessionInformation.Instance.SetHighestLevelCompletedForApp(SessionInformation.Instance.GetHighestLevelCompletedForApp() + 1);
			
			//SetUp(); // Recursive call because starsTarget and starsEarned need new values
		//}
		//else
		//{
			StartCoroutine(SpawnStars(starsEarned, starsTotal));
		//}
	}
	
	// TODO: Refactor function. It is currently clunky and has unnecessary duplication
    public IEnumerator SpawnStars(int starsEarned, int starsTotal)
    {
        Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition)
            / (starsTotal - 1);
		
        for (int index = 0; index < starsTotal; ++index)
        {
            GameObject newStar = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_starPrefab,
                m_lowTransform);
            newStar.transform.localPosition = delta * index;
            m_spawnedStars.Add(newStar);
			
			if(index < starsEarned)
			{
				m_spawnedStars[index].GetComponent<UISprite>().spriteName = m_starOnString;
				
				GameObject starToTween = m_spawnedStars[index];
				if(m_secondSpawn) 
				{
					WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
					m_particles.enableEmission = true;
					starToTween.GetComponent<UISprite>().depth += 2;
					//starToTween.GetComponent<ParticleSystem>().enableEmission = true;
					float tweenDuration = 1f;
					iTween.MoveFrom(starToTween, m_starTweenFrom.position, m_starOnTweenDuration);
					iTween.ScaleFrom(starToTween, Vector3.one * 10, m_starOnTweenDuration * 2);
					StartCoroutine(OnStarTweenFinish(starToTween));
				}
			}
        }
		
		if(starsEarned > 0 && SessionInformation.Instance.GetHasWonRecently() && !m_secondSpawn)
		{
			Debug.Log("Tweening last");
			
			GameObject starToTween;
			
			int lastEarnedStarIndex = starsEarned > starsTotal ? (m_spawnedStars.Count - 1) : (starsEarned - 1);			
			starToTween = m_spawnedStars[lastEarnedStarIndex];

			WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
			m_particles.enableEmission = true;
			starToTween.GetComponent<UISprite>().depth += 2;
			//starToTween.GetComponent<ParticleSystem>().enableEmission = true;
			float tweenDuration = 1f;
			iTween.MoveFrom(starToTween, m_starTweenFrom.position, m_starOnTweenDuration);
			iTween.ScaleFrom(starToTween, Vector3.one * 10, m_starOnTweenDuration * 2);
			StartCoroutine(OnStarTweenFinish(starToTween));
		}
		
		SessionInformation.Instance.SetHasWonRecently(false);
		
		if(starsEarned >= starsTotal)
		{
			yield return new WaitForSeconds(3f);
			
			Debug.Log("Destroying old stars");
			
			SessionInformation.Instance.SetHighestLevelCompletedForApp(SessionInformation.Instance.GetHighestLevelCompletedForApp() + 1);
			SessionInformation.Instance.SetStarsEarnedForHighestLevel(starsEarned - starsTotal);
			m_secondSpawn = true;
			
			foreach(GameObject star in m_spawnedStars)
			{
				StartCoroutine(star.GetComponent<Star>().Off());
			}
			m_spawnedStars.Clear();
			 
			yield return new WaitForSeconds(1f);
			Debug.Log("Creating old stars");
			
			SetUp();
		}

    }
	
	IEnumerator OnStarTweenFinish(GameObject spawnedStar)
	{
		yield return new WaitForSeconds(m_starOnTweenDuration);
		spawnedStar.GetComponent<UISprite>().depth -=2;
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		GameObject goldCoin = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_goldCoinPrefab, spawnedStar.transform);
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(goldCoin.GetComponent<GoldCoin>().TweenToPosition(m_unlockLettersBoardOnPosition.position));
	}
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LettersStarBar : MonoBehaviour {

    [SerializeField]
    private GameObject m_starPrefab;
    [SerializeField]
    private string m_starOffString;
    [SerializeField]
    private string m_starOnString;
	[SerializeField]
	private GameObject m_goldCoinPrefab;
    [SerializeField]
    private Transform m_lowTransform;
    [SerializeField]
    private Transform m_highTransform;
	[SerializeField]
	private float m_starOnTweenDuration;
	[SerializeField]
	private ParticleSystem m_particles;
	[SerializeField]
	private Transform m_unlockLettersBoardOnPosition;
	[SerializeField]
	private Transform m_starTweenFrom;

    List<GameObject> m_spawnedStars = new List<GameObject>();
	
	int m_starsTarget;
	int m_starsEarned;
	
	IEnumerator Start ()
	{
		m_particles.enableEmission = false;
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		SetUp();
	}
	
	void SetUp()
	{
		int sectionId = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds[SessionInformation.Instance.GetDifficulty()];
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
		
		int starsEarned = SessionInformation.Instance.GetStarsEarnedForHighestLevel();
		int starsTotal = dt.Rows.Count;
		
		
		if(starsEarned > starsTotal)
		{
			SessionInformation.Instance.SetHighestLevelCompletedForApp(SessionInformation.Instance.GetHighestLevelCompletedForApp() + 1);
			
			SetUp(); // Recursive call because starsTarget and starsEarned need new values
		}
		else
		{
			SpawnStars(starsEarned, starsTotal);
		}
	}

    public void SpawnStars(int starsEarned, int starsTotal)
    {
        Vector3 delta = (m_highTransform.transform.localPosition - m_lowTransform.transform.localPosition)
            / (starsTotal - 1);
		
        for (int index = 0; index < starsTotal; ++index)
        {
            GameObject newStar = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_starPrefab,
                m_lowTransform);
            newStar.transform.localPosition = delta * index;
            m_spawnedStars.Add(newStar);
			
			if(index < starsEarned)
			{
				m_spawnedStars[index].GetComponent<UISprite>().spriteName = m_starOnString;
			}
        }
		
		if(starsEarned > 0 && SessionInformation.Instance.GetHasWonRecently())
		{
			GameObject starToTween = m_spawnedStars[starsEarned - 1];
			WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
			m_particles.enableEmission = true;
			starToTween.GetComponent<UISprite>().depth += 2;
			//starToTween.GetComponent<ParticleSystem>().enableEmission = true;
			float tweenDuration = 1f;
			iTween.MoveFrom(starToTween, m_starTweenFrom.position, m_starOnTweenDuration);
			iTween.ScaleFrom(starToTween, Vector3.one * 10, m_starOnTweenDuration * 2);
			StartCoroutine(OnStarTweenFinish(starToTween));
		}
		
		SessionInformation.Instance.SetHasWonRecently(false);
    }
	
	IEnumerator OnStarTweenFinish(GameObject spawnedStar)
	{
		yield return new WaitForSeconds(m_starOnTweenDuration);
		spawnedStar.GetComponent<UISprite>().depth -=2;
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		GameObject goldCoin = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_goldCoinPrefab, spawnedStar.transform);
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(goldCoin.GetComponent<GoldCoin>().TweenToPosition(m_unlockLettersBoardOnPosition.position));
	}
}
*/