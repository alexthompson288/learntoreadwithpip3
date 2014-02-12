using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

public class StoryAssetBundleBuilder : EditorWindow {

    [MenuItem("Window/LTRWP/Story Asset Bundle Builder")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(StoryAssetBundleBuilder));
    }

    SqliteDatabase m_db;

    bool m_buildIos = false;
    bool m_buildWindows = false;

    void OnGUI()
    {
        if (m_db == null)
        {
            string cmsDbPath = Path.Combine(UnityEngine.Application.persistentDataPath,
       ("Stories_local-store.bytes"));
            Debug.Log("Database name for this game: " + cmsDbPath);
            m_db = new SqliteDatabase();
            m_db.Open(cmsDbPath);
        }

        if (m_db != null)
        {
            DataTable dataTable = m_db.ExecuteQuery(
                        "select * from stories where publishable='t' order by difficulty");

            int bookTotal = dataTable.Rows.Count;
            int bookIndex = 0;

            m_buildIos = EditorGUILayout.Toggle("Build iOS packs", m_buildIos);
            m_buildWindows = EditorGUILayout.Toggle("Build Standalone Windows packs", m_buildWindows);

            if (GUILayout.Button("Make all definitions"))
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    int bookId = Convert.ToInt32(dr["id"].ToString());

                    List<string> resources = new List<string>();
                    int pageIndex = 1;
                    while (true)
                    {
                        DataTable pageTable = m_db.ExecuteQuery("select * from storypages where story_id='" + bookId + "' and pageorder='" + pageIndex+ "'");

                        if (pageTable.Rows.Count == 0)
                        {
                            break;
                        }
                        DataRow page = pageTable.Rows[0];
                        if ((page["image"] != null) && (!string.IsNullOrEmpty(page["image"].ToString())))
                        {
                            resources.Add(page["image"].ToString());
                        }
                        if ((page["backgroundart"] != null) && (!string.IsNullOrEmpty(page["backgroundart"].ToString())))
                        {
                            resources.Add(page["backgroundart"].ToString());
                        }
                        if ((page["audio"] != null) && (!string.IsNullOrEmpty(page["audio"].ToString())))
                        {
                            resources.Add(page["audio"].ToString());
                        }
                        pageIndex++;
                    }

                    string assetPathAndName = "Assets/__BundleSource/" + dr["title"].ToString().Replace(" ", "_").Replace("?", "_").Replace("!","_").ToLower() + ".asset";
                    AssetBundleDefinition asset = (AssetBundleDefinition)AssetDatabase.LoadAssetAtPath(assetPathAndName, typeof(AssetBundleDefinition));
                    bool newAsset = false;
                    if (asset == null)
                    {
                        asset = ScriptableObject.CreateInstance<AssetBundleDefinition>();
                        newAsset = true;
                    }

                    asset.m_outputPath = "";
                    asset.m_streamingAssets = false;
                    List<UnityEngine.Object> foundObjects = new List<UnityEngine.Object>();

                    DirectoryInfo di = new DirectoryInfo(Application.dataPath);

                    int index = 0;
                    foreach (string tex in resources)
                    {

                        float progress = (float)bookIndex / (float)bookTotal
                            + (((float)index / (float)resources.Count) / (float)bookTotal);
                        EditorUtility.DisplayProgressBar("Building... " + (bookIndex+1) + " of " + bookTotal,
                            dr["title"].ToString(), progress);

                        FileInfo[] fi = di.GetFiles(Path.GetFileNameWithoutExtension(tex) + ".*", SearchOption.AllDirectories);
                        Debug.Log(tex.ToLower());
                        foreach (FileInfo f in fi)
                        {                            
                            if (Path.GetFileNameWithoutExtension(f.Name).ToLower() ==
                                Path.GetFileNameWithoutExtension(tex.ToLower()))
                            {
                                Debug.Log("Found matching file");
                                string foundFileName = 
                                    f.DirectoryName.Replace(@"\", @"/").Replace(Application.dataPath, "Assets")
                                    + "/" + f.Name;
                                Debug.Log("Asset file: " + foundFileName);
                                UnityEngine.Object foundObject =
                                    AssetDatabase.LoadAssetAtPath(foundFileName
                                    ,
                                    typeof(UnityEngine.Object));
                                if (!foundObjects.Contains(foundObject))
                                {
                                    foundObjects.Add(foundObject);
                                }                                
                            }
                        }
                        index++;
                    }

                    asset.m_assetsToBuildIn = foundObjects.ToArray();

                    if (newAsset)
                    {
                        AssetDatabase.CreateAsset(asset, assetPathAndName);
                    }
                    else
                    {
                        EditorUtility.SetDirty(asset);
                    }

                    if (m_buildWindows)
                    {
                        asset.BuildForPlatform(BuildTarget.StandaloneWindows);
                    }
                    if (m_buildIos)
                    {
                        asset.BuildForPlatform(BuildTarget.iPhone);
                    }

                    AssetDatabase.SaveAssets();
                    bookIndex++;

                }
                EditorUtility.ClearProgressBar();
            }

            

            GUILayout.Label("Found: " + dataTable.Rows.Count + " books");
            foreach (DataRow dr in dataTable.Rows)
            {                
                GUILayout.Label(dr["title"].ToString());                
            }
        }
        else
        {
            Debug.Log("No DB");
        }
    }
}
