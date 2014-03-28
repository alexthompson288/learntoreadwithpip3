using UnityEngine;
using System.Collections;


#if UNITY_IPHONE || UNITY_ANDROID
public enum GPGToastPlacement
{
	Top,
	Bottom,
	Center
}


public enum GPGLeaderboardTimeScope
{
	Unknown = -1,
	Today = 1,
	ThisWeek = 2,
	AllTime = 3
}
#endif