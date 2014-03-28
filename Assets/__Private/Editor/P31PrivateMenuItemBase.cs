using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;


public class P31PrivateMenuItemBase : MonoBehaviour
{
	protected static string getPathForTxtFile( string key )
	{
		return Application.dataPath + "/__Private/Editor/" + key + ".txt";
	}


	protected static string[] getAllSelectedAssetPaths()
	{
		var assetPaths = new List<string>();
		var allAssets = Selection.GetFiltered( typeof( UnityEngine.Object ), SelectionMode.DeepAssets );
		foreach( UnityEngine.Object item in allAssets )
		{
			var path = AssetDatabase.GetAssetPath( item.GetInstanceID() );
			assetPaths.Add( path );
		}

		return assetPaths.ToArray();
	}


	protected static void gatherAssetsAndCreateAssetBundle( string key )
	{
		// grab all our assets
		var idString = System.IO.File.ReadAllText( getPathForTxtFile( key ) );
		var assetPaths = idString.Split( new char[] { ',' } );

		UnityEngine.Debug.Log( string.Format( "total assetIds: {0}", assetPaths.Length ) );

		var allAssets = new List<UnityEngine.Object>();
		var allPaths = new List<string>();
		foreach( var path in assetPaths )
		{
			var actualAsset = AssetDatabase.LoadAssetAtPath( path, typeof( UnityEngine.Object ) );
			if( actualAsset == null )
			{
				UnityEngine.Debug.LogError( "Shit went really, really wrong. Couldnt find asset at path: " + path );
			}
			else
			{
				allPaths.Add( path );
				allAssets.Add( actualAsset );
			}
		}

		// select our items and make the unitypackage
		Selection.objects = allAssets.ToArray();
		createAssetBundle( allPaths.ToArray(), key );
	}


	protected static void storeSelectedAssetPathsToDisk( string packageName )
	{
		if( !EditorUtility.DisplayDialog( "Save selected items to disk?", "Are you sure you want to save the selected items to disk?", "Yes", "No" ) )
			return;

		var assetIds = getAllSelectedAssetPaths();
		System.IO.File.WriteAllText( getPathForTxtFile( packageName ), string.Join( ",", assetIds.ToArray() ) );
	}


	protected static void createAssetBundle( string[] filePaths, string filename )
	{
		var desktop = System.Environment.GetFolderPath( Environment.SpecialFolder.Desktop );

		//System.Environment.CurrentDirectory
		AssetDatabase.ExportPackage( filePaths, Path.Combine( desktop, filename ), ExportPackageOptions.IncludeDependencies );
	}


	private static List<UnityEngine.Object> getAllSelectedScripts()
	{
		return Selection.objects.Where( i => i.GetType() == typeof( MonoScript ) ).ToList();
	}


	[MenuItem( "prime[31] Plugin Helper/Document Selected Class", false, 0 )]
	static void documentSelectedItem()
	{
		var script = getAllSelectedScripts()[0];
		var pathToScript = Path.Combine( Application.dataPath.Replace( "Assets", string.Empty ), AssetDatabase.GetAssetPath( script ) );
		UnityEngine.Debug.Log( pathToScript );
		var args = string.Format( "/Users/desaro/Documents/dev/Unity3D/UnityPlugins/_AppleScriptCode/CodeGenerators/createBindingFile.php \"{0}\" forceDocs", pathToScript );

		var proc = new System.Diagnostics.Process
		{
    		StartInfo = new System.Diagnostics.ProcessStartInfo
			{
        		FileName = "php",
		        Arguments = args,
		        UseShellExecute = false,
		        RedirectStandardOutput = true,
		        CreateNoWindow = true
			}
		};

		proc.Start();
		while( !proc.StandardOutput.EndOfStream )
		{
			var line = proc.StandardOutput.ReadLine();
			UnityEngine.Debug.Log( line );
		}
	}


	[MenuItem( "prime[31] Plugin Helper/Document Selected Class", true )]
	static bool documentSelectedItemValidator()
	{
		if( Selection.objects.Length == 0 )
			return false;

		return Selection.objects.Length == 1 && getAllSelectedScripts().Count == 1;
	}

}
