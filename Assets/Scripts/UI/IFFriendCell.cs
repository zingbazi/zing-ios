// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFFriendCell : MonoBehaviour
{
	public UISysFontLabel label;
	public UISprite background;
	
	public string FriendId { get; set; }
	
	private string mFriendName;
	public string FriendName
	{
		get { return mFriendName; }
		set
		{
			if(mFriendName == null || !mFriendName.Equals(value)) {
				mFriendName = value;
				label.Text = mFriendName;
				label.MarkAsChanged();
			}
		}
	}
	
	
}
