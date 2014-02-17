using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;

public class CelebrationCoordinator : Singleton<CelebrationCoordinator> 
{
	[SerializeField]
	private TweenOnOffBehaviour[] m_trumpets;
	[SerializeField]
	private GameObject m_trumpetParent;
	[SerializeField]
	private GameObject m_rainLetterPrefab;
	[SerializeField]
	private Transform m_spawnLeft;
	[SerializeField]
	private Transform m_spawnRight;
	[SerializeField]
	private GameObject m_explosionLetterParent;
	[SerializeField]
	private GameObject m_troll;
	[SerializeField]
	private Transform m_explosionPosition;
	[SerializeField]
	private GameObject m_explosionLetterPrefab;
	[SerializeField]
	private Transform m_dropFromPosition;
	[SerializeField]
	private GameObject m_topBoundary;
	[SerializeField]
	private GameObject[] m_boundaries;
	[SerializeField]
	private CharacterPopper m_characterPopper;
	[SerializeField]
	private Transform m_textOff;
	[SerializeField]
	private GameObject m_newHighScore;
	[SerializeField]
	private GameObject m_levelUp;

	Vector3 m_textDefaultPos;

	List<GameObject> m_spawnedObjects = new List<GameObject>();

	IEnumerator Start()
	{
		m_textDefaultPos = m_newHighScore.transform.position;
		m_explosionLetterParent.transform.parent.gameObject.SetActive(false);

		yield return new WaitForSeconds(0.3f); // TweenOnOffBehaviour on trumpets need to initialize before we disable the parent
		m_trumpetParent.SetActive(false);
	}

	public IEnumerator Trumpet()
	{
		m_trumpetParent.SetActive(true);
		WingroveAudio.WingroveRoot.Instance.PostEvent("TRUMPET_1_QUIET");

		foreach(TweenOnOffBehaviour trumpet in m_trumpets)
		{
			trumpet.On();
		}

		yield return StartCoroutine(ShakeTrumpets());

		yield return new WaitForSeconds(3f);
	}

	IEnumerator ShakeTrumpets()
	{
		yield return new WaitForSeconds(m_trumpets[0].GetDuration() + 0.05f);
		foreach(TweenOnOffBehaviour trumpet in m_trumpets)
		{
			iTween.ShakePosition(trumpet.gameObject, Vector3.one * 0.02f, 2f);
		}
	}

	public IEnumerator TrumpetOff()
	{
		foreach(TweenOnOffBehaviour trumpet in m_trumpets)
		{
			trumpet.On();
		}

		yield return new WaitForSeconds(m_trumpets[0].GetTotalDurationOff());

		m_trumpetParent.SetActive(false);
	}

	//public IEnumerator RainLetters(float duration = 2.5f, int numPerSpawn = 1)
	public IEnumerator RainLetters(float duration = 2.5f, int numPerSpawn = 2)
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("BUBBLE_MOVE");
		
		m_topBoundary.SetActive(false);
		
		float minX = m_spawnLeft.transform.position.x;
		float maxX = m_spawnRight.transform.position.x;
		
		float timeElapsed = 0;
		while(timeElapsed < duration)
		{
			for(int i = 0; i < numPerSpawn; ++i)
			{
				GameObject newRainLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_rainLetterPrefab, m_spawnLeft);
				m_spawnedObjects.Add(newRainLetter);
				newRainLetter.transform.position = new Vector3(Random.Range(minX, maxX), newRainLetter.transform.position.y, newRainLetter.transform.position.z);
			}
			
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("BUBBLE_MOVE_STOP");
	}

	/*
	public IEnumerator RainLetters(float duration = 6, int numPerSpawn = 5)
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("BUBBLE_MOVE");

		m_topBoundary.SetActive(false);

		float minX = m_spawnLeft.transform.position.x;
		float maxX = m_spawnRight.transform.position.x;

		float timeElapsed = 0;
		while(timeElapsed < duration)
		{
			for(int i = 0; i < numPerSpawn; ++i)
			{
				GameObject newRainLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_rainLetterPrefab, m_spawnLeft);
				m_spawnedObjects.Add(newRainLetter);
				newRainLetter.transform.position = new Vector3(Random.Range(minX, maxX), newRainLetter.transform.position.y, newRainLetter.transform.position.z);
			}

			timeElapsed += Time.deltaTime;
			yield return null;
		}

		WingroveAudio.WingroveRoot.Instance.PostEvent("BUBBLE_MOVE_STOP");
	}
	*/

	public IEnumerator RainLettersNoFloor()
	{
		EnableBoundaries(false);
		
		yield return StartCoroutine(RainLetters(6, 10));

		yield return new WaitForSeconds(2.5f);
		
		EnableBoundaries(true);

		DestroySpawnedObjects();
	}

	public IEnumerator RainLettersThenFall()
	{
		Debug.Log("RainThenFall");
		yield return StartCoroutine(RainLetters());

		yield return new WaitForSeconds(0.8f);

		EnableBoundaries(false);

		yield return new WaitForSeconds(3f);

		EnableBoundaries(true);

		DestroySpawnedObjects();
	}

	public IEnumerator ExplodeLettersOffScreen()
	{
		EnableBoundaries(false);

		yield return StartCoroutine(ExplodeLetters());

		EnableBoundaries(true);

		DestroySpawnedObjects();
	}

	public IEnumerator ExplodeLetters()
	{
		m_troll.SetActive(true);

		float range = 150f;
		float minX = m_explosionLetterParent.transform.localPosition.x - range;
		float maxX = m_explosionLetterParent.transform.localPosition.x + range;

		float minY = m_explosionLetterParent.transform.localPosition.y - range;
		float maxY = m_explosionLetterParent.transform.localPosition.y + range;

		m_explosionLetterParent.transform.parent.gameObject.SetActive(true);
		m_explosionLetterParent.transform.localScale = Vector3.zero;

		float dropTweenDuration = 4.8f;

		Hashtable dropTweenVar = new Hashtable();
		dropTweenVar.Add("position", m_dropFromPosition);
		dropTweenVar.Add("time", dropTweenDuration);
		dropTweenVar.Add("easetype", iTween.EaseType.linear);
		iTween.MoveFrom(m_troll, dropTweenVar);
		iTween.MoveFrom(m_explosionLetterParent, dropTweenVar);

		WingroveAudio.WingroveRoot.Instance.PostEvent("BOMB_WHISTLE");

		//iTween.MoveFrom(m_troll, m_dropFromPosition.position, dropTweenDuration);
		//iTween.MoveFrom(m_explosionLetterParent, m_dropFromPosition.position, dropTweenDuration);

		yield return new WaitForSeconds(dropTweenDuration);

		for(int i = 0; i < 120; ++i)
		{
			GameObject newExplosionLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_explosionLetterPrefab, m_explosionLetterParent.transform);
			m_spawnedObjects.Add(newExplosionLetter);
			newExplosionLetter.transform.localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), newExplosionLetter.transform.localPosition.z);
		}

		float scaleTweenDuration = 0.2f;
		TweenScale.Begin(m_explosionLetterParent, scaleTweenDuration, Vector3.one);
		TweenScale.Begin(m_troll, scaleTweenDuration, Vector3.one * 1.5f);
		yield return new WaitForSeconds(scaleTweenDuration);
		m_troll.SetActive(false);
		Rigidbody[] explosionLetters = m_explosionLetterParent.GetComponentsInChildren<Rigidbody>() as Rigidbody[];
		foreach(Rigidbody letter in explosionLetters)
		{
			letter.AddExplosionForce(Random.Range(0.5f, 3f), m_explosionPosition.position, 0, 0, ForceMode.Impulse);
		}

		WingroveAudio.WingroveRoot.Instance.PostEvent("EXPLOSION_1");

		yield return new WaitForSeconds(3f);
	}

	public IEnumerator PopCharacter(string characterName)
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");

		string spriteName = characterName.ToLower() + "_state_b";
		m_characterPopper.GetComponentInChildren<UISprite>().spriteName = spriteName;

		m_characterPopper.PopCharacter();

		yield return null;
	}

	public IEnumerator NewHighScore(int score, float readDuration = 3f)
	{
		m_newHighScore.GetComponentInChildren<UILabel>().text = score.ToString();
		yield return StartCoroutine(TweenText(m_newHighScore, readDuration));
	}

	public IEnumerator LevelUp(int level, float readDuration = 0.8f)
	{
		m_levelUp.GetComponentInChildren<UILabel>().text = level.ToString();
		WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
		yield return StartCoroutine(TweenText(m_levelUp, readDuration));
	}

	IEnumerator TweenText(GameObject textGo, float readDuration)
	{
		Hashtable onVar = new Hashtable();
		
		float onDuration = 0.8f;
		
		onVar.Add("time", onDuration);
		onVar.Add("position", transform.position);
		onVar.Add("easetype", iTween.EaseType.easeOutBounce);
		
		iTween.MoveTo(textGo, onVar);
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		
		yield return new WaitForSeconds(onDuration + readDuration);
		
		Hashtable offVar = new Hashtable();
		
		float offDuration = 0.4f;
		
		offVar.Add("time", offDuration);
		offVar.Add("position", m_textOff);
		offVar.Add("easetype", iTween.EaseType.linear);
		
		iTween.MoveTo(textGo, offVar);
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
		
		yield return new WaitForSeconds(offDuration + 0.2f);
		
		textGo.transform.position = m_textDefaultPos;
	}

	void EnableBoundaries(bool enable)
	{
		foreach(GameObject boundary in m_boundaries)
		{
			boundary.SetActive(enable);
		}
	}

	void DestroySpawnedObjects()
	{
		for(int i = m_spawnedObjects.Count - 1; i > -1; --i)
		{
			Destroy(m_spawnedObjects[i]);
		}
	}

#if UNITY_EDITOR
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.T))
		{
			StartCoroutine(Trumpet());
		}

		if(Input.GetKeyDown(KeyCode.R))
		{
			StartCoroutine(RainLettersThenFall());
		}

		if(Input.GetKeyDown(KeyCode.E))
		{
			StartCoroutine(ExplodeLettersOffScreen());
		}

		if(Input.GetKeyDown(KeyCode.V))
		{
			StartCoroutine(PopCharacter("rod"));
		}

		if(Input.GetKeyDown(KeyCode.B))
		{
			StartCoroutine(PopCharacter("Pip"));
		}

		if(Input.GetKeyDown(KeyCode.N))
		{
			StartCoroutine(PopCharacter("DOT"));
		}

		if(Input.GetKeyDown(KeyCode.M))
		{
			StartCoroutine(PopCharacter("bunny"));
		}
	}
#endif
}
