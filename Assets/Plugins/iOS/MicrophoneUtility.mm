#import <AVFAudio/AVFAudio.h>

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

extern "C" void MicrophoneUtility_get_mic_permission()
{
    
}





