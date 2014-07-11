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
    private PipAnim m_pipAnim;
    [SerializeField]
    private SpriteAnim m_pipSpriteAnim;
    [SerializeField]
    private Transform m_pipLocation;
    [SerializeField]
    private UIPanel m_loginPanel;
    [SerializeField]
    private UIPanel m_successPanel;
    [SerializeField]
    private Transform m_waitingIcon;

    static string m_infoText = "Login";

    void Awake()
    {
        m_waitingIcon.transform.localScale = Vector3.zero;
        m_loginPanel.alpha = 1;
        m_successPanel.alpha = 0;

        if (!Application.isEditor)
        {
            m_passwordInput.GetComponent<UIInput>().isPassword = true;
        }

        m_loginButton.Unpressing += OnPressLogin;

        m_infoLabel.text = m_infoText;
    }

    IEnumerator RotateWaitingIcon()
    {
        m_waitingIcon.Rotate(Vector3.forward, 100 * Time.deltaTime);
        yield return null;
        StartCoroutine("RotateWaitingIcon");
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(TransitionScreen.WaitForInstance());

        if (m_pipAnim != null)
        {
            yield return new WaitForSeconds(0.8f);
            m_pipAnim.MoveToPos(m_pipLocation.position);
        }
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
        m_infoLabel.text = "Logging in...";
        iTween.ScaleTo(m_waitingIcon.gameObject, Vector3.one, 0.2f);
        StartCoroutine("RotateWaitingIcon");

        yield return new WaitForSeconds(0.8f);


        string tokenResponse = "";

        try
        {
            tokenResponse = UserHelpers.RequestToken(m_emailInput.text, m_passwordInput.text);
        }
        catch(WebException ex)
        {
            SetInfoText(ex, true);
        }

        bool hasToken = tokenResponse.Contains(UserHelpers.accessPrefix) && tokenResponse.Contains(UserHelpers.expirationPrefix);
        
        D.Log("hasToken: " + hasToken);
        D.Log("RESPONSE_CONTENT");
        D.Log(tokenResponse);
        
        if (hasToken)
        {
            string accessToken = UserHelpers.ParseResponse(tokenResponse, UserHelpers.accessPrefix, "\"");
            D.Log("ACCESS_TOKEN: " + accessToken);
            
            string expirationDate = UserHelpers.ParseResponse(tokenResponse, UserHelpers.expirationPrefix, "\"");
            D.Log("EXPIRATION_DATE: " + expirationDate);
            
            UserInfo.Instance.SaveUserDetails(m_emailInput.text, m_passwordInput.text, accessToken, expirationDate);

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

        iTween.ScaleTo(m_waitingIcon.gameObject, Vector3.zero, 0.2f);
        StopCoroutine("RotateWaitingIcon");
    }

    public void On()
    {
        m_tweenBehaviour.Off();
    }

#if UNITY_EDITOR
    void Update()
    {
        /*
        if(Input.GetKeyDown(KeyCode.O))
            StartCoroutine(Off());
        */
    }
#endif

    IEnumerator Off()
    {
        float panelTweenDuration = 0.25f;
        TweenAlpha.Begin(m_loginPanel.gameObject, panelTweenDuration, 0);
        TweenAlpha.Begin(m_successPanel.gameObject, panelTweenDuration, 1);

        m_pipSpriteAnim.PlayAnimation("JUMP");
        
        yield return new WaitForSeconds(0.22f);
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");

        yield return new WaitForSeconds(0.5f);

        if (m_tweenBehaviour != null)
        {
            m_tweenBehaviour.Off();

            yield return new WaitForSeconds(m_tweenBehaviour.GetTotalDurationOff());

            Destroy(gameObject);
        }
        else
        {
            TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
        }
    }
}
