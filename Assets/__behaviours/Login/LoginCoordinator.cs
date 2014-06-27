using UnityEngine;
using System.Collections;
using System;
using System.Net;

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

    void Awake()
    {
        m_waitingIcon.transform.localScale = Vector3.zero;
        m_waitingIcon.gameObject.SetActive(false);

        m_loginButton.Unpressing += OnPressLogin;
    }

    public void On()
    {
        m_tweenBehaviour.Off();
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
            SetInfoText(ex);
        }

        iTween.ScaleTo(m_waitingIcon, Vector3.one, scaleTweenDuration);
        yield return new WaitForSeconds(scaleTweenDuration + 0.1f);
        m_waitingIcon.gameObject.SetActive(false);
        
        bool hasToken = !tokenResponse.Contains("error");
        
        Debug.Log("hasToken: " + hasToken);
        Debug.Log("RESPONSE_CONTENT");
        Debug.Log(tokenResponse);
        
        if (hasToken)
        {
            string accessPrefix = "\"access_token\":\"";
            string accessToken = UserHelpers.ParseResponse(tokenResponse, accessPrefix, "\"");
            Debug.Log("ACCESS_TOKEN: " + accessToken);
            
            string expirationPrefix = "\"expiration_date\":\"";
            string expirationDate = UserHelpers.ParseResponse(tokenResponse, expirationPrefix, "\"");
            Debug.Log("EXPIRATION_DATE: " + expirationDate);
            
            UserInfo.Instance.SaveUserDetails(m_emailInput.text, accessToken, expirationDate);
            
            m_tweenBehaviour.On();

            bool isUserLegal = false;

            try
            {
                isUserLegal = UserHelpers.IsUserLegal();
            }
            catch(WebException ex)
            {
                SetInfoText(ex);
            }
            catch(UserException ex)
            {
                SetInfoText(ex);
            }

            if(isUserLegal)
            {
                m_tweenBehaviour.On();
            }
        } 
        else
        {
            // Account does not exist
            Debug.LogError("ACCOUNT DOES NOT EXIST");
            SetInfoText("Account does not exist");
        }
    }

    public void SetInfoText(UserException ex)
    {
        switch (ex.exceptionType)
        {
            case UserException.ExceptionType.Expired:
                break;

            case UserException.ExceptionType.InvalidToken:
                break;
        }
    }

    public void SetInfoText(WebException ex)
    {

    }

    public void SetInfoText(string text)
    {
        m_infoLabel.text = text;
    }
}
