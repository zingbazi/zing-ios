// Additions copyright (c) 2013 Empirical Development LLC. All rights reserved.

//
//  FacebookManager.m
//  Facebook
//
//  Created by Mike on 9/13/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "FacebookManager.h"
#import <objc/runtime.h>
#import <Accounts/Accounts.h>
#import <Social/Social.h>


NSString* const kFacebookUrlSchemeSuffixKey = @"kFacebookUrlSchemeKey";


void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnityPause( bool pause );
UIViewController *UnityGetGLViewController();


@implementation FacebookManager

@synthesize urlSchemeSuffix, appLaunchUrl, loginBehavior;

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

+ (void)load
{
	[[NSNotificationCenter defaultCenter] addObserver:[self sharedManager]
											 selector:@selector(applicationDidFinishLaunching:)
												 name:UIApplicationDidFinishLaunchingNotification
											   object:nil];

	[[NSNotificationCenter defaultCenter] addObserver:[self sharedManager]
											 selector:@selector(applicationDidBecomeActive:)
												 name:UIApplicationDidBecomeActiveNotification
											   object:nil];
}


+ (FacebookManager*)sharedManager
{
	static dispatch_once_t pred;
	static FacebookManager *_sharedInstance = nil;

	dispatch_once( &pred, ^{ _sharedInstance = [[self alloc] init]; } );
	return _sharedInstance;
}


+ (BOOL)userCanUseFacebookComposer
{
	Class slComposer = NSClassFromString( @"SLComposeViewController" );
	if( slComposer && [SLComposeViewController isAvailableForServiceType:SLServiceTypeFacebook] )
		return YES;
	return NO;
}


+ (NSString*)JSONStringFromObject:(NSObject*)object
{
	NSError *error = nil;
	NSData *jsonData = [NSJSONSerialization dataWithJSONObject:object options:0 error:&error];
	
	if( jsonData && !error )
		return [[[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] autorelease];
	else
		NSLog( @"jsonData was null, error: %@", [error localizedDescription] );
    
    return @"{}";
}


+ (NSObject*)objectFromJsonString:(NSString*)json
{
	NSError *error = nil;
	NSData *data = [NSData dataWithBytes:json.UTF8String length:json.length];
    NSObject *object = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingAllowFragments error:&error];
	
	if( error )
		NSLog( @"failed to deserialize JSON: %@ with error: %@", json, [error localizedDescription] );
    
    return object;
}


- (id)init
{
	if( ( self = [super init] ) )
	{
		self.loginBehavior = FBSessionLoginBehaviorUseSystemAccountIfPresent;

		// if we have an appId or urlSchemeSuffix tucked away, set it now
		if( [[NSUserDefaults standardUserDefaults] objectForKey:kFacebookUrlSchemeSuffixKey] )
			self.urlSchemeSuffix = [[NSUserDefaults standardUserDefaults] objectForKey:kFacebookUrlSchemeSuffixKey];

		NSDictionary *dict = [[NSBundle mainBundle] infoDictionary];
		if( ![[dict allKeys] containsObject:@"FacebookAppID"] )
		{
			NSLog( @"ERROR: You have not setup your Facebook app ID in the Info.plist file. Not having it in the Info.plist will cause your application to crash so the plugin is disabling itself." );
			return nil;
		}

		[self performSelector:@selector(publishPluginUsage) withObject:nil afterDelay:10];
	}
	return self;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - NSNotifications

- (void)applicationDidFinishLaunching:(NSNotification*)note
{
	// did we get launched with a userInfo dict?
	if( note.userInfo )
	{
		NSURL *url = [note.userInfo objectForKey:UIApplicationLaunchOptionsURLKey];
		if( url )
		{
			NSLog( @"recovered URL from jettisoned app. going to attempt login" );
			[self handleOpenURL:url sourceApplication:nil];
		}
	}
}


- (void)applicationDidBecomeActive:(NSNotification*)note
{
	[FBSession.activeSession handleDidBecomeActive];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private/Internal

+ (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
	return [[FacebookManager sharedManager] handleOpenURL:url sourceApplication:sourceApplication];
}


- (BOOL)handleOpenURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication
{
	NSLog( @"url used to open app: %@", url );
	self.appLaunchUrl = url.absoluteString;

	BOOL res = [FBAppCall handleOpenURL:url sourceApplication:sourceApplication];
	//BOOL res = [FBSession.activeSession handleOpenURL:url];

	return res;
}


- (NSString*)getAppId
{
	return [[[NSBundle mainBundle] infoDictionary] objectForKey:@"FacebookAppID"];
}


- (NSString*)urlEncodeValue:(NSString*)str
{
	NSString *result = (NSString*)CFURLCreateStringByAddingPercentEscapes( kCFAllocatorDefault, (CFStringRef)str, NULL, CFSTR( "?=&+" ), kCFStringEncodingUTF8 );
	return [result autorelease];
}


- (void)publishPluginUsage
{
	dispatch_async( dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_LOW, 0 ), ^
	{
		NSDictionary *dict = [NSDictionary dictionaryWithObjectsAndKeys:[self getAppId], @"appid",
							  @"prime31_socialnetworking", @"resource",
							  @"1.0.0", @"version", nil];

		// prep the post data
		NSString *post = [NSString stringWithFormat:@"plugin=featured_resources&payload=%@", [self urlEncodeValue:[FacebookManager JSONStringFromObject:dict]]];
		NSData *postData = [post dataUsingEncoding:NSASCIIStringEncoding allowLossyConversion:YES];
		NSString *postLength = [NSString stringWithFormat:@"%d", postData.length];

		// prep the request
		NSMutableURLRequest *request = [[[NSMutableURLRequest alloc] init] autorelease];
		[request setURL:[NSURL URLWithString:@"https://www.facebook.com/impression.php"]];
		[request setHTTPMethod:@"POST"];
		[request setValue:postLength forHTTPHeaderField:@"Content-Length"];
		[request setValue:@"application/x-www-form-urlencoded" forHTTPHeaderField:@"Content-Type"];
		[request setHTTPBody:postData];

		// send the request
		NSURLResponse *response = nil;
		[NSURLConnection sendSynchronousRequest:request returningResponse:&response error:NULL];
	});
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - SLComposer

- (void)showFacebookComposerWithMessage:(NSString*)message image:(UIImage*)image link:(NSString*)link
{
	if( ![FacebookManager userCanUseFacebookComposer] )
		return;

	Class slComposerClass = NSClassFromString( @"SLComposeViewController" );
	UIViewController *slComposer = [slComposerClass performSelector:@selector(composeViewControllerForServiceType:) withObject:@"com.apple.social.facebook"];

	if( !slComposer )
		return;

	// Add a message
	[slComposer performSelector:@selector(setInitialText:) withObject:message];

	// add an image
	if( image )
		[slComposer performSelector:@selector(addImage:) withObject:image];

	// add a link
	if( link )
		[slComposer performSelector:@selector(addURL:) withObject:[NSURL URLWithString:link]];

	// set a blocking handler for the tweet sheet
	[slComposer performSelector:@selector(setCompletionHandler:) withObject:^( NSInteger result )
	{
		UnityPause( false );
		[UnityGetGLViewController() dismissViewControllerAnimated:YES completion:nil];

		if( result == 1 )
			UnitySendMessage( "FacebookManager", "facebookComposerCompleted", "1" );
		else if( result == 0 )
			UnitySendMessage( "FacebookManager", "facebookComposerCompleted", "0" );
	}];

	// Show the tweet sheet
	UnityPause( true );
	[UnityGetGLViewController() presentViewController:slComposer animated:YES completion:nil];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - Facebook Share Dialog

- (void)showShareDialogWithParams:(FBShareDialogParams*)dialogParams
{
	[FBDialogs presentShareDialogWithParams:dialogParams clientState:nil handler:^( FBAppCall *call, NSDictionary *results, NSError *error )
	 {
		 if( error )
		 {
			 NSLog( @"Share Dialog error: %@", error );
			 UnitySendMessage( "FacebookManager", "shareDialogFailed", error.localizedDescription.UTF8String );
		 }
		 else
		 {
			 UnitySendMessage( "FacebookManager", "shareDialogSucceeded", [FacebookManager JSONStringFromObject:results].UTF8String );
		 }
	 }];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public

- (void)enableFrictionlessRequests
{
	if( [FBSession activeSession] == nil )
	{
		NSLog( @"error: there is no active session. You cannot enable frictionless requests until you have an active session" );
		return;
	}

	self.frictionlessRecipientCache = [[[FBFrictionlessRecipientCache alloc] init] autorelease];
	[self.frictionlessRecipientCache prefetchAndCacheForSession:[FBSession activeSession] completionHandler:^( FBRequestConnection *conn, id result, NSError *error )
	{
		if( error )
			NSLog( @"error prefethcing frictionless recipient cache: %@", error );
	}];
}


- (void)renewCredentialsForAllFacebookAccounts
{
	if( !NSClassFromString( @"ACAccountStore" ) )
		return;

	if( &ACAccountTypeIdentifierFacebook == NULL )
		return;

	ACAccountStore *accountStore = [[ACAccountStore alloc] init];

	if( !accountStore )
		return;

    ACAccountType *accountTypeFB = [accountStore accountTypeWithAccountTypeIdentifier:ACAccountTypeIdentifierFacebook];
	NSArray *facebookAccounts = [accountStore accountsWithAccountType:accountTypeFB];

	if( facebookAccounts.count == 0 )
		return;

	for( ACAccount *fbAccount in facebookAccounts )
	{
		[accountStore renewCredentialsForAccount:fbAccount completion:^( ACAccountCredentialRenewResult renewResult, NSError *error )
		{
			if( error )
				NSLog( @"account %@ failed to renew: %@", fbAccount, error );
			else
				NSLog( @"account %@ renewed successfully", fbAccount );
		}];
	}
}


- (void)startSessionQuietly
{
	[FBSettings publishInstall:[self getAppId]];
	[FBSettings setDefaultAppID:[self getAppId]];

	// create a session manually in case we have a url scheme suffix
	FBSession *facebookSession = [[FBSession alloc] initWithAppID:[self getAppId] permissions:nil urlSchemeSuffix:self.urlSchemeSuffix tokenCacheStrategy:nil];
	[FBSession setActiveSession:facebookSession];

	if( [FBSession openActiveSessionWithAllowLoginUI:NO] )
	{
		UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessTokenData.accessToken.UTF8String );
	}
}


- (BOOL)isLoggedIn
{
	if( !FBSession.activeSession )
		return NO;

	return FBSession.activeSession.isOpen;
}


- (NSString*)accessToken
{
    return FBSession.activeSession.accessTokenData.accessToken;
}


- (NSArray*)sessionPermissions
{
	return FBSession.activeSession.permissions;
}


- (void)loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions:(NSMutableArray*)permissions urlSchemeSuffix:(NSString*)aUrlSchemeSuffix
{
	// store the url scheme suffix for later use
	self.urlSchemeSuffix = aUrlSchemeSuffix;
	[[NSUserDefaults standardUserDefaults] setObject:self.urlSchemeSuffix forKey:kFacebookUrlSchemeSuffixKey];

	if( [self isLoggedIn] )
	{
		UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessTokenData.accessToken.UTF8String );
		return;
	}

	// we must have email, user_birthday, or user_location to authorize!
	if( ![permissions containsObject:@"email"] && ![permissions containsObject:@"user_birthday"] && ![permissions containsObject:@"user_location"] )
		[permissions addObject:@"email"];


	FBSession *facebookSession = [[FBSession alloc] initWithAppID:[self getAppId] permissions:permissions urlSchemeSuffix:aUrlSchemeSuffix tokenCacheStrategy:nil];
	[FBSession setActiveSession:facebookSession];
    [FBSession openActiveSessionWithAllowLoginUI:YES];
}


- (void)loginWithRequestedPermissions:(NSMutableArray*)permissions urlSchemeSuffix:(NSString*)aUrlSchemeSuffix
{
	// store the url scheme suffix for later use
	self.urlSchemeSuffix = aUrlSchemeSuffix;
	[[NSUserDefaults standardUserDefaults] setObject:self.urlSchemeSuffix forKey:kFacebookUrlSchemeSuffixKey];

	if( [self isLoggedIn] )
	{
		UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessTokenData.accessToken.UTF8String );
		return;
	}


	FBSession *facebookSession = [[FBSession alloc] initWithAppID:[self getAppId] permissions:permissions urlSchemeSuffix:self.urlSchemeSuffix tokenCacheStrategy:nil];
	[FBSession setActiveSession:facebookSession];

	// change the behavior here to force different types. the available tyeps are: FBSessionLoginBehaviorUseSystemAccountIfPresent, FBSessionLoginBehaviorWithFallbackToWebView,
	// FBSessionLoginBehaviorWithNoFallbackToWebView, FBSessionLoginBehaviorForcingWebView
	[facebookSession openWithBehavior:self.loginBehavior completionHandler:^( FBSession *sess, FBSessionState status, NSError *error )
	{
		 if( FB_ISSESSIONOPENWITHSTATE( status ) )
		 {
			 UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessTokenData.accessToken.UTF8String );
		 }
		 else
		 {
			 if( status == FBSessionStateClosed )
			 {
				 NSLog( @"session closed" );
				 //UnitySendMessage( "FacebookManager", "facebookDidLogout", "" );
			 }
			 else if( error )
			 {
				 NSLog( @"session creation error: %@ userInfo: %@", error, error.userInfo ? error.userInfo : @"no userInfo" );
				 NSString *errorString = error.localizedDescription != nil ? error.localizedDescription : @"unknown error";
				 UnitySendMessage( "FacebookManager", "loginFailed", errorString.UTF8String );
			 }
			 else
			 {
				 NSLog( @"session creation failed with no error. Session: %@, status: %i", sess, status );
				 UnitySendMessage( "FacebookManager", "loginFailed", "Unknown Error" );
			 }
		 }
	 }];
}


- (void)reauthorizeWithReadPermissions:(NSArray*)permissions
{
	[[FBSession activeSession] requestNewReadPermissions:permissions completionHandler:^( FBSession *session, NSError *error )
	{
		if( error )
			UnitySendMessage( "FacebookManager", "reauthorizationFailed", error.localizedDescription.UTF8String );
		else
			UnitySendMessage( "FacebookManager", "reauthorizationSucceeded", "" );
	}];
}


- (void)reauthorizeWithPublishPermissions:(NSArray*)permissions defaultAudience:(FBSessionDefaultAudience)audience
{
	[[FBSession activeSession] requestNewPublishPermissions:permissions defaultAudience:audience completionHandler:^( FBSession *session, NSError *error )
	{
		if( error )
			UnitySendMessage( "FacebookManager", "reauthorizationFailed", error.localizedDescription.UTF8String );
		else
			UnitySendMessage( "FacebookManager", "reauthorizationSucceeded", "" );
	}];
}


- (void)logout
{
	[FBSession.activeSession closeAndClearTokenInformation];
	//[FBSession.activeSession close];
}


- (void)showDialog:(NSString*)dialogType withParms:(NSMutableDictionary*)dict
{
	id dialogHandler = ^( FBWebDialogResult result, NSURL *resultURL, NSError *error )
	{
		if( result == FBWebDialogResultDialogCompleted )
		{
			UnitySendMessage( "FacebookManager", "dialogCompletedWithUrl", resultURL ? resultURL.absoluteString.UTF8String : "" );
		}
		else
		{
			NSString *errorString = @"Unknown Error";
			if( error )
				errorString = error.localizedDescription;

			UnitySendMessage( "FacebookManager", "dialogFailedWithError", errorString.UTF8String );
		}
	};

	if( [dialogType isEqualToString:@"apprequests"] )
	{
		NSArray *allKeys = [dict allKeys];
		NSString *message = @"You forgot to pass in a message parameter";
		NSString *title = @"You forgot to pass in a title parameter";

		if( [allKeys containsObject:@"message"] )
			message = [dict objectForKey:@"message"];

		if( [allKeys containsObject:@"title"] )
			title = [dict objectForKey:@"title"];

		if( [message isEqualToString:@""] || [title isEqualToString:@""] )
			NSLog( @"Note that the apprequests dialog requires both a title and message parameter. The plugin just saved you from an error by adding them for you" );

		[FBWebDialogs presentRequestsDialogModallyWithSession:nil
													  message:message
														title:title
												   parameters:dict
													  handler:dialogHandler
												  friendCache:self.frictionlessRecipientCache];
		return;
	}


	[FBWebDialogs presentDialogModallyWithSession:nil dialog:dialogType parameters:dict handler:dialogHandler];
}


- (void)requestWithGraphPath:(NSString*)path httpMethod:(NSString*)method params:(NSDictionary*)params
{
	[FBRequestConnection startWithGraphPath:path parameters:params HTTPMethod:method completionHandler:^( FBRequestConnection *conn, id result, NSError *error )
	{
		if( error )
		{
			UnitySendMessage( "FacebookManager", "graphRequestFailed", [[error localizedDescription] UTF8String] );
		}
		else
		{
			NSString *json = [FacebookManager JSONStringFromObject:result];
			UnitySendMessage( "FacebookManager", "graphRequestCompleted", json.UTF8String );
		}
	}];
}


- (void)requestWithRestMethod:(NSString*)restMethod httpMethod:(NSString*)method params:(NSMutableDictionary*)params
{
	FBRequest *req = [[[FBRequest alloc] initWithSession:FBSession.activeSession restMethod:restMethod parameters:params HTTPMethod:method] autorelease];
	FBRequestConnection *connection = [[FBRequestConnection alloc] init];
	[connection addRequest:req completionHandler:^( FBRequestConnection *conn, id result, NSError *error )
	{
		if( error )
		{
			UnitySendMessage( "FacebookManager", "restRequestFailed", [[error localizedDescription] UTF8String] );
		}
		else
		{
			NSString *json = [FacebookManager JSONStringFromObject:result];
			UnitySendMessage( "FacebookManager", "restRequestCompleted", json.UTF8String );
		}
		[conn autorelease];
	}];
	[connection start];
}

@end




#if UNITY_VERSION < 420

#import "AppController.h"
@implementation AppController(FacebookURLHandler)

#else

#import "UnityAppController.h"
@implementation UnityAppController(FacebookURLHandler)

#endif

- (BOOL)application:(UIApplication*)application
			openURL:(NSURL*)url
  sourceApplication:(NSString*)sourceApplication
		 annotation:(id)annotation
{
	// Any classes added here should have a class method with the signature application:openURL:sourceApplication:annotation:
	NSArray *classesThatNeedToHandleOpenUrl = @[ @"GPlayManager", @"FacebookManager" ];

	for( NSString *className in classesThatNeedToHandleOpenUrl )
	{
		Class klass = NSClassFromString( className );
		if( [klass respondsToSelector:@selector(application:openURL:sourceApplication:annotation:)] )
			[klass application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
	}

	return YES;
}

@end


