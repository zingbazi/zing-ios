// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIDraggablePanel))]
[AddComponentMenu("IFES/Draggable Table")]
public class UITableViewPanel : MonoBehaviour
{
	public float cellHeight = 88f;
	public int cellCount = 20;
	public UIWidget background;
	public GameObject cellPrefab;
	public delegate void ConfigureCellForRowDelegate(GameObject cell, int row);
	public ConfigureCellForRowDelegate configureCellForRow;
	
	private UIPanel panel;
	private UIDraggablePanel draggablePanel;
	private float lastCellHeight = float.MaxValue;
	private int lastCellCount = int.MaxValue;

	private Range currentVisibleRange = Range.zero;
	BetterList<GameObject> cells;
	
	private Transform cellParent;

	private struct Range
	{
		public int location;
		public int length;
		
		public Range(int loc, int len)
		{
			location = loc;
			length = len;
		}
		
		public static Range zero { get { return new Range(0, 0); } }
		
		public int Max { get { return location + length; } }
		
		public static bool operator ==(Range lhs, Range rhs) { return lhs.Equals(rhs); }

		public static bool operator !=(Range lhs, Range rhs) { return !lhs.Equals(rhs);	}
		
		public override bool Equals(object obj)
		{
			if(obj == null) return false;
			
			if(!obj.GetType().Equals(typeof(Range))) {
				return false;
			}
			
			Range other = (Range)obj;
			
			return location == other.location && length == other.length;
		}
		
		public override int GetHashCode() { return location.GetHashCode() ^ length.GetHashCode(); }
	}

	private bool isInitialized = false;
	
	public void Reset()
	{
		lastCellCount = 0;
		lastCellHeight = 0f;
		isInitialized = false;
		if(cells != null) {
			foreach(GameObject cell in cells) {
				NGUITools.Destroy(cell);
			}
			cells = null;
		}
		configureCellForRow = null;
	}
	
	void Start()
	{
		panel = GetComponent<UIPanel>();
		draggablePanel = GetComponent<UIDraggablePanel>();
	}
	
	void Update()
	{
		cellCount = Mathf.Clamp(cellCount, 0, int.MaxValue);
		cellHeight = Mathf.Clamp(cellHeight, 0f, float.MaxValue);
		Vector4 clipRange = panel.clipRange;
		if(background != null && (cellHeight != lastCellHeight || cellCount != lastCellCount)) {
			lastCellCount = cellCount;
			lastCellHeight = cellHeight;
			ScaleBackground();
		}
		
		
		int maxVisibleCells = Mathf.CeilToInt(clipRange.w / cellHeight) + 1;
		int absoluteTopmostCell = CalculateTopmostVisibleCellIndex();
		int topmostCell = Mathf.Min(absoluteTopmostCell, cellCount - maxVisibleCells);
		Range visibleRange = new Range(topmostCell, maxVisibleCells);
		
		if(!isInitialized) {
			cells = new BetterList<GameObject>();
			draggablePanel.ResetPosition();	
			cellParent = new GameObject("Cells").transform;
			cellParent.parent = transform;
			cellParent.localScale = Vector3.one;
			cellParent.localRotation = Quaternion.identity;
			cellParent.localPosition = new Vector3(clipRange.x, draggablePanel.bounds.max.y, 0f);
			
			for(int i = 0; i < Mathf.Min(visibleRange.length, cellCount); i++) {
				GameObject cell = Instantiate(cellPrefab) as GameObject;

				Transform t = cell.transform;
				t.parent = cellParent;
				t.localScale = Vector3.one;
				t.localRotation = Quaternion.identity;
				t.localPosition = new Vector3(0f, -(cellHeight / 2f) + -(i * cellHeight), 0f);
				
				UIPanel badPanel = cell.GetComponent<UIPanel>();
				if(badPanel != null) NGUITools.Destroy(badPanel);
				
				if(configureCellForRow != null) {
					configureCellForRow(cell, i);
				}
				cells.Add(cell);
			}
			
			currentVisibleRange = visibleRange;
			isInitialized = true;
		}

		if(currentVisibleRange != visibleRange) {
			
			int offset = currentVisibleRange.location - visibleRange.location;
			if(offset < 0) {
				// Prune early cells
				offset = Mathf.Abs(offset);
				for(int i = 0; i < offset; i++) {
					GameObject pruned = cells.buffer[0];
					cells.RemoveAt(0);
					cells.Add(pruned);
					
					int newIndex = currentVisibleRange.Max + i;
					Vector3 pos = pruned.transform.localPosition;
					pos.y = -(cellHeight / 2f) + -(newIndex * cellHeight);
					pruned.transform.localPosition = pos;
					if(configureCellForRow != null) {
						configureCellForRow(pruned, newIndex);
					}
				}
			} else {
				//Prune late cells
				for(int i = 1; i <= offset; i++) {
					int lastIndex = cells.size - 1;
					GameObject pruned = cells.buffer[lastIndex];
					cells.RemoveAt(lastIndex);
					cells.Insert(0, pruned);
					
					int newIndex = visibleRange.location + (offset - i);
					Vector3 pos = pruned.transform.localPosition;
					pos.y = -(cellHeight / 2f) + -(newIndex * cellHeight);
					pruned.transform.localPosition = pos;

					if(configureCellForRow != null) {
						configureCellForRow(pruned, newIndex);
					}
				}
			}
			currentVisibleRange = visibleRange;
		}
	}
	
	int CalculateTopmostVisibleCellIndex()
	{
		Vector4 clipRange = panel.clipRange;
		
		// Find the top of the clip rect
		float topClipY = clipRange.y + clipRange.w / 2f;
		
		// Shift the origin to the top
		float normalizedTopBackgroundY = topClipY - draggablePanel.bounds.max.y;  
		
		// Clamp to ignore any affects of bouce scrolling
		float clampedTopY = Mathf.Clamp(normalizedTopBackgroundY, float.MinValue, 0f);
		
		// Flip the axis so we can deal with positive values of y
		float topY = Mathf.Abs(clampedTopY);
		
		return Mathf.FloorToInt(topY / cellHeight);
	}
	
	void ScaleBackground()
	{
		Vector3 scale = Vector3.one;
		scale.x = panel.clipRange.z;
		scale.y = cellCount * cellHeight;
		if(scale.y == 0) scale.y = .001f;
		background.cachedTransform.localScale = scale;
	}
}
