using UnityEngine;
using System.Collections;
using System;

public class UserException : Exception
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

    public UserException(ExceptionType myExceptionType)
    {
        m_exceptionType = myExceptionType;
    }
}

public static class UserHelpers 
{
	static string m_url = "http://learnwithpip-staging.herokuapp.com/api/v1/";
	//http://private-281e5-pip.apiary-mock.com/api/v1/token
	//http://private-281e5-pip.apiary-mock.com/api/v1/users/me
	
	static string m_tokenRequest = "tokens";
	static string m_userRequest = "users/me";

	public static string ParseResponse(string responseContent, string prefix, string end)
	{
		int prefixIndex = responseContent.IndexOf(prefix);
		string info = responseContent.Substring(prefixIndex + prefix.Length);
		int endIndex = info.IndexOf(end);

		return endIndex != -1 ? info.Substring (0, endIndex) : info;
	}
	
	public static string RequestToken(string email, string password)
	{
		#if UNITY_EDITOR
		// Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
		//System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		#endif
		
		D.Log(m_url + m_tokenRequest);
		var request = System.Net.WebRequest.Create(m_url + m_tokenRequest) as System.Net.HttpWebRequest;
		request.KeepAlive = true;
		request.Method = "POST";
		request.ContentType="application/json";
		
		#if UNITY_EDITOR
		//k+1@x8.io / testtest
		// tom@learnwithpip.com / pip12345
		//email = "tom@learnwithpip.com";
		//password = "pip12345";
		#endif
		
		byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\n    \"email\": \"" + email + "\",\n    \"password\": \"" + password + "\"\n}");
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
	
	static string GetUser()
	{
		#if UNITY_EDITOR
		// Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
		//System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		#endif
		
		D.Log (m_url + m_userRequest);
		var request = System.Net.WebRequest.Create(m_url + m_userRequest) as System.Net.HttpWebRequest;
		request.KeepAlive = true;
		request.Method = "GET";
		request.ContentType="application/json";
		request.Headers.Add("authorization", "Token token=\"" + UserInfo.Instance.GetAccessToken() + "\""); //q3nXHxmk6ovg-dnoXupy
		request.ContentLength = 0;

		string responseContent=null;
		using (var response = request.GetResponse() as System.Net.HttpWebResponse) {
			using (var reader = new System.IO.StreamReader(response.GetResponseStream())) {
				responseContent = reader.ReadToEnd();
			}
		}
		
		return responseContent;
	}

	public static bool IsUserLegal()
	{
		D.Log ("UserHelpers.GetUserState()");
		string userResponse = GetUser();
		
		if (!userResponse.Contains("error"))
        {
            string userPrefix = "\"can_access_content\":";

            string canAccessContent = ParseResponse(userResponse, userPrefix, "}");
            D.Log("canAccessContent: " + canAccessContent);

            if (canAccessContent == "true")
            {
                D.Log("GOOD");
                return true;
            } 
            else
            {
                D.Log("EXPIRED");
                throw new UserException(UserException.ExceptionType.Expired);
            }
        } 
        else
        {
            D.Log("INVALID_TOKEN");
            throw new UserException(UserException.ExceptionType.InvalidToken);
        }
	}
}
