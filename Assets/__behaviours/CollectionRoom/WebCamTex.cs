using UnityEngine;
using System.Collections;

public class WebCamTex : MonoBehaviour
{

    WebCamTexture m_wct;

    // Use this for initialization
    void Start()
    {
        if (SessionInformation.Instance.GetWebCamTexture() == null)
        {
            string frontCamName = null;
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                foreach (WebCamDevice wcd in devices)
                {
                    if (wcd.isFrontFacing)
                    {
                        frontCamName = wcd.name;
                    }
                }

                if (frontCamName == null)
                {
                    frontCamName = devices[0].name;
                }

                if (frontCamName != null)
                {
                    Debug.Log("Using device " + frontCamName);
                    WebCamTexture wct = new WebCamTexture(frontCamName, 1024, 768, 30);
                    SessionInformation.Instance.SetWebCamTexture(wct);

                }
            }
        }
        m_wct = SessionInformation.Instance.GetWebCamTexture();
        GetComponent<UITexture>().mainTexture = m_wct;
        if ((m_wct != null) && (!m_wct.isPlaying))
        {
            m_wct.Play();
        }
    }

    void OnDestroy()
    {
        if (m_wct != null)
        {
            m_wct.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<UITexture>().mainTexture = m_wct;
    }
}
