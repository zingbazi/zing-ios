using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_IPHONE || UNITY_ANDROID

#if UNITY_IPHONE
using FB = FacebookBinding;
#else
using FB = FacebookAndroid;
#endif


public static class FacebookCombo
{
	public static void init()
	{
		FB.init();
	}

	
	// Authenticates the user
	public static void login()
	{
		loginWithReadPermissions( new string[] {} );
	}
	
	
	// Authenticates the user for the provided permissions
	public static void loginWithReadPermissions( string[] permissions )
	{
		FB.loginWithReadPermissions( permissions );
	}
	
	
	// Reauthorizes with the requested read permissions
	public static void reauthorizeWithReadPermissions( string[] permissions )
	{
		FB.reauthorizeWithReadPermissions( permissions );
	}
	
	
	// Reauthorizes with the requested publish permissions and audience
	public static void reauthorizeWithPublishPermissions( string[] permissions, FacebookSessionDefaultAudience defaultAudience )
	{
		FB.reauthorizeWithPublishPermissions( permissions, defaultAudience );
	}
	
	
	// Checks to see if the current session is valid
	public static bool isSessionValid()
	{
		return FB.isSessionValid();
	}
	
	
	// Gets the current access token
	public static string getAccessToken()
	{
		return FB.getAccessToken();
	}
	
	
	// Gets the permissions granted to the current access token
	public static List<object> getSessionPermissions()
	{
		return FB.getSessionPermissions();
	}
	
	
	// Logs the user out and invalidates the token
	public static void logout()
	{
		FB.logout();
	}
	
	
	// Full access to any existing or new Facebook dialogs that get added. See Facebooks documentation for parameters and dialog types
	public static void showDialog( string dialogType, Dictionary<string,string> options )
	{
		FB.showDialog( dialogType, options );
	}

}
#endif