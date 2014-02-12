using UnityEngine;
using System.Collections;

public class LifeSprite : MonoBehaviour {


    [SerializeField]
    private UISprite m_sprite;

    public void SetSprite(string sprite)
    {
        m_sprite.spriteName = sprite;
    }
}
