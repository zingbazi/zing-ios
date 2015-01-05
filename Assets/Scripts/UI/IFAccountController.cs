// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Prime31;

public class IFAccountController : MonoBehaviour {
	
	public UISysFontInput emailInput;
	public UISysFontInput passwordInput;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
//	public UISysFontLabel loggedInAsLabel;
	
	private UIPanel mPanel;
	
	public static IFAccountController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.AccountScreenPrefab == null) {
			return IFAccountController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.AccountScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}
		
		return go.GetComponent<IFAccountController>();
	}
	
	public static IFAccountController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFAccountController>();
	}

	public static IFAccountController Create()
	{
		return Create("Account Screen");
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
			mUsername = value;
			PlayerPrefs.SetString(IFConstants.UsernamePrefsKey, value);
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
			mUserid = value;
			PlayerPrefs.SetInt(IFConstants.RemoteUserIdKey, value);
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
			mEmailAddress = value;
			PlayerPrefs.SetString(IFConstants.EmailAddressPrefsKey, value);
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
			mAuthToken = value;
			PlayerPrefs.SetString(IFConstants.AccessTokenPrefsKey, value);
		}
	}

	public IEnumerator Login(Hashtable data)
	{
		data["duid"] = SystemInfo.deviceUniqueIdentifier;

		emailInput.enabled = false;
		passwordInput.enabled = false;
		
		Hashtable headers = new Hashtable();
		headers["Content-Type"] = "application/json";
		headers["Accept"] = "application/json";
		
		string jsonString = MiniJSON.jsonEncode(data);
		
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;
		
		string url;
		if(data.ContainsKey("facebook_token")) {
			url = IFGameManager.SharedManager.remoteURLs.Users;
		} else {
			url = IFGameManager.SharedManager.remoteURLs.SignIn;
		}
		
		WWW web = new WWW(url, Encoding.UTF8.GetBytes(jsonString), headers);
		yield return web;

		indicator.Dismiss();

		emailInput.enabled = true;
		passwordInput.enabled = true;

		if(web.error == null) {
			PlayerPrefs.DeleteAll();
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			if(responseHash != null && responseHash.ContainsKey("error")) {
				string message = responseHash["error"] as string;
				IFAlertViewController.ShowAlert(IFUtils.SafeLocalize(message), Localization.Localize("Please try again."), Localization.Localize("Try Again"));
			} else {
				AuthToken = (string)responseHash["access_token"];
				Username = (string)responseHash["username"];
				Userid = Convert.ToInt32(responseHash["id"]);
				if(data.ContainsKey("email")) {
					EmailAddress = data["email"].ToString();
				} else {
					EmailAddress = emailInput.text;
				}

				IFGameManager.UserIsRegistered = true;

				if(responseHash.ContainsKey("email")) {
					EmailAddress = responseHash["email"] as string;
				}
				PlayerPrefs.SetString(IFConstants.FacebookId, responseHash["facebook_id"] as string);
				PlayerPrefs.SetString(IFConstants.UserGenderPrefsKey, responseHash["gender"] as string);
				PlayerPrefs.SetString(IFConstants.UserLocationPrefsKey, responseHash["location"] as string);
				PlayerPrefs.SetString(IFConstants.UserAgeRangePrefsKey, responseHash["age_range"] as string);

				PlayerPrefs.Save();
				IFAlertViewController.ShowAlert(Localization.Localize("Logged in as:")+ " " + Username, Localization.Localize("Logged in"), Localization.Localize("OK"), (controller, okWasSelected) => {
					IFGameManager.SharedManager.TransitionToHomeScreen();
				});
			}
		} else {
			PlayerPrefs.DeleteAll();
			IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), Localization.Localize("Problem logging in"), Localization.Localize("Try Again"));
			IFActivityIndicator.DismissAll();
		}
	}
	
	void Start()
	{
		mPanel = GetComponentInChildren<UIPanel>();
		emailInput.text = (EmailAddress == null) ? "" : EmailAddress;
		passwordInput.text = "";
		TouchScreenKeyboard.hideInput = true;
	}
	
	void OnDisable()
	{
		IFActivityIndicator.DismissAll();
	}
	
	void OnEnable()
	{
		emailInput.text = (EmailAddress == null) ? "" : EmailAddress;
		passwordInput.text = "";
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
	
	void SubmitTapped()
	{
		if(emailInput.text.Length > 0 && passwordInput.text.Length > 0) {
			Hashtable data = new Hashtable();
			data["email"] = emailInput.text;
			data["password"] = passwordInput.text;
			StartCoroutine(Login(data));
		}
	}
	
	void ForgotPasswordTapped()
	{
		IFGameManager.SharedManager.TransitionToPasswordResetScreen(IFGameManager.TransitionDirection.LeftToRight, () => {
			IFGameManager.SharedManager.TransitionToLoginScreen(IFGameManager.TransitionDirection.RightToLeft, () => true);
			return false;
		});
	}
	
	void OnInputChanged()
	{
		if(emailInput.text.Length > 0 && passwordInput.text.Length > 0) {
			
		}
	}
	
	void DoRegistration()
	{
		IFGameManager.SharedManager.TransitionToRegistrationScreen(IFGameManager.TransitionDirection.Default, () => {
			IFGameManager.SharedManager.TransitionToLoginScreen(IFGameManager.TransitionDirection.RightToLeft, () => true);
			return false;
		});
	}
	
	IEnumerator FacebookUserIdIsRegistered(string facebookId, Action<bool> callback)
	{
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;
		
		WWW web = new WWW(IFGameManager.SharedManager.remoteURLs.Users + "/" + facebookId + "/facebook");
		yield return web;
		
		indicator.Dismiss();
		
		if(web.error == null) {
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			if(responseHash != null && responseHash.ContainsKey("id")) {
				callback(true);
			} else {
				callback(false);
			}
		} else {
			callback(false);
		}
	}
	
	void LoginWithFacebook()
	{
		Dictionary<string, object> parameters = new Dictionary<string, object> {
			{ "fields", "email" },
			{ "access_token", IFFacebookBinding.getAccessToken() }
		};
		
		Facebook.instance.graphRequest("me", HTTPVerb.GET, parameters, (error, result) => {
			Hashtable data = new Hashtable();
			data["facebook_token"] = IFFacebookBinding.getAccessToken();
			
			if(error != null || result == null) {
				StartCoroutine(Login(data));
			}
			
			Hashtable resultHash = result as Hashtable;
			string facebookId = resultHash["id"] as string;

			StartCoroutine(FacebookUserIdIsRegistered(facebookId, (isRegistered) => {
				if(isRegistered) {
					StartCoroutine(Login(data));
				} else {
					IFGameManager.SharedManager.RegisterNewUser(data, (success, regError) => {
						if(success) {
							IFAlertViewController.ShowAlert(Localization.Localize("Successfully registered as:")+" "+PlayerPrefs.GetString(IFConstants.UsernamePrefsKey), Localization.Localize("Success"), Localization.Localize("OK"), (IFAlertViewController av, bool ok) => {
								IFGameManager.SharedManager.TransitionToHomeScreen();
							});
						} else {
							IFAlertViewController.ShowAlert(regError.message, regError.title);
						}					
					});
				}
			}));
		});
	}
	
	void FacebookButtonTapped()
	{
#if UNITY_EDITOR
		LoginWithFacebook();
#else
		Action SetupFacebook = () => {
			if(IFFacebookBinding.isSessionValid()) {
				LoginWithFacebook();
			} else {
				FacebookManager.sessionOpenedEvent += () => {
					LoginWithFacebook();
				};
				string[] permissions = new string[] { "email", "user_relationships" };
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
}
