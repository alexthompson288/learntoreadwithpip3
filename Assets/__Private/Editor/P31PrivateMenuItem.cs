using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


public class P31PrivateMenuItem : P31PrivateMenuItemBase
{
	private static string _iosPackageName = "PlayGameServices.unitypackage";
	private static string _androidPackageName = "PlayGameServicesAndroid.unitypackage";
	private static string _comboPackageName = "PlayGameServicesCombo.unitypackage";


	[MenuItem( "prime[31] Plugin Helper/Store Selected Items (iOS)" )]
	static void storeiOS()
	{
		storeSelectedAssetPathsToDisk( _iosPackageName );
	}


	[MenuItem( "prime[31] Plugin Helper/Create (iOS)" )]
	static void makeiOS()
	{
		gatherAssetsAndCreateAssetBundle( _iosPackageName );
	}


	[MenuItem( "prime[31] Plugin Helper/Store Selected Items (Android)" )]
	static void storeAndroid()
	{
		storeSelectedAssetPathsToDisk( _androidPackageName );
	}


	[MenuItem( "prime[31] Plugin Helper/Create (Android)" )]
	static void makeAndroid()
	{
		gatherAssetsAndCreateAssetBundle( _androidPackageName );
	}


	[MenuItem( "prime[31] Plugin Helper/Store Selected Items (Combo)" )]
	static void storeCombo()
	{
		storeSelectedAssetPathsToDisk( _comboPackageName );
	}


	[MenuItem( "prime[31] Plugin Helper/Create (Combo)" )]
	static void makeCombo()
	{
		gatherAssetsAndCreateAssetBundle( _comboPackageName );
	}


	[MenuItem( "prime[31] Plugin Helper/Create (All)", false, 0 )]
	static void makeAll()
	{
		gatherAssetsAndCreateAssetBundle( _iosPackageName );
		gatherAssetsAndCreateAssetBundle( _androidPackageName );
		gatherAssetsAndCreateAssetBundle( _comboPackageName );
	}

}
