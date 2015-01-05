// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFToggleDebugInfoButton : MonoBehaviour {
	
	
	public GameObject[] itemsToToggle;

	public void OnDoubleClick()
	{
		foreach(GameObject go in itemsToToggle) {
			NGUITools.SetActive(go, !go.activeSelf);
		}
		IFGameManager.LoadableAssets.ClearCache();
	}
	
}
