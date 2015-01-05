// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class IFHintController : MonoBehaviour {
	
	public int topMargin = 100;
	public int bottomMargin = 100;
	public UISysFontLabel titleLabel;
	public GameObject hintButton;
	public UISysFontLabel hintButtonLabel;
	public TextAsset template;
	public TextAsset androidTemplate;
	public string fontFileName;
	
	public Action HintWasDismissed;

	private bool needsUpdate = false;
	
	private UIPanel mPanel;
	
	public static IFHintController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.HintScreenPrefab == null) {
			return IFHintController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.HintScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFHintController>();
	}
	
	public static IFHintController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFHintController>();
	}

	public static IFHintController Create()
	{
		return Create("Hint Screen");
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
	
	public string Title
	{
		get
		{
			return titleLabel.Text;
		}
		set
		{
			if(!titleLabel.Text.Equals(value)) {
				titleLabel.Text = value;
			}
		}
	}

	public string ButtonTitle
	{
		get
		{
			return hintButtonLabel.Text;
		}
		set
		{
			if(!hintButtonLabel.Text.Equals(value)) {
				hintButtonLabel.Text = value;
			}
		}
	}
	
	private string mQuestionText;
	public string QuestionText
	{
		get
		{
			return mQuestionText;
		}
		set
		{
			mQuestionText = value;
			needsUpdate = true;
		}
	}
	
	private string mHintText;
	public string HintText
	{
		get
		{
			return mHintText;
		}
		set
		{
			mHintText = value;
			needsUpdate = true;
		}
	}
	
	string FontFileURL
	{
		get
		{
#if UNITY_EDITOR
			return null;		
#elif UNITY_IPHONE
			return "file://" + Application.dataPath + "/Raw/" + fontFileName;
#elif UNITY_ANDROID
			return "file:///android_asset/" + fontFileName;
#endif
		}
	}
	
	public static IFHintController PresentControllerWithQuestionAndHintText(string questionText, string hintText, Action completion)
	{
		IFHintController hintController = IFGameManager.LoadableAssets.cachedHintController;
		if(hintController == null) {
			hintController = IFHintController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedHintController = hintController;
		}
		
		hintController.gameObject.SetActive(true);
		
		Transform hintTransform = hintController.transform;
		hintTransform.parent = IFGameManager.SharedManager.mainPanel.cachedTransform;
		hintTransform.localScale = Vector3.one;
		hintTransform.localPosition = Vector3.zero;
		hintTransform.localRotation = Quaternion.identity;
		
//		Bounds hintBounds = NGUIMath.CalculateRelativeWidgetBounds(hintTransform);
//		
//		TweenPosition tp = TweenPosition.Begin(hintController.gameObject, .4f, new Vector3(0f, 0f, -100f));
//		tp.from = new Vector3(0f, hintBounds.max.y * -2f, -100f);
//		tp.method = UITweener.Method.EaseInOut;
//		tp.onFinished = (tween) => {
//			hintController.QuestionText = questionText;
//			hintController.HintText = hintText;
//		};

		hintController.HintWasDismissed = completion;
		hintController.QuestionText = questionText;
		hintController.HintText = hintText;
		return hintController;
	}
	
	string RenderHTMLFile()
	{
#if UNITY_IPHONE
		StringBuilder templateBuilder = new StringBuilder(template.text);
#elif UNITY_ANDROID
		StringBuilder templateBuilder = new StringBuilder(androidTemplate.text);
#endif
		templateBuilder.Replace("%title%", Localization.Localize("Hint"));
		templateBuilder.Replace("%top_header%", QuestionText);
		templateBuilder.Replace("%text_explaination%", HintText);
		templateBuilder.Replace("%font_url%", FontFileURL);
		string filePath = Application.temporaryCachePath + "/current_hint.html";
		File.WriteAllText(filePath, templateBuilder.ToString(), Encoding.UTF8);
		return filePath;
	}
	
	void Start()
	{
		mPanel = GetComponentInChildren<UIPanel>();
		UIEventListener.Get(hintButton).onClick += ResumeButtonTapped;
		NGUITools.AddWidgetCollider(hintButton);
	}
	
	void Update()
	{
		if(needsUpdate) {
			string renderedPath = RenderHTMLFile();
			IFUtils.ShowNativeWebViewWithPath(renderedPath, () => {
				if(HintWasDismissed != null) {
					HintWasDismissed();
				}
				if(gameObject != null) {
					gameObject.SetActive(false);
				}
			});
			needsUpdate = false;
		}
	}
	
	public void ResumeButtonTapped(GameObject sender)
	{
		Bounds hintBounds = NGUIMath.CalculateRelativeWidgetBounds(transform);
		
		TweenPosition tp = TweenPosition.Begin(gameObject, .4f, new Vector3(0f, hintBounds.max.y * -2f, 10f));
		tp.method = UITweener.Method.EaseInOut;
		tp.onFinished = (tween) => {
			if(HintWasDismissed != null) {
				HintWasDismissed();
			}
			NGUITools.Destroy(gameObject);
		};

	}
	
}
