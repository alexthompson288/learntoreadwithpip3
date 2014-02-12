using UnityEngine;
using System.Collections;

// TODO: Delete
public class LoadTexFromResources : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<UITexture>().mainTexture = Resources.Load ("Images/collection_room_additional_images 1/_cap") as Texture2D;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
