﻿using UnityEngine;
using System.Collections;

public class PlacedDraggable : MonoBehaviour {

    private Transform m_popCutoff;

    public void SetUp(Transform popCutoff)
    {
        m_popCutoff = popCutoff;
    }

    void OnDrag(Vector2 drag)
    {
        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
        transform.position = 
            new Vector3(camPos.origin.x,camPos.origin.y,0);
    }

    void OnPress(bool press)
    {
        if (!press)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("DROP_STICKER");
            if (transform.position.y < m_popCutoff.position.y)
            {
                CollectionRoomCoordinator.Instance.DestroyPoppedSprite(gameObject);
            }
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("PICK_UP_STICKER");
        }
    }
}
