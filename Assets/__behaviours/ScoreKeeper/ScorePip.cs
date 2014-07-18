﻿using UnityEngine;
using System.Collections;

public class ScorePip : ScoreKeeper 
{
    [SerializeField]
    private Transform m_bottom;
    [SerializeField]
    private Transform m_top;
    [SerializeField]
    private Transform m_pipParent;
    [SerializeField]
    private float m_platformTweenDuration = 0.3f;
    [SerializeField]
    private iTween.EaseType m_platformEaseType = iTween.EaseType.easeOutQuad;
    [SerializeField]
    private float m_collectableTweenSpeed = 2f;
    [SerializeField]
    private iTween.EaseType m_collectableEaseType = iTween.EaseType.easeOutQuad;
    [SerializeField]
    private Transform m_collectionPoint;
    [SerializeField]
    private SpriteAnim m_pipAnim;
    [SerializeField]
    private AnimManager m_popAnimManager;
    [SerializeField]
    private SplineFollower m_popSplineFollower;
    [SerializeField]
    private GameObject m_branchPrefab;
    
    float m_pointDistance;
    
    public override void SetTargetScore(int targetScore)
    {
        base.SetTargetScore(targetScore);
        
        float delta = Mathf.Abs((m_top.localPosition - m_bottom.localPosition).magnitude);
        m_pointDistance = Mathf.Lerp(0, delta, 1f / (float)m_targetScore);

        int mirrorIndex = 0;
        for (int heightIndex = targetScore - 1; heightIndex > 0; --heightIndex, ++mirrorIndex)
        {
            GameObject newBranch = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_branchPrefab, transform);
            newBranch.GetComponent<UISprite>().depth = 0;

            int localScaleX = mirrorIndex % 2 == 0 ? -1 : 1;
            newBranch.transform.localScale = new Vector3(localScaleX, 1, 1);

            newBranch.transform.localPosition = new Vector3(newBranch.transform.localPosition.x, m_bottom.localPosition.y + (m_pointDistance * heightIndex), m_pipParent.localPosition.z);
        }
    }
    
    public override void UpdateScore(int delta = 1)
    {
        base.UpdateScore(delta);
        UpdatePlatformLevel(delta); 
    }
    
    public override IEnumerator UpdateScore(GameObject targetGo, int delta = 1)
    {
        targetGo.transform.parent = m_collectionPoint;
        targetGo.layer = m_collectionPoint.gameObject.layer;
        
        base.UpdateScore(delta);
        
        iTween.Stop(targetGo);
        
        yield return null;
        
        float cauldronTweenDuration = Mathf.Abs(((m_collectionPoint.transform.position - targetGo.transform.position).magnitude) / m_collectableTweenSpeed);
        
        Hashtable scaleTweenArgs = new Hashtable();
        scaleTweenArgs.Add("scale", Vector3.one * 0.2f);
        scaleTweenArgs.Add("time", cauldronTweenDuration);
        scaleTweenArgs.Add("easetype", iTween.EaseType.linear);
        iTween.ScaleTo(targetGo, scaleTweenArgs);
        
        Hashtable positionTweenArgs = new Hashtable();
        positionTweenArgs.Add("position", m_collectionPoint.position);
        positionTweenArgs.Add("speed", m_collectableTweenSpeed);
        positionTweenArgs.Add("easetype", m_collectableEaseType);
        iTween.MoveTo(targetGo, positionTweenArgs);
        
        yield return new WaitForSeconds(cauldronTweenDuration);
        
        Destroy(targetGo);
        
        UpdatePlatformLevel(delta);
    }
    
    void UpdatePlatformLevel(int delta)
    {
        if (delta != 0)
        {
            Hashtable tweenArgs = new Hashtable();
            tweenArgs.Add("time", m_platformTweenDuration);
            tweenArgs.Add("easetype", m_platformEaseType);
            tweenArgs.Add("position", new Vector3(m_pipParent.localPosition.x, m_bottom.localPosition.y + (m_pointDistance * m_score), m_pipParent.localPosition.z));
            tweenArgs.Add("islocal", true);
            
            iTween.MoveTo(m_pipParent.gameObject, tweenArgs);
            
            //m_pipAnim.StopRandom();
            m_pipAnim.AnimFinished += OnScoreAnimFinish;
        }

        string animName = delta > 0 ? "THUMBS_UP" : "SAD";
        m_pipAnim.PlayAnimation(animName);

        PlayAudio(delta);
    }
    
    bool m_hasFinishedJumpAnim = false;

    public override IEnumerator On()
    {
        m_popAnimManager.StopRandom();
        
        m_popSplineFollower.ChangePath("WIN");
        
        //m_pipAnim.StopRandom();
        
        m_pipAnim.AnimFinished -= OnScoreAnimFinish;
        m_pipAnim.AnimFinished += OnJumpAnimFinish;
        
        m_pipAnim.PlayAnimation("JUMP");

        yield return new WaitForSeconds(0.22f);

        //WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YIPPIDYPOP");
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");
        
        while (!m_hasFinishedJumpAnim)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.3f);
        
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("speed", 400);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        tweenArgs.Add("position", new Vector3(m_pipAnim.transform.localPosition.x + 500, m_pipAnim.transform.localPosition.y));
        tweenArgs.Add("islocal", true);
        
        iTween.MoveTo(m_pipAnim.gameObject, tweenArgs);  
        
        m_pipAnim.PlayAnimation("WALK");
        
        yield return new WaitForSeconds(1.5f);
    }
    
    void OnScoreAnimFinish(SpriteAnim anim, string animName)
    {
        m_pipAnim.AnimFinished -= OnScoreAnimFinish;
    }
    
    void OnJumpAnimFinish(SpriteAnim anim, string animName)
    {
        m_pipAnim.AnimFinished -= OnJumpAnimFinish;
        m_hasFinishedJumpAnim = true;
    }

    public override void PlayIncorrectAnimation()
    {
        m_pipAnim.PlayAnimation("SAD", false);
    }
   
#if UNITY_EDITOR
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(On());
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            UpdateScore(-1);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateScore(1);
        }
    }
#endif
    
    public override bool HasCompleted()
    {
        return m_score >= m_targetScore;
    }
}