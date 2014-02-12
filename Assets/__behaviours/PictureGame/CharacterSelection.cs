using UnityEngine;
using System.Collections;

public class CharacterSelection : MonoBehaviour {

    [SerializeField]
    private GamePlayer m_gamePlayer;
    [SerializeField]
    private int m_characterIndex;
    [SerializeField]
    private TweenOnOffBehaviour m_bannerTween;

    private bool m_selected = false;

    void Start()
    {
        m_gamePlayer.RegisterCharacterSelection(this);
    }

    public int GetCharacterIndex()
    {
        return m_characterIndex;
    }

    void OnClick()
    {
        collider.enabled = false;
        m_selected = true;
        m_gamePlayer.SelectCharacter(m_characterIndex);
        WingroveAudio.WingroveRoot.Instance.PostEvent("PLAYER_" + m_characterIndex + "_SELECT");
        m_bannerTween.Off();
        GetComponent<AtoBPressButton>().ManualActivate(true);
    }

    public void DeactivatePress(bool final)
    {
        if (final || !m_selected)
        {
            collider.enabled = false;
            GetComponent<TweenOnOffBehaviour>().Off();
        }
    }


}
