using UnityEngine;

public class GarbageCollectManager : MonoBehaviour 
{
    [SerializeField]
    private int frameFreq = 30;  

    void Update()   
    {  
        if (Time.frameCount % frameFreq == 0)
        {
            System.GC.Collect();  
        }
    }  
} 