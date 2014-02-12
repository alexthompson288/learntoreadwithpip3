using UnityEngine;
using System.Collections;

public class TurnSwipeDetect : MonoBehaviour {

    double m_dragStartTime;
    Vector2 m_totalDrag;

    void OnPress(bool press)
    {
        if (press)
        {
            m_dragStartTime = AudioSettings.dspTime;
            m_totalDrag = Vector2.zero;
        }
        else
        {
            double dragDuration = AudioSettings.dspTime - m_dragStartTime;
            if (dragDuration < 2.0f && dragDuration > 0.05f)
            {
                if (Mathf.Abs(Vector2.Dot(m_totalDrag, Vector2.right)) > 0.3f)
                {
                    if (m_totalDrag.x > 32)
                    {
                        StoryReaderLogic.Instance.TurnPage(true);
                    }
                    else if (m_totalDrag.x < -32)
                    {
                        StoryReaderLogic.Instance.TurnPage(false);
                    }
                }
            }
        }
    }

    void OnDrag(Vector2 delta)
    {
        m_totalDrag += delta;
    }
}
