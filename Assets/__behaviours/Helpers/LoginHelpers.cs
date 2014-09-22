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
        var request = System.Net.WebRequest.Create(m_url + m_registerExtension) as System.Net.HttpWebRequest;
        request.KeepAlive = true;
        request.Method = "POST";
        request.ContentType="application/json";

        string byteString = "{\n    \"email\": \"" + email + "\",\n    \"password\": \"" + password + "\",\n    \"name\": \"" + name + "\",\n    \"password_confirmation\": \"" + password + "\"\n}";

        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(byteString);

        request.ContentLength = byteArray.Length;
        using (var writer = request.GetRequestStream()){writer.Write(byteArray, 0, byteArray.Length);}
        
        
        string responseContent=null;
        
        using (var response = request.GetResponse() as System.Net.HttpWebResponse)
        {
            using (var reader = new System.IO.StreamReader(response.GetResponseStream())) 
            {
                responseContent = reader.ReadToEnd();
            }
        }

        return responseContent;
    }
	
	public static string RequestToken(string email, string password)
	{	
		var request = System.Net.WebRequest.Create(m_url + m_tokenExtension) as System.Net.HttpWebRequest;
		request.KeepAlive = true;
		request.Method = "POST";
		request.ContentType="application/json";
		
		byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\n    \"email\": \"" + email + "\",\n    \"password\": \"" + password + "\"\n}");
		request.ContentLength = byteArray.Length;
		using (var writer = request.GetRequestStream()){writer.Write(byteArray, 0, byteArray.Length);}
		

		string responseContent=null;

		using (var response = request.GetResponse() as System.Net.HttpWebResponse)
        {
			using (var reader = new System.IO.StreamReader(response.GetResponseStream())) 
            {
				responseContent = reader.ReadToEnd();
			}
		}

		return responseContent;
	}
	
	static string GetUser(string accessToken)
	{
		var request = System.Net.WebRequest.Create(m_url + m_userExtension) as System.Net.HttpWebRequest;
		request.KeepAlive = true;
		request.Method = "GET";
		request.ContentType="application/json";
        request.Headers.Add("authorization", "Token token=\"" + accessToken + "\""); //q3nXHxmk6ovg-dnoXupy
		request.ContentLength = 0;

		string responseContent=null;
		using (var response = request.GetResponse() as System.Net.HttpWebResponse) 
        {
			using (var reader = new System.IO.StreamReader(response.GetResponseStream())) 
            {
				responseContent = reader.ReadToEnd();
			}
		}
		
		return responseContent;
	}

	public static bool IsUserLegal(string accessToken)
	{
		string userResponse = GetUser(accessToken);
       		
		if (!userResponse.Contains("error"))
        {
            D.Log("userResponse: " + userResponse);
            return true;
            /*
            string userPrefix = "\"can_access_content\":";

            string canAccessContent = ParseResponse(userResponse, userPrefix, "}");

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
            */
        } 
        else
        {
            throw new LoginException(LoginException.ExceptionType.InvalidToken);
        }
	}
}
