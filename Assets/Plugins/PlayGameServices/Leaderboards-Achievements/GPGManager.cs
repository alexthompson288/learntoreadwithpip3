using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;



#if UNITY_IPHONE || UNITY_ANDROID
public class GPGManager : AbstractManager
{
	// Fired when authentication succeeds. Includes the user_id
	public static event Action<string> authenticationSucceededEvent;

	// Fired when authentication fails
	public static event Action<string> authenticationFailedEvent;

	// iOS only. Fired when the user signs out. This could happen if in a leaderboard they touch the settings button and logout from there.
	public static event Action userSignedOutEvent;

	// Fired when data fails to reload for a key. This particular model data is usually the player info or leaderboard/achievement metadata that is auto loaded.
	public static event Action<string> reloadDataForKeyFailedEvent;

	// Fired when data is reloaded for a key
	public static event Action<string> reloadDataForKeySucceededEvent;

	// Android only. Fired when a license check fails
	public static event Action licenseCheckFailedEvent;

	// Android only. Fired when a profile image is loaded with the full path to the image
	public static event Action<string> profileImageLoadedAtPathEvent;
	
	// Fired when the user finishes sharing via the showShareDialog method. The string parameter will contain either an error message if sharing did not occur
	// or null if they shared successfullu
	public static event Action<string> finishedSharingEvent;


	// ##### ##### ##### ##### ##### ##### #####
	// ## Cloud Data
	// ##### ##### ##### ##### ##### ##### #####

	// Fired when loading cloud data fails. Provides the status code/GPGAppStateStatus.
	public static event Action<string> loadCloudDataForKeyFailedEvent;

	// Fired when loading cloud data succeeds and includes the key and data
	public static event Action<int,string> loadCloudDataForKeySucceededEvent;

	// Fired when updating cloud data fails. Provides the status code/GPGAppStateStatus.
	public static event Action<string> updateCloudDataForKeyFailedEvent;

	// Fired when updating cloud data succeeds and includes the key and data
	public static event Action<int,string> updateCloudDataForKeySucceededEvent;

	// Fired when clearing cloud data fails. Provides the status code/GPGAppStateStatus.
	public static event Action<string> clearCloudDataForKeyFailedEvent;

	// Fired when clearing cloud data succeeds and includes the key
	public static event Action<string> clearCloudDataForKeySucceededEvent;

	// Fired when deleting cloud data fails. Provides the status code/GPGAppStateStatus.
	public static event Action<string> deleteCloudDataForKeyFailedEvent;

	// Fired when deleting cloud data succeeds and includes the key
	public static event Action<string> deleteCloudDataForKeySucceededEvent;


	// ##### ##### ##### ##### ##### ##### #####
	// ## Achievements
	// ##### ##### ##### ##### ##### ##### #####

	// Fired when unlocking an achievement fails. Provides the achievementId and the error in that order.
	public static event Action<string,string> unlockAchievementFailedEvent;

	// Fired when unlocking an achievement succeeds. Provides the achievementId and a bool that lets you know if it was newly unlocked.
	public static event Action<string,bool> unlockAchievementSucceededEvent;

	// Fired when incrementing an achievement fails. Provides the achievementId and the error in that order.
	public static event Action<string,string> incrementAchievementFailedEvent;

	// Fired when incrementing an achievement succeeds. Provides the achievementId and a bool that lets you know if it was newly unlocked (on iOS, on Android the bool indicates success)
	public static event Action<string,bool> incrementAchievementSucceededEvent;

	// Fired when revealing an achievement fails. Provides the achievementId and the error in that order.
	public static event Action<string,string> revealAchievementFailedEvent;

	// Fired when revealing an achievement succeeds. The string lets you know the achievementId.
	public static event Action<string> revealAchievementSucceededEvent;


	// ##### ##### ##### ##### ##### ##### #####
	// ## Leaderboards
	// ##### ##### ##### ##### ##### ##### #####

	// Fired when submitting a score fails. Provides the leaderboardId and the error in that order.
	public static event Action<string,string> submitScoreFailedEvent;

	// Fired when submitting a scores succeeds. Returns the leaderboardId and a dictionary with some extra data with the fields from
	// the GPGScoreReport class: https://developers.google.com/games/services/ios/api/interface_g_p_g_score_report
	public static event Action<string,Dictionary<string,object>> submitScoreSucceededEvent;

	// Fired when loading scores fails. Provides the leaderboardId and the error in that order.
	public static event Action<string,string> loadScoresFailedEvent;

	// Fires when loading scores succeeds
	public static event Action<List<GPGScore>> loadScoresSucceededEvent;



	static GPGManager()
	{
		AbstractManager.initialize( typeof( GPGManager ) );
	}


	private void fireEventWithIdentifierAndError( Action<string,string> theEvent, string json )
	{
		if( theEvent == null )
			return;

		var dict = json.dictionaryFromJson();
		if( dict != null && dict.ContainsKey( "identifier" ) && dict.ContainsKey( "error" ) )
			theEvent( dict["identifier"].ToString(), dict["error"].ToString() );
		else
			Debug.LogError( "json could not be deserialized to an identifier and an error: " + json );
	}


	private void fireEventWithIdentifierAndBool( Action<string,bool> theEvent, string param )
	{
		if( theEvent == null )
			return;

		var parts = param.Split( new char[] { ',' } );
		if( parts.Length == 2 )
			theEvent( parts[0], parts[1] == "1" );
		else
			Debug.LogError( "param could not be deserialized to an identifier and an error: " + param );
	}


	public void userSignedOut( string empty )
	{
		userSignedOutEvent.fire();
	}


	public void reloadDataForKeyFailed( string error )
	{
		reloadDataForKeyFailedEvent.fire( error );
	}


	public void reloadDataForKeySucceeded( string param )
	{
		reloadDataForKeySucceededEvent.fire( param );
	}


	public void licenseCheckFailed( string param )
	{
		licenseCheckFailedEvent.fire();
	}


	public void profileImageLoadedAtPath( string path )
	{
		profileImageLoadedAtPathEvent.fire( path );
	}
	
	
	public void finishedSharing( string errorOrNull )
	{
		finishedSharingEvent.fire( errorOrNull );
	}


	public void loadCloudDataForKeyFailed( string error )
	{
		loadCloudDataForKeyFailedEvent.fire( error );
	}


	public void loadCloudDataForKeySucceeded( string json )
	{
		var obj = json.dictionaryFromJson();
		loadCloudDataForKeySucceededEvent.fire( int.Parse( obj["key"].ToString() ), obj["data"].ToString() );
	}


	public void updateCloudDataForKeyFailed( string error )
	{
		updateCloudDataForKeyFailedEvent.fire( error );
	}


	public void updateCloudDataForKeySucceeded( string json )
	{
		var obj = json.dictionaryFromJson();
		updateCloudDataForKeySucceededEvent.fire( int.Parse( obj["key"].ToString() ), obj["data"].ToString() );
	}


	public void clearCloudDataForKeyFailed( string error )
	{
		clearCloudDataForKeyFailedEvent.fire( error );
	}


	public void clearCloudDataForKeySucceeded( string param )
	{
		clearCloudDataForKeySucceededEvent.fire( param );
	}


	public void deleteCloudDataForKeyFailed( string error )
	{
		deleteCloudDataForKeyFailedEvent.fire( error );
	}


	public void deleteCloudDataForKeySucceeded( string param )
	{
		deleteCloudDataForKeySucceededEvent.fire( param );
	}


	public void unlockAchievementFailed( string json )
	{
		fireEventWithIdentifierAndError( unlockAchievementFailedEvent, json );
	}


	public void unlockAchievementSucceeded( string param )
	{
		fireEventWithIdentifierAndBool( unlockAchievementSucceededEvent, param );
	}


	public void incrementAchievementFailed( string json )
	{
		fireEventWithIdentifierAndError( incrementAchievementFailedEvent, json );
	}


	public void incrementAchievementSucceeded( string param )
	{
		var parts = param.Split( new char[] { ',' } );
		if( parts.Length == 2 )
			incrementAchievementSucceededEvent.fire( parts[0], parts[1] == "1" );
	}


	public void revealAchievementFailed( string json )
	{
		fireEventWithIdentifierAndError( revealAchievementFailedEvent, json );
	}


	public void revealAchievementSucceeded( string achievementId )
	{
		revealAchievementSucceededEvent.fire( achievementId );
	}


	public void submitScoreFailed( string json )
	{
		fireEventWithIdentifierAndError( submitScoreFailedEvent, json );
	}


	public void submitScoreSucceeded( string json )
	{
		if( submitScoreSucceededEvent != null )
		{
			var dict = json.dictionaryFromJson();
			string leaderboardId = "Unknown";

			if( dict.ContainsKey( "leaderboardId" ) )
				leaderboardId = dict["leaderboardId"].ToString();

			submitScoreSucceededEvent( leaderboardId, dict );
		}
	}


	public void loadScoresFailed( string json )
	{
		fireEventWithIdentifierAndError( loadScoresFailedEvent, json );
	}


	public void loadScoresSucceeded( string json )
	{
		if( loadScoresSucceededEvent != null )
			loadScoresSucceededEvent( Json.decode<List<GPGScore>>( json ) );
	}


	public void authenticationSucceeded( string param )
	{
		authenticationSucceededEvent.fire( param );
	}


	public void authenticationFailed( string error )
	{
		authenticationFailedEvent.fire( error );
	}

}
#endif
