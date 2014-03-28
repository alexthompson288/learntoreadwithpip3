using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prime31;


#if UNITY_IOS
public class GPGMultiplayer
{
	[DllImport("__Internal")]
	private static extern void _gplayRegisterDeviceToken( byte[] token, int tokenLength, bool isProductionEnvironment );

	// iOS only. Registers the push device token with Google. isProductionEnvironment controls whether Apple's production
	// or sandbox servers will be used to send the push notifications. The push device token can be retrieved in the usual
	// Unity way via the NotificationServices class methods.
	public static void registerDeviceToken( byte[] deviceToken, bool isProductionEnvironment )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplayRegisterDeviceToken( deviceToken, deviceToken.Length, isProductionEnvironment );
	}


	[DllImport("__Internal")]
	private static extern void _gplayShowInvitationInbox();

	// Shows the invitation inbox with all the game invitations the current user has available. If a user selects one of the invitations
	// the room is joined automatically.
	public static void showInvitationInbox()
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplayShowInvitationInbox();
	}


	[DllImport("__Internal")]
	private static extern void _gplayCreateRoomProgrammatically( int minAutoMatchPlayers, int maxAutoMatchPlayers, int exclusiveBitmask );

	// Creates a room with the provided auto-match criteria.
	// Exclusive bitmasks for the automatching request. The logical AND of each pairing of automatching requests must equal zero for auto-match.
	// If there are no exclusivity requirements for the game, this value should just be set to 0
	public static void createRoomProgrammatically( int minAutoMatchPlayers, int maxAutoMatchPlayers, int exclusiveBitmask = 0 )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplayCreateRoomProgrammatically( minAutoMatchPlayers, maxAutoMatchPlayers, exclusiveBitmask );
	}


	[DllImport("__Internal")]
	private static extern void _gplayShowPlayerSelector( int minPlayers, int maxPlayers );

	// Shows the player selector. minPlayers and maxPlayers does NOT include the current player.
	public static void showPlayerSelector( int minPlayers, int maxPlayers )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplayShowPlayerSelector( minPlayers, maxPlayers );
	}


	[DllImport("__Internal")]
	private static extern void _gplayJoinRoomWithInvitation( string invitationId );

	// Joins a room with the given invitationId which is aquired when an invite is received and the onInvitationReceivedEvent fires
	public static void joinRoomWithInvitation( string invitationId )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplayJoinRoomWithInvitation( invitationId );
	}


	[DllImport("__Internal")]
	private static extern void _gplayShowWaitingRoom( int minParticipantsToStart );

	// Android only. Shows the waiting room UI. minParticipantsToStart is the minimum number of participants that must be connected to the room
	// (including the current player) for the "Start playing" menu item to become enabled. Use int.MaxValue to require all players
	// to be connected.
	public static void showWaitingRoom( int minParticipantsToStart )
	{
		Debug.Log( "showWaitingRoom is only available on Android" );
	}


	[DllImport("__Internal")]
	private static extern void _gplayLeaveRoom();

	// Leaves the current room and exits the real-time multiplayer match
	public static void leaveRoom()
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplayLeaveRoom();
	}


	[DllImport("__Internal")]
	private static extern string _gplayGetParticipants();

	// Gets all the participants in the current room
	public static List<GPGMultiplayerParticipant> getParticipants()
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return null;

		var json = _gplayGetParticipants();
		return Json.decode<List<GPGMultiplayerParticipant>>( json );
	}


	[DllImport("__Internal")]
	private static extern string _gplayGetCurrentPlayerParticipantId();

	// Gets the participantId of the current player
	public static string getCurrentPlayerParticipantId()
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return null;

		return _gplayGetCurrentPlayerParticipantId();
	}


	#region Sending Realtime Messages

	[DllImport("__Internal")]
	private static extern void _gplaySendRealtimeMessageToAll( bool reliable, byte[] message, int messageLength );

	[DllImport("__Internal")]
	private static extern void _gplaySendRealtimeMessage( string participantId, bool reliable, byte[] message, int messageLength );

	// Send a message to a participant in a real-time room reliably
	public static void sendReliableRealtimeMessage( string participantId, byte[] message )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplaySendRealtimeMessage( participantId, true, message, message.Length );
	}


	// Send a message to all participants in a real-time room reliably
	public static void sendReliableRealtimeMessageToAll( byte[] message )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplaySendRealtimeMessageToAll( true, message, message.Length );
	}


	// Send a message to a participant in a real-time room unreliably
	public static void sendUnreliableRealtimeMessage( string participantId, byte[] message )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplaySendRealtimeMessage( participantId, false, message, message.Length );
	}


	// Send a message to all participants in a real-time room unreliably
	public static void sendUnreliableRealtimeMessageToAll( byte[] message )
	{
		if( Application.platform != RuntimePlatform.IPhonePlayer )
			return;

		_gplaySendRealtimeMessageToAll( false, message, message.Length );
	}

	#endregion

}
#endif