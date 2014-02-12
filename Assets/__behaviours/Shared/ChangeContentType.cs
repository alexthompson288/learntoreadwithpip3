using UnityEngine;
using System.Collections;

public class ChangeContentType : MonoBehaviour 
{
	[SerializeField]
	private GameDataBridge.ContentType m_contentType;

	// Use this for initialization
	void Start () 
	{
		GameDataBridge.Instance.SetContentType(m_contentType);	
	}
}
