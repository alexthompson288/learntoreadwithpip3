using UnityEngine;
using System.Collections;
using Wingrove;
using System;
using System.Collections.Generic;
using System.Linq;

public class BubbleMapCoordinator : MonoBehaviour 
{
	[SerializeField]
	private UILabel m_highScoreLabel;

#if UNITY_EDITOR
	[SerializeField]
	private bool m_resetProgress;
#endif

	private static bool m_isStandalone;
	public static bool isStandalone
	{
		get
		{
			return m_isStandalone;
		}
	}

	static bool m_newHighScore;
	public static void SetNewHighScore(bool newHighScore)
	{
		m_newHighScore = newHighScore;
	}

	static bool m_leveledUp;
	public static void SetLeveledUp(bool leveledUp)
	{
		m_leveledUp = leveledUp;
	}

	static string m_bubbleGame;
	public static void SetBubbleGame(string bubbleGame)
	{
		m_bubbleGame = bubbleGame;
	}

	public static string GetBubbleGame()
	{
		return m_bubbleGame;
	}

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase()); // Not strictly necessary to wait for database, 
		                                                               // but we do need to make sure that GameDataBridge.Instance is not null

		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Sets);

		m_isStandalone = true;

		if(String.IsNullOrEmpty(m_bubbleGame))
		{
			m_bubbleGame = "NewBubblePopLetters";
		}
		
		SkillProgressInformation.Instance.SetCurrentSkill(m_bubbleGame);

#if UNITY_EDITOR
		if(m_resetProgress)
		{
			PlayerPrefs.SetInt("NewBubblePopLetters", 0);
			SkillProgressInformation.Instance.SetProgress("NewBubblePopLetters", 0);

			PlayerPrefs.SetInt("NewBubblePopWords", 0);
			SkillProgressInformation.Instance.SetProgress("NewBubblePopWords", 0);
		}
#endif

		ClickEvent[] points = UnityEngine.Object.FindObjectsOfType(typeof(ClickEvent)) as ClickEvent[];
		Array.Sort(points, ComparePointsByName);

		Debug.Log("CurrentSkillProgress: " + SkillProgressInformation.Instance.GetCurrentSkillProgress());

		int maxLevel = SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1;

		if(maxLevel < 2)
		{
			JourneyPip.Instance.SetCurrentPoint(points[0].transform);
		}
		else if(maxLevel >= points.Length - 1)
		{
			JourneyPip.Instance.SetCurrentPoint(points[points.Length - 1].transform);
		}

		Debug.Log("Max Level: " + maxLevel);
		
		for(int i = 0; i < points.Length; ++i)
		{
			points[i].OnSingleClick += OnPointClick;
			
			int level = GetPointLevel(points[i]);
			
			if(level > maxLevel || level == -1)
			{
				points[i].transform.FindChild("Face").GetComponent<UISprite>().spriteName = "icon_head_troll";
				Destroy(points[i].GetComponent<WobbleGUIElement>());
			}
			else if(level == maxLevel)
			{
				JourneyPip.Instance.SetCurrentPoint(points[i].transform);
			}
		}


		// High Score

		if(!PlayerPrefs.HasKey(m_bubbleGame))
		{
			PlayerPrefs.SetInt(m_bubbleGame, 0);
		}
		
		int highScore = PlayerPrefs.GetInt(m_bubbleGame);
		m_highScoreLabel.text = highScore.ToString();

		/*
		if(m_newHighScore && highScore != 0)
		{
			//Debug.Log(
			yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
			StartCoroutine(CelebrationCoordinator.Instance.NewHighScore(highScore));
			yield return StartCoroutine(CelebrationCoordinator.Instance.ExplodeLettersOffScreen());
		}

		m_newHighScore = false;
		*/


		// Level Up
		if(m_leveledUp)
		//if(1 == 1)
		{
			StartCoroutine(CelebrationCoordinator.Instance.RainLettersThenFall());
			yield return new WaitForSeconds(2f);
			StartCoroutine(CelebrationCoordinator.Instance.LevelUp(maxLevel, 2f));
		}

		m_leveledUp = false;

		/*
		foreach(ClickEvent point in points)
		{
			if(GetPointLevel(point) == SkillProgressInformation.Instance.GetCurrentSkillProgress() + 1)
			{
				yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
				point.transform.FindChild("Face").GetComponent<UISprite>().spriteName = "icon_head_troll";
				WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_GULP");
				float tweenDuration = 0.5f;
				TweenScale.Begin(point.gameObject, tweenDuration, Vector3.zero);
				yield return new WaitForSeconds(tweenDuration + 0.8f);
				point.transform.FindChild("Face").GetComponent<UISprite>().spriteName = "icon_head_pip";
				WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
				TweenScale.Begin(point.gameObject, tweenDuration, Vector3.one);
				yield return new WaitForSeconds(tweenDuration + 0.8f);

				WobbleGUIElement wobbleBehaviour = point.gameObject.AddComponent<WobbleGUIElement>() as WobbleGUIElement;
				wobbleBehaviour.SetAmount(3);
				wobbleBehaviour.SetSpeed(2);
			}
		}
		*/
	}

#if UNITY_EDITOR
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.D))
		{
			StartCoroutine(CelebrationCoordinator.Instance.NewHighScore(6));
			StartCoroutine(CelebrationCoordinator.Instance.ExplodeLettersOffScreen());
		}
	}
#endif

	void OnPointClick(ClickEvent point)
	{
		int level = GetPointLevel(point);
		
		if(point.GetComponent<WobbleGUIElement>() != null)
		{
			Debug.Log("Level: " + level);

			SkillProgressInformation.Instance.SetCurrentLevel(level); 


			TransitionScreen.Instance.ChangeLevel(m_bubbleGame, false);
		}
		else
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("HAPPY_GAWP");
		}
	}

	int GetPointLevel(ClickEvent point)
	{
		try
		{
			return Convert.ToInt32(point.transform.parent.name);
		}
		catch
		{
			return -1;
		}
	}
	
	private static int ComparePointsByName(ClickEvent a, ClickEvent b)
	{
		if(a == null)
		{
			if(b == null)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
		else
		{
			if(b == null)
			{
				return 1;
			}
			else
			{
				string aName = a.transform.parent.name;
				string bName = b.transform.parent.name;
				
				if(aName.Length == bName.Length)
				{
					return a.transform.parent.name.CompareTo(b.transform.parent.name);
				}
				else if(aName.Length < bName.Length)
				{
					return -1;
				}
				else
				{
					return 1;
				}
			}
		}
	}
}
