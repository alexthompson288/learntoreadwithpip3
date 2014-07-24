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
		////D.Log("StoryPageDisplayer.Show()");

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
                ////D.Log(scale);
                m_scaleTransform.localScale = new Vector3(scale.x, scale.y, 1);
                m_scaleTransform.localPosition = new Vector3((1 - scale.x) * 512, (1 - scale.y) * -384);
            }
        }
        else
        {
            m_scaleTransform.localScale = Vector3.one;
        }
        m_pageBG.material.mainTexture = bg;
		////D.Log("m_pageBG.material.mainTexture: " + m_pageBG.material.mainTexture);

		////D.Log("Finished setting images");
    }

    public void Turn(Transform other)
    {
        StartCoroutine(TurnPage(other));
    }

    public void TurnBack(Transform other)
    {
        StartCoroutine(TurnPageBack(other));
    }

	public IEnumerator TurnPageBack(Transform other)
	{
		other.localPosition = new Vector3(-520, 0, 0.2f);
		transform.localPosition = new Vector3(-520, 0, 0.0f);
		transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));

		Hashtable tweenVar = new Hashtable();
		tweenVar.Add("rotation", Vector3.one);
		tweenVar.Add("time", StoryReaderLogic.Instance.GetPageTurnDuration());
		tweenVar.Add("easetype", iTween.EaseType.linear);

		iTween.RotateTo(gameObject, tweenVar);

		//iTween.RotateTo(gameObject, new Vector3(0, 0, 0), StoryReaderLogic.Instance.GetPageTurnDuration());

		yield return new WaitForSeconds(StoryReaderLogic.Instance.GetPageTurnDuration());
	}
	
	public IEnumerator TurnPage(Transform other)
	{
		Hashtable tweenVar = new Hashtable();
		tweenVar.Add("rotation", new Vector3(0, 90, 0));
		tweenVar.Add("time", StoryReaderLogic.Instance.GetPageTurnDuration());
		tweenVar.Add("easetype", iTween.EaseType.linear);
		
		iTween.RotateTo(gameObject, tweenVar);

		//iTween.RotateTo(gameObject, new Vector3(0, 90, 0), StoryReaderLogic.Instance.GetPageTurnDuration());
		
		yield return new WaitForSeconds(StoryReaderLogic.Instance.GetPageTurnDuration() + 0.5f);
		
		transform.localPosition = new Vector3(-520, 0, 0.2f);
		transform.rotation = Quaternion.identity;
		
		other.localPosition = new Vector3(-520, 0, 0);
	}

	/*
    public IEnumerator TurnPageBack(Transform other)
    {
		////D.Log("StoryPageDisplay.TurnBackCo()");
        other.localPosition = new Vector3(-520, 0, 0.2f);
        transform.localPosition = new Vector3(-520, 0, 0.0f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        iTween.RotateTo(gameObject, new Vector3(0, 0, 0), 3.0f);

        //yield return new WaitForSeconds(3.5f);
		yield return new WaitForSeconds(3.0f);
    }

    public IEnumerator TurnPage(Transform other)
    {
		////D.Log("StoryPageDisplay.TurnCo()");
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
    */
}
