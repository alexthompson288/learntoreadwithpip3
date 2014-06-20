using UnityEngine;
using System.Collections;

public class Collector : MonoBehaviour
{
    [SerializeField]
    protected SpriteAnim m_anim;
    [SerializeField]
    private Transform m_collectionLocation;

    public virtual IEnumerator Collect(GameObject collectable) { yield return null; }
    public virtual IEnumerator MoveToPos(Vector3 pos) { yield return null; }
}
