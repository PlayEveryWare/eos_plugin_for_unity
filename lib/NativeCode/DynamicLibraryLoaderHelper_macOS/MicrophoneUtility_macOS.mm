#import <AVFAudio/AVFAudio.h>
#import <AVFoundation/AVFoundation.h>


bool permissionGranted = false;
extern "C" bool MicrophoneUtility_get_mic_permission()
{
    if (@available(macOS 10.14, *))
    {
        switch ([AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio])
        {
            case AVAuthorizationStatusAuthorized:
                return true;
            case AVAuthorizationStatusNotDetermined:
                [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
                    permissionGranted=granted;
                }];
                return permissionGranted;
            case AVAuthorizationStatusDenied:
                return false;
            case AVAuthorizationStatusRestricted:
                return false;
        }
    }
    return true;
}
