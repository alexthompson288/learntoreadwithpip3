using UnityEngine;
using System.Collections;

public class MainMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton m_readButton;
    [SerializeField]
    private PipButton m_mathsButton;

    IEnumerator Start()
    {
        m_readButton.SetPipColor(ColorInfo.PipColor.OrangeYellow);
        m_mathsButton.SetPipColor(ColorInfo.PipColor.DeepPink);

        yield return StartCoroutine(GameManager.WaitForInstance());

        m_readButton.Unpressed += OnPressRead;
        m_mathsButton.Unpressed += OnPressMaths;
    }

    void OnPressRead(PipButton button)
    {
        GameManager.Instance.SetProgramme(ProgrammeInfo.basicReading);
        TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
    }

    void OnPressMaths(PipButton button)
    {
        GameManager.Instance.SetProgramme(ProgrammeInfo.basicMaths);
        TransitionScreen.Instance.ChangeLevel("NewNumberGameMenu", false);
    }
}
