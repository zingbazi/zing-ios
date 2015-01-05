// Additions copyright (c) 2013 Empirical Development LLC. All rights reserved.

/*
 * Copyright (c) 2012 Mario Freitas (imkira@gmail.com)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UISysFontLabel))]
public class UISysFontLabelEditor : UIWidgetInspector
{
  protected UISysFontLabel _label;

	protected override bool DrawProperties()
	{
		_label = (UISysFontLabel)target;
		ISysFontTexturableEditor.DrawInspectorGUI(_label);
		GUILayout.BeginHorizontal();
		{
			EditorGUIUtility.LookLikeControls(90f);

			bool clip = EditorGUILayout.Toggle("Clip to bounds", _label.ClipToBounds, GUILayout.Width(110f));
			if (clip != _label.ClipToBounds)
			{
				Undo.RegisterUndo(_label, "SysFont Clip to Bounds Change");
				_label.ClipToBounds = clip;
			}
			
			EditorGUIUtility.LookLikeControls(90f);
			
			int clipWidth = EditorGUILayout.IntField("Local Clip Width", (int)_label.LocalClipWidth, GUILayout.Width(140f));
			if (clipWidth != _label.LocalClipWidth)
			{
				Undo.RegisterUndo(_label, "SysFont Local Clip Width Change");
				_label.LocalClipWidth = (float)clipWidth;
			}
			EditorGUIUtility.LookLikeControls();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		{
			EditorGUIUtility.LookLikeControls(100f);
			bool rtl = EditorGUILayout.Toggle("Right to Left Text", _label.RightToLeftText, GUILayout.Width(110f));
			if(rtl != _label.RightToLeftText)
			{
				Undo.RegisterUndo(_label, "SysFont Toggle Right To Left Text");
				_label.RightToLeftText = rtl;
			}

			EditorGUIUtility.LookLikeControls(90f);

			int textHeightAdjust = EditorGUILayout.IntField("Text Height Adjust", (int)_label.textHeightAdjust, GUILayout.Width(140f));
			if (textHeightAdjust != _label.textHeightAdjust)
			{
				Undo.RegisterUndo(_label, "SysFont Text Height Adjust");
				_label.textHeightAdjust = textHeightAdjust;
			}
			EditorGUIUtility.LookLikeControls();
		}
		GUILayout.EndHorizontal();
		return true;
	}

  [MenuItem("NGUI/Create a SysFont Label")]
  static public void AddLabel()
  {
    GameObject go = NGUIMenu.SelectedRoot();

    if (NGUIEditorTools.WillLosePrefab(go))
    {
      NGUIEditorTools.RegisterUndo("Add a SysFont Label", go);

      GameObject child = new GameObject("UISysFontLabel");
			child.layer = go.layer;
      child.transform.parent = go.transform;

      UISysFontLabel label = child.AddComponent<UISysFontLabel>();
      label.MakePixelPerfect();
      Vector3 pos = label.transform.localPosition;
      pos.z = -1f;
      label.transform.localPosition = pos;
      label.Text = "Hello World";
      Selection.activeGameObject = child;
    }
  }
}
