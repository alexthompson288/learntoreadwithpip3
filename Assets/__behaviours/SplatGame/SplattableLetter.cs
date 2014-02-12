using UnityEngine;
using System.Collections;

public class SplattableLetter : MonoBehaviour {

    [SerializeField]
    private UILabel m_letterLabel;
    [SerializeField]
    private float m_maxSpeed;
    [SerializeField]
    private UISprite m_sprite;

    string m_letter;
    Vector3 m_moveDirection;

    string m_onSpriteName;
    string m_offSpriteName;

    Transform m_min;
    Transform m_max;

    public void SetUp(string letter, Transform min, Transform max, SplatGameSkin gameSkin)
    {
        m_min = min;
        m_max = max;
        m_letter = letter;
        m_letterLabel.text = m_letter;
        m_moveDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0).normalized;        

        m_sprite.atlas = gameSkin.m_spriteAtlas;

        int randSprite = Random.Range(0, gameSkin.m_spriteNames.Length/2);
        m_onSpriteName = gameSkin.m_spriteNames[randSprite*2];
        m_offSpriteName = gameSkin.m_spriteNames[(randSprite*2)+1];

        m_sprite.spriteName = m_onSpriteName;
        m_sprite.transform.localScale = gameSkin.m_scale * Vector3.one;

        Vector2 colliderSize = m_sprite.localSize * gameSkin.m_scale;
        ((SphereCollider)collider).radius = colliderSize.magnitude * 0.25f;

        transform.position = new Vector3(Random.Range(m_min.position.x, m_max.position.x),
            Random.Range(m_min.position.y, m_max.position.y), 0);

        iTween.ScaleFrom(gameObject, Vector3.zero, 1.0f);
    }

    void OnClick()
    {
        SplatGameCoordinator.Instance.LetterClicked(m_letter, gameObject);
        StartCoroutine(HitEffect());
    }

    public void SplatDestroy()
    {
        StopAllCoroutines();
        StartCoroutine(Splat());
    }


    IEnumerator HitEffect()
    {
        m_sprite.spriteName = m_offSpriteName;
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.3f);
        yield return new WaitForSeconds(1.0f);
        m_sprite.spriteName = m_onSpriteName;
    }

    IEnumerator Splat()
    {                
        collider.enabled = false;
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    void Update()
    {
        if (rigidbody.velocity.magnitude < m_maxSpeed)
        {
            m_moveDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0).normalized;        
            rigidbody.AddForce(m_moveDirection * Time.deltaTime * m_maxSpeed);
        }
    }
}
