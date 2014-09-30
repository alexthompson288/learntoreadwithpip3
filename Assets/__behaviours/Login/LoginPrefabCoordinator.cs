using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;

public class LoginPrefabCoordinator : Singleton<LoginPrefabCoordinator>
{
    [SerializeField]
    private TweenBehaviour m_menuTween;
    [SerializeField]
    private TweenBehaviour m_loginRegisterTween;
    [SerializeField]
    private UIPanel m_loginRegisterPanel;
    [SerializeField]
    private UIPanel m_successPanel;
    [SerializeField]
    private UILabel m_emailInput;
    [SerializeField]
    private UILabel m_passwordInput;
    [SerializeField]
    private PipButton m_loginButton;
    [SerializeField]
    private PipButton m_callRegisterButton;
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
    [SerializeField]
    private UILabel m_registerNameLabel;
    [SerializeField]
    private UILabel m_registerEmailLabel;
    [SerializeField]
    private UILabel[] m_registerPasswordLabels;
    [SerializeField]
    private PipButton m_registerButton;
    [SerializeField]
    private PipButton m_dismissButton;
    [SerializeField]
    private PipButton m_callLoginButton;
    [SerializeField]
    private PipButton m_dummyRegisterButton;
    
    static string m_infoText = "Login";
    
    private PipAnim m_pipAnim;
    private SpriteAnim m_pipSpriteAnim;

    void Awake()
    {
        m_emailInput.GetComponent<UIInput>().onSubmit.Add(new EventDelegate(this, "OnEnterEmail"));
        
        m_loginRegisterPanel.alpha = 1;
        m_successPanel.alpha = 0;
        
        m_loginButton.Unpressing += OnUnpressLogin;
        m_callRegisterButton.Unpressing += OnUnpressCallRegisterButton;
        m_callLoginButton.Unpressing += OnUnpressCallLoginButton;
        m_registerButton.Unpressing += OnUnpressRegisterButton;
        m_dummyRegisterButton.Unpressing += OnUnpressDummyRegisterButton;

        m_dismissButton.Unpressing += OnUnpressDismissButton;
        
        m_infoLabel.text = m_infoText;        
    }

    public void On()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        m_menuTween.On();
    }

    void Off()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        m_menuTween.Off();
    }

    void OnUnpressDismissButton(PipButton button)
    {
        Off();
    }

    void EditEmailInput()
    {
        m_emailInput.text = m_emailInput.text.ToLower().Replace("\n","");
    }
    
    void OnEnterEmail()
    {
        EditEmailInput();
    }

    void OnUnpressCallLoginButton(PipButton button)
    {
        //D.Log("OnUnpressCallLoginButton()");
        m_loginRegisterTween.On();
    }
    
    void OnUnpressCallRegisterButton(PipButton button)
    {
        //D.Log("OnUnpressCallRegisterButton()");
        m_loginRegisterTween.Off();
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
        //D.Log(log);
        #endif
    }
    
    IEnumerator Start()
    {
        On();

        yield return StartCoroutine(TransitionScreen.WaitForInstance());
        
        GameObject newPip = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, m_pipSpawnLocation);
        m_pipAnim = newPip.GetComponent<PipAnim>() as PipAnim;
        m_pipSpriteAnim = m_pipAnim.GetAnim() as SpriteAnim;
        
        yield return new WaitForSeconds(0.8f);
        
        m_pipAnim.MoveToPos(m_pipOnLocation.position);
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

    void OnUnpressDummyRegisterButton(PipButton button)
    {
        try
        {
            //D.Log("Attempting registration");
            string responseContent = LoginHelpers.Register("tom+1@learnwithpip.com", "password", "Tom");
            //D.Log("registerResponseContent: " + responseContent);
            m_loginRegisterTween.On();
        }
        catch(WebException ex)
        {
            //D.Log("REGISTER FAIL");
            if ((ex.Response is System.Net.HttpWebResponse))
            {
                //D.Log("HTTP - StatusCode: " + (ex.Response as System.Net.HttpWebResponse).StatusCode);
            }
            else
            {
                //D.Log("Not HTTP - Exception: " + ex.Message);
            }
        }
    }

    void OnUnpressRegisterButton(PipButton button)
    {
        //D.Log("LoginPrefabCoordinator.OnUnpressRegisterButton()");

        HashSet<string> passwords = new HashSet<string>();
        
        foreach (UILabel label in m_registerPasswordLabels)
        {
            passwords.Add(label.text);
        }
        
        if (passwords.Count == 1)
        {
            try
            {
                //D.Log("Attempting registration");
                string responseContent = LoginHelpers.Register(m_registerEmailLabel.text, m_registerPasswordLabels[0].text, m_registerNameLabel.text);
                //D.Log("registerResponseContent: " + responseContent);
                m_loginRegisterTween.On();
            }
            catch(WebException ex)
            {
                //D.Log("REGISTER FAIL");
                if ((ex.Response is System.Net.HttpWebResponse))
                {
                    //D.Log("HTTP - StatusCode: " + (ex.Response as System.Net.HttpWebResponse).StatusCode);
                }
                else
                {
                    //D.Log("Not HTTP - Exception: " + ex.Message);
                }
            }
        }
        else
        {
            m_infoLabel.text = "Invalid password";
        }
    }

    void OnUnpressLogin(PipButton button)
    {
        EditEmailInput();
        
        if (m_emailInput.text == "pipoffline" && m_passwordInput.GetComponent<UIInput>().value == "pipoffline")
        {
            Off();
            LoginInfo.Instance.SetIsValid(true);
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
            TweenAlpha.Begin(m_loginRegisterPanel.gameObject, panelTweenDuration, 0);
            TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);
            
            m_pipSpriteAnim.PlayAnimation("JUMP");
            
            yield return new WaitForSeconds(0.22f);
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
            
            yield return new WaitForSeconds(0.5f);
            
            SetInfoText("Login");
            
            Off();
        } 
        
        m_loginButtonLabel.text = "Login";
    }
}
