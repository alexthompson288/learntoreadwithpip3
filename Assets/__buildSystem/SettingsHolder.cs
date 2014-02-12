using UnityEngine;
using System.Collections;

public class SettingsHolder : Singleton<SettingsHolder> {

    [SerializeField]
    private ScriptableObject m_settings;

    public ScriptableObject GetSettings()
    {
        return m_settings;
    }

    public void SetSettings(ScriptableObject settings)
    {
        m_settings = settings;
    }
}
