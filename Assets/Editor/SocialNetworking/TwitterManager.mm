// Additions copyright (c) 2013 Empirical Development LLC. All rights reserved.

//
//  P31Twitter.m
//  SocialNetworking
//
//  Created by Mike on 9/11/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "TwitterManager.h"
#import "P31MutableOauthRequest.h"
#import "OARequestParameter.h"
#import "AddAccountController.h"
#include <Twitter/Twitter.h>


void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnityPause( bool shouldPause );
UIViewController *UnityGetGLViewController();

NSString *const kLoggedInUser = @"kLoggedInUser";


@implementation TwitterManager

@synthesize consumerKey = _consumerKey, consumerSecret = _consumerSecret;


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

+ (TwitterManager*)sharedManager
{
	static TwitterManager *sharedSingleton;
	
	if( !sharedSingleton )
		sharedSingleton = [[TwitterManager alloc] init];
	
	return sharedSingleton;
}


+ (BOOL)isTweetSheetSupported
{
	return NSClassFromString( @"TWTweetComposeViewController" ) != nil;
}


+ (BOOL)userCanTweet
{
	Class twComposer = NSClassFromString( @"TWTweetComposeViewController" );
	if( twComposer && [twComposer performSelector:@selector(canSendTweet)] )
		return YES;
	return NO;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private (not anymore)

- (NSString*)extractUsernameFromHTTPBody:(NSString*)body
{
	if( !body )
		return nil;
	
	NSArray	*tuples = [body componentsSeparatedByString: @"&"];
	if( tuples.count < 1 )
		return nil;
	
	for( NSString *tuple in tuples )
	{
		NSArray *keyValueArray = [tuple componentsSeparatedByString: @"="];
		
		if( keyValueArray.count == 2 )
		{
			NSString *key = [keyValueArray objectAtIndex: 0];
			NSString *value = [keyValueArray objectAtIndex: 1];
			
			if( [key isEqualToString:@"screen_name"] )
				return value;
		}
	}
	
	return nil;
}


- (void)completeLoginWithResponseData:(NSString*)data
{
	NSString *username = [self extractUsernameFromHTTPBody:data];
	if( !username )
	{
		UnitySendMessage( "TwitterManager", "loginFailed", [data UTF8String] );
	}
	else
	{
		// save the token for posting
		[[NSUserDefaults standardUserDefaults] setObject:data forKey:kLoggedInUser];
		[[NSUserDefaults standardUserDefaults] synchronize];
		
		// send success message back to Unity
		UnitySendMessage( "TwitterManager", "loginSucceeded", username.UTF8String );
	}
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private

- (void)showViewControllerModallyInWrapper:(UIViewController*)viewController
{
	// pause the game
	UnityPause( true );
	
	// show the mail composer on iPad in a form sheet
	if( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad )
		viewController.modalPresentationStyle = UIModalPresentationFormSheet;
	
	// show the controller
	[UnityGetGLViewController() presentViewController:viewController animated:YES completion:nil];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public

- (void)unpauseUnity
{
	UnityPause( false );
}


- (BOOL)isLoggedIn
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( tokenString )
		return YES;
	return NO;
}


- (NSString*)loggedInUsername
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
		return @"";
	return [self extractUsernameFromHTTPBody:tokenString];
}


- (void)showOauthLoginDialog
{
    AddAccountController *con = [[AddAccountController alloc] initWithNibName:nil bundle:nil];
    UINavigationController *navCon = [[UINavigationController alloc] initWithRootViewController:con];
    navCon.navigationBar.barStyle = UIBarStyleBlack;
    
    [self showViewControllerModallyInWrapper:navCon];
    [con release];
    [navCon release];
}


- (void)logout
{
	[[NSUserDefaults standardUserDefaults] setObject:nil forKey:kLoggedInUser];
	[[NSUserDefaults standardUserDefaults] synchronize];
}


- (void)postStatusUpdate:(NSString*)status withImageAtPath:(NSString*)path
{
	// token check
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
	{
		UnitySendMessage( "TwitterManager", "requestFailed", "User is not logged in" );
		return;
	}
	OAToken *token = [[OAToken alloc] initWithHTTPResponseBody:tokenString];
	
	// setup the request
	P31MutableOauthRequest *request = [[[P31MutableOauthRequest alloc] initWithUrl:@"https://api.twitter.com/1.1/statuses/update_with_media.json"
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:token] autorelease];
	[request setHTTPMethod:@"POST"];	
	
	NSString *boundary = @"---------------------------14737809831466499882746641449";
	NSString *contentType = [NSString stringWithFormat:@"multipart/form-data; boundary=%@", boundary];																		
	[request addValue:contentType forHTTPHeaderField:@"Content-Type"];


	NSMutableData *body = [NSMutableData data];

	// file
	UIImage *image = [UIImage imageWithContentsOfFile:path];
	[body appendData:[[NSString stringWithFormat:@"--%@\r\n", boundary] dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[@"Content-Disposition: attachment; name=\"media[]\"; filename=\"screenshot.png\"\r\n" dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[@"Content-Type: application/octet-stream\r\n\r\n" dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:UIImagePNGRepresentation( image )];
	[body appendData:[@"\r\n" dataUsingEncoding:NSUTF8StringEncoding]];

	// text parameter
	[body appendData:[[NSString stringWithFormat:@"--%@\r\n", boundary] dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[[NSString stringWithFormat:@"Content-Disposition: form-data; name=\"status\"\r\n\r\n"] dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[status dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[@"\r\n" dataUsingEncoding:NSUTF8StringEncoding]];

	// close form
	[body appendData:[[NSString stringWithFormat:@"--%@--\r\n", boundary] dataUsingEncoding:NSUTF8StringEncoding]];

	// set request body
	[request setHTTPBody:body];
	[request prepareRequest];

	
	dispatch_async( dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_BACKGROUND, 0 ),
	^{
		NSError *error = nil;
		NSURLResponse *response = nil;
		NSData *responseData = [NSURLConnection sendSynchronousRequest:request returningResponse:&response error:&error];
		NSString *data = [[[NSString alloc] initWithData:responseData encoding:NSUTF8StringEncoding] autorelease];
		
		dispatch_async( dispatch_get_main_queue(),
		^{
			if( error )
			{
				UnitySendMessage( "TwitterManager", "requestFailed", [[error localizedDescription] UTF8String] );
			}
			else if( data && data.length )
			{
				UnitySendMessage( "TwitterManager", "requestSucceeded", data.UTF8String );
			}
			else
			{
				UnitySendMessage( "TwitterManager", "requestFailed", "Unknown error. No data returned and no network error occurred" );
			}
		});
	});
}


- (void)performRequest:(NSString*)methodType path:(NSString*)path params:(NSDictionary*)params
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
	{
		UnitySendMessage( "TwitterManager", "requestFailed", "User is not logged in" );
		return;
	}
	
	OAToken *token = [[OAToken alloc] initWithHTTPResponseBody:tokenString];
	
	if( ![path hasPrefix:@"/"] )
		path = [@"/" stringByAppendingString:path];
	
	NSString *url = [NSString stringWithFormat:@"https://api.twitter.com%@", path];
	P31MutableOauthRequest *request = [[[P31MutableOauthRequest alloc] initWithUrl:url
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:token] autorelease];
	
	[request setHTTPMethod:[methodType uppercaseString]];
	
	// add the parameters (OARequestParameter)
	if( params )
	{
		NSArray *allKeys = [params allKeys];
		NSMutableArray *oaParameters = [NSMutableArray arrayWithCapacity:allKeys.count];
		
		for( NSString *key in allKeys )
		{
			OARequestParameter *p = [[OARequestParameter alloc] initWithName:key value:[params objectForKey:key]];
			[oaParameters addObject:p];
			[p release];
		}
		[request setParameters:oaParameters];
	}
	
	[request prepareRequest];
	
	dispatch_async( dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_BACKGROUND, 0 ),
   ^{
	   NSError *error = nil;
	   NSURLResponse *response = nil;
	   NSData *responseData = [NSURLConnection sendSynchronousRequest:request returningResponse:&response error:&error];
	   NSString *data = [[[NSString alloc] initWithData:responseData encoding:NSUTF8StringEncoding] autorelease];
	   
	   dispatch_async( dispatch_get_main_queue(),
	  ^{
		  if( error )
		  {
			  UnitySendMessage( "TwitterManager", "requestFailed", [[error localizedDescription] UTF8String] );
		  }
		  else if( data && data.length )
		  {
			  UnitySendMessage( "TwitterManager", "requestSucceeded", data.UTF8String );
		  }
		  else
		  {
			  UnitySendMessage( "TwitterManager", "requestFailed", "Unknown error. No data returned and no network error occurred" );
		  }
	  });
   });
}


- (void)showTweetComposerWithMessage:(NSString*)message image:(UIImage*)image link:(NSString*)link
{
	// early out if we cant tweet
	if( ![TwitterManager isTweetSheetSupported] )
		return;
	
	// Create the tweet sheet
	SLComposeViewController *tweetSheet = [[SLComposeViewController alloc] init];
	
	if( !tweetSheet )
		return;
	
	// Add a tweet message
	[tweetSheet performSelector:@selector(setInitialText:) withObject:message];
	
	// add an image
	if( image )
		[tweetSheet performSelector:@selector(addImage:) withObject:image];
	
	// add a link
	if( link )
		[tweetSheet addURL:[NSURL URLWithString:link]];

	// set a blocking handler for the tweet sheet
	tweetSheet.completionHandler = ^( TWTweetComposeViewControllerResult result )
	{
		UnityPause( false );
		[UnityGetGLViewController() dismissViewControllerAnimated:YES completion:nil];
		
		if( result == TWTweetComposeViewControllerResultDone )
			UnitySendMessage( "TwitterManager", "tweetSheetCompleted", "1" );
		else if( result == TWTweetComposeViewControllerResultCancelled )
			UnitySendMessage( "TwitterManager", "tweetSheetCompleted", "0" );
	};
	
	// Show the tweet sheet
	UnityPause( true );
	[UnityGetGLViewController() presentViewController:tweetSheet animated:YES completion:nil];
	
	[tweetSheet release];
}


@end
