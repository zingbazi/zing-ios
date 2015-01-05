// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Sortable Grid")]
public class UISortableGrid : MonoBehaviour
{
	public enum Arrangement
	{
		Horizontal,
		Vertical,
	}

	public Arrangement arrangement = Arrangement.Horizontal;
	public int maxPerLine = 0;
	public float cellWidth = 200f;
	public float cellHeight = 200f;
	public bool repositionNow = false;
	public bool sorted = false;
	public bool hideInactive = true;

	bool mStarted = false;
	
	void Start ()
	{
		mStarted = true;
		Reposition();
	}

	void Update ()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}
	
	public delegate int GridSortDelegate(Transform a, Transform b);
	public GridSortDelegate sortDelegate;
	
	private int DoSort(Transform a, Transform b)
	{
		if(sortDelegate != null) {
			return sortDelegate(a, b);
		}
		return string.Compare(a.name, b.name);
	}

	public void Reposition ()
	{
		if (!mStarted)
		{
			repositionNow = true;
			return;
		}

		Transform myTrans = transform;

		int x = 0;
		int y = 0;

		if (sorted)
		{
			List<Transform> list = new List<Transform>();

			for (int i = 0; i < myTrans.childCount; ++i)
			{
				Transform t = myTrans.GetChild(i);
				if (t && (!hideInactive || NGUITools.GetActive(t.gameObject))) list.Add(t);
			}
			list.Sort(DoSort);

			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				Transform t = list[i];

				if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

				float depth = t.localPosition.z;
				t.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, depth) :
					new Vector3(cellWidth * y, (-cellHeight * x) - (cellHeight / 2), depth);

				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
			}
		}
		else
		{
			for (int i = 0; i < myTrans.childCount; ++i)
			{
				Transform t = myTrans.GetChild(i);

				if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

				float depth = t.localPosition.z;
				t.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, depth) :
					new Vector3(cellWidth * y, (-cellHeight * x) - (cellHeight / 2), depth);

				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
			}
		}

		UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		if (drag != null) drag.UpdateScrollbars(true);
	}
}