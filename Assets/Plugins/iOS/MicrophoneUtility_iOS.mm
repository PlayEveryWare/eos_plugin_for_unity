#import <AVFAudio/AVFAudio.h>
#import <AVFoundation/AVFoundation.h>

extern "C" void MicrophoneUtility_set_default_audio_session()
{
    AVAudioSession *session = AVAudioSession.sharedInstance;
    NSError *error = nil;
    [session setCategory:AVAudioSessionCategoryPlayAndRecord
                    mode:AVAudioSessionModeVoiceChat
                 options:AVAudioSessionCategoryOptionDefaultToSpeaker|AVAudioSessionCategoryOptionDuckOthers
                   error:&error];
    if (nil == error)
    {
        // continue here
    }
}

extern "C" bool MicrophoneUtility_get_mic_permission()
{
    AVAudioSession *session = AVAudioSession.sharedInstance;
    switch ([session recordPermission])
    {
        case AVAudioSessionRecordPermissionGranted:
            return true;
        default:
            return false;
    }
}


