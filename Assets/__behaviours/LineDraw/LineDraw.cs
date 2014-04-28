using UnityEngine;
using System.Collections;

public class LineDraw : MonoBehaviour 
{
    public delegate void LineDragEvent(LineDraw line, Vector2 delta);
    public event LineDragEvent LineDragEventHandler;

    UICamera.MouseOrTouch m_input;
    public UICamera.MouseOrTouch input
    {
        get
        {
            return m_input;
        }
    }

    Camera m_camera;
    public Camera camera
    {
        get
        {
            return m_camera;
        }
    }

    protected virtual void OnPress(bool pressed)
    {
        m_input = pressed ? UICamera.currentTouch : null;
        m_camera = pressed ? UICamera.currentCamera : null;

        if (pressed)
        {
            LineDrawManager.Instance.CreateLine(this);
        } 
    }

    void OnDrag(Vector2 delta)
    {
        if (LineDragEventHandler != null)
        {
            LineDragEventHandler(this, delta);
        }
    }
}
