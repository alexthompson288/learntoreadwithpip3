using UnityEngine;
using System.Collections;

public class ShowPipPadForWord : MonoBehaviour {

	[SerializeField]
	private UISprite m_sprite;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private Color m_decodeableColor;
	[SerializeField]
	private Color m_nonDecodeableColor;

    private string m_word;
	private string m_editedWord;
	private bool m_showPad;

    public void SetUp(string word, Vector3 wordSize, bool showPad = true)
    {
        m_word = word;

        BoxCollider bc = (BoxCollider)collider;
        bc.size = wordSize;
        bc.center = new Vector3(wordSize.x / 2, bc.center.y, bc.center.z);

		m_editedWord = word.Replace(".", "").Replace(",", "").Replace(" ", "").Replace("?", "");

        if (m_sprite != null)
        {
            m_sprite.width = (int)wordSize.x;
            m_sprite.height = (int)wordSize.y;
        }

		m_showPad = showPad;
    }

	public void Highlight(bool decodeable)
	{
        if (m_sprite != null)
        {
            m_sprite.enabled = true;

            if (decodeable)
            {
                m_sprite.color = m_decodeableColor;
            } else
            {
                m_sprite.color = m_nonDecodeableColor;
            }
        }
	}

    void OnClick()
    {
		if(m_showPad)
		{
        	PipPadBehaviour.Instance.Show(m_word.ToLower(), true);
		}
		else if(m_audioSource != null)
		{
			m_audioSource.clip = LoaderHelpers.LoadAudioForWord(m_editedWord);
			m_audioSource.Play();
		}
    }
}
