using UnityEngine;
using System.Collections;

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

    void Awake()
    {
        m_loginButton.Unpressing += OnPressLogin;
    }

    public void On()
    {
        m_tweenBehaviour.Off();
    }

    void OnPressLogin(PipButton button)
    {
        string tokenResponse = UserHelpers.RequestToken(m_emailInput.text, m_passwordInput.text);
        
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
            /*
            UserHelpers.UserState userState = UserHelpers.GetUserState();

            switch(userState)
            {
                case UserHelpers.UserState.Good:
                    m_tweenBehaviour.On();
                    break;
                case UserHelpers.UserState.Expired:
                    Debug.LogError("SUBSCRIPTION EXPIRED");
                    break;
                case UserHelpers.UserState.InvalidToken:
                    Debug.LogError("INVALID TOKEN");
                    break;
            }
            */
        } 
        else
        {
            // Account does not exist
            Debug.LogError("ACCOUNT DOES NOT EXIST");
        }
    }


}
