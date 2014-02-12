//
//  AudioRecorderManager.m
//  AudioRecordTest
//


#import "AudioRecorderManager.h"
#import <CoreAudio/CoreAudioTypes.h>
#import <AudioToolbox/AudioToolbox.h>


void UnitySendMessage( const char * className, const char * methodName, const char * param );


@implementation AudioRecorderManager

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

+ (AudioRecorderManager*)sharedManager
{
	static AudioRecorderManager *sharedSingleton;
	
	if( !sharedSingleton )
		sharedSingleton = [[AudioRecorderManager alloc] init];
	
	return sharedSingleton;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - Private

- (void)prepareAudioSessionForRecording
{
	[[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayAndRecord error:NULL];
	
	UInt32 audioRouteOverride = kAudioSessionOverrideAudioRoute_Speaker;
	AudioSessionSetProperty( kAudioSessionProperty_OverrideAudioRoute, sizeof( audioRouteOverride ), &audioRouteOverride );
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public

- (NSError*)prepareToRecordFile:(NSString*)filePath
{
	// gaurd against double recordings and allocations
	if( _recorder )
		[self completeRecording];

	NSDictionary *settings = [NSDictionary dictionaryWithObjectsAndKeys:
	 [NSNumber numberWithFloat:44100.0], AVSampleRateKey,
	 [NSNumber numberWithInt:kAudioFormatLinearPCM], AVFormatIDKey,
	 [NSNumber numberWithInt:1], AVNumberOfChannelsKey,
	 [NSNumber numberWithInt:AVAudioQualityMax], AVEncoderAudioQualityKey, nil];


	NSError *error = nil;
	_recorder = [[AVAudioRecorder alloc] initWithURL:[NSURL fileURLWithPath:filePath] settings:settings error:&error];
	_recorder.delegate = self;
	
	// If we didnt get an error, prepare to record so the recording starts faster
	if( !error )
	{
		if( ![_recorder prepareToRecord] )
			return [NSError errorWithDomain:@"com.prime31.AudioRecorder" code:-1 userInfo:[NSDictionary dictionaryWithObject:@"Could not prepare recording" forKey:NSLocalizedDescriptionKey]];
	}
	
	return error;
}


- (BOOL)record
{
	// set the audio category to record while recording
	[self prepareAudioSessionForRecording];
	
	return [_recorder record];
}


- (BOOL)recordForDuration:(NSTimeInterval)duration
{
	// set the audio category to record while recording
	[self prepareAudioSessionForRecording];
	
	return [_recorder recordForDuration:duration];
}


- (void)pause
{
	[_recorder pause];
}


- (void)stop
{
	// restore the category on stop
	[[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategorySoloAmbient error:NULL];
	
	[_recorder stop];
}


- (void)setMeteringEnabled:(BOOL)enable
{
	_recorder.meteringEnabled = enable;
}


- (CGFloat)averagePower
{
	// If metering isnt enabled, then return 0
	if( !_recorder.meteringEnabled )
		return 0.0f;
	
	// update the meters to get fresh data
	[_recorder updateMeters];
	return [_recorder averagePowerForChannel:0];
}


- (CGFloat)peakPower
{
	// If metering isnt enabled, then return 0
	if( !_recorder.meteringEnabled )
		return 0.0f;
	
	// update the meters to get fresh data
	[_recorder updateMeters];
	return [_recorder peakPowerForChannel:0];
}


- (void)completeRecording
{
	if( [_recorder isRecording] )
		[_recorder stop];
	
	[_recorder release];
	_recorder = nil;
}


- (BOOL)isRecording
{
	return [_recorder isRecording];
}


- (NSTimeInterval)currentTime
{
	return [_recorder currentTime];
}


- (NSString*)fileUrl
{
	return [_recorder.url absoluteString];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark AVAudioRecorderDelegate

/* audioRecorderDidFinishRecording:successfully: is called when a recording has been finished or stopped. This method is NOT called if the recorder is stopped due to an interruption. */
- (void)audioRecorderDidFinishRecording:(AVAudioRecorder*)recorder successfully:(BOOL)flag
{
	if( flag )
		UnitySendMessage( "AudioRecorderManager", "audioRecorderDidFinishRecording", [[recorder.url absoluteString] UTF8String] );
}


/* if an error occurs while encoding it will be reported to the delegate. */
- (void)audioRecorderEncodeErrorDidOccur:(AVAudioRecorder *)recorder error:(NSError *)error
{
	UnitySendMessage( "AudioRecorderManager", "audioRecorderEncoderFailed", [[error localizedDescription] UTF8String] );
}


/* audioRecorderEndInterruption:withFlags: is called when the audio session interruption has ended and this recorder had been interrupted while recording. */
/* Currently the only flag is AVAudioSessionInterruptionFlags_ShouldResume. */
- (void)audioRecorderEndInterruption:(AVAudioRecorder *)recorder withFlags:(NSUInteger)flags
{
	[self record];
}

@end

