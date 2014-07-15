using UnityEngine;
using System.Collections;
using System;
using System.Net;
using WingroveAudio;

public class LoginCoordinator : Singleton<LoginCoordinator> 
{
    [SerializeField]
    private UILabel m_emailInput;
    [SerializeField]
    private UILabel m_passwordInput;
    [SerializeField]
    private PipButton m_loginButton;
    [SerializeField]
    private UILabel m_infoLabel;
    [SerializeField]
    private GameObject m_pipPrefab;
    [SerializeField]
    private Transform m_pipSpawnLocation;
    [SerializeField]
    private Transform m_pipOnLocation;
    [SerializeField]
    private UIPanel m_loginPanel;
    [SerializeField]
    private UIPanel m_successPanel;

    static string m_infoText = "Login";

    private PipAnim m_pipAnim;
    private SpriteAnim m_pipSpriteAnim;

    void Awake()
    {
        m_loginPanel.alpha = 1;
        m_successPanel.alpha = 0;

        if (!Application.isEditor)
        {
            m_passwordInput.GetComponent<UIInput>().isPassword = true;
        }

        m_loginButton.Unpressing += OnPressLogin;

        m_infoLabel.text = m_infoText;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(TransitionScreen.WaitForInstance());

        GameObject newPip = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, m_pipSpawnLocation);
        m_pipAnim = newPip.GetComponent<PipAnim>() as PipAnim;
        m_pipSpriteAnim = newPip.GetComponentInChildren<SpriteAnim>() as SpriteAnim;

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
            D.Log("StatusCode: " + (ex.Response as System.Net.HttpWebResponse).StatusCode);
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

    void OnPressLogin(PipButton button)
    {
        StartCoroutine(OnPressLoginCo());
    }

    IEnumerator OnPressLoginCo()
    {
        string tokenResponse = "";

        try
        {
            tokenResponse = LoginHelpers.RequestToken(m_emailInput.text, m_passwordInput.text);
        }
        catch(WebException ex)
        {
            SetInfoText(ex, true);
        }

        bool hasToken = tokenResponse.Contains(LoginHelpers.accessPrefix) && tokenResponse.Contains(LoginHelpers.expirationPrefix);
        
        D.Log("hasToken: " + hasToken);
        D.Log("RESPONSE_CONTENT");
        D.Log(tokenResponse);
       
        if (hasToken)
        {
            string accessToken = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.accessPrefix, "\"");
            D.Log("ACCESS_TOKEN: " + accessToken);
            
            string expirationDate = LoginHelpers.ParseResponse(tokenResponse, LoginHelpers.expirationPrefix, "\"");
            D.Log("EXPIRATION_DATE: " + expirationDate);
            
            LoginInfo.Instance.SaveUserDetails(m_emailInput.text, m_passwordInput.text, accessToken, expirationDate);

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
                TweenAlpha.Begin(m_loginPanel.gameObject, panelTweenDuration, 0);
                TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);
                
                m_pipSpriteAnim.PlayAnimation("JUMP");
                
                yield return new WaitForSeconds(0.22f);
                
                WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
                
                yield return new WaitForSeconds(0.5f);
                
                //TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
            }
        } 
    }
}
