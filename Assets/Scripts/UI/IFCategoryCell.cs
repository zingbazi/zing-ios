// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFCategoryCell : MonoBehaviour {

	public UISysFontLabel label;
	public UISprite background;

	private IFQuestionCategory mCategory;
	public IFQuestionCategory Category
	{
		get { return mCategory; }
		set
		{
			if(mCategory == null || !mCategory.Equals(value)) {
				mCategory = value;
				label.Text = mCategory.Name;
				label.MarkAsChanged();
			}
		}
	}
}
