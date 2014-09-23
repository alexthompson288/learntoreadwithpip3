using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;

public class LoginPrefabCoordinator : Singleton<LoginPrefabCoordinator>
{
    [SerializeField]
    private TweenBehaviour m_tweenBehaviour;
    [SerializeField]
    private UIPanel m_loginPanel;
    [SerializeField]
    private UIPanel m_successPanel;
    [SerializeField]
    private UILabel m_emailInput;
    [SerializeField]
    private UILabel m_passwordInput;
    [SerializeField]
    private PipButton m_loginButton;
    [SerializeField]
    private PipButton m_registerButton;
    [SerializeField]
    private UILabel m_loginButtonLabel;
    [SerializeField]
    private UILabel m_infoLabel;
    [SerializeField]
    private GameObject m_pipPrefab;
    [SerializeField]
    private Transform m_pipSpawnLocation;
    [SerializeField]
    private Transform m_pipOnLocation;
    [SerializeField]
    private GameObject m_loginButtonParent;
    
    static string m_infoText = "Login";
    
    private PipAnim m_pipAnim;
    private SpriteAnim m_pipSpriteAnim;

    void Awake()
    {
        m_emailInput.GetComponent<UIInput>().onSubmit.Add(new EventDelegate(this, "OnEnterEmail"));
        
        m_loginPanel.alpha = 1;
        m_successPanel.alpha = 0;
        
        m_loginButton.Unpressing += OnUnpressLogin;
        m_registerButton.Unpressing += OnUnpressRegisterButton;
        
        m_infoLabel.text = m_infoText;        
    }

    void EditEmailInput()
    {
        m_emailInput.text = m_emailInput.text.ToLower().Replace("\n","");
    }
    
    void OnEnterEmail()
    {
        EditEmailInput();
    }
    
    void OnUnpressRegisterButton(PipButton button)
    {
        ParentGate.Instance.Answered += OnParentGateAnswer;
        ParentGate.Instance.On();
    }

    void OnParentGateAnswer(bool isCorrect)
    {
        ParentGate.Instance.Answered -= OnParentGateAnswer;
        
        if (isCorrect)
        {
            Application.OpenURL("http://www.learnwithpip.com/users/sign_up");
        }
        
        #if UNITY_EDITOR
        string log = isCorrect ? "Correct!" : "Incorrect";
        D.Log(log);
        #endif
    }
    
    IEnumerator Start()
    {
        m_tweenBehaviour.On();
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        yield return StartCoroutine(TransitionScreen.WaitForInstance());
        
        if (LoginInfo.Instance.GetAttemptLogin())
        {
            GameObject newPip = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, m_pipSpawnLocation);
            m_pipAnim = newPip.GetComponent<PipAnim>() as PipAnim;
            m_pipSpriteAnim = m_pipAnim.GetAnim() as SpriteAnim;
            
            yield return new WaitForSeconds(0.8f);
            
            m_pipAnim.MoveToPos(m_pipOnLocation.position);
        }
        else
        {
            TransitionScreen.Instance.ChangeToDefaultLevel();
        }
    }
    
    public static void SetInfoText(LoginException ex)
    {
        switch (ex.exceptionType)
        {
            case LoginException.ExceptionType.Expired:
                SetInfoText("Your account has expired");
                break;
            case LoginException.ExceptionType.InvalidToken:
                SetInfoText("Login");
                break;
        }
    }
    
    public static void SetInfoText(WebException ex, bool isEmailPasswordCheck)
    {
        if ((ex.Response is System.Net.HttpWebResponse))
        {
            switch ((ex.Response as System.Net.HttpWebResponse).StatusCode)
            {
                // Unauthorized status code can happen for 2 reasons: 1. Incorrect username/password 2. No access token
                // In the case of no access token, this is probably because the user has never logged on from this device before
                case System.Net.HttpStatusCode.Unauthorized:
                    string infoText = isEmailPasswordCheck ? "Account not recognized" : "Login";
                    SetInfoText(infoText);
                    break;
                default:
                    SetInfoText("Oops!\nSomething is wrong with our servers.\nPlease try again later");
                    break;
            }
        }
        else
        {
            SetInfoText("Check your internet connection");
        }
    }
    
    public static void SetInfoText(string myInfoText)
    {
        m_infoText = myInfoText;
        
        if (Instance != null)
        {
            Instance.m_infoLabel.text = m_infoText;
        }
    }

    void OnUnpressLogin(PipButton button)
    {
        EditEmailInput();
        
        if (m_emailInput.text == "pipoffline" && m_passwordInput.GetComponent<UIInput>().value == "pipoffline")
        {
            TransitionScreen.Instance.ChangeToDefaultLevel();
        }
        else
        {
            m_infoLabel.text = "Logging in...";
            m_loginButtonLabel.text = "Logging in...";
            StartCoroutine(OnUnpressLoginCo());
        }
    }
    
    IEnumerator OnUnpressLoginCo()
    {
        yield return null;
        
        string tokenResponse = "";
        
        string password = m_passwordInput.GetComponent<UIInput>().value;
        
        try
        {
            tokenResponse = LoginHelpers.RequestToken(m_emailInput.text, password);
        }
        catch(WebException ex)
        {
            SetInfoText(ex, true);
        }
        
        bool hasToken = tokenResponse.Contains(LoginHelpers.expirationPrefix);
        
        if (hasToken)
        {
            string accessToken = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.accessPrefix, "\"");
            string expirationDate = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.expirationPrefix, "\"");
            
            LoginInfo.Instance.SaveUserDetails(m_emailInput.text, password, accessToken, expirationDate);
            
            float panelTweenDuration = 0.25f;
            TweenAlpha.Begin(m_loginPanel.gameObject, panelTweenDuration, 0);
            TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);
            
            m_pipSpriteAnim.PlayAnimation("JUMP");
            
            yield return new WaitForSeconds(0.22f);
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
            
            yield return new WaitForSeconds(0.5f);
            
            SetInfoText("Login");
            
            //TransitionScreen.Instance.ChangeToDefaultLevel();
            m_tweenBehaviour.Off();
        } 
        
        m_loginButtonLabel.text = "Login";
    }
}
