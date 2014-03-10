using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TransitionScreen : Singleton<TransitionScreen> 
{
	[SerializeField]
	private Texture2D[] m_backgroundTextures;
	[SerializeField]
	private AudioClip[] m_transitionSounds;
	
	public GameObject ScreenCover;

    static Stack<string> m_backStack = new Stack<string>();

    static string m_loadingToScene = null;
    static string m_emptySceneName = "DeliberatelyEmptyScene";

    public bool m_isEmptyScene;

	static int m_currentTextureIndex;

	bool m_screenHasExited = false;

	public static IEnumerator WaitForScreenExit()
	{
		while(TransitionScreen.Instance == null)
		{
			yield return null;
		}

		while(!TransitionScreen.Instance.m_screenHasExited)
		{
			yield return null;
		}
	}

	// Use this for initialization
	IEnumerator Start () 
    {
#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("Name", Application.loadedLevelName);
		FlurryBinding.logEventWithParameters("NewLevel", ep, true);
#endif

		//Debug.Log("TransitionScreen.Start()");

		ScreenCover.GetComponent<UITexture>().mainTexture = m_backgroundTextures[m_currentTextureIndex];
		if (m_isEmptyScene)
		{
			Debug.Log("EmptyScene");
		}
		Debug.Log("Unloading unused assets");
		Resources.UnloadUnusedAssets();
		GC.Collect();
		Debug.Log("Total Memory: " + GC.GetTotalMemory(false));
        if (m_isEmptyScene)
        {
			//Debug.Log("Unloading unused assets");
            //Resources.UnloadUnusedAssets();
            //GC.Collect();
            yield return new WaitForSeconds(0.2f);
            if (m_loadingToScene == null)
            {
                Debug.Log("Moving to scene 0");

#if UNITY_IPHONE
				ep = new Dictionary<string, string>();
				ep.Add("Name", Application.loadedLevelName);
				FlurryBinding.endTimedEvent("NewLevel", ep);
#endif

                Application.LoadLevel(0);
            }
            //else if(m_loadingToScene != "NewVoyage")
			else
            {
                Debug.Log("Moving to scene " + m_loadingToScene);

#if UNITY_IPHONE
				ep = new Dictionary<string, string>();
				ep.Add("Name", Application.loadedLevelName);
				FlurryBinding.endTimedEvent("NewLevel", ep);
#endif

                Application.LoadLevel(m_loadingToScene);

				while(Application.isLoadingLevel)
				{
					Debug.Log("Loading " + m_loadingToScene);
					yield return null;
				}
            }
			/*
			else
			{
				Debug.Log("TEMPORARY TEST");
				Debug.Log("Don't move to NewVoyage");
				Debug.Log("Stay in DeliberatelyEmptyScene");
				Debug.Log("Current Scene: " + Application.loadedLevelName);
			}
			*/
            yield break;
        }

		//Debug.Log("About to tween transition screen");

        yield return null;
		Vector3 newPos=new Vector3(1024.0f, 0.0f, 0.0f);
		var config=new GoTweenConfig()
			.vector3Prop( "localPosition", newPos )
			.setEaseType( GoEaseType.QuadInOut );

		GoTween tween=new GoTween(ScreenCover.transform, 0.8f, config);
		tween.setOnCompleteHandler(c => FinishedIntro());

		Go.addTween(tween);

		//Debug.Log("Called tween for transition screen");

		if(m_transitionSounds.Length > 0)
		{
			AudioClip clip = m_transitionSounds[UnityEngine.Random.Range(0, m_transitionSounds.Length)];
			if(clip != null)
			{
				GetComponent<AudioSource>().clip = clip;
				GetComponent<AudioSource>().Play();
			}
		}

		//Debug.Log("Played audio");

		//Debug.Log("FINISHED TransitionScreen.Start()");

		/*
        if (WingroveAudio.WingroveRoot.Instance != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRANSITION_ON");
        }
        */

		yield return new WaitForSeconds(0.8f); // 0.8f is duration of ScreenCover tween

		m_screenHasExited = true;
	}
	
	void Awake () {
        ScreenCover.SetActive(true);
        ScreenCover.transform.localPosition = Vector3.zero;
		if ( SettingsHolder.Instance != null )
		{
			Texture2D texture = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_transitionScreenTexture;
			if ( texture != null )
			{
				ScreenCover.GetComponent<UITexture>().mainTexture = texture;
			}
		}
	}

    public void GoBack(bool addToStack = false)
    {
        while (true)
        {
            if (m_backStack.Count == 0)
            {
                Debug.Log("Back stack empty, going to start menu!");
                ChangeLevel(null, false);
                break;
            }
            else
            {
                string levelName = m_backStack.Pop();
                Debug.Log("Pulled " + levelName + " from back stack!");
                if (levelName != Application.loadedLevelName)
                {
                    Debug.Log("Is another scene - let's go!");
                    ChangeLevel(levelName, false);
                    break;
                }
            }
        }
    }

	public void ChangeLevel(string level, bool addToStack)
	{
		int stackCount = m_backStack.Count;
		Debug.Log("stack.Count: " + stackCount);
		
		if(stackCount > 0)
		{
			Debug.Log("stackCount: " + stackCount);
			Debug.Log("stack.Peek(): " + m_backStack.Peek());
		}
		
		if ( SettingsHolder.Instance != null )
		{
			Texture2D texture = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_transitionScreenTexture;
			if ( texture != null )
			{
				ScreenCover.GetComponent<UITexture>().mainTexture = texture;
			}
		}

        if (WingroveAudio.WingroveRoot.Instance != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRANSITION_OFF");
            WingroveAudio.WingroveRoot.Instance.PostEvent("SCENE_CHANGE");
        }
        


		m_currentTextureIndex = UnityEngine.Random.Range(0, m_backgroundTextures.Length);
		//Debug.Log("Current texture: " + m_currentTextureIndex);
		ScreenCover.GetComponent<UITexture>().mainTexture = m_backgroundTextures[m_currentTextureIndex];
		//ScreenCover.GetComponent<UITexture>().mainTexture = m_backgroundTextures[UnityEngine.Random.Range(0, m_backgroundTextures.Length)];

		if(m_transitionSounds.Length > 0)
		{
			AudioClip clip = m_transitionSounds[UnityEngine.Random.Range(0, m_transitionSounds.Length)];
			if(clip != null)
			{
				GetComponent<AudioSource>().clip = clip;
				GetComponent<AudioSource>().Play();
			}
		}

        ScreenCover.SetActive(true);

		Vector3 newPos=new Vector3(0.0f, 0.0f, 0.0f);
		var config=new GoTweenConfig()
			.vector3Prop( "localPosition", newPos )
			.setEaseType( GoEaseType.QuadInOut );

		Debug.Log("level: " + level);

		GoTween tween=new GoTween(ScreenCover.transform, 0.8f, config);
        if (addToStack)
        {
            //m_backStack.Push(level);
			m_backStack.Push(Application.loadedLevelName);
            Debug.Log("Adding " + level + " to back stack!");
        }

        if (level != null)
        {
            tween.setOnCompleteHandler(c => LoadNextLevel(level));
        }
        else
        {
            tween.setOnCompleteHandler(c => LoadStartLevel());
        }

        UnityEngine.Object[] uic = GameObject.FindObjectsOfType(typeof(UICamera));
        foreach (UICamera cam in uic)
        {
            cam.enabled = false;
        }

		Go.addTween(tween);
	}
	
    //public IEnumerator ChangeLevelDelayed(string level, float delay, bool addToBackStack)
    //{
    //    yield return new WaitForSeconds(delay);
    //    ChangeLevel(level, addToBackStack);
    //}
	
	// Reset the cover to the left
	void FinishedIntro () 
    {
        ScreenCover.transform.localPosition = new Vector3(-1024, 0, 0);
        ScreenCover.SetActive(false);
	}
	
	// Reset the cover to the left
	void LoadNextLevel (string level) 
	{
		Debug.Log("Next level: " + level + ". Now loading empty scene");
        m_loadingToScene = level;

		/*
		// Manually unload all textures from memory (except for the one currently covering the screen).
		Texture2D screenTex = ScreenCover.GetComponent<UITexture>().mainTexture as Texture2D;
		Texture2D[] textures = UnityEngine.Object.FindObjectsOfType<Texture2D>() as Texture2D[];
		Debug.Log("Found " + textures.Length + " textures");
		foreach(Texture2D tex in textures)
		{
			if(tex != screenTex)
			{
				Debug.Log("Destroying");
				NGUITools.Destroy(tex);
			}
			else
			{
				Debug.Log("Don't delete! tex is screenTex");
			}
		}
		GC.Collect();
		*/

		UITexture screenTex = ScreenCover.GetComponent<UITexture>() as UITexture;
		UITexture[] textures = UnityEngine.Object.FindObjectsOfType<UITexture>() as UITexture[];
		//Debug.Log("There are " + textures.Length + " textures");
		for(int i = 0; i < textures.Length; ++i)
		{
			if(textures[i] != screenTex)
			{
				if(textures[i].mainTexture != null)
				{
					//Debug.Log("Unloading texture");
					Resources.UnloadAsset(textures[i].mainTexture);
				}

				NGUITools.Destroy(textures[i]);
			}
		}

		/*
		UISprite[] sprites = UnityEngine.Object.FindObjectsOfType<UISprite>() as UISprite[];
		Debug.Log("There are " + sprites.Length + " sprites");
		for(int i = 0; i < sprites.Length; ++i)
		{
			if(sprites[i].mainTexture != null)
			{
				Debug.Log("Unloading sprite");
				Resources.UnloadAsset(sprites[i].mainTexture);
			}

			NGUITools.Destroy(sprites[i]);
		}
		*/


		GC.Collect();

#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("Name", Application.loadedLevelName);
		FlurryBinding.endTimedEvent("NewLevel", ep);
#endif

        Application.LoadLevel(m_emptySceneName);
	}

    // Reset the cover to the left
    void LoadStartLevel()
    {
        //m_loadingToScene = null;
		//m_loadingToScene = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_startingSceneName;
		m_loadingToScene = "NewVoyage";

#if UNITY_IPHONE
		Dictionary<string, string> ep = new Dictionary<string, string>();
		ep.Add("Name", Application.loadedLevelName);
		FlurryBinding.endTimedEvent("NewLevel", ep);
#endif

        Application.LoadLevel(m_emptySceneName);
    }	
	
	
	public void FlashTransitionScreen()
	{
        ScreenCover.SetActive(true);
        Vector3 newPos=new Vector3(0.0f, 0.0f, 0.0f);
		var config=new GoTweenConfig()
			.vector3Prop( "localPosition", newPos )
			.setEaseType( GoEaseType.QuadInOut );

		GoTween tween=new GoTween(ScreenCover.transform, 0.8f, config);
		Go.addTween(tween);
		
		newPos=new Vector3(1024.0f, 0.0f, 0.0f);
		config=new GoTweenConfig()
			.vector3Prop( "localPosition", newPos )
			.setEaseType( GoEaseType.QuadInOut )
			.setDelay( 1.2f);

		tween=new GoTween(ScreenCover.transform, 0.8f, config);
		Go.addTween(tween);
        tween.setOnCompleteHandler(c => FinishedIntro());
	}
	
	// TODO: Deprecate this function and find a better way of making sure that the Challenge Menu stays on the stack or gets put back on the stack when you leave it
	public void PushToStack(string sceneName)
	{
		if(m_backStack.Count == 0 || (m_backStack.Count > 0 && m_backStack.Peek() != sceneName))
		{
			Debug.Log("Pushing: " + sceneName);
			m_backStack.Push(sceneName);
		}
		else
		{
			Debug.Log(sceneName + " is already on back stack");
		}
	}
	
}
