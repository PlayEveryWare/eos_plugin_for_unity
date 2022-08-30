#import <UIKit/UIKit.h>
#import <AuthenticationServices/AuthenticationServices.h>

extern "C" void* OverlayLoginUtility_get_application_window()
{
    return (void*)CFBridgingRetain([[[UIApplication sharedApplication] delegate] window]);
}

@interface ContextProvider : NSObject <ASAuthorizationControllerPresentationContextProviding, ASWebAuthenticationPresentationContextProviding>
@end

@implementation ContextProvider

extern "C" void* OverlayLoginUtility_create_context_provider()
{
    return (void*)CFBridgingRetain([ContextProvider new]);
}

- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller
API_AVAILABLE(ios(13.0)){
    return [[[UIApplication sharedApplication] delegate] window];
}

- (ASPresentationAnchor)presentationAnchorForWebAuthenticationSession:(ASWebAuthenticationSession *)session
API_AVAILABLE(ios(12.0)){
    return [[[UIApplication sharedApplication] delegate] window];
}


@end
