using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CollectionRoomCoordinator : Singleton<CollectionRoomCoordinator> 
{
    /*
    [SerializeField]
    UITexture m_backgroundTexture;
    [SerializeField]
    private Transform m_poppedSpriteHierarchy;
    [SerializeField]
    private GameObject m_poppedSpritePrefab;
    [SerializeField]
    private Transform m_popCutoff;
    [SerializeField]
    private Transform[] m_texturesWebCamScale;
    [SerializeField]
    private Transform m_webCamTextureTransform;
    [SerializeField]
    private string[] m_textureNames;

    private List<GameObject> m_poppedSprites = new List<GameObject>();
    int m_currentTexture = 0;

	// Use this for initialization
	IEnumerator Start () 
    {
		SetBackground(0);
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        
		
		yield return new WaitForSeconds(0.5f);
		if(SessionInformation.Instance.GetHasNewItem())
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("NEW_STUFF");
			SessionInformation.Instance.SetHasNewItem(false);
			yield return new WaitForSeconds(1.5f);
		}
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("COLLECTION_ROOM_1");
        yield return new WaitForSeconds(3.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("COLLECTION_ROOM_2");
        yield return new WaitForSeconds(7.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("COLLECTION_ROOM_3");
        yield return new WaitForSeconds(7.0f);
	}

    public void SetBackground(int change)
    {        
        m_currentTexture += change;
        if (change != 0)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("COLLECTION_ROOM_IMAGE_" + m_currentTexture);
        }
        if (m_currentTexture == m_textureNames.Length)
        {
            m_currentTexture = 0;
        }
        if (m_currentTexture < 0)
        {
            m_currentTexture = m_textureNames.Length - 1;
        }
        m_backgroundTexture.mainTexture = (Texture2D)Resources.Load("Images/collection_room/" + m_textureNames[m_currentTexture]);
        Resources.UnloadUnusedAssets();
        m_webCamTextureTransform.parent = m_texturesWebCamScale[m_currentTexture];
        m_webCamTextureTransform.localScale = new Vector3(1, -1, 1);
        m_webCamTextureTransform.localPosition = Vector3.zero;
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().Play();
        }
    }

    public void ClearAll()
    {
        foreach (GameObject poppedSprite in m_poppedSprites)
        {
            Destroy(poppedSprite);
        }
        m_poppedSprites.Clear();
    }

    void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }

    public GameObject AddPoppedSprite(string textureName, Texture2D texture, Vector3 position)
    {
        GameObject newPop = 
            SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_poppedSpritePrefab, m_poppedSpriteHierarchy);

        newPop.transform.position = position;

        newPop.GetComponentInChildren<UITexture>().mainTexture = texture;

        m_poppedSprites.Add(newPop);

        newPop.GetComponentInChildren<PlacedDraggable>().SetUp(m_popCutoff);

        return newPop;
    }

    public void DestroyPoppedSprite(GameObject poppedSprite)
    {
        m_poppedSprites.Remove(poppedSprite);
        Destroy(poppedSprite);
    }
    */
}
