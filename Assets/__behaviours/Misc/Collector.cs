using UnityEngine;
using System.Collections;

public class Collector : MonoBehaviour
{
    [SerializeField]
    protected SpriteAnim m_anim;
    [SerializeField]
    protected Transform m_collectionLocation;
    [SerializeField]
    protected float m_moveSpeed = 1;

    protected Vector3 m_targetPos;
    protected bool m_isMoving = false;

    public virtual void StartAnim() {}
    public virtual void MoveToPos(Vector3 pos) {}
    public virtual IEnumerator Collect(GameObject collectable) { yield return null; }

    public void SetMoveSpeed(float myMoveSpeed)
    {
        m_moveSpeed = myMoveSpeed;
    }

    public void StopAnim()
    {
        m_anim.Stop();
    }
}
