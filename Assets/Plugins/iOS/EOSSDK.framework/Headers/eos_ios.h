// Copyright Epic Games, Inc. All Rights Reserved.
#pragma once

#import <UIKit/UIKit.h>
#include "eos_types.h"

#pragma pack(push, 8)

/** The most recent version of the EOS_IOS_Auth_CredentialsOptions structure. */
#define EOS_IOS_AUTH_CREDENTIALSOPTIONS_API_LATEST 2

/** A callback function used to create snapshot views when the application is backgrounded while Account Portal is visible.
 *
 *  Each call should return a new instance.
 *  UIView must be retained using: CFBridgingRetain(viewInstance)
 *  If the view requires a CGRect for initWithFrame, CGRectZero should work
 *  Layout should be implemented in `layoutSubviews` or via constraints
 *  SDK will resize the UIView to match the UIWindow returned from ASWebAuthenticationPresentationContextProviding
 *  SDK will set autoresizing mask for fullscreen on the UIView (flexible width and height)
 */
EOS_DECLARE_CALLBACK_RETVALUE(UIView*, EOS_Auth_IOS_CreateBackgroundSnapshotView, void* Context);

/**
 * Options for initializing login for IOS.
 */
EOS_STRUCT(EOS_IOS_Auth_CredentialsOptions, (
	/** API version of EOS_IOS_Auth_CredentialsOptions. */
	int32_t ApiVersion;

	/** When calling EOS_Auth_Login
	 *  NSObject that implements the ASWebAuthenticationPresentationContextProviding protocol,
	 *  typically this is added to the applications UIViewController.
	 *  Required for iOS 13+ only, for earlier versions this value must be a nullptr.
	 *  Object must be retained and cast to a void* using: (void*)CFBridgingRetain(presentationContextProviding)
	 *  EOSSDK will release this bridged object when the value is consumed for iOS 13+.
	 */
	void* PresentationContextProviding;

	/** A callback function used to create snapshot views when the application is backgrounded while Account Portal is visible.
	 *
	 *  Each call should return a new instance.
	 *  UIView must be retained using: CFBridgingRetain(viewInstance)
     *  If the view requires a CGRect for initWithFrame, CGRectZero should work
     *  Layout should be implemented in `layoutSubviews` or via constraints
	 *  SDK will resize the UIView to match the UIWindow returned from ASWebAuthenticationPresentationContextProviding
	 *  SDK will set autoresizing mask for fullscreen on the UIView (flexible width and height)
	 */
	EOS_Auth_IOS_CreateBackgroundSnapshotView CreateBackgroundSnapshotView;

	/** Context data to pass back in the CreateBackgroundSnapshotView */
	void* CreateBackgroundSnapshotViewContext;

));

#pragma pack(pop)
