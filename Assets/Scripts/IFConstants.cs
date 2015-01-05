// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFConstants : Object {
	public static string UsernamePrefsKey = "IFUsername";
	public static string EmailAddressPrefsKey = "IFEmailAddress";
	public static string AccessTokenPrefsKey = "IFAuthToken";
	public static string RemoteUserIdKey = "IFRemoteUserId";
	public static string LastChallengeSync = "IFLastChallengeSync";
	public static string LastHighScoreSync = "IFLastHighScoreSync";
	public static string LastQuestionSync = "IFLastQuestionSync";
	public static string FacebookId = "IFFacebookId";
	public static string UserIsRegistered = "IFUserIsRegistered";
	public static string UserGenderPrefsKey = "IFGender";
	public static string UserAgeRangePrefsKey = "IFAgeRange";
	public static string UserLocationPrefsKey = "IFLocation";
	public static string PlaysUntilNextNag = "IFPlaysUntilNextNag";
	public static string PlaysUntilNextSurvey = "IFPlaysUntilNextSurvey";
	public static string TotalSurveyRequests = "IFTotalSurveyRequests";
	public static string SurveyStatus = "IFSurveyStatus";
	public static string BackgroundMusicOnPreference = "IFBackgroundMusicOnPreference";
	public static string SoundEffectOnPreference = "IFSoundEffectOnPreference";
	
#if UNITY_IPHONE
	public static float spriteDesignWidth = 640f;
#elif UNITY_ANDROID
	public static float spriteDesignWidth = 768f;
#else
	public static float spriteDesignWidth = 0f;
#endif
}
