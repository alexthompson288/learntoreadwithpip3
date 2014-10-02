using UnityEngine;
using System.Collections;

public class BasicGameMenuCoordinator : Singleton<BasicGameMenuCoordinator> 
{
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private PipColorWidgets m_colorBehaviour;

    ColorInfo.PipColor m_pipColor;

    public void On(ColorInfo.PipColor myPipColor, bool isMaths)
    {
        m_pipColor = myPipColor;
        m_colorBehaviour.SetPipColor(m_pipColor);

        string programmeName = isMaths ? ProgrammeInfo.basicMaths : ProgrammeInfo.basicReading;

        GameManager.Instance.SetProgramme(programmeName);
        m_titleLabel.text = isMaths ? "Maths" : "Reading";
    }

}
