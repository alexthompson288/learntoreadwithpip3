using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.IO;

public class AssetBundleDefinition : ScriptableObject
{

    [SerializeField]
    public Object[] m_assetsToBuildIn;
    [SerializeField]
    public string m_outputPath;
    [SerializeField]
    public bool m_streamingAssets;

    public void BuildForPlatform(BuildTarget targetPlatform)
    {
        string path = string.Format("{0}/{1}/{2}/{3}.unity3d", 
            m_streamingAssets ? 
            Application.streamingAssetsPath : Path.Combine(Application.dataPath,"___ToUpload/"), 
            m_outputPath, targetPlatform.ToString(), name);

        int buildOptions = 0;
        buildOptions |= (int)BuildAssetBundleOptions.CollectDependencies;
        buildOptions |= (int)BuildAssetBundleOptions.CompleteAssets;
        buildOptions |= (int)BuildAssetBundleOptions.UncompressedAssetBundle;
		
		
		
		if ( m_assetsToBuildIn.Length > 0 )
		{
	
	        Object mainObject = m_assetsToBuildIn[0];
	        Object[] otherObjects = m_assetsToBuildIn.Skip(1).ToArray();
	
	        string directory = Path.GetDirectoryName(path);
	        Directory.CreateDirectory(directory);
	        Debug.Log("Building to " + path);
	        BuildPipeline.BuildAssetBundle(mainObject, otherObjects, path, (BuildAssetBundleOptions)buildOptions, targetPlatform);
	
	        AssetDatabase.Refresh();
			
		}
    }
}
