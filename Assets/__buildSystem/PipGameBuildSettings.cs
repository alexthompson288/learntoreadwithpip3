using UnityEngine;
using System.Collections;

public class PipGameBuildSettings : ScriptableObject
{
    [SerializeField]
    public int[] m_difficultyDatabaseIds;
    [SerializeField]
    public int[] m_nonsenseDatabaseIds;

    [SerializeField]
    public string m_buildDescription;
	[SerializeField]
	public Texture2D m_transitionScreenTexture;
	
	[SerializeField]
	public Texture2D m_mainMenuBackground;
	[SerializeField]
	public Texture2D m_primaryBackground;
	[SerializeField]
	public Texture2D m_secondaryBackground;
	
	[SerializeField]
	public bool m_alwaysUseOfflineDatabase;

	[SerializeField]
	public bool m_isEverythingUnlocked;

    [SerializeField]
    private string m_releaseUserType;
    public string m_userType
    {
        get
        {
            return Debug.isDebugBuild ? "Developer" : m_releaseUserType;
        }
    }

	[SerializeField]
	public bool m_disableNavMenu;

	[SerializeField]
	public string m_startingSceneName;


	public string GetConvertedName()
	{
		return m_buildDescription.Replace(":","_").Replace(' ','_');
	}
}
