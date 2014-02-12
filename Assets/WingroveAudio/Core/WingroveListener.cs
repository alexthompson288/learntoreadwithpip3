using UnityEngine;
using System.Collections;
namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Wingrove Listeners")]
    public class WingroveListener : MonoBehaviour
    {

        // Use this for initialization
        void OnEnable()
        {
            if (WingroveRoot.Instance != null)
            {
                WingroveRoot.Instance.RegisterListener(this);
            }
        }

        void OnDisable()
        {
            if (WingroveRoot.Instance != null)
            {
                WingroveRoot.Instance.UnregisterListener(this);
            }
        }
    }

}