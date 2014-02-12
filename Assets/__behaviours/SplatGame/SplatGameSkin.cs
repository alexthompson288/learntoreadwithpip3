using UnityEngine;
using System.Collections;

public class SplatGameSkin : MonoBehaviour {

    [SerializeField]
    public UIAtlas m_spriteAtlas;
    [SerializeField]
    public string[] m_spriteNames;
    [SerializeField]
    public Texture2D m_backgroundTexture;
    [SerializeField]
    public float m_scale = 1.5f;
    [SerializeField]
    public string m_ambienceAudioEvent;
}
