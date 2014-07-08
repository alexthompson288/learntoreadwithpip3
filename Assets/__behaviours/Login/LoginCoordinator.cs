using UnityEngine;
using System.Collections;
using System;
using System.Net;
using WingroveAudio;

public class LoginCoordinator : Singleton<LoginCoordinator> 
{
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private UILabel m_emailInput;
    [SerializeField]
    private UILabel m_passwordInput;
    [SerializeField]
    private PipButton m_loginButton;
    [SerializeField]
    private UILabel m_infoLabel;
    [SerializeField]
    private GameObject m_waitingIcon;
    [SerializeField]
    private SpriteAnim m_pipAnim;
    [SerializeField]
    private UIPanel m_loginPanel;
    [SerializeField]
    private UIPanel m_successPanel;

    static string m_infoText = "Login";

    void Awake()
    {
        m_loginPanel.alpha = 1;
        m_successPanel.alpha = 0;

        if (!Application.isEditor)
        {
            m_passwordInput.GetComponent<UIInput>().isPassword = true;
        }
       
        m_waitingIcon.transform.localScale = Vector3.zero;
        m_waitingIcon.gameObject.SetActive(false);

        m_loginButton.Unpressing += OnPressLogin;

        m_infoLabel.text = m_infoText;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(TransitionScreen.WaitForInstance());

        LifetimeEventTrigger[] audioTriggers = UnityEngine.Object.FindObjectsOfType(typeof(LifetimeEventTrigger)) as LifetimeEventTrigger[];
        foreach (LifetimeEventTrigger audioTrigger in audioTriggers)
        {
            if(audioTrigger.GetStartEvent() == "PIP_THEME" && audioTrigger.GetOnDestroyEvent() == "MUSIC_STOP")
            {
                audioTrigger.RemoveOnDestroyEvent();
            }
        }

        TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
    }

    public static void SetInfoText(UserException ex)
    {
        switch (ex.exceptionType)
        {
            case UserException.ExceptionType.Expired:
                SetInfoText("Your account has expired");
                break;
            case UserException.ExceptionType.InvalidToken:
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
        float scaleTweenDuration = 0.3f;

        m_waitingIcon.gameObject.SetActive(true);
        iTween.ScaleTo(m_waitingIcon, Vector3.one, scaleTweenDuration);
        yield return new WaitForSeconds(scaleTweenDuration);

        string tokenResponse = "";

        try
        {
            tokenResponse = UserHelpers.RequestToken(m_emailInput.text, m_passwordInput.text);
        }
        catch(WebException ex)
        {
            SetInfoText(ex, true);
        }

        iTween.ScaleTo(m_waitingIcon, Vector3.one, scaleTweenDuration);
        yield return new WaitForSeconds(scaleTweenDuration + 0.1f);
        m_waitingIcon.gameObject.SetActive(false);


        string accessPrefix = "\"access_token\":\"";
        string expirationPrefix = "\"expiration_date\":\"";

        bool hasToken = tokenResponse.Contains(accessPrefix) && tokenResponse.Contains(expirationPrefix);
        
        D.Log("hasToken: " + hasToken);
        D.Log("RESPONSE_CONTENT");
        D.Log(tokenResponse);
        
        if (hasToken)
        {
            string accessToken = UserHelpers.ParseResponse(tokenResponse, accessPrefix, "\"");
            D.Log("ACCESS_TOKEN: " + accessToken);
            
            string expirationDate = UserHelpers.ParseResponse(tokenResponse, expirationPrefix, "\"");
            D.Log("EXPIRATION_DATE: " + expirationDate);
            
            UserInfo.Instance.SaveUserDetails(m_emailInput.text, accessToken, expirationDate);

            bool isUserLegal = false;

            try
            {
                isUserLegal = UserHelpers.IsUserLegal();
            } 
            catch (WebException ex)
            {
                SetInfoText(ex, false);
            } 
            catch (UserException ex)
            {
                SetInfoText(ex);
            }

            if (isUserLegal)
            {
                StartCoroutine(Off());
            }
        } 
    }

    public void On()
    {
        m_tweenBehaviour.Off();
    }

#if UNITY_EDITOR
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
            StartCoroutine(Off());
    }
#endif

    IEnumerator Off()
    {
        float panelTweenDuration = 0.25f;
        TweenAlpha.Begin(m_loginPanel.gameObject, panelTweenDuration, 0);
        TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);

        m_pipAnim.PlayAnimation("JUMP");
        
        yield return new WaitForSeconds(0.22f);
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");

        yield return new WaitForSeconds(0.5f);

        m_tweenBehaviour.Off();

        yield return new WaitForSeconds(m_tweenBehaviour.GetTotalDurationOff());

        Destroy(gameObject);
    }
}
