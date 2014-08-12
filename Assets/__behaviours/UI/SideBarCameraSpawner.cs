using UnityEngine;
using System.Collections;

public class SideBarCameraSpawner : Singleton<SideBarCameraSpawner> 
{
    [SerializeField]
    private GameObject m_sideBarCameraPrefab;

    public void InstantiateSideBarCamera()
    {
        GameObject sideBarCamera = (GameObject)GameObject.Instantiate(m_sideBarCameraPrefab, Vector3.zero, Quaternion.identity);
        sideBarCamera.transform.parent = transform;
        sideBarCamera.transform.localScale = Vector3.one;
    }
}
