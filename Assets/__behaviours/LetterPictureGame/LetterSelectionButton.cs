using UnityEngine;
using System.Collections;

public class LetterSelectionButton : MonoBehaviour {

    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UITexture m_backgroundTexture;
    [SerializeField]
    private ParticleSystem m_particleSystem;
    [SerializeField]
    private Texture2D m_correctFrame;

    private bool m_isCorrect;
    LetterPictureGamePlayer m_player; 

    public void SetUp(bool isCorrect, string text, LetterPictureGamePlayer player, Transform offLocation)
    {
        m_player = player;
        m_isCorrect = isCorrect;
        m_label.text = text;

        m_particleSystem.enableEmission = false;

        TweenOnOffBehaviour tob = GetComponent<TweenOnOffBehaviour>();
        tob.SetUp(offLocation, transform);
    }

    void OnClick()
    {
        m_player.WordClicked(m_isCorrect, this);
        collider.enabled = false;
        if (!m_isCorrect)
        {
            iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.3f);
            TweenColor.Begin(m_backgroundTexture.gameObject, 0.5f, new Color(0.75f, 0.75f, 0.75f));
        }
        else
        {
            m_backgroundTexture.mainTexture = m_correctFrame;
            m_particleSystem.enableEmission = true;
        }
    }

    public void RemoveCorrect(Transform offCorrect)
    {
        iTween.Stop(gameObject);
        TweenOnOffBehaviour tob = GetComponent<TweenOnOffBehaviour>();
        tob.SetUp(offCorrect, transform);
        tob.SetOffDelay(0.5f);
        tob.Off();
    }

    public void Remove()
    {
        TweenOnOffBehaviour tob = GetComponent<TweenOnOffBehaviour>();
        tob.Off();
    }

}
