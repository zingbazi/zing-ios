// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IFFacebookBinding
{
	public static bool IsReady { get; private set; }

	public static bool IsReachable { get; private set; }

	public static void CheckReachability(Action<bool, IFError> callback)
	{
		IFUploadManager.CanReachDomainAsync("facebook.com", (domainSuccess, domainError) => {
			if(domainSuccess) {
				string testUrl = "https://graph.facebook.com/cocacola";
				IFUploadManager.CanReachURLAsync(testUrl, (urlSuccess, urlError) => {
					IsReachable = urlSuccess;
					callback(urlSuccess, new IFError(urlError.title, Localization.Localize("facebook.com is not available. Please check your internet connection and try again.")));
				});
			} else {
				callback(domainSuccess, domainError);
			}
		});
	}

	public static void init(Action<bool, IFError> callback = null)
	{
		if(!IsReady) {
			FacebookManager.loginFailedEvent += FacebookLoginFailed;
			if(!IsReachable) {
				CheckReachability((success, error) => {
					if(success) {
						CommonInit();
					}
					IsReady = success;
					if(callback != null) callback(success, error);
				});
			} else {
				CommonInit();
				IsReady = true;
				if(callback != null) callback(true, IFError.Null);
			}
		}
	}

	static void CommonInit()
	{
#if UNITY_IPHONE
		FacebookBinding.init();
		FacebookBinding.renewCredentialsForAllFacebookAccounts();
#elif UNITY_ANDROID
		FacebookAndroid.init(false);
#endif
	}
	
	public static bool isSessionValid()
	{
		if(!IsReady && !IsReachable) {
			return false;
		}
#if UNITY_IPHONE
		return FacebookBinding.isSessionValid();
#elif UNITY_ANDROID
		return FacebookAndroid.isSessionValid();
#endif
	}

	public static string getAccessToken()
	{
		if(!IsReady && !IsReachable) {
			return null;
		}

#if UNITY_EDITOR
		return "CAABZBHYke9T0BAHpi8gYcTJuZBhIrKhEysaZAwIXItIHa9SFparjEvo6dAecmm1YMjsLNYvBptfyyZBnD0nnekZA0hMkOjZCk8ZBdRoSkU2NBgfZA9anjnP0vXzPczzQLyhcUEq0rCYXUUvlrOd5lKS49FaFW2CZCcoW0RtFvZAZC5ZCZACgNw8sM8X1gWJ7ngV62d7mARHPVltupEdVqSnehXoZBttlZBCgLZBIaZBwZD";
#elif UNITY_IPHONE
		return FacebookBinding.getAccessToken();
#elif UNITY_ANDROID
		return FacebookAndroid.getAccessToken();
#endif
	}

	static void FacebookLoginFailed(string error)
	{
		IFActivityIndicator.DismissAll();
		if(error.Contains("com.facebook.sdk error 2")) {
			IFAlertViewController.ShowAlert(Localization.Localize("Please ensure that the Zing! switch is ON in Settings -> Facebook."), Localization.Localize("Facebook Login Permission Error"));
		} else {
			IFAlertViewController.ShowAlert(error, Localization.Localize("Facebook Login Error"));
		}
	}
	
	public static void loginWithReadPermissions(string[] permissions)
	{
		if(!IsReady && !IsReachable) {
			return;
		}

#if UNITY_IPHONE
		FacebookBinding.loginWithReadPermissions(permissions);
#elif UNITY_ANDROID
		FacebookAndroid.loginWithReadPermissions(permissions);
#endif
	}
	
	public static List<object> getSessionPermissions()
	{
		if(!IsReady && !IsReachable) {
			return null;
		}

#if UNITY_IPHONE
		return FacebookBinding.getSessionPermissions();
#elif UNITY_ANDROID
		return FacebookAndroid.getSessionPermissions();
#endif
	}
	
	public static void reauthorizeWithPublishPermissions(string[] permissions)
	{
		if(!IsReady && !IsReachable) {
			return;
		}
#if UNITY_IPHONE
		FacebookBinding.reauthorizeWithPublishPermissions(permissions, FacebookSessionDefaultAudience.Friends);
#elif UNITY_ANDROID
		FacebookAndroid.reauthorizeWithPublishPermissions(permissions, FacebookSessionDefaultAudience.Friends);
#endif
	}
}

