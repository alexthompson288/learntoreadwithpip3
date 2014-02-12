using UnityEngine;
using System.Collections;

public class DraggableWord : MonoBehaviour {

    Vector3 m_dragOffset;


    void Start()
    {
        transform.localScale = Vector3.one;
        iTween.ScaleFrom(gameObject, Vector3.zero, 0.5f);
    }

    void OnPress(bool press)
    {
        if (press)
        {
            Ray camPos = UICamera.currentCamera.ScreenPointToRay(
    new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
            m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
        }
        else
        {
            SillySensibleCoordinator.Instance.WordDropped();
        }
    }

    void OnDrag(Vector2 dragAmount)
    {
        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
        transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;

        m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
    }

    IEnumerator DestroyCoroutine()
    {
        collider.enabled = false;
        iTween.Stop(gameObject);
        iTween.ScaleTo(gameObject, Vector3.zero, 0.5f);

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    public void Off()
    {
        StartCoroutine(DestroyCoroutine());
    }
}
