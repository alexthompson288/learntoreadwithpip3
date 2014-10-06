using UnityEngine;
using System.Collections;

public class Countable : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_sprite;

    bool m_isSelected = false;
    public bool isSelected
    {
        get
        {
            return m_isSelected;
        }
    }

    void Awake()
    {
        iTween.ScaleFrom(gameObject, Vector3.zero, 0.25f);
    }

    public void SetUp(string spriteName)
    {
        m_sprite.spriteName = spriteName;
        m_sprite.color = Color.gray;
    }

    void OnClick()
    {
        m_isSelected = !m_isSelected;
        m_sprite.color = m_isSelected ? Color.white : Color.gray;
    }

    public void Off()
    {
        iTween.Stop(gameObject);
        StartCoroutine(OffCo());
    }

    IEnumerator OffCo()
    {
        float tweenDuration = 0.25f;
        iTween.ScaleTo(gameObject, Vector3.zero, tweenDuration);
        yield return new WaitForSeconds(tweenDuration);
        yield return null;
        Destroy(gameObject);
    }
}
