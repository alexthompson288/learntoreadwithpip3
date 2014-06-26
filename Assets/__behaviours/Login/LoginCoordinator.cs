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
        string responseContent = RequestToken();

        bool loginSuccess = !responseContent.Contains("error");

        Debug.Log("loginSuccess: " + loginSuccess);
        Debug.Log("RESPONSE_CONTENT");
        Debug.Log(responseContent);

        if (loginSuccess)
        {
            string accessPrefix = "\"access_token\": \"";
            string accessToken = ParseResponse(responseContent, accessPrefix);
            Debug.Log("ACCESS_TOKEN: " + accessToken);

            string expirationPrefix = "\"expiration_date\": \"";
            string expirationDate = ParseResponse(responseContent, expirationPrefix);
            Debug.Log("EXPIRATION_DATE: " + expirationDate);

            UserInfo.Instance.LogIn(m_emailInput.text, accessToken, expirationDate);

            m_tweenBehaviour.On();
        }
        else
        {
            UserInfo.Instance.LogOut();
        }
    }

    string ParseResponse(string responseContent, string prefix)
    {
        int prefixIndex = responseContent.IndexOf(prefix);

        string info = responseContent.Substring(prefixIndex + prefix.Length);

        int endIndex = info.IndexOf("\"");

        return info.Substring(0, endIndex);
    }

    string RequestToken()
    {
        Debug.Log("RequestToken()");

        Debug.Log("email: " + m_emailInput.text);
        Debug.Log("password: " + m_passwordInput.text);
        
        #if UNITY_EDITOR
        // Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        #endif
        
        Debug.Log("Creating request");
        var request = System.Net.WebRequest.Create("http://private-281e5-pip.apiary-mock.com/api/v1/token") as System.Net.HttpWebRequest;
        request.KeepAlive = true;
        request.Method = "POST";
        request.ContentType="application/json";
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\n    \"email\": \"email@example.com\",\n    \"password\": \"password\"\n}");
        //byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(System.String.Format("{\n    \"email\": \"{0}\",\n    \"password\": \"{1}\"\n}", m_emailInput.text, m_passwordInput.text));
        //byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(System.String.Format("{\n    \"email\": \"{0}\",\n    \"password\": \"{1}\"\n}", "email@example.com", "password"));
        request.ContentLength = byteArray.Length;
        using (var writer = request.GetRequestStream()){writer.Write(byteArray, 0, byteArray.Length);}
        
        Debug.Log("Getting response");
        string responseContent=null;
        using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
            using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
                responseContent = reader.ReadToEnd();
            }
        }
        
        Debug.Log("RequestToken() - RESPONSE_CONTENT");
        Debug.Log(responseContent);

        return responseContent;
    }

    string GetUser()
    {
        #if UNITY_EDITOR
        // Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        #endif
        
        var request = System.Net.WebRequest.Create("http://private-281e5-pip.apiary-mock.com/api/v1/users/me") as System.Net.HttpWebRequest;
        request.KeepAlive = true;
        request.Method = "GET";
        request.ContentType="application/json";
        request.Headers.Add("authorization", "\"Token token=%{access_token}\"");
        request.ContentLength = 0;
        
        string responseContent=null;
        using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
            using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
                responseContent = reader.ReadToEnd();
            }
        }
        
        Debug.Log("GetUser() - RESPONSE_CONTENT");
        Debug.Log(responseContent);

        return responseContent;
    }
}
