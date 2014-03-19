using UnityEngine;
using System.Collections;
using System;

public class TimeHelpers : MonoBehaviour 
{
	public static string BuildDateTimeString(DateTime time)
	{
		string dts = System.String.Format("start: {0:d} - {1:g}", time.Date, time.TimeOfDay);
		int lastDecimalIndex = dts.LastIndexOf (".");
		return dts.Substring (0, lastDecimalIndex);
	}
}
