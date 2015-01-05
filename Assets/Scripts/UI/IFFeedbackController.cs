using UnityEngine;
using System.Collections;

public class IFFeedbackController : MonoBehaviour {

	public UISysFontLabel feedbackInputLabel;
	public Transform feedbackInputBackground;
	public float textPadding = 10f;
	public UIPanel scrollingPanel;

	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;

	private UIPanel mPanel;

	public static IFFeedbackController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.FeedbackScreenPrefab == null) {
			return IFFeedbackController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.FeedbackScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFFeedbackController>();
	}

	public static IFFeedbackController Create(string name)
	{
		GameObject go = new GameObject(name);
		return go.AddComponent<IFFeedbackController>();
	}

	public static IFFeedbackController Create()
	{
		return Create("Feedback Screen");
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
		Vector3 backgroundScale = feedbackInputBackground.localScale;
		backgroundScale.x = screenWidth - feedbackInputBackground.parent.localPosition.x * 2f;
		feedbackInputBackground.localScale = backgroundScale;
		feedbackInputLabel.MaxWidthPixels = Mathf.FloorToInt(backgroundScale.x - textPadding * 2f);
		feedbackInputLabel.cachedTransform.localPosition = new Vector3(backgroundScale.x / 2f - textPadding, backgroundScale.y / 2f - textPadding, -1f);
		feedbackInputLabel.pivot = UIWidget.Pivot.TopRight;
		feedbackInputLabel.MarkAsChanged();
		Vector4 clipRange = scrollingPanel.clipRange;
		clipRange.z = backgroundScale.x;
		clipRange.w = backgroundScale.y;
		scrollingPanel.clipRange = clipRange;
		Vector3 panelPos = new Vector3(backgroundScale.x / 2f, -backgroundScale.y / 2f, 0f);
		scrollingPanel.cachedTransform.localPosition = panelPos;
		TouchScreenKeyboard.hideInput = true;
	}

	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
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

	void SubmitButtonTapped()
	{
		if(string.IsNullOrEmpty(feedbackInputLabel.Text)) {
			IFAlertViewController.ShowAlert(Localization.Localize("Please type your feedback in the provided form."), Localization.Localize("Empty Feedback"));
			return;
		}
		IFGameManager.SharedManager.QueueFeedbackComment(feedbackInputLabel.Text);
		IFAlertViewController.ShowAlert(Localization.Localize("Your message will be sent to the developer."), Localization.Localize("Thank You"), Localization.Localize("Done"), (alert, ok) => {
			Close();
		});
	}
}
