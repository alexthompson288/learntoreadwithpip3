using UnityEngine;
using System.Collections;

public class StoryPageDisplayer : MonoBehaviour 
{
    [SerializeField]
    private Renderer m_pageFG;
    [SerializeField]
    private Renderer m_pageBG;
    [SerializeField]
    private Transform m_scaleTransform;
	
    public void Show(Texture2D fg, Texture2D bg)
    {
		//Debug.Log("StoryPageDisplayer.Show()");

        m_pageFG.material.mainTexture = fg;
        if (fg != null)
        {
            fg.wrapMode = TextureWrapMode.Clamp;
        }

		m_pageFG.enabled = (fg != null);

        //if (bg == null)
        //{
        //    bg = (Texture2D)Resources.Load("Images/storypages/story_bg_3");
        //}
        if ( bg != null )
        {
            if (fg != null)
            {
                Vector2 scale = new UnityEngine.Vector2((float)fg.width / 2048.0f,
                    (float)fg.height / 1536.0f);
                //Debug.Log(scale);
                m_scaleTransform.localScale = new Vector3(scale.x, scale.y, 1);
                m_scaleTransform.localPosition = new Vector3((1 - scale.x) * 512, (1 - scale.y) * -384);
            }
        }
        else
        {
            m_scaleTransform.localScale = Vector3.one;
        }
        m_pageBG.material.mainTexture = bg;
		//Debug.Log("m_pageBG.material.mainTexture: " + m_pageBG.material.mainTexture);

		//Debug.Log("Finished setting images");
    }

    public void Turn(Transform other)
    {
		Debug.Log("StoryPageDisplay.Turn()");
        StartCoroutine(TurnPage(other));
    }

    public void TurnBack(Transform other)
    {
		Debug.Log("StoryPageDisplay.TurnBack()");
        StartCoroutine(TurnPageBack(other));
    }

    public IEnumerator TurnPageBack(Transform other)
    {
		Debug.Log("StoryPageDisplay.TurnBackCo()");
        other.localPosition = new Vector3(-520, 0, 0.2f);
        transform.localPosition = new Vector3(-520, 0, 0.0f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        iTween.RotateTo(gameObject, new Vector3(0, 0, 0), 3.0f);

        //yield return new WaitForSeconds(3.5f);
		yield return new WaitForSeconds(3.0f);
    }

    public IEnumerator TurnPage(Transform other)
    {
		Debug.Log("StoryPageDisplay.TurnCo()");
        //Animation[] anims = GetComponentsInChildren<Animation>();
        //foreach (Animation anim in anims)
        //{
        //    anim.Play();
        //    foreach (AnimationState ast in anim)
        //    {
        //        ast.wrapMode = WrapMode.ClampForever;
        //    }
        //}
        //yield return new WaitForSeconds(1.0f);

        iTween.RotateTo(gameObject, new Vector3(0, 90, 0), 3.0f);

        yield return new WaitForSeconds(3.5f);

        transform.localPosition = new Vector3(-520, 0, 0.2f);
        transform.rotation = Quaternion.identity;
        //foreach (Animation anim in anims)
        //{
        //    foreach (AnimationState ast in anim)
        //    {
        //        ast.time = 0.0f;
        //    }
        //    anim.Sample();
        //    anim.Stop();
        //}

        other.localPosition = new Vector3(-520, 0, 0);

		//yield return new WaitForSeconds(3.0f);
    }
}
