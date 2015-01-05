// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("IFES/Localize")]
public class IFLocalize : MonoBehaviour
{
	/// <summary>
	/// Localization key.
	/// </summary>

	public string key;

	string mLanguage;
	bool mStarted = false;

	/// <summary>
	/// This function is called by the Localization manager via a broadcast SendMessage.
	/// </summary>

	void OnLocalize (Localization loc) { if (mLanguage != loc.currentLanguage) Localize(); }

	/// <summary>
	/// Localize the widget on enable, but only if it has been started already.
	/// </summary>

	void OnEnable () { if (mStarted && Localization.instance != null) Localize(); }

	/// <summary>
	/// Localize the widget on start.
	/// </summary>

	void Start ()
	{
		mStarted = true;
		if (Localization.instance != null) Localize();
	}

	/// <summary>
	/// Force-localize the widget.
	/// </summary>

	public void Localize ()
	{
		Localization loc = Localization.instance;
		UIWidget w = GetComponent<UIWidget>();
		UILabel lbl = w as UILabel;
		UISprite sp = w as UISprite;
		UISysFontLabel sysFontLabel = w as UISysFontLabel;

		// If no localization key has been specified, use the label's text as the key
		if (string.IsNullOrEmpty(mLanguage) && string.IsNullOrEmpty(key)) {
			if(lbl != null) key = lbl.text;
			else if(sysFontLabel != null) key = sysFontLabel.Text;
		} 

		// If we still don't have a key, leave the value as blank
		string val = string.IsNullOrEmpty(key) ? "" : loc.Get(key);

		if (lbl != null)
		{
			// If this is a label used by input, we should localize its default value instead
			UIInput input = NGUITools.FindInParents<UIInput>(lbl.gameObject);
			if (input != null && input.label == lbl) input.defaultText = val;
			else lbl.text = val;
		}
		else if (sysFontLabel != null)
		{
			UISysFontInput input = NGUITools.FindInParents<UISysFontInput>(sysFontLabel.gameObject);
			if (input != null && input.label == sysFontLabel) input.defaultText = val;
			else sysFontLabel.Text = val;
		}
		else if (sp != null)
		{
			sp.spriteName = val;
			sp.MakePixelPerfect();
		}
		mLanguage = loc.currentLanguage;
	}
}
