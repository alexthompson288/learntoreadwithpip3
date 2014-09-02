using UnityEngine;
using System.Collections;
using Wingrove;
using System.Collections.Generic;

public class CreateStickerManager : Singleton<CreateStickerManager>
{
    [SerializeField]
    private GameObject m_poppedSpritePrefab;
    [SerializeField]
    private Transform m_poppedSpriteParent;
    [SerializeField]
    private Transform m_popCutoff;

    List<GameObject> m_poppedSprites = new List<GameObject>();

    public GameObject AddPoppedSprite(string textureName, Texture2D texture, Vector3 position)
    {
        GameObject newPop = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_poppedSpritePrefab, m_poppedSpriteParent);
        
        newPop.transform.position = position;
        
        newPop.GetComponentInChildren<UITexture>().mainTexture = texture;
        
        m_poppedSprites.Add(newPop);
        
        newPop.GetComponentInChildren<PlacedDraggable>().SetUp(m_popCutoff);
        newPop.GetComponentInChildren<PlacedDraggable>().MovedBelowCutoff += DestroyPoppedSprite;
        
        return newPop;
    }

    public void DestroyPoppedSprite(GameObject poppedSprite)
    {
        m_poppedSprites.Remove(poppedSprite);
        Destroy(poppedSprite);
    }

    public void ClearAll()
    {
        foreach (GameObject poppedSprite in m_poppedSprites)
        {
            Destroy(poppedSprite);
        }
        m_poppedSprites.Clear();
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().Play();
        }
    }

    public Transform GetPopCutoff()
    {
        return m_popCutoff;
    }
}
