using UnityEngine;
using System.Collections;

public class ScreenSetUp : MonoBehaviour {

    [SerializeField]
    private float m_zRot1Player = 0;
    [SerializeField]
    private float m_zRot2Player = 90;
    [SerializeField]
    private float m_scale1Player = 1;
    [SerializeField]
    private float m_scale2Player = 0.75f;
    [SerializeField]
    private Transform m_rotateScaleTransform;
    [SerializeField]
    private Rect m_rect2;
    [SerializeField]
    private Rect m_rect1;
    [SerializeField]
    private Camera[] m_cameras;
    [SerializeField]
    private bool m_enabledOnePlayer;
    [SerializeField]
    private bool m_adjustFor43;

	// Use this for initialization
	void Awake() 
    {
		bool twoPlayer;
		
		try
		{
        	twoPlayer = (SessionInformation.Instance.GetNumPlayers() == 2);
		}
		catch
		{
			twoPlayer = false;
		}

        //Debug.Log("ScreenSetUp - twoPlayer: " + twoPlayer);
		
        if (twoPlayer)
        {
            m_rotateScaleTransform.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, m_zRot2Player));
            m_rotateScaleTransform.transform.localScale = Vector3.one * m_scale2Player;

            Rect adjust = m_rect2;
            if (m_adjustFor43)
            {
                adjust.width = adjust.width * ((4.0f / 3.0f) / m_cameras[0].aspect);
                Debug.Log(adjust.width);
                if (adjust.x == 0)
                {
                    adjust.x += m_rect2.width - adjust.width;
                }
            }

            foreach (Camera c in m_cameras)
            {
                c.rect = adjust;
            }
        }
        else
        {
            if (!m_enabledOnePlayer)
            {
                foreach (Camera c in m_cameras)
                {
                    c.gameObject.SetActive(false);
                }
            }
            m_rotateScaleTransform.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, m_zRot1Player));
            m_rotateScaleTransform.transform.localScale = Vector3.one * m_scale1Player;

            foreach (Camera c in m_cameras)
            {
                c.rect = m_rect1;
            }
        }
	}
}
