//
//  AudioRecorderBinding.m
//  AudioRecordTest


#import "AudioRecorderManager.h"


// Converts NSString to C style string by way of copy (Mono will free it)
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]



// returns an emptry string on success or an error message on failure.  MUST be called before recording can occur
const char * _audioRecorderPrepareToRecordFile( const char * filename )
{
	NSString *file = GetStringParam( filename );
	NSString *filePath = [[NSHomeDirectory() stringByAppendingPathComponent:@"Documents"] stringByAppendingPathComponent:file];
	
	NSError *error = [[AudioRecorderManager sharedManager] prepareToRecordFile:filePath];
	if( error )
		return MakeStringCopy( [error localizedDescription] );
	return MakeStringCopy( @"" );
}


// Also used to resume after stopping
bool _audioRecorderRecord()
{
	return [[AudioRecorderManager sharedManager] record];
}


bool _audioRecorderRecordForDuration( float duration )
{
	return [[AudioRecorderManager sharedManager] recordForDuration:duration];
}


void _audioRecorderPause()
{
	[[AudioRecorderManager sharedManager] pause];
}


// Finishes the recording and frees up memory by deallocing the recorder
void _audioRecorderStop( bool finishRecording )
{
	[[AudioRecorderManager sharedManager] stop];
	[[AudioRecorderManager sharedManager] completeRecording];
}


// audio metering methods
void _audioRecorderEnableMetering( bool shouldEnable )
{
	[[AudioRecorderManager sharedManager] setMeteringEnabled:shouldEnable];
}


float _audioRecorderGetPeakPower()
{
	return [[AudioRecorderManager sharedManager] peakPower];
}


float _audioRecorderGetAveragePower()
{
	return [[AudioRecorderManager sharedManager] averagePower];
}


float _audioRecorderGetCurrentTime()
{
	return [[AudioRecorderManager sharedManager] currentTime];
}


bool _audioRecorderIsRecording()
{
	return [[AudioRecorderManager sharedManager] isRecording];
}


const char * _audioRecorderGetFileUrl()
{
	NSString *url = [[AudioRecorderManager sharedManager] fileUrl];
	
	// nil guard ourselves
	if( url )
		return MakeStringCopy( url );
	return MakeStringCopy( @"" );
}
