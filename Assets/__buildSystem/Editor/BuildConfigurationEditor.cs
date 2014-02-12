using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Conflict resolver test

[CustomEditor(typeof(BuildConfiguration))]
public class BuildConfigurationEditor : Editor {

    void MoveAllAssets(string source, string dest)
    {
		Debug.Log("MoveAllAssets");
		Debug.Log("source: " + source);
		Debug.Log("dest: " + dest);

        DirectoryInfo sourceDI = new DirectoryInfo(
            Path.Combine(Application.dataPath, source));

        FileInfo[] fi = sourceDI.GetFiles("*.*", SearchOption.AllDirectories);

        EditorUtility.DisplayProgressBar("Moving files", source + " to " + dest, 0.0f);

        int index = 0;
        foreach (FileInfo file in fi)
        {
            EditorUtility.DisplayProgressBar("Moving files", source + " to " + dest, (float)index / (float)fi.Length);
            index++;
            if (!file.Extension.Contains("meta"))
            {
                string filePath = Path.GetDirectoryName(file.FullName).Replace(@"\",@"/");

                string filePathRelative = filePath.Replace(Application.dataPath,"").Replace(source, "");
                
                string sourceAsset = 
                    "Assets/" + (filePath.Replace(Application.dataPath, "")) + "/" +
                    file.Name;
                string destPath = "Assets/" + dest + filePathRelative;

                while (sourceAsset.Contains("//"))
                {
                    sourceAsset = sourceAsset.Replace("//", "/");
                }
                while (destPath.Contains("//"))
                {
                    destPath = destPath.Replace("//", "/");
                }

                if (!Directory.Exists(Application.dataPath + destPath.Substring("Assets".Length)))
                {
                    Directory.CreateDirectory(Application.dataPath + destPath.Substring("Assets".Length));
                    AssetDatabase.Refresh();
                }


                if (!file.Name.StartsWith("."))
                {
                    string result = AssetDatabase.MoveAsset(sourceAsset,
                        destPath + "/" + file.Name);
                }
                //break;
            }
        }

        EditorUtility.ClearProgressBar();
    }

    bool m_devBuild;

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        BuildConfiguration bc = (BuildConfiguration)target;
		//Debug.Log("Found BuildConfiguration");
		//Debug.Log(Path.Combine(Application.dataPath, bc.m_outputFolder));

        bool build = false;
        BuildTarget bt = BuildTarget.iPhone;
        
        if (GUILayout.Button("Build Win"))
        {
            bt = BuildTarget.StandaloneWindows;
            build = true;
        }
        if (GUILayout.Button("Build iOs"))
        {
            bt = BuildTarget.iPhone;
            build = true;
        }
        if (GUILayout.Button("Build Mac"))
        {
            bt = BuildTarget.StandaloneOSXIntel;
            build = true;
        }
        if (GUILayout.Button("Build Android"))
        {
            bt = BuildTarget.Android;
            build = true;
        }

        m_devBuild = EditorGUILayout.Toggle("Development build?", m_devBuild);

        if (GUILayout.Button("Copy to Editor Settings"))
        {
            EditorBuildSettingsScene[] ebsc = new EditorBuildSettingsScene[bc.m_includedLevels.Length];
            for (int index = 0; index < bc.m_includedLevels.Length; ++index )
            {
                ebsc[index] = new EditorBuildSettingsScene(bc.m_includedLevels[index], true);
            }
            EditorBuildSettings.scenes = ebsc;
        }

        bool doPaths = true;

        if (build)
        {
            DirectoryInfo di = new DirectoryInfo(
                Path.Combine(Application.dataPath, "ResourcesBase/"));

            DirectoryInfo[] subDirs = di.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                DirectoryInfo foundResourceDir = null;
                DirectoryInfo foundXResourceDir = null;
                foreach (DirectoryInfo rDirs in subDir.GetDirectories())
                {
                    if (rDirs.Name == "Resources")
                    {
                        foundResourceDir = rDirs;
                    }
                    if (rDirs.Name == "XResources")
                    {
                        foundXResourceDir = rDirs;
                    }
                }

                if (foundResourceDir == null)
                {
                    AssetDatabase.CreateFolder("Assets/ResourcesBase/" + subDir.Name, "Resources");
                }
                if (foundXResourceDir == null)
                {
                    AssetDatabase.CreateFolder("Assets/ResourcesBase/" + subDir.Name, "XResources");
                }

                if (doPaths)
                {
                    if (bc.m_includedBaseResourceFolders.Contains(subDir.Name))
                    {
                        MoveAllAssets("ResourcesBase/" + subDir.Name + "/XResources",
                            "ResourcesBase/" + subDir.Name + "/Resources");
                    }
                    else
                    {
                        MoveAllAssets("ResourcesBase/" + subDir.Name + "/Resources",
                            "ResourcesBase/" + subDir.Name + "/XResources");
                    }
                }
            }

            AssetDatabase.Refresh();

            bc.m_systemPrefab.SetSettings(bc.m_embeddedSettings);
            EditorUtility.SetDirty(bc);
            EditorApplication.SaveAssets();

            PlayerSettings.bundleIdentifier = bc.m_packageName;
            PlayerSettings.iPhoneBundleIdentifier = bc.m_packageName;
            int[] sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.iPhone);
            Texture2D[] tex = new Texture2D[sizes.Length];
            for (int index = 0; index < sizes.Length; ++index)
            {
                if (sizes[index] <= 1024)
                {
                    tex[index] = bc.m_icon;
                }
                else
                {
                    tex[index] = bc.m_splashScreen;
                }
            }
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iPhone,
                tex);
            PlayerSettings.iOS.applicationDisplayName = bc.m_applicationDisplayName;

			Debug.Log("Building");
			Debug.Log(Path.Combine(Application.dataPath, bc.m_outputFolder));

			foreach(string levelName in bc.m_includedLevels)
			{
				Debug.Log(levelName);
			}

            BuildPipeline.BuildPlayer(
                bc.m_includedLevels,
                Path.Combine(Application.dataPath, bc.m_outputFolder),
                bt,
                m_devBuild ? BuildOptions.Development : BuildOptions.None);

            // put all folders back
            subDirs = di.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                DirectoryInfo foundResourceDir = null;
                foreach (DirectoryInfo rDirs in subDir.GetDirectories())
                {
                    if (rDirs.Name == "Resources" || rDirs.Name == "XResources")
                    {
                        foundResourceDir = rDirs;
                    }
                }

                FileInfo foundResourceMeta = null;
                foreach (FileInfo rFiles in subDir.GetFiles())
                {
                    if (rFiles.Name == "Resources.meta" || rFiles.Name == "XResources.meta")
                    {
                        foundResourceMeta = rFiles;
                    }
                }

                if ((foundResourceDir != null) && (foundResourceMeta != null))
                {
                    if (doPaths)
                    {
                        MoveAllAssets("ResourcesBase/" + subDir.Name + "/XResources",
                            "ResourcesBase/" + subDir.Name + "/Resources");
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();

    }
	
}
