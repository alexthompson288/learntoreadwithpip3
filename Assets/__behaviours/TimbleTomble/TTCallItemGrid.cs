using UnityEngine;
using System.Collections;

public class TTCallItemGrid : MonoBehaviour 
{
	[SerializeField]
	private string methodName;

	// Use this for initialization
	void OnClick () 
	{
		TTItemLoader.Instance.SendMessage(methodName);
	}
}
