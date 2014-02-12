using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;


public class VideoTextureManager : AbstractManager
{
#if UNITY_IPHONE
	private static Dictionary<string, VideoTexture> videoTextureInstances = new Dictionary<string, VideoTexture>();


	static VideoTextureManager()
	{
		AbstractManager.initialize( typeof( VideoTextureManager ) );
	}
	
	
	public static void registerInstance( string instanceId, VideoTexture instance )
	{
		videoTextureInstances[instanceId] = instance;
	}
	
	
	public static void deRegisterInstance( string instanceId )
	{
		if( videoTextureInstances.ContainsKey( instanceId ) )
			videoTextureInstances.Remove( instanceId );
	}
	
	
	public void videoDidStart( string instanceId )
	{
		if( videoTextureInstances.ContainsKey( instanceId ) )
			videoTextureInstances[instanceId].onStarted();
	}


	public void videoDidFinish( string instanceId )
	{
		if( videoTextureInstances.ContainsKey( instanceId ) )
			videoTextureInstances[instanceId].onComplete();
	}

#endif
}

