using UnityEngine;
using System.Collections;

public class LevelMenuCustomButton : MonoBehaviour 
{
	void OnClick () 
	{
		GameDataBridge.Instance.SetContentType(GameDataBridge.ContentType.Custom);
		LevelMenuCoordinator.Instance.SelectCustom();
	}
}
