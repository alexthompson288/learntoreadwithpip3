using UnityEngine;
using System.Collections;

public class ChangeContentType : MonoBehaviour 
{
	[SerializeField]
	private Game.Session m_contentType;

	// Use this for initialization
	void Start () 
	{
		Game.SetSession(m_contentType);	
	}
}
