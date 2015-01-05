// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using Prime31;

public class IFRegistrationController : MonoBehaviour {
	
	public UISysFontInput usernameInput;
	public UISysFontInput emailInput;
	public UISysFontInput passwordInput;
	public UISysFontInput passwordConfirmInput;
	public UISysFontPopupList agePopup;
	public UISysFontInput locationInput;
	public UISysFontPopupList genderPopup;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	
	private UIPanel mPanel;
	
	public static IFRegistrationController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.AccountScreenPrefab == null) {
			return IFRegistrationController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.RegistrationScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFRegistrationController>();
	}
	
	public static IFRegistrationController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFRegistrationController>();
	}

	public static IFRegistrationController Create()
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
				mUsername = PlayerPrefs.GetString(IFConstants.UsernamePrefsKey);
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
				mEmailAddress = PlayerPrefs.GetString(IFConstants.EmailAddressPrefsKey);
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

	void RegisterTapped()
	{
		if(string.IsNullOrEmpty(passwordInput.text) || string.IsNullOrEmpty(passwordConfirmInput.text) || !passwordInput.text.Equals(passwordConfirmInput.text)) {
			IFAlertViewController.ShowAlert(Localization.Localize("Please re-type your password."), Localization.Localize("Passwords Do Not Match"));
			return;
		} else if(string.IsNullOrEmpty(emailInput.text) || !IFUtils.IsValidEmail(emailInput.text)) {
			IFAlertViewController.ShowAlert(Localization.Localize("Please enter a valid email address."), Localization.Localize("Invalid Email Address"));
			return;
		}
		if(emailInput.text.Length > 0 && passwordInput.text.Length > 0) {
			Hashtable data = new Hashtable();
			data["username"] = usernameInput.text;
			data["email"] = emailInput.text;
			data["password"] = passwordInput.text;
			data["age_range"] = agePopup.selection;
			if(!string.IsNullOrEmpty(locationInput.text)) {
				data["location"] = locationInput.text;
			}
			if(!genderPopup.selection.Equals("No Selection")) {
				data["gender"] = genderPopup.selection.ToLower()[0].ToString();	
			}
			
			IFGameManager.SharedManager.RegisterNewUser(data, (success, error) => {
				if(success) {
					IFAlertViewController.ShowAlert(Localization.Localize("Successfully registered as:")+" "+data["username"], Localization.Localize("Success"), Localization.Localize("OK"), (IFAlertViewController av, bool ok) => {
						IFGameManager.SharedManager.TransitionToHomeScreen();
					});
				} else {
					IFAlertViewController.ShowAlert(error.message, error.title);
				}
			});
		}
		
	}
	
	void RegisterWithFacebook()
	{
		Hashtable data = new Hashtable();
		data["facebook_token"] = IFFacebookBinding.getAccessToken();
		
		IFGameManager.SharedManager.RegisterNewUser(data, (success, error) => {
			if(success) {
				IFAlertViewController.ShowAlert(Localization.Localize("Successfully registered as:")+" "+PlayerPrefs.GetString(IFConstants.UsernamePrefsKey), Localization.Localize("Success"), Localization.Localize("OK"), (IFAlertViewController av, bool ok) => {
					IFGameManager.SharedManager.TransitionToHomeScreen();
				});
			} else {
				IFAlertViewController.ShowAlert(error.message, error.title);
			}
		});
		
	}
	
	void FacebookButtonTapped()
	{
#if UNITY_EDITOR
		RegisterWithFacebook();
#else
		Action SetupFacebook = () => {
			if(IFFacebookBinding.isSessionValid()) {
				RegisterWithFacebook();
			} else {
				FacebookManager.sessionOpenedEvent += () => {
					RegisterWithFacebook();
				};

				string[] permissions = new string[] { "email" };
				IFFacebookBinding.loginWithReadPermissions(permissions);
			}
		};

		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;

		if(!IFFacebookBinding.IsReady) {
			IFFacebookBinding.init((success, error) => {
				indicator.Dismiss();
				if(success) {
					SetupFacebook();
				} else {
					IFAlertViewController.ShowAlert(error.message, error.title);
				}
			});
		} else {
			indicator.Dismiss();
			IFFacebookBinding.CheckReachability((success, error) => {
				indicator.Dismiss();
				if(success) {
					SetupFacebook();
				} else {
					IFAlertViewController.ShowAlert(error.message, error.title);
				}
			});
		}
#endif
	}
	
	void OnInputChanged()
	{
		if(emailInput.text.Length > 0 && passwordInput.text.Length > 0) {
			
		}
	}

}
