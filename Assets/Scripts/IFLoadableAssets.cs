// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFLoadableAssets : MonoBehaviour
{
	public GameObject GamePrefab;
	public GameObject HighScoresScreenPrefab;
	public GameObject HomeScreenPrefab;
	public GameObject AccountScreenPrefab;
	public GameObject RegistrationScreenPrefab;
	public GameObject AlertViewPrefab;
	public GameObject MultiplayerSetupScreenPrefab;
	public GameObject ActivityIndicatorPrefab;
	public GameObject MyGamesScreenPrefab;
	public GameObject ChallengeGameCellPrefab;
	public GameObject ReviewScreenPrefab;
	public GameObject ReviewPagePrefab;
	public GameObject ForfeitWindowPrefab;
	public GameObject HintScreenPrefab;
	public GameObject AboutScreenPrefab;
	public GameObject RegisterNagWindowPrefab;
	public GameObject SettingsScreenPrefab;
	public GameObject FeedbackScreenPrefab;
	public GameObject PasswordResetScreenPrefab;
	public GameObject ProfileScreenPrefab;
	public GameObject HelpScreenPrefab;
	
	public IFHomeScreenController cachedHomeScreenController;
	public IFGameController cachedGameController;
	public IFHighScoresController cachedHighScoreController;
	public IFAccountController cachedAccountController;
	public IFRegistrationController cachedRegistrationController;
	public IFAlertViewController cachedAlertViewController;
	public IFMultiplayerSetupController cachedMultiplayerSetupController;
	public IFMyGamesController cachedMyGamesController;
	public IFGameReviewController cachedGameReviewController;
	public IFAboutScreenController cachedAboutScreenController;
	public IFSettingsController cachedSettingsController;
	public IFHintController cachedHintController;
	public IFFeedbackController cachedFeedbackController;
	public IFPasswordResetController cachedPasswordResetController;
	public IFProfileController cachedProfileController;
	public IFHelpScreenController cachedHelpScreenController;
	
	protected static IFLoadableAssets mSharedAssets = null;
	
	public static IFLoadableAssets SharedAssets
	{
		get
		{
			if(mSharedAssets == null) {
				GameObject go = GameObject.Find("_IFLoadableAssets");
				if(go != null) {
					NGUITools.Destroy(go);
				}
				go = new GameObject("_IFLoadableAssets");
				mSharedAssets = go.AddComponent<IFLoadableAssets>();
			}
			return mSharedAssets;
		}
	}
	
	void OnDestroy()
	{
		if(mSharedAssets != null) {
			mSharedAssets = null;
		}
	}
	
	void Awake()
	{
		if(mSharedAssets != null && !Object.ReferenceEquals(mSharedAssets, this)) {
			mSharedAssets.enabled = false;
		}
		mSharedAssets = this;
	}

	private MonoBehaviour DestroyIfInactive(MonoBehaviour controller)
	{
		if(controller != null && !controller.gameObject.activeSelf) {
			NGUITools.Destroy(controller.gameObject);
			return null;
		}
		return controller;
	}
	
	public void ClearCache()
	{
		cachedHomeScreenController = (IFHomeScreenController)DestroyIfInactive(cachedHomeScreenController);
		cachedGameController = (IFGameController)DestroyIfInactive(cachedGameController);
		cachedHighScoreController = (IFHighScoresController)DestroyIfInactive(cachedHighScoreController);
		cachedAccountController = (IFAccountController)DestroyIfInactive(cachedAccountController);
		cachedRegistrationController = (IFRegistrationController)DestroyIfInactive(cachedRegistrationController);
		cachedAlertViewController = (IFAlertViewController)DestroyIfInactive(cachedAlertViewController);
		cachedMultiplayerSetupController = (IFMultiplayerSetupController)DestroyIfInactive(cachedMultiplayerSetupController);
		cachedMyGamesController = (IFMyGamesController)DestroyIfInactive(cachedMyGamesController);
		cachedGameReviewController = (IFGameReviewController)DestroyIfInactive(cachedGameReviewController);
		cachedAboutScreenController = (IFAboutScreenController)DestroyIfInactive(cachedAboutScreenController);
		cachedSettingsController = (IFSettingsController)DestroyIfInactive(cachedSettingsController);
		cachedHintController = (IFHintController)DestroyIfInactive(cachedHintController);
		cachedFeedbackController = (IFFeedbackController)DestroyIfInactive(cachedFeedbackController);
		cachedPasswordResetController = (IFPasswordResetController)DestroyIfInactive(cachedPasswordResetController);
		cachedProfileController = (IFProfileController)DestroyIfInactive(cachedProfileController);
		cachedHelpScreenController = (IFHelpScreenController)DestroyIfInactive(cachedHelpScreenController);
	}
}
