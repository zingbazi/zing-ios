using UnityEngine;
using System.Collections;

public class IFPasswordResetController : MonoBehaviour {

	public UISysFontLabel emailInputLabel;
	public Transform emailInputBackground;
	public Vector2 textPadding = new Vector2(10f, 20f);

	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;

	private UIPanel mPanel;

	public static IFPasswordResetController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.PasswordResetScreenPrefab == null) {
			return IFPasswordResetController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.PasswordResetScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFPasswordResetController>();
	}

	public static IFPasswordResetController Create(string name)
	{
		GameObject go = new GameObject(name);
		return go.AddComponent<IFPasswordResetController>();
	}

	public static IFPasswordResetController Create()
	{
		return Create("Password Reset Screen");
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
		float screenWidth = Screen.width * UIRoot.GetPixelSizeAdjustment(gameObject);
		Vector3 backgroundScale = emailInputBackground.localScale;
		backgroundScale.x = screenWidth - emailInputBackground.parent.localPosition.x * 2f;
		emailInputBackground.localScale = backgroundScale;
		emailInputLabel.MaxWidthPixels = Mathf.FloorToInt(backgroundScale.x - textPadding.x * 2f);
		emailInputLabel.cachedTransform.localPosition = new Vector3(backgroundScale.x - textPadding.x, -textPadding.y, -1f);
		emailInputLabel.MarkAsChanged();
		TouchScreenKeyboard.hideInput = true;
	}

	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		Close();
	}

	void Close()
	{
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}

	void ResetButtonTapped()
	{
		if(!IFUtils.IsValidEmail(emailInputLabel.Text)) {
			IFAlertViewController.ShowAlert(Localization.Localize("Please enter a valid email address."), Localization.Localize("Invalid Email"));
			return;
		}
		
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		IFUploadManager.SharedManager.SubmitPasswordResetRequest(emailInputLabel.Text, (success) => {
			indicator.Dismiss();
			IFAlertViewController.ShowAlert(Localization.Localize("Instructions on resetting your password have been emailed to you."), 
											Localization.Localize("Password Reset"), 
											Localization.Localize("Done"),
											(controller, okWasSelected) =>
			{
				Close();
			});
		});
	}

}
