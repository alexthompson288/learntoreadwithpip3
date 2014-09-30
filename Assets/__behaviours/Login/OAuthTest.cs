using UnityEngine;
using System.Collections;

public class OAuthTest : MonoBehaviour 
{
    [SerializeField]
    private bool m_requestToken;
    [SerializeField]
    private bool m_getUser;


    void Start()
    {
        ////////D.Log("OAuthTest.Start()");

        if (m_requestToken)
        {
            ////////D.Log("Requesting token");
            RequestToken();
        }

        if (m_getUser)
        {
            ////////D.Log("Getting user");
            GetUser();
        }
    }

    void RequestToken()
    {
        ////////D.Log("RequestToken()");

#if UNITY_EDITOR
        // Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif

        ////////D.Log("Creating request");
        var request = System.Net.WebRequest.Create("http://private-281e5-pip.apiary-mock.com/api/v1/token") as System.Net.HttpWebRequest;
        request.KeepAlive = true;
        request.Method = "POST";
        request.ContentType="application/json";
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\n    \"email\": \"email@example.com\",\n    \"password\": \"password\"\n}");
        request.ContentLength = byteArray.Length;
        using (var writer = request.GetRequestStream()){writer.Write(byteArray, 0, byteArray.Length);}

        ////////D.Log("Getting response");
        string responseContent=null;
        using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
            using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
                responseContent = reader.ReadToEnd();
            }
        }

        ////////D.Log("RequestToken() - RESPONSE_CONTENT");
        ////////D.Log(responseContent);
    }

    void GetUser()
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

        ////////D.Log("GetUser() - RESPONSE_CONTENT");
        ////////D.Log(responseContent);
    }
}
