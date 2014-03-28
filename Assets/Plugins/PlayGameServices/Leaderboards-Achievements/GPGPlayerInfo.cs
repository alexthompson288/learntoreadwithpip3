using UnityEngine;
using Prime31;



#if UNITY_IPHONE || UNITY_ANDROID
public class GPGPlayerInfo
{
	public string avatarUrl;
	public string avatarUrlHiRes; // Android only
	public string name;
	public string playerId;
}
#endif