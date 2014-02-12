using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

    [SerializeField]
    private TweenOnOffBehaviour[] m_playerNumberSelections;

    [SerializeField]
    private TweenOnOffBehaviour[] m_difficultSelections;

    IEnumerator Start()
    {
        yield return null;
        SessionInformation.Instance.SetRetryScene(Application.loadedLevelName);
        if (!SessionInformation.Instance.SupportsTwoPlayer())
        {
            foreach (TweenOnOffBehaviour two in m_playerNumberSelections)
            {
                two.gameObject.SetActive(false);
                two.Off();
            }
            SessionInformation.Instance.SetNumPlayers(1);
            StartCoroutine(ShowDifficultySelect(0.0f));
        }
    }

    public void SelectPlayerCount(int playerCount)
    {
        SessionInformation.Instance.SetNumPlayers(playerCount);
        foreach (TweenOnOffBehaviour two in m_playerNumberSelections)
        {
            two.Off();
        }
        StartCoroutine(ShowDifficultySelect(1.5f));
    }

    IEnumerator ShowDifficultySelect(float delay)
    {
        yield return new WaitForSeconds(delay);
        int index = 0;
        WingroveAudio.WingroveRoot.Instance.PostEvent("DIFFICULTY_SELECT");
        foreach (TweenOnOffBehaviour two in m_difficultSelections)
        {
            two.On();
            ++index;
        }
    }

    public void SelectDifficulty(int difficulty)
    {
        SessionInformation.Instance.SetDifficulty(difficulty);
        foreach (TweenOnOffBehaviour two in m_difficultSelections)
        {
            two.Off();
        }
        Invoke("ChangeLevel", 1.5f);
    }

    public void ChangeLevel()
    {
        TransitionScreen.Instance.ChangeLevel(SessionInformation.Instance.GetSelectedGame(), false);
    }
}
