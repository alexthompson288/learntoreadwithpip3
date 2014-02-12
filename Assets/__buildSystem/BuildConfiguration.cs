using UnityEngine;
using System.Collections;

public class BuildConfiguration : ScriptableObject {

    [SerializeField]
    public string[] m_includedLevels;
    [SerializeField]
    public string[] m_includedBaseResourceFolders;
    [SerializeField]
    public string m_packageName;
    [SerializeField]
    public string m_outputFolder;
    [SerializeField]
    public Texture2D m_icon;
    [SerializeField]
    public Texture2D m_splashScreen;
    [SerializeField]
    public string m_applicationDisplayName;
    [SerializeField]
    public ScriptableObject m_embeddedSettings;
    [SerializeField]
    public SettingsHolder m_systemPrefab;
}
