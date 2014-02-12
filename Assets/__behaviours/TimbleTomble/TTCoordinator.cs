using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class TTCoordinator : Singleton<TTCoordinator> 
{
	public delegate void SettingsChange(TTSettings newSettings);
	public event SettingsChange OnSettingsChange;

	[SerializeField]
	private string[] m_settingsResourceNames;
	[SerializeField]
	private float[] m_magicThresholds;
	//[SerializeField]
	//private GrayscaleEffect m_grayscaleEffect;
	[SerializeField]
	private Transform m_poppedSpriteHierarchy;
	[SerializeField]
	private GameObject m_poppedSpritePrefab;
	[SerializeField]
	private Transform m_popCutoff;
	[SerializeField]
	private string m_settingsResourcePath;
	[SerializeField]
	private UISlider m_slider;
	[SerializeField]
	private TTTroll[] m_trolls;
	[SerializeField]
	private Texture2D m_defaultBackground;
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private string m_freeBackgroundPath;
	[SerializeField]
	private AudioSource m_audioSource;


	private List<GameObject> m_poppedSprites = new List<GameObject>();
	private List<Transform> m_trollMouthPositions = new List<Transform>();

	TTSettings m_settings = null;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(TTInformation.WaitForLoad());

		/*
		Texture2D startBackground = Resources.Load<Texture2D>("tt_backgrounds/" + m_freeBackgroundPath);
		if(startBackground != null)
		{
			m_background.mainTexture = startBackground;
		}
		
		TTInformation.Instance.UnlockItem(startBackground.name);
		*/
		
		foreach(TTTroll troll in m_trolls)
		{
			troll.StartOff();
		}

		TTInformation.Instance.OnMagicChange += OnMagicChange;

		Array.Sort(m_magicThresholds);

		yield return new WaitForSeconds(0.5f);

		FindSettings();

		Debug.Log("m_settings: " + m_settings.name);

		UpdateSlider();
	}

	public void SetBackground(Texture2D newBackground)
	{
		m_background.mainTexture = newBackground;
	}

	void OnMagicChange()
	{
		UpdateSlider();
		FindSettings();
	}

	void OnDestroy()
	{
		if(TTInformation.Instance != null)
		{
			TTInformation.Instance.OnMagicChange -= OnMagicChange;
		}
	}

	void FindSettings()
	{
		float magic = TTInformation.Instance.GetMagic();

		TTSettings settings = null;

		for(int i = 0; i < m_magicThresholds.Length; ++i)
		{
			if(magic < m_magicThresholds[i])
			{
				i = Mathf.Clamp(i, 0, m_settingsResourceNames.Length);
				GameObject go = Resources.Load(m_settingsResourcePath + m_settingsResourceNames[i]) as GameObject;
				settings = go.GetComponent<TTSettings>() as TTSettings;
				break;
			}
		}
		
		if(settings == null)
		{
			GameObject go = Resources.Load(m_settingsResourcePath + m_settingsResourceNames[m_settingsResourceNames.Length - 1]) as GameObject;
			settings = go.GetComponent<TTSettings>() as TTSettings;
		}

		if(m_settings != settings)
		{
			m_settings = settings;

			m_background.mainTexture = settings.m_background;

			Debug.Log("New Settings: " + m_settings.name);

			m_audioSource.clip = m_settings.m_audioClip;
			
			if(m_audioSource.clip != null)
			{
				Debug.Log("Playing");

				m_audioSource.Play();
			}
			else
			{
				Debug.Log("Clip is null");
			}

			if(OnSettingsChange != null)
			{
				OnSettingsChange(m_settings);
			}

			// TODO: Deprecate SettingsUpdate(), they should all subscribe to OnSettingsChange instead
			SettingsUpdate();
		}

		Resources.UnloadUnusedAssets();
	}

	void SettingsUpdate()
	{
		//m_grayscaleEffect.enabled = m_settings.m_grayscale;

		for(int i = 0; i < m_trolls.Length; ++i)
		{
			if(i < m_settings.m_numTrolls)
			{
				m_trolls[i].On();
			}
			else
			{
				m_trolls[i].Off();
			}
		}

		TTPip.Instance.UpdateAnimBehaviour(m_settings);
	}

	public void PlayClip(AudioClip clip)
	{
		if (clip != null)
		{
			GetComponent<AudioSource>().clip = clip;
			GetComponent<AudioSource>().Play();
		}
	}

	public GameObject AddPoppedSprite(string textureName, Texture2D texture, Vector3 position, int price)
	{
		GameObject newPop = 
			SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_poppedSpritePrefab, m_poppedSpriteHierarchy);
		
		newPop.transform.position = position;
		
		newPop.GetComponentInChildren<UITexture>().mainTexture = texture;
		
		m_poppedSprites.Add(newPop);
		
		newPop.GetComponentInChildren<TTPlacedDraggable>().SetUp(m_popCutoff, price);
		
		return newPop;
	}

	public GameObject AddAnimatedSprite(string animatedPrefabName, Vector3 position)
	{
		GameObject newAnimated = null;

		GameObject animatedPrefab = Resources.Load("tt_animatedStickerPrefabs/" + animatedPrefabName) as GameObject;
		if(animatedPrefab != null)
		{
			newAnimated = 
				SpawningHelpers.InstantiateUnderWithIdentityTransforms(animatedPrefab, m_poppedSpriteHierarchy);

			newAnimated.transform.position = position;

			m_poppedSprites.Add(newAnimated);

			newAnimated.GetComponentInChildren<TTPlacedAnimated>().SetUp(m_popCutoff);
		}

		return newAnimated;
	}
	
	public void DestroyPoppedSprite(GameObject poppedSprite)
	{
		m_poppedSprites.Remove(poppedSprite);
		Destroy(poppedSprite);
	}

	public void FeedTroll(GameObject foodItem, Transform foodButton)
	{
		StartCoroutine(FeedTrollCo(foodItem, foodButton));
	}

	IEnumerator FeedTrollCo(GameObject foodItem, Transform foodButton) // TODO: This logic should go into the troll
	{
		Transform closestMouth = null;
		float closestDistance = float.MaxValue;
		foreach(Transform mouth in m_trollMouthPositions)
		{
			if(Vector3.Distance(foodItem.transform.position, mouth.position) < closestDistance)
			{
				closestMouth = mouth;
				closestDistance = Vector3.Distance(foodItem.transform.position, mouth.position);
			}
		}

		iTween.MoveTo(foodItem, closestMouth.position, 0.5f);

		yield return new WaitForSeconds(0.5f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_GULP");
		iTween.ScaleTo(foodItem, Vector3.zero, 0.5f);

		yield return new WaitForSeconds(0.2f);

		closestMouth.parent.GetComponent<TTTroll>().Burp();

		yield return new WaitForSeconds(0.2f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_RANDOM_BURP");

		int price = foodItem.GetComponent<TTPlacedDraggable>().GetPrice();

		TTInformation.Instance.SetMagic(TTInformation.Instance.GetMagic() + (price * 5));
		TTGoldCoinBar.Instance.SpendCoin(foodButton.position, price);

		yield return new WaitForSeconds(0.5f);

		DestroyPoppedSprite(foodItem);
	}

	void UpdateSlider()
	{
		m_slider.value = TTInformation.Instance.GetMagic() / 100f;
	}

	public void AddMouthPosition(Transform mouth)
	{
		m_trollMouthPositions.Add(mouth);
	}

	public void RemoveMouthPosition(Transform mouth)
	{
		m_trollMouthPositions.Remove(mouth);
	}


	bool HasSettings()
	{
		return (m_settings != null);
	}

	public static IEnumerator WaitForSettings()
	{
		while(TTCoordinator.Instance == null)
		{
			yield return null;
		}
		while(!TTCoordinator.Instance.HasSettings())
		{
			yield return null;
		}
	}

	public TTSettings GetSettings()
	{
		return m_settings;
	}
}
