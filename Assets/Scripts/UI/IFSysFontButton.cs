// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Data;

[ExecuteInEditMode]
public class IFSysFontButton : MonoBehaviour {
	
	public enum ButtonType { Default, Question, Answer }
	
	public ButtonType buttonType = ButtonType.Default;
	
	public UISysFontLabel fontLabel;
	public UIWidget background;
	private UIButton button;
	public Vector2 textPadding = new Vector2(10f, 10f);
	private Vector2 lastTextPadding = Vector2.zero;
	public Vector2 touchTargetPadding = Vector3.zero;
	private string defaultText;
	private bool needsLayout = false;
	
	private string mText = "";
	public string Text
	{
		get
		{
			return mText;
		}
		set
		{
			if(mText == null || !mText.Equals(value)) {
				mText = value;
				fontLabel.Text = mText;
			}
		}
	}
	
	public bool isEnabled
	{
		get
		{
			return button.isEnabled;
		}
		set
		{
			if(buttonType != ButtonType.Question) {
				button.isEnabled = value;	
			}
		}
	}
	
	private string mActionMethodName = null;
	public string ActionMethodName
	{
		get
		{
			return mActionMethodName;
		}
		set
		{
			if(mActionMethodName == null || !mActionMethodName.Equals(value)) {
				mActionMethodName = value;
				UIButtonMessage buttonMessage = GetComponent<UIButtonMessage>();
				buttonMessage.functionName = mActionMethodName;
			}

		}
	}
	
	private GameObject mActionTarget = null;
	public GameObject ActionTarget
	{
		get
		{
			return mActionTarget;
		}
		set
		{
			if(mActionTarget == null || !Object.ReferenceEquals(mActionTarget, value)) {
				mActionTarget = value;
				UIButtonMessage buttonMessage = GetComponent<UIButtonMessage>();
				buttonMessage.target = mActionTarget;
			}
		}
	}
	
	void Start()
	{
		button = GetComponent<UIButton>();
//		attachedCollider = GetComponent<BoxCollider>();
		UpdateFontLayoutMetricsIfNecessary();
		needsLayout = true;
	}
	
	void OnEnable()
	{
		UpdateFontLayoutMetricsIfNecessary();
		fontLabel.FontLabelTextureDidChange += FontLabelTextureDidChange;
//		needsLayout = true;
	}
	
	void OnDisable()
	{
		fontLabel.FontLabelTextureDidChange -= FontLabelTextureDidChange;
	}
	
	void FontLabelTextureDidChange(UISysFontLabel label)
	{
		needsLayout = true;
	}
	
	void Update()
	{
		UpdateFontLayoutMetricsIfNecessary();
		if(needsLayout) {
			DoLayout();
			needsLayout = false;
		}
	}
	
	void UpdateFontLayoutMetricsIfNecessary()
	{
		if(lastTextPadding != textPadding) {
			Vector3 backgroundScale = background.cachedTransform.localScale;
			fontLabel.MaxWidthPixels = (int)(backgroundScale.x - textPadding.x * 2f);
			lastTextPadding = textPadding;
			needsLayout = true;
		}
	}
	
	void DoLayout()
	{
//		Vector3 scale = background.cachedTransform.localScale;
//		scale.y = Mathf.Ceil(fontLabel.FontTexture.TextHeightPixels + (textPadding.y * 2f));
//		background.cachedTransform.localScale = scale;
//		
//		Vector3 position = background.cachedTransform.localPosition;
//		position.y = Mathf.Ceil(scale.y / -2f);
//		background.cachedTransform.localPosition = position;

		if(buttonType == ButtonType.Question) {
			button.isEnabled = false;
			return;
		}


		BoxCollider buttonCollider = NGUITools.AddWidgetCollider(gameObject);
		Vector3 colliderSize = buttonCollider.size;
		
		colliderSize.x += touchTargetPadding.x * 2f;
		colliderSize.y += touchTargetPadding.y * 2f;
		
		buttonCollider.size = colliderSize;
	}
}
