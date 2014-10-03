using UnityEngine;
using System.Collections;
using Wingrove;

public class SessionCompleteCoordinator : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_sessionCompleteButtonPrefab;
    [SerializeField]
    private UIGrid m_grid;
    [SerializeField]
    private string[] m_spriteNames;
    [SerializeField]
    private GameObject m_collectable;
    [SerializeField]
    private UISprite m_collectableBackground;
    [SerializeField]
    private UILabel m_collectableLabel;
    [SerializeField]
    private UITexture m_collectableIcon;
    [SerializeField]
    private TweenOnOffBehaviour m_bennyTween;
    [SerializeField]
    private SpriteAnim m_bennyAnim;
    [SerializeField]
    private UISprite m_bennySprite;
    [SerializeField]
    private Transform m_bennyCollectionLocation;
    [SerializeField]
    private Transform m_bennyEndLocation;
    [SerializeField]
    private AnimManager m_pipAnimManager;
    [SerializeField]
    private Transform m_pipLocation;
    [SerializeField]
    private ClickEvent m_nextButton;
    [SerializeField]
    private TweenOnOffBehaviour m_nextButtonTween;


    RotateConstantly m_currentRotateBehaviour = null;

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        bool hasSetLabel = false;
        bool hasSetIcon = false;


        int sessionId = VoyageInfo.Instance.currentSessionId;
        
        for (int i = 0; i < m_spriteNames.Length; ++i)
        {
            GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionCompleteButtonPrefab, m_grid.transform);
            newButton.GetComponent<ClickEvent>().Unpressed += OnButtonClick;
            newButton.GetComponent<ClickEvent>().SetString(m_spriteNames[i]);
            newButton.GetComponentInChildren<UISprite>().spriteName = m_spriteNames[i];
        }
        
//        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programmodules WHERE id=" + OldVoyageInfo.Instance.currentModuleId);
//        
//        if(dt.Rows.Count > 0 && dt.Rows[0]["modulereward"] != null)
//        {
//            string moduleReward = dt.Rows[0]["modulereward"].ToString();
//            
//            if(moduleReward == "Phonemes")
//            {
//                dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE programsession_id=" + sessionId);
//                
//                foreach (DataRow phoneme in dt.Rows)
//                {
//                    if (phoneme["is_target_phoneme"] != null && phoneme ["is_target_phoneme"].ToString() == "t")
//                    {
//                        m_collectableLabel.text = phoneme["phoneme"].ToString();
//                        hasSetLabel = true;
//                    }
//                }
//            }
//        } 

        m_collectableLabel.gameObject.SetActive(hasSetLabel);
        m_collectableIcon.gameObject.SetActive(hasSetIcon);

        m_grid.Reposition();

        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
        yield return new WaitForSeconds(0.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("BENNY_ANOTHER_LETTER_2");

        m_pipAnimManager.PlayAnimation("WALK");

        float pipTweenDuration = 1f;

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_pipLocation.position);
        tweenArgs.Add("time", pipTweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        iTween.MoveTo(m_pipAnimManager.gameObject, tweenArgs);

        yield return new WaitForSeconds(pipTweenDuration);

        m_pipAnimManager.PlayAnimation("THUMBS_UP");
	}

    IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(0.25f);

        m_bennySprite.depth = 15;

        m_bennyTween.On();

        iTween.ScaleTo(m_collectable, Vector3.one * 1.1f, m_bennyTween.GetDuration());

        WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_PAD_DISAPPEAR");

        yield return new WaitForSeconds(m_bennyTween.GetDuration());

        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
        m_pipAnimManager.PlayAnimation("JUMP");

        m_bennyAnim.PlayAnimation("OPEN");

        yield return new WaitForSeconds(0.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");

        m_bennySprite.depth = 5;

        float smallScaleDuration = 0.5f;

        iTween.ScaleTo(m_collectable, Vector3.zero, smallScaleDuration);
        iTween.MoveTo(m_collectable, m_bennyCollectionLocation.position, smallScaleDuration);

        yield return new WaitForSeconds(smallScaleDuration);

        m_bennyAnim.PlayAnimation("CLOSE");

        yield return new WaitForSeconds(0.5f);

        iTween.MoveTo(m_bennyTween.gameObject, m_bennyEndLocation.position, 1f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

        GameManager.Instance.CompleteGame();
    }

    void OnButtonClick(ClickEvent click)
    {
        if (m_currentRotateBehaviour != null)
        {
            m_currentRotateBehaviour.enabled = false;
        }

        m_currentRotateBehaviour = click.GetComponent<RotateConstantly>() as RotateConstantly;
        m_currentRotateBehaviour.enabled = true;

        m_collectableBackground.spriteName = click.GetString();

        VoyageInfo.Instance.AddSessionBackground(VoyageInfo.Instance.currentSessionId, click.GetString());

        StartCoroutine(CompleteGame());
    }
}
