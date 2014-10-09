using UnityEngine;
using System.Collections;

public class AppPauseChecker : Singleton<AppPauseChecker>
{
    public delegate void AppPauseCheckerEventHandler();
    public event AppPauseCheckerEventHandler LaunchedAfterAppPause;

    bool m_hasStarted = false; // TODO: Find out whether this variable is necessary. It is used in LoginInfo (where this code was copied from)
    bool m_hasPausedApp = false;

    void OnApplicationPause()
    {
        if (m_hasStarted)
        {
            m_hasPausedApp = true;
        }
    }

    void Start()
    {
        m_hasStarted = true;
        InvokeRepeating("CheckForAppPause", 10, 10);
    }

    void CheckForAppPause()
    {
        if (m_hasPausedApp)
        {
            m_hasPausedApp = false;

            if(LaunchedAfterAppPause != null)
            {
                LaunchedAfterAppPause();
            }
        }
    }
}
