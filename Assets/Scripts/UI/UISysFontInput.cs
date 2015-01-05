// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;

[AddComponentMenu("NGUI/UI/Input (Sys Font)")]
public class UISysFontInput : MonoBehaviour
{
	
	public delegate char Validator (string currentText, char nextChar);

	public enum KeyboardType
	{
		Default = 0,
		ASCIICapable = 1,
		NumbersAndPunctuation = 2,
		URL = 3,
		NumberPad = 4,
		PhonePad = 5,
		NamePhonePad = 6,
		EmailAddress = 7,
	}

	public delegate void OnSubmit (string inputString);

	/// <summary>
	/// Current input, available inside OnSubmit callbacks.
	/// </summary>

	static public UISysFontInput current;

	/// <summary>
	/// Text label modified by this input.
	/// </summary>

	public UISysFontLabel label;
	
	public UISprite background;
	
	/// <summary>
	/// Maximum number of characters allowed before input no longer works.
	/// </summary>

	public int maxChars = 0;

	/// <summary>
	/// Visual carat character appended to the end of the text when typing.
	/// </summary>

	public string caratChar = "|";

	/// <summary>
	/// Delegate used for validation.
	/// </summary>

	public Validator validator;

	/// <summary>
	/// Type of the touch screen keyboard used on iOS and Android devices.
	/// </summary>

	public KeyboardType type = KeyboardType.Default;
	
	public float labelLeadingPadding = 10f;

	/// <summary>
	/// Whether this input field should hide its text.
	/// </summary>

	public bool isPassword = false;

	/// <summary>
	/// Whether to use auto-correction on mobile devices.
	/// </summary>

	public bool autoCorrect = false;

	/// <summary>
	/// Whether the label's text value will be used as the input's text value on start.
	/// By default the label is just a tooltip of sorts, letting you choose helpful
	/// half-transparent text such as "Press Enter to start typing", while the actual
	/// value of the input field will remain empty.
	/// </summary>

	public bool useLabelTextAtStart = false;

	/// <summary>
	/// Color of the label when the input field has focus.
	/// </summary>

	public Color activeColor = Color.white;

	/// <summary>
	/// Event receiver that will be notified when the input field submits its data (enter gets pressed).
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Function that will be called on the event receiver when the input field submits its data.
	/// </summary>

	public string functionName = "OnSubmit";

	/// <summary>
	/// Delegate that will be notified when the input field submits its data (by default that's when Enter gets pressed).
	/// </summary>

	public OnSubmit onSubmit;

	string mText = "";
	string mDefaultText = "";
	Color mDefaultColor = Color.white;
	UIWidget.Pivot mPivot = UIWidget.Pivot.Left;
	float mPosition = 0f;
	private UIDraggablePanel draggablePanel;

#if UNITY_IPHONE || UNITY_ANDROID
#if UNITY_3_4
	iPhoneKeyboard mKeyboard;
#else
	TouchScreenKeyboard mKeyboard;
#endif
#else
	string mLastIME = "";
#endif
	
	void Start()
	{
		draggablePanel = GetComponentInChildren<UIDraggablePanel>();
	}
	
	/// <summary>
	/// Input field's current text value.
	/// </summary>

	public virtual string text
	{
		get
		{
			if (mDoInit) Init();
			return mText;
		}
		set
		{
			if (mDoInit) Init();
			mText = value;

			if (label != null)
			{
				if (string.IsNullOrEmpty(value)) value = mDefaultText;

//				label.supportEncoding = false;
				if(selected) {
					label.Text = value + caratChar;
				} else {
					label.Text = value;
				}
				label.showLastPasswordChar = selected;
				label.color = (selected || value != mDefaultText) ? activeColor : mDefaultColor;
			}
		}
	}

	/// <summary>
	/// Whether the input is currently selected.
	/// </summary>

	public bool selected
	{
		get
		{
			return UICamera.selectedObject == gameObject;
		}
		set
		{
			if (!value && UICamera.selectedObject == gameObject) UICamera.selectedObject = null;
			else if (value) UICamera.selectedObject = gameObject;
		}
	}

	/// <summary>
	/// Set the default text of an input.
	/// </summary>

	public string defaultText
	{
		get
		{
			return mDefaultText;
		}
		set
		{
			if (label.Text == mDefaultText) label.Text = value;
			mDefaultText = value;
		}
	}

	/// <summary>
	/// Labels used for input shouldn't support color encoding.
	/// </summary>

	protected void Init ()
	{
		if (mDoInit)
		{
			mDoInit = false;
			if (label == null) label = GetComponentInChildren<UISysFontLabel>();

			if (label != null)
			{
				if (useLabelTextAtStart) mText = label.Text;
				mDefaultText = label.Text;
				mDefaultColor = label.color;
				label.supportEncoding = false;
				label.password = isPassword;
				mPivot = label.pivot;
				mPosition = label.cachedTransform.localPosition.x;
			}
			else enabled = false;
		}
	}

	bool mDoInit = true;

	/// <summary>
	/// If the object is currently highlighted, it should also be selected.
	/// </summary>

	void OnEnable () { if (UICamera.IsHighlighted(gameObject)) OnSelect(true); }

	/// <summary>
	/// Remove the selection.
	/// </summary>

	void OnDisable () { if (UICamera.IsHighlighted(gameObject)) OnSelect(false); }

	/// <summary>
	/// Selection event, sent by UICamera.
	/// </summary>

	void OnSelect (bool isSelected)
	{
		if (mDoInit) Init();

		if (label != null && enabled && NGUITools.GetActive(gameObject))
		{
			label.showLastPasswordChar = isSelected;
			if (isSelected)
			{
				mText = (label.Text == mDefaultText) ? "" : label.Text;
				label.color = activeColor;
				if (isPassword) label.password = true;

#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android)
				{
#if UNITY_3_4
					mKeyboard = iPhoneKeyboard.Open(mText, (iPhoneKeyboardType)((int)type), autoCorrect);
#else
					if (isPassword)
					{
						mKeyboard = TouchScreenKeyboard.Open(mText, TouchScreenKeyboardType.Default, false, false, true);
					}
					else
					{
						mKeyboard = TouchScreenKeyboard.Open(mText, (TouchScreenKeyboardType)((int)type), autoCorrect, label.IsMultiLine);
					}
#endif
				}
				else
#endif
				{
					Input.imeCompositionMode = IMECompositionMode.On;
					Transform t = label.cachedTransform;
					Vector3 offset = label.pivotOffset;
					offset.y += label.relativeSize.y;
					offset = t.TransformPoint(offset);
					Input.compositionCursorPos = UICamera.currentCamera.WorldToScreenPoint(offset);
				}
				UpdateLabel();
			}
			else
			{
#if UNITY_IPHONE || UNITY_ANDROID
				if (mKeyboard != null)
				{
					mKeyboard.active = false;
				}
#endif
				if (string.IsNullOrEmpty(mText))
				{
					label.Text = mDefaultText;
					label.color = mDefaultColor;
					if (isPassword) label.password = false;
				}
				else label.Text = mText;

//				label.showLastPasswordChar = false;
				Input.imeCompositionMode = IMECompositionMode.Off;
				RestoreLabel();
			}
		}
	}

#if UNITY_IPHONE || UNITY_ANDROID
	/// <summary>
	/// Update the text and the label by grabbing it from the iOS/Android keyboard.
	/// </summary>

	void Update()
	{
		if (mKeyboard != null)
		{
			string text = mKeyboard.text;

			if (mText != text)
			{
				mText = "";

				for (int i = 0; i < text.Length; ++i)
				{
					char ch = text[i];
					if (validator != null) ch = validator(mText, ch);
					if (ch != 0) mText += ch;
				}

				if (mText != text) mKeyboard.text = mText;
				UpdateLabel();
			}

			if (mKeyboard.done)
			{
				mKeyboard = null;
				current = this;
				if (onSubmit != null) onSubmit(mText);
				if (eventReceiver == null) eventReceiver = gameObject;
				eventReceiver.SendMessage(functionName, mText, SendMessageOptions.DontRequireReceiver);
				current = null;
				selected = false;
			}
		}
	}
#else
	void Update ()
	{
		if (selected && mLastIME != Input.compositionString)
		{
			mLastIME = Input.compositionString;
			UpdateLabel();
		}
	}
#endif

	/// <summary>
	/// Input event, sent by UICamera.
	/// </summary>

	void OnInput (string input)
	{
		if (mDoInit) Init();

		if (selected && enabled && NGUITools.GetActive(gameObject))
		{
			// Mobile devices handle input in Update()
			if (Application.platform == RuntimePlatform.Android) return;
			if (Application.platform == RuntimePlatform.IPhonePlayer) return;

			for (int i = 0, imax = input.Length; i < imax; ++i)
			{
				char c = input[i];

				if (c == '\b')
				{
					// Backspace
					if (mText.Length > 0)
					{
						mText = mText.Substring(0, mText.Length - 1);
						SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (c == '\r' || c == '\n')
				{
					if (UICamera.current.submitKey0 == KeyCode.Return || UICamera.current.submitKey1 == KeyCode.Return)
					{
						// Not multi-line input, or control isn't held
						if (!label.IsMultiLine || (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)))
						{
							// Enter
							current = this;
							if (onSubmit != null) onSubmit(mText);
							if (eventReceiver == null) eventReceiver = gameObject;
							eventReceiver.SendMessage(functionName, mText, SendMessageOptions.DontRequireReceiver);
							current = null;
							selected = false;
							return;
						}
					}

					// If we have an input validator, validate the input first
					if (validator != null) c = validator(mText, c);

					// If the input is invalid, skip it
					if (c == 0) continue;

					// Append the character
					if (c == '\n' || c == '\r')
					{
						if (label.IsMultiLine) mText += "\n";
					}
					else mText += c;

					// Notify the listeners
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
				}
				else if (c >= ' ')
				{
					// If we have an input validator, validate the input first
					if (validator != null) c = validator(mText, c);

					// If the input is invalid, skip it
					if (c == 0) continue;

					// Append the character and notify the "input changed" listeners.
					mText += c;
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
				}
			}

			// Ensure that we don't exceed the maximum length
			UpdateLabel();
		}
	}

	/// <summary>
	/// Update the visual text label, capping it at maxChars correctly.
	/// </summary>

	void UpdateLabel ()
	{
		if (mDoInit) Init();
		if (maxChars > 0 && mText.Length > maxChars) mText = mText.Substring(0, maxChars);

//		if (label.font != null)
//		{
			// Start with the text and append the IME composition and carat chars
		string processed;

		if (isPassword && selected)
		{
			processed = "";
			for (int i = 0, imax = mText.Length; i < imax; ++i) processed += "*";
			processed += Input.compositionString;
			processed += caratChar;
		}
		else
		{
			if(selected) {
				processed = mText + Input.compositionString + caratChar;
			} else {
				processed = mText;
			}
		}

		label.supportEncoding = false;
		label.showLastPasswordChar = selected;
		label.Text = processed;
		label.UpdateTextureIfNecessary();
		
		float textWidth = label.TextWidthPixels + (labelLeadingPadding * 2f);
		float backgroundWidth = background.cachedTransform.localScale.x;
		float backgroundHeight = background.cachedTransform.localScale.y;
		
		Vector3 labelPosition = label.cachedTransform.localPosition;
		if(label.IsMultiLine) {
			if(label.RightToLeftText) {
				label.pivot = UIWidget.Pivot.TopRight;
				labelPosition.x = (backgroundWidth / 2f) - labelLeadingPadding;
			} else {
				label.pivot = UIWidget.Pivot.TopLeft;
				labelPosition.x = labelLeadingPadding;
			}

			labelPosition.y = (backgroundHeight / 2f) - labelLeadingPadding;
		} else {
			if(label.RightToLeftText) {
				if(textWidth > background.cachedTransform.localScale.x) {
					label.pivot = UIWidget.Pivot.Left;
					labelPosition.x = labelLeadingPadding;
				} else {
					label.pivot = UIWidget.Pivot.Right;
					labelPosition.x = backgroundWidth - labelLeadingPadding;
				}
			} else {
				if(textWidth > background.cachedTransform.localScale.x) {
					label.pivot = UIWidget.Pivot.Right;
					labelPosition.x = backgroundWidth - labelLeadingPadding;

				} else {
					label.pivot = UIWidget.Pivot.Left;
					labelPosition.x = labelLeadingPadding;
				}
			}
		}
		label.cachedTransform.localPosition = labelPosition;
		
		
		if(label.IsMultiLine && draggablePanel != null) {
			float textHeight = label.TextHeightPixels + (labelLeadingPadding * 2f);
			if(textHeight > background.cachedTransform.localScale.y) {
				draggablePanel.SetDragAmount(1, 1, false);
			}
		}
		
		if(label.ClipToBounds) {
			label.LocalClipWidth = backgroundWidth - (labelLeadingPadding * 2f);	
		}
	}
	

	/// <summary>
	/// Restore the input label's pivot point and position.
	/// </summary>

	void RestoreLabel ()
	{
		if (label != null)
		{
			label.pivot = mPivot;
			Vector3 pos = label.cachedTransform.localPosition;
			pos.x = mPosition;
			label.cachedTransform.localPosition = pos;
			if(label.IsMultiLine && draggablePanel != null) {
				draggablePanel.SetDragAmount(1, 0, false);
			}
		}
	}
}
