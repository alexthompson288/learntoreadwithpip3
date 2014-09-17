using UnityEngine;
using System.Collections;
using System;

public class LoginException : Exception
{
    public enum ExceptionType
    {
        Expired,
        InvalidToken
    }

    ExceptionType m_exceptionType;
    public ExceptionType exceptionType
    {
        get
        {
            return m_exceptionType;
        }
    }

    public LoginException(ExceptionType myExceptionType)
    {
        m_exceptionType = myExceptionType;
    }
}

public static class LoginHelpers 
{
    private static string m_accessPrefix = "\"access_token\":\"";
    public static string accessPrefix
    {
        get
        {
            return m_accessPrefix;
        }
    }
    
    private static string m_expirationPrefix = "\"expiration_date\":\"";
    public static string expirationPrefix
    {
        get
        {
            return m_expirationPrefix;
        }
    }

    private static string m_dateTimeFormat = "yyyy-MM-dd";
    public static string dateTimeFormat
    {
        get
        {
            return m_dateTimeFormat;
        }
    }

    static string m_url = "http://www.learnwithpip.com/api/v1/";
	//static string m_url = "http://learnwithpip-staging.herokuapp.com/api/v1/";
	
	static string m_tokenExtension = "tokens";
	static string m_userExtension = "users/me";
    static string m_registerExtension = "newusers";

	public static string ParseResponse(string responseContent, string prefix, string end)
	{
		int prefixIndex = responseContent.IndexOf(prefix);
		string info = responseContent.Substring(prefixIndex + prefix.Length);
		int endIndex = info.IndexOf(end);

		return endIndex != -1 ? info.Substring (0, endIndex) : info;
	}

    public static string Register(string email, string password, string name)
    {
        ////D.Log("LoginHelpers.Register()");
        #if UNITY_EDITOR
        // Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
        //System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        #endif
        
        var request = System.Net.WebRequest.Create(m_url + m_registerExtension) as System.Net.HttpWebRequest;
        request.KeepAlive = true;
        request.Method = "POST";
        request.ContentType="application/json";

        // email, password, name, password confirmation
        //byte[] bytfArray = System.Text.Encoding.UTF8.GetBytes("{\n    \"email\": \"" + email + "\",\n    \"password\": \"" + password + "\"\n}");

        ////D.Log("email: " + email);
        ////D.Log("password: " + password);
        ////D.Log("name: " + name);

        //string byteString = String.Format("{\n    \"email\": \"{0}\",\n    \"password\": \"{1}\",\n    \"name\": \"{2}\",\n    \"password_confirmation\": \"{3}\"\n}", 
                                          //new object[] { email, password, name, password });

        string byteString = "{\n    \"email\": \"" + email + "\",\n    \"password\": \"" + password + "\",\n    \"name\": \"" + name + "\",\n    \"password_confirmation\": \"" + password + "\"\n}";


        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(byteString);
            
        Debug.Log(byteString);


        request.ContentLength = byteArray.Length;
        using (var writer = request.GetRequestStream()){writer.Write(byteArray, 0, byteArray.Length);}
        
        
        string responseContent=null;
        
        using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
            using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
                responseContent = reader.ReadToEnd();
            }
        }

        return responseContent;
    }
	
	public static string RequestToken(string email, string password)
	{
		#if UNITY_EDITOR
		// Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
		//System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		#endif
		
		var request = System.Net.WebRequest.Create(m_url + m_tokenExtension) as System.Net.HttpWebRequest;
		request.KeepAlive = true;
		request.Method = "POST";
		request.ContentType="application/json";
		
		byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\n    \"email\": \"" + email + "\",\n    \"password\": \"" + password + "\"\n}");
		request.ContentLength = byteArray.Length;
		using (var writer = request.GetRequestStream()){writer.Write(byteArray, 0, byteArray.Length);}
		

		string responseContent=null;

		using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
			using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
				responseContent = reader.ReadToEnd();
			}
		}

        D.Log("responseContent: " + responseContent);

		return responseContent;
	}
	
	static string GetUser(string accessToken)
	{
		#if UNITY_EDITOR
		// Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
		//System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		#endif
		
		//////D.Log (m_url + m_userExtension);
		var request = System.Net.WebRequest.Create(m_url + m_userExtension) as System.Net.HttpWebRequest;
		request.KeepAlive = true;
		request.Method = "GET";
		request.ContentType="application/json";
        request.Headers.Add("authorization", "Token token=\"" + accessToken + "\""); //q3nXHxmk6ovg-dnoXupy
		request.ContentLength = 0;

		string responseContent=null;
		using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
			using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
				responseContent = reader.ReadToEnd();
			}
		}
		
		return responseContent;
	}

	public static bool IsUserLegal(string accessToken)
	{
		//////D.Log ("LoginHelpers.GetUserState()");
		string userResponse = GetUser(accessToken);

        //////D.Log("IsUserLegal.userResponse: " + userResponse);
		
		if (!userResponse.Contains("error"))
        {
            string userPrefix = "\"can_access_content\":";

            string canAccessContent = ParseResponse(userResponse, userPrefix, "}");
            //////D.Log("canAccessContent: " + canAccessContent);

            if (canAccessContent == "true")
            {
                //////D.Log("GOOD");
                return true;
            } 
            else
            {
                //////D.Log("EXPIRED");
                throw new LoginException(LoginException.ExceptionType.Expired);
            }
        } 
        else
        {
            //////D.Log("INVALID_TOKEN");
            throw new LoginException(LoginException.ExceptionType.InvalidToken);
        }
	}
}
