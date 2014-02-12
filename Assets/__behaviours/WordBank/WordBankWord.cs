using UnityEngine;
using System.Collections;

public class WordBankWord : MonoBehaviour {

    [SerializeField]
    private UILabel m_textLabel;
    [SerializeField]
    private UISprite m_sprite;
    [SerializeField]
    private GameObject m_unlockedHierarchy;
    [SerializeField]
    private Transform[] m_starSprites;

    DataRow m_word;
    bool m_isUnlocked;

    public void SetUp(DataRow word)
    {
        m_word = word;
        m_textLabel.text = word["word"].ToString();
        Refresh(true);
    }

    public void Refresh(bool original)
    {
        if (SessionInformation.Instance.IsWordUnlocked(m_word["word"].ToString()))
        {
            if (!m_isUnlocked)
            {
                if (original)
                {
                    //m_sprite.spriteName = m_unlockedSpriteName;
                    m_sprite.enabled = false;
                    m_unlockedHierarchy.SetActive(true);
                }
                else
                {
                    StartCoroutine(ShowUnlock());
                }
                m_isUnlocked = true;
            }
        }
    }

    IEnumerator ShowUnlock()
    {
        // yeah yeah, this isn't a good way to get it :\
        transform.parent.parent.GetComponent<UIDraggablePanel>().enabled = false;

        iTween.ScaleTo(gameObject, Vector3.one * 1.2f, 1.0f);
        Vector3 pos = transform.position;
        Vector3 tPos = pos * 0.5f;
        iTween.MoveTo(gameObject, tPos, 1.0f);

        foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
        {
            widget.depth += 5;
        }

        yield return new WaitForSeconds(1.0f);

        CharacterPopper cPop = (CharacterPopper)GameObject.FindObjectOfType(typeof(CharacterPopper));
        if (cPop != null)
        {
            cPop.PopCharacter();
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
        iTween.RotateBy(m_sprite.gameObject, new Vector3(0, 0, Random.Range(-2.0f, 2.0f)), 2.0f);
        yield return new WaitForSeconds(0.1f);
        iTween.MoveTo(m_sprite.gameObject, m_sprite.transform.position + (Random.onUnitSphere * 1.5f), 2.0f);
        yield return new WaitForSeconds(0.1f);
        TweenAlpha.Begin(m_sprite.gameObject, 1.0f, 0.0f);
        m_unlockedHierarchy.SetActive(true);
        foreach (Transform t in m_starSprites)
        {
            t.localScale = Vector3.zero;
        }
        foreach (Transform t in m_starSprites)
        {
            iTween.ScaleTo(t.gameObject, Vector3.one * 2.0f, 0.75f);
            yield return new WaitForSeconds(0.3f);
        }
        iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
        iTween.MoveTo(gameObject, pos, 1.0f);
        yield return new WaitForSeconds(1.0f);

        foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
        {
            widget.depth -= 5;
        }

        // yeah yeah, this isn't a good way to get it :\
        transform.parent.parent.GetComponent<UIDraggablePanel>().enabled = true;
        m_sprite.enabled = false;
        yield break;
    }

    void OnClick()
    {
        string wordAsString = m_word["word"].ToString();
        PipPadBehaviour.Instance.Show(wordAsString);
        if (!SessionInformation.Instance.IsWordUnlocked(wordAsString))
        {
            WordBankGrid.Instance.ShowThreeWords(wordAsString);
        }
    }
}
