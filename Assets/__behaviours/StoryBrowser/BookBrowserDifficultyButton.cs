using UnityEngine;
using System.Collections;

public class BookBrowserDifficultyButton : MonoBehaviour {

    [SerializeField]
    private int m_difficulty;
    [SerializeField]
    private NewStoryBrowserBookPopulator m_gridPopulator;

    void OnClick()
    {
		Debug.Log("BookBrowserDifficultyButton - " + name + ".OnClick()");
        m_gridPopulator.JumpToDifficulty(m_difficulty);
    }
}
