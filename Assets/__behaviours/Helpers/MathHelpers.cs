using UnityEngine;
using System.Collections;

public class MathHelpers : MonoBehaviour 
{
    public static bool IsPrime(int number)
    {
        int boundary = Mathf.FloorToInt(Mathf.Sqrt(number));
        
        if (number == 1)
        {
            return false;
        } 
        else if (number == 2)
        {
            return true;
        }
        
        for (int i = 2; i <= boundary; ++i)  
        {
            if (number % i == 0)  
            {
                return false;
            }
        }
        
        return true;        
    }
}
