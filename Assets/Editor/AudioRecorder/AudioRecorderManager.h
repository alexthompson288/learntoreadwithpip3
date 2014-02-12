//
//  AudioRecorderManager.h
//  AudioRecordTest
//


#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>


@interface AudioRecorderManager : NSObject <AVAudioRecorderDelegate>
{
	AVAudioRecorder *_recorder;
}


+ (AudioRecorderManager*)sharedManager;

- (NSError*)prepareToRecordFile:(NSString*)filePath;

- (BOOL)record;

- (BOOL)recordForDuration:(NSTimeInterval)duration;

- (BOOL)isRecording;

- (void)pause;

- (void)stop;

// metering methods
- (void)setMeteringEnabled:(BOOL)enable;

- (CGFloat)averagePower;

- (CGFloat)peakPower;

- (void)completeRecording;

- (NSTimeInterval)currentTime;

- (NSString*)fileUrl;


@end
