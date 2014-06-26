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
        yield return StartCoroutine(GameManager.WaitForInstance());

        m_readButton.Unpressed += OnPressRead;
        m_mathsButton.Unpressed += OnPressMaths;
    }

    void OnPressRead(PipButton button)
    {
        GameManager.Instance.SetCurrentProgramme("Reading1");
        TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
    }

    void OnPressMaths(PipButton button)
    {
        GameManager.Instance.SetCurrentProgramme("Maths1");
        TransitionScreen.Instance.ChangeLevel("NewNumberGameMenu", false);
    }
}
