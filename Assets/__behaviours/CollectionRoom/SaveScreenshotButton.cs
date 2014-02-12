using UnityEngine;
using System.Collections;

public class SaveScreenshotButton : MonoBehaviour {

    [SerializeField]
    UICamera m_interactionCamera;
    [SerializeField]
    GameObject m_screenshotHierarchy;
    [SerializeField]
    UITexture m_cameraFlash;


    void OnClick()
    {
        StartCoroutine(TakeScreenshot());
    }

    IEnumerator TakeScreenshot()
    {
        m_interactionCamera.enabled = false;

        m_screenshotHierarchy.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("CAMERA_CLICK");
        m_cameraFlash.gameObject.SetActive(true);

        m_cameraFlash.color = Color.white;
        TweenColor tc = TweenColor.Begin(m_cameraFlash.gameObject, 0.45f, new Color(1.0f, 1.0f, 1.0f, 0.0f));

        yield return new WaitForSeconds(0.5f);

        m_cameraFlash.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(ScreenshotManager.Save("Collection Room", "Learn To Read With Pip"));

        yield return new WaitForSeconds(0.5f);

        m_screenshotHierarchy.SetActive(false);

        m_interactionCamera.enabled = true;
    }
}
