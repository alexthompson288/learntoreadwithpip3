using UnityEngine;
using System.Collections;

public class SelectGameButton : MonoBehaviour {

    [SerializeField]
    private string m_gameToSelect;
    [SerializeField]
    private bool m_supportsTwoPlayer;
    [SerializeField]
    private string m_optionSelectionMenu;
    [SerializeField]
    private GameObject m_multiEnableHierarchy;
    [SerializeField]
    private string m_audioCue;
    [SerializeField]
    private UISprite[] m_starSprites;
    [SerializeField]
    private string m_starOnString;
    [SerializeField]
    private string m_starOffString;

    void Start()
    {
		//Debug.Log("SelectGameButton.Start()");
		//if(m_starSprites.Length == 0)
		//{
			//return;
		//}

#if UNITY_STANDALONE
		m_supportsTwoPlayer = false;
#endif
		//Debug.Log("Supports 2: " + m_supportsTwoPlayer);
        m_multiEnableHierarchy.SetActive(m_supportsTwoPlayer);
		//Debug.Log("multiIconActive: " + m_multiEnableHierarchy.activeInHierarchy);

		if(m_starSprites.Length != 0)
		{
	        int difficultyLevel = SessionInformation.Instance.GetHighestLevelCompletedForGame(
	            m_gameToSelect);


	        for (int index = 0; index < (difficultyLevel); ++index)
	        {
	            m_starSprites[index].spriteName = m_starOnString;
	        }
	        for (int index = (difficultyLevel); index < m_starSprites.Length; ++index)
	        {
	            m_starSprites[index].spriteName = m_starOffString;
	        }
		}
    }

    void OnClick()
    {
        StartCoroutine(GameSelected());
    }

    IEnumerator GameSelected()
    {
        if (!string.IsNullOrEmpty(m_audioCue))
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(m_audioCue);
        }

        Object[] sgbs = GameObject.FindObjectsOfType(typeof(SelectGameButton));
        foreach (SelectGameButton sgb in sgbs)
        {
            if (sgb != this)
            {
                sgb.GetComponent<TweenOnOffBehaviour>().Off();
            }
        }
        collider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        iTween.MoveTo(gameObject, Vector3.zero, 1.0f);
        yield return new WaitForSeconds(0.75f);
        UIWidget[] widgets = GetComponentsInChildren<UIWidget>();
        foreach (UIWidget widget in widgets)
        {
            TweenAlpha.Begin(widget.gameObject, 1.0f, 0.0f);
        }
        iTween.ScaleTo(gameObject, Vector3.one * 1.2f, 1.0f);
        
        yield return new WaitForSeconds(0.75f);
        SessionInformation.Instance.SelectGame(m_gameToSelect, m_supportsTwoPlayer);
        TransitionScreen.Instance.ChangeLevel(m_optionSelectionMenu, false);
    }
}
