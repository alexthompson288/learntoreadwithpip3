using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class AlphabetBookCoordinator : MonoBehaviour 
{
	[SerializeField]
	private LetterButton m_letterButton;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private string[] m_backgroundNames;
	[SerializeField]
	private GameObject m_alphabetBookBackgroundPrefab;

	Dictionary<GameObject, string> m_spawnedBackgrounds = new Dictionary<GameObject, string>();

	ThrobGUIElement m_currentThrobBehaviour = null;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("ALPHABET_BOOK_INSTRUCTIONS");

		for(int i = 0; i < m_locators.Length && i < m_backgroundNames.Length; ++i)
		{
			GameObject newBackground = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_alphabetBookBackgroundPrefab, m_locators[i]);
			newBackground.GetComponent<ClickEvent>().OnSingleClick += OnBackgroundClick;
			newBackground.GetComponentInChildren<UISprite>().spriteName = m_backgroundNames[i];
			m_spawnedBackgrounds.Add(newBackground, m_backgroundNames[i]);
		}

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		//List<DataRow> phonemes = GameDataBridge.Instance.GetSectionLetters(1405);
		List<DataRow> phonemes = GameDataBridge.Instance.GetSectionLetters();

		if(phonemes.Count > 0)
		{
			m_letterButton.SetUp(phonemes[0]);

			JourneyInformation.Instance.SetLastLetterUnlocked(phonemes[0]);

			string spriteName = AlphabetBookInformation.Instance.GetTexture(System.Convert.ToInt32(phonemes[0]["id"]));

			// If we have already got a spriteName for this phoneme then make that sprite throb
			if(spriteName != null) 
			{
				foreach(KeyValuePair<GameObject, string> kvp in m_spawnedBackgrounds)
				{
					if(kvp.Value == spriteName)
					{
						m_currentThrobBehaviour = kvp.Key.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
						m_currentThrobBehaviour.On();
					}
				}
			}
			// If there is no spriteName for this phoneme then save the default sprite, just in case the player exits without selecting anything
			else
			{
				AlphabetBookInformation.Instance.AddTexture(phonemes[0], "icon_circle_white");
			}
		}
	}

	void OnBackgroundClick(ClickEvent clickEvent)
	{
		if(m_currentThrobBehaviour != null)
		{
			m_currentThrobBehaviour.Off();
		}

		m_currentThrobBehaviour = clickEvent.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
		m_currentThrobBehaviour.On();

		string spriteName = m_spawnedBackgrounds[clickEvent.gameObject];

#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("SpriteName", spriteName);
		FlurryBinding.logEventWithParameters("AlphabetBookChoose", ep, false);
#endif

		m_letterButton.SetBackgroundSpriteName(spriteName);
		AlphabetBookInformation.Instance.AddTexture(m_letterButton.GetData(), spriteName);
	}
}
