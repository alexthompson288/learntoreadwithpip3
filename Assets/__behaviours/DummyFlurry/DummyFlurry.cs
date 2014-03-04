using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlurryBinding
{
	// Starts up your Flurry analytics session.  Call on application startup.
	public static void startSession( string apiKey ) {}
		
	// Logs an event to Flurry.  If isTimed is true, this will be a timed event and it should be paired with a call to endTimedEvent
	public static void logEvent( string eventName, bool isTimed ) {}
			
	// Logs an event with optional key/value pairs
	public static void logEventWithParameters( string eventName, Dictionary<string,string> parameters, bool isTimed ) {}
			
	// Ends a timed event that was previously started
	public static void endTimedEvent( string eventName ) {}
			
	public static void endTimedEvent( string eventName, Dictionary<string,string> parameters ) {}

	// Sets the users age
	public static void setAge( int age ) {}
		
	// Sets the users gender
	public static void setGender( string gender ) {}
			
	// Sets the users unique id
	public static void setUserId( string userId ) {}
			
	// Sets whether Flurry should upload session reports when your app closes
	public static void setSessionReportsOnCloseEnabled( bool sendSessionReportsOnClose ) {}
			
	// Sets whether Flurry should upload session reports when your app is paused
	public static void setSessionReportsOnPauseEnabled( bool setSessionReportsOnPauseEnabled ) {}
}
