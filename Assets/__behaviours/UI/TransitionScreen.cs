using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TransitionScreen : Singleton<TransitionScreen> 
{   
    [SerializeField]
    private UITexture m_screenTexture;

    float m_tweenDuration = 0.2f;

    static string m_loadingToScene = null;
    static string m_emptySceneName = "DeliberatelyEmptyScene";
    
    public bool m_isEmptyScene;

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

    void Awake()
    {
        m_screenTexture.color = Color.white;
    }
    
    // Use this for initialization
    IEnumerator Start () 
    {  
		Debug.Log ("LEVEL " + Application.loadedLevelName);
        if (m_isEmptyScene)
        {
            yield return new WaitForSeconds(0.05f);
            if (string.IsNullOrEmpty(m_loadingToScene))
            {
                Application.LoadLevel(0);
            }
            else
            {
                Application.LoadLevel(m_loadingToScene);
                
                while(Application.isLoadingLevel)
                {
                    yield return null;
                }
            }
            yield break;
        }
        
        
        yield return null;
        
        Color newCol = new Color(1f, 1f, 1f, 0f);
        var config = new GoTweenConfig()
            .colorProp("color", newCol)
                .setEaseType(GoEaseType.Linear);

        GoTween tween = new GoTween(m_screenTexture, m_tweenDuration, config);
        tween.setOnCompleteHandler(c => OnScreenExit());
        
        Go.addTween(tween);
        
        if (WingroveAudio.WingroveRoot.Instance != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRANSITION_ON");
        }
    }

    void OnScreenExit()
    {
        m_screenHasExited = true;
    }
    
    public void ChangeLevel(string level, bool addToStack)
    {
        if (WingroveAudio.WingroveRoot.Instance != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TRANSITION_OFF");
        }        

        Color newCol = new Color(1f, 1f, 1f, 1f);
        var config = new GoTweenConfig()
            .colorProp("color", newCol)
                .setEaseType(GoEaseType.Linear);

        GoTween tween = new GoTween(m_screenTexture, m_tweenDuration, config);
        tween.setOnCompleteHandler(c => LoadNextLevel(level));
        
        UnityEngine.Object[] uic = GameObject.FindObjectsOfType(typeof(UICamera));
        foreach (UICamera cam in uic)
        {
            cam.enabled = false;
        }

        Go.addTween(tween);
    }
    
    // Reset the cover to the left
    void LoadNextLevel (string level) 
    {
        m_loadingToScene = level;     
        Application.LoadLevel(m_emptySceneName);
    }
}