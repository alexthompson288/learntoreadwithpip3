using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prime31;



#if UNITY_IPHONE
public class PlayGameServices
{
	#region Settings

	// Android only. Enables high detail logs
	public static void enableDebugLog( bool shouldEnable )
	{}


	[DllImport("__Internal")]
	private static extern void _gplaySetWelcomeBackToastSettings( int placement, int offset );

	// Sets the placement of the welcome back toast
	public static void setWelcomeBackToastSettings( GPGToastPlacement placement, int offset )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplaySetWelcomeBackToastSettings( (int)placement, offset );
	}


	[DllImport("__Internal")]
	private static extern void _gplaySetAchievementToastSettings( int placement, int offset );

	// Sets the placement of the achievement toast
	public static void setAchievementToastSettings( GPGToastPlacement placement, int offset )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplaySetAchievementToastSettings( (int)placement, offset );
	}

	#endregion


	#region Auth and Sharing

	[DllImport("__Internal")]
	private static extern void _gplayInit( string clientId, bool requestAppStateScope, bool fetchMetadataAfterAuthentication, bool pauseUnityWhileShowingFullScreenViews );

	// iOS only. This should be called at application launch. It will attempt to authenticate the user silently. If you need the AppState scope permission
	// (cloud storage requires it) pass true for the requestAppStateScope parameter
	public static void init( string clientId, bool requestAppStateScope, bool fetchMetadataAfterAuthentication = true, bool pauseUnityWhileShowingFullScreenViews = true )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayInit( clientId, requestAppStateScope, fetchMetadataAfterAuthentication, pauseUnityWhileShowingFullScreenViews );
	}


	// Android only. This will attempt to sign in the user with no UI.
	public static void attemptSilentAuthentication()
	{}


	[DllImport("__Internal")]
	private static extern void _gplayAuthenticate();

	// Starts the authentication process which will happen either in the Google+ app, Chrome or Mobile Safari
	public static void authenticate()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayAuthenticate();
	}


	[DllImport("__Internal")]
	private static extern void _gplaySignOut();

	// Logs the user out
	public static void signOut()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplaySignOut();
	}


	[DllImport("__Internal")]
	private static extern bool _gplayIsSignedIn();

	// Checks to see if there is a currently signed in user. Utilizes a terrible hack due to a bug with Play Game Services connection status.
	public static bool isSignedIn()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _gplayIsSignedIn();

		return false;
	}


	[DllImport("__Internal")]
	private static extern string _gplayGetLocalPlayerInfo();

	// Gets the logged in players details
	public static GPGPlayerInfo getLocalPlayerInfo()
	{
		var player = new GPGPlayerInfo();

		if( Application.platform == RuntimePlatform.IPhonePlayer )
			Json.decode<GPGPlayerInfo>( _gplayGetLocalPlayerInfo() );

		return player;
	}


	[DllImport("__Internal")]
	private static extern void _gplayReloadAchievementAndLeaderboardData();

	// Reloads all Play Game Services related metadata
	public static void reloadAchievementAndLeaderboardData()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayReloadAchievementAndLeaderboardData();
	}


	// Android only. Loads a profile image from a Uri. Once loaded the profileImageLoadedAtPathEvent will fire.
	public static void loadProfileImageForUri( string uri )
	{}


	[DllImport("__Internal")]
	private static extern void _gplayShowShareDialog( string prefillText, string urlToShare );

	// Shows a native Google+ share dialog with optional prefilled message (iOS only) and optional url to share
	public static void showShareDialog( string prefillText = null, string urlToShare = null )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayShowShareDialog( prefillText, urlToShare );
	}


	#endregion


	#region Cloud Data

	[DllImport("__Internal")]
	private static extern void _gplaySetStateData( string data, int key );

	// Sets the cloud data for the given key. Note that this does not upload it to the cloud. Calling updateCloudDataForKey will save it to the cloud.
	public static void setStateData( string data, int key )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplaySetStateData( data, key );
	}


	[DllImport("__Internal")]
	private static extern string _gplayStateDataForKey( int key );

	// Gets the cloud data for the given key
	public static string stateDataForKey( int key )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _gplayStateDataForKey( key );

		return string.Empty;
	}


	[DllImport("__Internal")]
	private static extern void _gplayLoadCloudDataForKey( int key, bool useRemoteDataForConflictResolution );

	// Downloads cloud data for the given key. The associated loadCloudDataForKeyFailed/Succeeded event will fire when complete.
	public static void loadCloudDataForKey( int key, bool useRemoteDataForConflictResolution = true )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayLoadCloudDataForKey( key, useRemoteDataForConflictResolution );
	}


	[DllImport("__Internal")]
	private static extern void _gplayUpdateCloudDataForKey( int key, bool useRemoteDataForConflictResolution );

	// Updates cloud data for the given key. The associated updateCloudDataForKeyFailed/Succeeded event will fire when complete.
	public static void updateCloudDataForKey( int key, bool useRemoteDataForConflictResolution = true )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayUpdateCloudDataForKey( key, useRemoteDataForConflictResolution );
	}


	[DllImport("__Internal")]
	private static extern void _gplayClearCloudDataForKey( int key, bool useRemoteDataForConflictResolution );

	// Clears cloud data for the given key
	public static void clearCloudDataForKey( int key, bool useRemoteDataForConflictResolution = true )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayClearCloudDataForKey( key, useRemoteDataForConflictResolution );
	}


	[DllImport("__Internal")]
	private static extern void _gplayDeleteCloudDataForKey( int key, bool useRemoteDataForConflictResolution );

	// Deletes cloud data for the given key
	public static void deleteCloudDataForKey( int key, bool useRemoteDataForConflictResolution = true )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayDeleteCloudDataForKey( key, useRemoteDataForConflictResolution );
	}

	#endregion


	#region Achievements

	[DllImport("__Internal")]
	private static extern void _gplayShowAchievements();

	// Shows the achievements screen
	public static void showAchievements()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayShowAchievements();
	}


	[DllImport("__Internal")]
	private static extern void _gplayRevealAchievement( string achievementId );

	// Reveals the achievement if it was previously hidden
	public static void revealAchievement( string achievementId )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayRevealAchievement( achievementId );
	}


	[DllImport("__Internal")]
	private static extern void _gplayUnlockAchievement( string achievementId, bool showsCompletionNotification );

	// Unlocks the achievement
	public static void unlockAchievement( string achievementId, bool showsCompletionNotification = true )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayUnlockAchievement( achievementId, showsCompletionNotification );
	}


	[DllImport("__Internal")]
	private static extern void _gplayIncrementAchievement( string achievementId, int numSteps );

	// Increments the achievement. Only works on achievements setup as incremental in the Google Developer Console.
	// Fires the incrementAchievementFailed/Succeeded event when complete.
	public static void incrementAchievement( string achievementId, int numSteps )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayIncrementAchievement( achievementId, numSteps );
	}


	[DllImport("__Internal")]
	private static extern string _gplayGetAllAchievementMetadata();

	// Gets the achievement metadata
	public static List<GPGAchievementMetadata> getAllAchievementMetadata()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return Json.decode<List<GPGAchievementMetadata>>( _gplayGetAllAchievementMetadata() );

		return new List<GPGAchievementMetadata>();
	}

	#endregion


	#region Leaderboards

	[DllImport("__Internal")]
	private static extern void _gplayShowLeaderboard( string leaderboardId, int timeScope );

	// Shows the requested leaderboard and time scope
	public static void showLeaderboard( string leaderboardId, GPGLeaderboardTimeScope timeScope )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayShowLeaderboard( leaderboardId, (int)timeScope );
	}


	[DllImport("__Internal")]
	private static extern void _gplayShowLeaderboards();

	// Shows a list of all learderboards
	public static void showLeaderboards()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayShowLeaderboards();
	}


	[DllImport("__Internal")]
	private static extern void _gplaySubmitScore( string leaderboardId, System.Int64 score );

	// Submits a score for the given leaderboard. Fires the submitScoreFailed/Succeeded event when complete.
	public static void submitScore( string leaderboardId, System.Int64 score )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplaySubmitScore( leaderboardId, score );
	}


	[DllImport("__Internal")]
	private static extern void _gplayLoadScoresForLeaderboard( string leaderboardId, int timeScope, bool isSocial, bool personalWindow );

	// Loads scores for the given leaderboard. Fires the loadScoresFailed/Succeeded event when complete.
	public static void loadScoresForLeaderboard( string leaderboardId, GPGLeaderboardTimeScope timeScope, bool isSocial, bool personalWindow )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_gplayLoadScoresForLeaderboard( leaderboardId, (int)timeScope, isSocial, personalWindow );
	}


	[DllImport("__Internal")]
	private static extern string _gplayGetAllLeaderboardMetadata();

	// Gets all the leaderboards metadata
	public static List<GPGLeaderboardMetadata> getAllLeaderboardMetadata()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return Json.decode<List<GPGLeaderboardMetadata>>( _gplayGetAllLeaderboardMetadata() );

		return new List<GPGLeaderboardMetadata>();
	}

	#endregion

}
#endif

