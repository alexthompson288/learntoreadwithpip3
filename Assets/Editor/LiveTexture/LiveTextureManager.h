//
//  ARManager.h
//  Unity-iPhone
//
//  Created by Mike on 12/17/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <OpenGLES/ES1/gl.h>
#import <OpenGLES/ES1/glext.h>


typedef enum {
	CaptureSessionPreset192x144, // same as low
	CaptureSessionPreset640x480,
	CaptureSessionPreset1280x720
} CaptureSessionPreset;



#define USE_FAST_PATH 1


@protocol LiveTextureDelegate;

@interface LiveTextureManager : NSObject <AVCaptureVideoDataOutputSampleBufferDelegate, AVCaptureFileOutputRecordingDelegate>
{
	GLuint _textureId;
	int _width;
	int _height;
}
@property (nonatomic, assign) id<LiveTextureDelegate> delegate;
@property (nonatomic, retain) NSString *cameraDeviceId;
@property (nonatomic, assign) GLuint textureId;
@property (nonatomic, assign) int width;
@property (nonatomic, assign) int height;



+ (LiveTextureManager*)sharedManager;

+ (BOOL)isCaptureAvailable;

+ (NSArray*)availableDevices;


- (void)startCameraCaptureWithDevice:(NSString*)deviceId preset:(CaptureSessionPreset)capturePreset;

- (void)stopCameraCapture;

- (void)setExposureMode:(AVCaptureExposureMode)exposureMode;

- (void)setFocusMode:(AVCaptureFocusMode)focusMode;

- (void)sendMessageToGameObject:(NSString*)gameObjectName method:(NSString*)methodName param:(NSString*)parameter;

@end


@protocol LiveTextureDelegate <NSObject>
- (void)cameraDidProcessFrameWithOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer;
@end

