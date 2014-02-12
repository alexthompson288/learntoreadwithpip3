using System;

public class UnixDate
{
	public static double Now { get { return UnixDate.FromSystemDate(DateTime.Now); }}
	public static double FromSystemDate(DateTime sysDate) { return (sysDate - UnixDate.EpochSystemDate).TotalSeconds; }
	public static DateTime ToSystemDate(double unixDate) { return UnixDate.EpochSystemDate.AddSeconds(unixDate); }
	
	private static DateTime EpochSystemDate = new DateTime(1970,1,1);
}