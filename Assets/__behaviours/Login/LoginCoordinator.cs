using UnityEngine;
using System.Collections;
using System;
using System.Net;
using WingroveAudio;
using System.Collections.Generic;

public class LoginCoordinator : Singleton<LoginCoordinator> 
{
    [SerializeField]
    private UILabel m_emailInput;
    [SerializeField]
    private UILabel m_passwordInput;
    [SerializeField]
    private PipButton m_loginButton;
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
    private UIPanel m_loginRegisterPanel;
    [SerializeField]
    private UIPanel m_successPanel;
    [SerializeField]
    private GameObject m_loginButtonParent;
    [SerializeField]
    private PipButton m_callLoginButton;
    [SerializeField]
    private PipButton m_callRegisterButton;
    [SerializeField]
    private TweenBehaviour m_registerTweenBehaviour;
    [SerializeField]
    private UILabel m_registerNameLabel;
    [SerializeField]
    private UILabel m_registerEmailLabel;
    [SerializeField]
    private UILabel[] m_registerPasswordLabels;
    [SerializeField]
    private PipButton m_registerButton;
 
    static string m_infoText = "Login";

    private PipAnim m_pipAnim;
    private SpriteAnim m_pipSpriteAnim;

    bool m_hasEnteredEmail = false;
    bool m_hasEnteredPassword = false;

    void Awake()
    {
#if UNITY_STANDALONE
        m_loginButtonParent.transform.localScale = Vector3.one * 1.25f;
#endif

        m_emailInput.GetComponent<UIInput>().onSubmit.Add(new EventDelegate(this, "OnEnterEmail"));
        m_passwordInput.GetComponent<UIInput>().onSubmit.Add(new EventDelegate(this, "OnEnterPassword"));

        m_loginRegisterPanel.alpha = 1;
        m_successPanel.alpha = 0;

        m_loginButton.Unpressing += OnPressLogin;

        m_infoLabel.text = m_infoText;

        m_callRegisterButton.Unpressing += OnUnpressCallRegisterButton;
        m_callLoginButton.Unpressing += OnUnpressCallLoginButton;
        m_registerButton.Unpressing += OnUnpressRegisterButton;
    }

    void OnEnterEmail()
    {
        m_hasEnteredEmail = true;

        EditEmailInput();

        if (m_hasEnteredPassword)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
            iTween.ScaleTo(m_loginButtonParent, Vector3.one * 1.25f, 0.2f);
        }
    }

    void OnEnterPassword()
    {
        m_hasEnteredPassword = true;

        if (m_hasEnteredEmail)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
            iTween.ScaleTo(m_loginButtonParent, Vector3.one * 1.25f, 0.2f);
        }
    }

    void OnUnpressCallRegisterButton(PipButton button)
    {
        //m_infoLabel.text = "Register";
        //m_registerTweenBehaviour.Off();
        Application.OpenURL("http://www.learnwithpip.com/users/sign_up");
    }

    void OnUnpressCallLoginButton(PipButton button)
    {
        m_infoLabel.text = "Login";
        m_registerTweenBehaviour.On();
    }

    void OnUnpressRegisterButton(PipButton button)
    {
        Debug.Log("LoginCoordinator.OnUnpressRegisterButton()");
        HashSet<string> passwords = new HashSet<string>();

        foreach (UILabel label in m_registerPasswordLabels)
        {
            passwords.Add(label.text);
        }

        if (passwords.Count == 1)
        {
            try
            {
                string responseContent = LoginHelpers.Register(m_registerEmailLabel.text, m_registerPasswordLabels[0].text, m_registerNameLabel.text);
                Debug.Log("REGISTER SUCCESS");
                Debug.Log(responseContent);
            }
            catch(WebException ex)
            {
                Debug.Log("REGISTER FAIL");
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

    IEnumerator Start()
    {
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
            ////D.Log("HTTP - StatusCode: " + (ex.Response as System.Net.HttpWebResponse).StatusCode);
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
            ////D.Log("Not HTTP - Exception: " + ex.Message);
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

    void EditEmailInput()
    {
        m_emailInput.text = m_emailInput.text.ToLower().Replace("\n","");
    }

    void OnPressLogin(PipButton button)
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
            StartCoroutine(OnPressLoginCo());
        }
    }

    IEnumerator OnPressLoginCo()
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

        bool hasToken = tokenResponse.Contains(LoginHelpers.accessPrefix) && tokenResponse.Contains(LoginHelpers.expirationPrefix);
        
        ////D.Log("hasToken: " + hasToken);
        ////D.Log("RESPONSE_CONTENT: " + tokenResponse);
       
        if (hasToken)
        {
            string accessToken = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.accessPrefix, "\"");
            ////D.Log("ACCESS_TOKEN: " + accessToken);
            
            string expirationDate = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.expirationPrefix, "\"");
            ////D.Log("EXPIRATION_DATE: " + expirationDate);
            
            LoginInfo.Instance.SaveUserDetails(m_emailInput.text, password, accessToken, expirationDate);


            float panelTweenDuration = 0.25f;
            TweenAlpha.Begin(m_loginRegisterPanel.gameObject, panelTweenDuration, 0);
            TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);
            
            m_pipSpriteAnim.PlayAnimation("JUMP");
            
            yield return new WaitForSeconds(0.22f);
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
            
            yield return new WaitForSeconds(0.5f);
            
            SetInfoText("Login");
            
            TransitionScreen.Instance.ChangeToDefaultLevel();

            /*
            bool isUserLegal = false;

            try
            {
                isUserLegal = LoginHelpers.IsUserLegal(accessToken);
            } 
            catch (WebException ex)
            {
                SetInfoText(ex, false);
            } 
            catch (LoginException ex)
            {
                SetInfoText(ex);
            }

            if (isUserLegal)
            {
                float panelTweenDuration = 0.25f;
                TweenAlpha.Begin(m_loginRegisterPanel.gameObject, panelTweenDuration, 0);
                TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);
                
                m_pipSpriteAnim.PlayAnimation("JUMP");
                
                yield return new WaitForSeconds(0.22f);
                
                WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
                
                yield return new WaitForSeconds(0.5f);

                SetInfoText("Login");
                
                TransitionScreen.Instance.ChangeToDefaultLevel();
            }
            */
        } 

        m_loginButtonLabel.text = "Login";
    }
}
