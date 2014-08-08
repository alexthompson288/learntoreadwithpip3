using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathHelpers : MonoBehaviour 
{
    static HashSet<int> m_timesTableNumbers = new HashSet<int>();

    public static bool IsTimesTableNum(int number)
    {
        if (m_timesTableNumbers.Count == 0)
        {
            for(int i = 0; i < 13; ++i)
            {
                for(int j = 0; j < 13; ++j)
                {
                    //D.Log(System.String.Format("{0} * {1} = {2}", i, j, i * j));
                    m_timesTableNumbers.Add(i * j);
                }
            }
        }

        return m_timesTableNumbers.Contains(number);
    }

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

    public static int GetDigitCount(int i)
    {
        return i.ToString().Replace("-", "").Length;
    }
}
