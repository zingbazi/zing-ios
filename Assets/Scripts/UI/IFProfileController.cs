// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFProfileController : MonoBehaviour {
	
	public UISysFontLabel usernameDetailLabel;
	public UISysFontLabel emailDetailLabel;
	public UISysFontLabel ageDetailLabel;
	public UISysFontLabel locationDetailLabel;
	public UISysFontLabel genderDetailLabel;
	public IFToggleSwitch musicSwitch;
	public IFToggleSwitch soundEffectsSwitch;
	
	private UIPanel mPanel;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	
	public static IFProfileController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.ProfileScreenPrefab == null) {
			return IFProfileController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.ProfileScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFProfileController>();
	}
	
	public static IFProfileController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFProfileController>();
	}

	public static IFProfileController Create()
	{
		return Create("Profile Screen");
	}
	
	public UIPanel panel
	{
		get
		{
			if(mPanel == null) {
				mPanel = GetComponentInChildren<UIPanel>();
			}
			return mPanel;
		}
	}
	
	void Awake()
	{
		mPanel = GetComponentInChildren<UIPanel>();
		usernameDetailLabel.Text = PlayerPrefs.GetString(IFConstants.UsernamePrefsKey, "");
		emailDetailLabel.Text = PlayerPrefs.GetString(IFConstants.EmailAddressPrefsKey, "");
		ageDetailLabel.Text = PlayerPrefs.GetString(IFConstants.UserAgeRangePrefsKey, "");
		locationDetailLabel.Text = PlayerPrefs.GetString(IFConstants.UserLocationPrefsKey, "");
		string genderValue = PlayerPrefs.GetString(IFConstants.UserGenderPrefsKey, "");
		if(!string.IsNullOrEmpty(genderValue)) {
			if(genderValue.Equals("f")) {
				genderValue = Localization.Localize("Female");
			} else {
				genderValue = Localization.Localize("Male");
			}
		}
		genderDetailLabel.Text = genderValue;


		string musicPref = PlayerPrefs.GetString(IFConstants.BackgroundMusicOnPreference, "on");
		musicSwitch.SetOn(musicPref.Equals("on"), false);

		string soundPref = PlayerPrefs.GetString(IFConstants.SoundEffectOnPreference, "on");
		soundEffectsSwitch.SetOn(soundPref.Equals("on"), false);
	}

	void MusicSwitchToggled(bool isOn)
	{
		if(isOn) {
			PlayerPrefs.SetString(IFConstants.BackgroundMusicOnPreference, "on");
			IFGameManager.SharedManager.UpdateSoundConfiguration(false);
		} else {
			PlayerPrefs.SetString(IFConstants.BackgroundMusicOnPreference, "off");
			IFGameManager.SharedManager.PauseBackgroundMusic();
		}
	}

	void SoundEffectsSwitchToggled(bool isOn)
	{
		if(isOn) {
			PlayerPrefs.SetString(IFConstants.SoundEffectOnPreference, "on");
		} else {
			PlayerPrefs.SetString(IFConstants.SoundEffectOnPreference, "off");
		}
		IFGameManager.SharedManager.UpdateSoundConfiguration(true);
	}

	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
	
	void EditButtonTapped()
	{
		IFGameManager.SharedManager.TransitionToSettingsScreen(IFGameManager.TransitionDirection.LeftToRight, () => {
			IFGameManager.SharedManager.TransitionToProfileScreen(IFGameManager.TransitionDirection.RightToLeft, () => true);
			return false;	
		});
	}
}
