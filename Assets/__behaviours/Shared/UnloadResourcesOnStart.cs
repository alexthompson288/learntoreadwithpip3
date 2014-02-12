using UnityEngine;
using System.Collections;

public class UnloadResourcesOnStart : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Resources.UnloadUnusedAssets();
	}
}
