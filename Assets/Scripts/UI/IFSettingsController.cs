// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using Prime31;

public class IFSettingsController : MonoBehaviour {
	
	public UISysFontInput usernameInput;
	public UISysFontInput emailInput;
	public UISysFontInput passwordInput;
	public UISysFontInput passwordConfirmInput;
	public UISysFontPopupList agePopup;
	public UISysFontInput locationInput;
	public UISysFontPopupList genderPopup;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	
	private UIPanel mPanel;
	
	public static IFSettingsController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.SettingsScreenPrefab == null) {
			return IFSettingsController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.SettingsScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFSettingsController>();
	}
	
	public static IFSettingsController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFSettingsController>();
	}

	public static IFSettingsController Create()
	{
		return Create("Registration Screen");
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
	
	void Start()
	{
		mPanel = GetComponentInChildren<UIPanel>();
		TouchScreenKeyboard.hideInput = true;
	}

	void OnDisable()
	{
		IFActivityIndicator.DismissAll();
	}
	
	void OnEnable()
	{
		usernameInput.text = Username;
		emailInput.text = EmailAddress;
		locationInput.text = PlayerPrefs.GetString(IFConstants.UserLocationPrefsKey);
		
		string ageRange = PlayerPrefs.GetString(IFConstants.UserAgeRangePrefsKey);
		if(!string.IsNullOrEmpty(ageRange)) {
			agePopup.selection = ageRange;
		}

		string gender = PlayerPrefs.GetString(IFConstants.UserGenderPrefsKey);
		if(!string.IsNullOrEmpty(gender)) {
			if(gender.Equals("m")) {
				genderPopup.selection = "Male";
			} else if(gender.Equals("f")){
				genderPopup.selection = "Female";
			} else {
				genderPopup.selection = "No Selection";
			}
		}
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
	
	private string mUsername;
	private string Username
	{
		get
		{
			if(mUsername == null) {
				mUsername = PlayerPrefs.GetString(IFConstants.UsernamePrefsKey, "");
			}
			return mUsername;
		}
		set
		{
			if(mUsername == null || !mUsername.Equals(value)) {
				mUsername = value;
				PlayerPrefs.SetString(IFConstants.UsernamePrefsKey, value);
			}
		}
	}

	private int mUserid = -1;
	private int Userid
	{
		get
		{
			if(mUserid < 0) {
				mUserid = PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey);
			}
			return mUserid;
		}
		set
		{
			if(mUserid != value) {
				mUserid = value;
				PlayerPrefs.SetInt(IFConstants.RemoteUserIdKey, value);
			}
		}
	}

	private string mEmailAddress;
	private string EmailAddress
	{
		get
		{
			if(mEmailAddress == null) {
				mEmailAddress = PlayerPrefs.GetString(IFConstants.EmailAddressPrefsKey, "");
			}
			return mEmailAddress;
		}
		set
		{
			if(mEmailAddress == null || !mEmailAddress.Equals(value)) {
				mEmailAddress = value;
				PlayerPrefs.SetString(IFConstants.EmailAddressPrefsKey, value);
			}
		}
	}
	
	private string mAuthToken;
	private string AuthToken
	{
		get
		{
			if(mAuthToken == null) {
				mAuthToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
			}
			return mAuthToken;
		}
		set
		{
			if(mAuthToken == null || !mAuthToken.Equals(value)) {
				mAuthToken = value;
				PlayerPrefs.SetString(IFConstants.AccessTokenPrefsKey, value);
			}
		}
	}
	
//	IEnumerator FetchUserDetails()
//	{
//		yield return null;
//	}

	void UpdateButtonTapped()
	{
		if(!passwordInput.text.Equals(passwordConfirmInput.text)) {
			IFAlertViewController.ShowAlert(Localization.Localize("Please re-type your password."), Localization.Localize("Passwords Do Not Match"));
			return;
		}
		
		Hashtable data = new Hashtable();
		if(!usernameInput.text.Equals(Username)) {
			data["username"] = usernameInput.text;	
		}
		
		data["email"] = emailInput.text;
		data["age_range"] = agePopup.selection;
		data["location"] = locationInput.text;
		
		if(!genderPopup.selection.Equals("No Selection")) {
			data["gender"] = genderPopup.selection.ToLower()[0].ToString();	
		}
		
		
		if(passwordInput.text.Length > 0) {
			data["password"] = passwordInput.text;
		}

		IFGameManager.SharedManager.UpdateSettingsForUser(data, (success, errorMessage) => {
			if(success) {
				IFAlertViewController.ShowAlert(Localization.Localize("Settings updated."), Localization.Localize("Success"), Localization.Localize("OK"), (IFAlertViewController av, bool ok) => {
					IFGameManager.SharedManager.TransitionToHomeScreen();
				});
			} else {
				IFAlertViewController.ShowAlert(errorMessage.message, errorMessage.title);
			}
		});
	}

	void OnInputChanged()
	{
		if(emailInput.text.Length > 0 && passwordInput.text.Length > 0) {
			
		}
	}
}
