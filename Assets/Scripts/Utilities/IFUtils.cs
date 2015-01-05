// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

public struct IFError
{
	public string title;
	public string message;
	
	public IFError(string errorTitle, string errorMessage)
	{
		title = errorTitle;
		message = errorMessage;
	}
	
	public static IFError Null
	{
		get
		{
			return new IFError(null, null);
		}
	}
	
	public static bool IsNull(IFError error)
	{
		return error.title == null && error.message == null;
	}
}


public static class IFUtils {

	public static string SafeLocalize(string key)
	{
		string result = Localization.Localize(key);
		if(result.Equals(key)) {
#if UNITY_EDITOR
			Debug.Log("Falling back on the safe localized error message.");
#endif
			return Localization.Localize("Error");
		}
		return result;
	}
	
	private static DateTime mStartupTime = DateTime.MinValue;
	public static DateTime TimeAtStartup
	{
		get
		{
			if(mStartupTime == DateTime.MinValue) {
				TimeSpan since = new TimeSpan(0, 0, 0, Convert.ToInt32(Time.realtimeSinceStartup));
				mStartupTime = DateTime.UtcNow.Subtract(since);	
			}
			return mStartupTime;
		}
	}
	
	public static bool IsValidEmail(string strIn)
	{
		if (String.IsNullOrEmpty(strIn)) return false;

		// Use IdnMapping class to convert Unicode domain names.
		string asciiString = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper, RegexOptions.None);

		if (asciiString == null) return false;

		return Regex.IsMatch(asciiString, 
			@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + 
			@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", 
			RegexOptions.IgnoreCase);
	}

	private static string DomainMapper(Match match)
	{
		// IdnMapping class with default property values.
		IdnMapping idn = new IdnMapping();

		string domainName = match.Groups[2].Value;

		return match.Groups[1].Value + idn.GetAscii(domainName);
	}
	
	public static string ResponseHeadersToString(this WWW webRequest)
	{
		StringBuilder sb = new StringBuilder();
		foreach(KeyValuePair<string, string> kv in webRequest.responseHeaders) {
			sb.Append(kv.Key+": "+kv.Value+"\n");
		}
		return sb.ToString();
	}
	
	private static System.Random mRandom;
	public static int GetRandomIntInRange(int minValue, int maxValue)
	{
		if(mRandom == null) {
			mRandom = new System.Random();
		}
		return mRandom.Next(minValue, maxValue);
	}
	
	public static void SetEnabledAllCollidersInChildren(GameObject go, bool isEnabled)
	{
		foreach(BoxCollider collider in go.GetComponentsInChildren<BoxCollider>()) {
			collider.enabled = isEnabled;
		}
	}
	
	#region Native Web Browser (P31)
	
	public static void ShowNativeWebViewWithPath(string url, Action viewDismissCallback)
	{
#if UNITY_EDITOR
		IFAlertViewController.ShowAlert("No native browser in the Unity Editor", "OOPS", "OK", (controller, okWasSelected) => {
			if(viewDismissCallback != null) {
				viewDismissCallback();
			}
		});
#elif UNITY_IPHONE
		EtceteraManager.dismissingViewControllerEvent += viewDismissCallback;
		EtceteraBinding.showWebPage(url, true);
#elif UNITY_ANDROID
		EtceteraAndroidManager.webViewCancelledEvent += viewDismissCallback;
		EtceteraAndroid.showWebView("file://"+url);
#endif
	}

	public static void ShowNativeWebViewWithURL(string url, Action viewDismissCallback)
	{
#if UNITY_EDITOR
		IFAlertViewController.ShowAlert("No native browser in the Unity Editor", "OOPS", "OK", (controller, okWasSelected) => {
			if(viewDismissCallback != null) {
				viewDismissCallback();
			}
		});
#elif UNITY_IPHONE
		EtceteraManager.dismissingViewControllerEvent += viewDismissCallback;
		EtceteraBinding.showWebPage(url, true);
#elif UNITY_ANDROID
		EtceteraAndroidManager.webViewCancelledEvent += viewDismissCallback;
		EtceteraAndroid.showWebView(url);
#endif
	}

	#endregion
	
	#region Debug stuff
	
	public static void DumpPlayerPrefs()
	{
		StringBuilder sb = new StringBuilder();
		
		if(PlayerPrefs.HasKey(IFConstants.UsernamePrefsKey))
			sb.AppendLine(IFConstants.UsernamePrefsKey + " = " + PlayerPrefs.GetString(IFConstants.UsernamePrefsKey));
		
		if(PlayerPrefs.HasKey(IFConstants.EmailAddressPrefsKey))
			sb.AppendLine(IFConstants.EmailAddressPrefsKey + " = " + PlayerPrefs.GetString(IFConstants.EmailAddressPrefsKey));
		
		if(PlayerPrefs.HasKey(IFConstants.RemoteUserIdKey))
			sb.AppendLine(IFConstants.RemoteUserIdKey + " = " + PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey));
		
		if(PlayerPrefs.HasKey(IFConstants.AccessTokenPrefsKey))
			sb.AppendLine(IFConstants.AccessTokenPrefsKey + " = " + PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey));
		
		if(PlayerPrefs.HasKey(IFConstants.FacebookId))
			sb.AppendLine(IFConstants.FacebookId + " = " + PlayerPrefs.GetString(IFConstants.FacebookId));
		
		if(PlayerPrefs.HasKey(IFConstants.UserIsRegistered))
			sb.AppendLine(IFConstants.UserIsRegistered + " = " + PlayerPrefs.GetString(IFConstants.UserIsRegistered));

		if(PlayerPrefs.HasKey(IFConstants.UserAgeRangePrefsKey))
			sb.AppendLine(IFConstants.UserAgeRangePrefsKey + " = " + PlayerPrefs.GetString(IFConstants.UserAgeRangePrefsKey));

		if(PlayerPrefs.HasKey(IFConstants.UserLocationPrefsKey))
			sb.AppendLine(IFConstants.UserLocationPrefsKey + " = " + PlayerPrefs.GetString(IFConstants.UserLocationPrefsKey));
		
		if(PlayerPrefs.HasKey(IFConstants.UserGenderPrefsKey))
			sb.AppendLine(IFConstants.UserGenderPrefsKey + " = " + PlayerPrefs.GetString(IFConstants.UserGenderPrefsKey));

		if(PlayerPrefs.HasKey(IFConstants.LastChallengeSync))
			sb.AppendLine(IFConstants.LastChallengeSync + " = " + PlayerPrefs.GetString(IFConstants.LastChallengeSync));
		
		if(PlayerPrefs.HasKey(IFConstants.LastHighScoreSync))
			sb.AppendLine(IFConstants.LastHighScoreSync + " = " + PlayerPrefs.GetString(IFConstants.LastHighScoreSync));

		if(PlayerPrefs.HasKey(IFConstants.SurveyStatus))
			sb.AppendLine(IFConstants.SurveyStatus + " = " + PlayerPrefs.GetString(IFConstants.SurveyStatus));

		if(PlayerPrefs.HasKey(IFConstants.PlaysUntilNextSurvey))
			sb.AppendLine(IFConstants.PlaysUntilNextSurvey + " = " + PlayerPrefs.GetInt(IFConstants.PlaysUntilNextSurvey));

		if(PlayerPrefs.HasKey(IFConstants.TotalSurveyRequests))
			sb.AppendLine(IFConstants.TotalSurveyRequests + " = " + PlayerPrefs.GetInt(IFConstants.TotalSurveyRequests));

		if(PlayerPrefs.HasKey(IFConstants.LastQuestionSync))
			sb.AppendLine(IFConstants.LastQuestionSync + " = " + PlayerPrefs.GetString(IFConstants.LastQuestionSync));

		if(PlayerPrefs.HasKey(IFConstants.BackgroundMusicOnPreference))
			sb.AppendLine(IFConstants.BackgroundMusicOnPreference + " = " + PlayerPrefs.GetString(IFConstants.BackgroundMusicOnPreference));

		if(PlayerPrefs.HasKey(IFConstants.SoundEffectOnPreference))
			sb.AppendLine(IFConstants.SoundEffectOnPreference + " = " + PlayerPrefs.GetString(IFConstants.SoundEffectOnPreference));

		Debug.Log(sb.ToString());
	}
	
	#endregion
}

