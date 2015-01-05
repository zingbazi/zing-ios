using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Prime31;


public class FacebookManager : AbstractManager
{
#if UNITY_ANDROID || UNITY_IPHONE
	// Fired after a successful login attempt was made
	public static event Action sessionOpenedEvent;

	// Fired just before the login succeeded event. For interal use only.
	public static event Action preLoginSucceededEvent;

	// Fired when an error occurs while logging in
	public static event Action<string> loginFailedEvent;

	// Fired when a custom dialog completes with the url passed back from the dialog
	public static event Action<string> dialogCompletedWithUrlEvent;

	// Fired when the post message or custom dialog fails
	public static event Action<string> dialogFailedEvent;

	// Fired when a graph request finishes
	public static event Action<object> graphRequestCompletedEvent;

	// Fired when a graph request fails
	public static event Action<string> graphRequestFailedEvent;

	// iOS only. Fired when a rest request finishes
	public static event Action<object> restRequestCompletedEvent;

	// iOS only. Fired when a rest request fails
	public static event Action<string> restRequestFailedEvent;

	// iOS only. Fired when the Facebook composer completes. True indicates success and false cancel/failure.
	public static event Action<bool> facebookComposerCompletedEvent;

	// Fired when reauthorization succeeds
	public static event Action reauthorizationSucceededEvent;

	// Fired when reauthorization fails
	public static event Action<string> reauthorizationFailedEvent;

	// Fired when the share dialog fails
	public static event Action<string> shareDialogFailedEvent;

	// Fired when the share dialog succeeds
	public static event Action<Dictionary<string,object>> shareDialogSucceededEvent;



	static FacebookManager()
	{
		AbstractManager.initialize( typeof( FacebookManager ) );
	}



	public void sessionOpened( string accessToken )
	{
		preLoginSucceededEvent.fire();
		Facebook.instance.accessToken = accessToken;

		sessionOpenedEvent.fire();
	}


	public void loginFailed( string error )
	{
		loginFailedEvent.fire( error );
	}


	public void dialogCompletedWithUrl( string url )
	{
		dialogCompletedWithUrlEvent.fire( url );
	}


	public void dialogFailedWithError( string error )
	{
		dialogFailedEvent.fire( error );
	}


	public void graphRequestCompleted( string json )
	{
		if( graphRequestCompletedEvent != null )
		{
			object obj = Prime31.Json.jsonDecode( json );
			graphRequestCompletedEvent.fire( obj );
		}
	}


	public void graphRequestFailed( string error )
	{
		graphRequestFailedEvent.fire( error );
	}


	// iOS only
	public void restRequestCompleted( string json )
	{
		if( restRequestCompletedEvent != null )
		{
			var obj = Prime31.Json.jsonDecode( json );
			restRequestCompletedEvent.fire( obj );
		}
	}


	// iOS only
	public void restRequestFailed( string error )
	{
		graphRequestFailedEvent.fire( error );
	}


	// iOS only
	public void facebookComposerCompleted( string result )
	{
		facebookComposerCompletedEvent.fire( result == "1" );
	}


	public void reauthorizationSucceeded( string empty )
	{
		reauthorizationSucceededEvent.fire();
	}


	public void reauthorizationFailed( string error )
	{
		reauthorizationFailedEvent.fire( error );
	}


	// iOS only
	public void shareDialogFailed( string error )
	{
		shareDialogFailedEvent.fire( error );
	}


	// iOS only
	public void shareDialogSucceeded( string json )
	{
		shareDialogSucceededEvent.fire( json.dictionaryFromJson() );
	}

#endif
}