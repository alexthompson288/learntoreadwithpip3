using UnityEngine;
using System.Collections;

public class WrapDragPanel : MonoBehaviour
{
    [SerializeField]
    private UIDraggablePanel m_draggablePanel;

    UIDragPanelContents[] m_dragPanelContents;

    // Update is called once per frame
    void Update()
    {
        m_dragPanelContents = GetComponentsInChildren<UIDragPanelContents>();
        UIGrid grid = GetComponent<UIGrid>();

        for (int count = 0; count < 10; ++count)
        {
            UIDragPanelContents mostRight = null;
            UIDragPanelContents mostLeft = null;
            foreach (UIDragPanelContents dpc in m_dragPanelContents)
            {
                if ((mostRight == null) || (dpc.transform.position.x > mostRight.transform.position.x))
                {
                    mostRight = dpc;
                }
                if ((mostLeft == null) || (dpc.transform.position.x < mostLeft.transform.position.x))
                {
                    mostLeft = dpc;
                }
            }

            if ((mostRight != null) && (mostLeft != null))
            {
                if (mostRight.transform.position.x > -mostLeft.transform.position.x)
                {
                    mostRight.transform.localPosition =
                        mostLeft.transform.localPosition - new Vector3(grid.cellWidth, 0, 0);
                }
                else
                {
                    mostLeft.transform.localPosition =
                        mostRight.transform.localPosition + new Vector3(grid.cellWidth, 0, 0);
                }
            }
        }
    }
}
