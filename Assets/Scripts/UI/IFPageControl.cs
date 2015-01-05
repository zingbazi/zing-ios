// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IFPageControl : MonoBehaviour {
	
	public int pageCount = 1;
	private int lastPageCount = 0;
	
	public int selectedPageIndex = 0;
	private int lastSelectedPageIndex = 0;
	
	public GameObject indicatorPrefab;
	
	private IFPageIndicator[] indicators;
	
	private UIGrid grid;
	private Transform cachedTransform;
	
	void Start()
	{
		cachedTransform = GetComponent<Transform>();
		grid = GetComponent<UIGrid>();
		grid.Reposition();
	}
	
	void Update()
	{
		pageCount = Mathf.Clamp(pageCount, 0, int.MaxValue);
		selectedPageIndex = Mathf.Clamp(selectedPageIndex, 0, pageCount);
		
		if(lastPageCount != pageCount) {
			lastPageCount = pageCount;
			
			if(indicators != null && indicators.Length > 0) {
				foreach(IFPageIndicator oldIndicator in indicators) {
					if(oldIndicator != null) {
						NGUITools.Destroy(oldIndicator.gameObject);	
					}
				}
			}

			if(pageCount > 0) {
				indicators = new IFPageIndicator[pageCount];
				for(int i = 0; i < pageCount; i++) {
					GameObject indicatorGO = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
					indicatorGO.transform.parent = cachedTransform;
					indicators[i] = indicatorGO.GetComponent<IFPageIndicator>();
					indicators[i].selected = (i == selectedPageIndex) ? true : false;
				}
				grid.Reposition();
				float width = pageCount * grid.cellWidth;
				Vector3 pos = cachedTransform.localPosition;
				pos.x = (width / -2f) + (grid.cellWidth / 2f);
				cachedTransform.localPosition = pos;
//				Bounds bounds = NGUIMath.ca
//				Vector3 pos = cachedTransform.localPosition;
//				pos.x = -bounds.extents.x;
//				cachedTransform.localPosition = pos;
			}
		}
		
		if(lastSelectedPageIndex != selectedPageIndex) {
			lastSelectedPageIndex = selectedPageIndex;
			
			for(int i = 0; i < pageCount; i++) {
				indicators[i].selected = (i == selectedPageIndex) ? true : false;
			}
		}
	}
}
