using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private PipButton[] m_activityButtons;
    [SerializeField]
    private GameObject m_bookButtonPrefab;
    [SerializeField]
    private UIGrid m_bookGrid;


    PipButton m_currentColorButton = null;
    PipButton m_currentBookButton = null;
}
