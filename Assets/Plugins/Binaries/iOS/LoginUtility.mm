#import <UnityFramework/UnityFramework.h>
#import <AuthenticationServices/AuthenticationServices.h>

#define kUnityFrameworkPath @"/Frameworks/UnityFramework.framework"

@interface UnityAppController(EOS)<ASAuthorizationControllerPresentationContextProviding,ASWebAuthenticationPresentationContextProviding>
@end

@implementation UnityAppController(EOS)

extern "C"
{
    UnityFramework* GetUnityFramework()
    {
        NSString* bundlePath = [[NSBundle mainBundle] bundlePath];
        bundlePath = [bundlePath stringByAppendingString:kUnityFrameworkPath];

        NSBundle* bundle = [NSBundle bundleWithPath: bundlePath];
        if ([bundle isLoaded] == false)
        {
            [bundle load];
        }
     
        return [bundle.principalClass getInstance];
    }
    
    //void* should be casted from a UnityAppController*
    void* LoginUtility_get_app_controller()
    {
        if (@available(iOS 13.0, *))
        {
            UnityFramework* unityFramework = GetUnityFramework();
            
            UnityAppController* unityAppController = [unityFramework appController];
            if (unityAppController != nil)
            {
                return (void*)CFBridgingRetain(unityAppController);
            }
        }
        return nil;
    }

}

- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller
API_AVAILABLE(ios(13.0)){
    return self.window;
}

- (ASPresentationAnchor)presentationAnchorForWebAuthenticationSession:(ASWebAuthenticationSession *)session
API_AVAILABLE(ios(12.0)){
    return self.window;
}


@end
