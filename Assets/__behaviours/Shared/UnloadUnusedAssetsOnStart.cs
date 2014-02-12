using UnityEngine;
using System.Collections;

// TODO: Deprecate this script. This script is a temporary fix to the bug where the game crashes if the collection room is opened twice. 
public class UnloadUnusedAssetsOnStart : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Resources.UnloadUnusedAssets();	
	}
}
