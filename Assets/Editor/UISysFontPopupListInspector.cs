// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UISysFontPopupLists.
/// </summary>

[CustomEditor(typeof(UISysFontPopupList))]
public class UISysFontPopupListInspector : Editor
{
	UISysFontPopupList mList;

	void RegisterUndo ()
	{
		NGUIEditorTools.RegisterUndo("Popup List Change", mList);
	}

	void OnSelectAtlas (MonoBehaviour obj)
	{
		RegisterUndo();
		mList.atlas = obj as UIAtlas;
	}
	
//	void OnSelectFont (MonoBehaviour obj)
//	{
//		RegisterUndo();
//		mList.font = obj as UIFont;
//	}

	void OnBackground (string spriteName)
	{
		RegisterUndo();
		mList.backgroundSprite = spriteName;
		Repaint();
	}

	void OnHighlight (string spriteName)
	{
		RegisterUndo();
		mList.highlightSprite = spriteName;
		Repaint();
	}

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		mList = target as UISysFontPopupList;

		ComponentSelector.Draw<UIAtlas>(mList.atlas, OnSelectAtlas);
//		ComponentSelector.Draw<UIFont>(mList.font, OnSelectFont);

		GUILayout.BeginHorizontal();
		UISysFontLabel lbl = EditorGUILayout.ObjectField("Text Label", mList.textLabel, typeof(UISysFontLabel), true) as UISysFontLabel;

		if (mList.textLabel != lbl)
		{
			RegisterUndo();
			mList.textLabel = lbl;
			if (lbl != null) lbl.Text = mList.selection;
		}
		GUILayout.Space(44f);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		string cocoaFontName = EditorGUILayout.TextField("iOS/Mac Font Name", mList.cocoaFontName, GUILayout.ExpandWidth(true));
		
		if(!mList.cocoaFontName.Equals(cocoaFontName)) {
			RegisterUndo();
			mList.cocoaFontName = cocoaFontName;
		}
		GUILayout.Space(44f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		string androidFontName = EditorGUILayout.TextField("Android Font Name", mList.androidFontName, GUILayout.ExpandWidth(true));
		if(!mList.androidFontName.Equals(androidFontName)) {
			RegisterUndo();
			mList.androidFontName = androidFontName;
		}
		GUILayout.Space(44f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		int fontSize = EditorGUILayout.IntField("Font Size", mList.fontSize, GUILayout.ExpandWidth(true));
		if(mList.fontSize != fontSize) {
			RegisterUndo();
			mList.fontSize = fontSize;
		}
		GUILayout.Space(44f);
		GUILayout.EndHorizontal();
		
		if (mList.atlas != null)
		{
			NGUIEditorTools.SpriteField("Background", mList.atlas, mList.backgroundSprite, OnBackground);
			NGUIEditorTools.SpriteField("Highlight", mList.atlas, mList.highlightSprite, OnHighlight);

			GUILayout.BeginHorizontal();
			GUILayout.Space(6f);
			GUILayout.Label("Options");
			GUILayout.EndHorizontal();

			string text = "";
			foreach (string s in mList.items) text += s + "\n";

			GUILayout.Space(-14f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(84f);
			string modified = EditorGUILayout.TextArea(text, GUILayout.Height(100f));
			GUILayout.EndHorizontal();

			if (modified != text)
			{
				RegisterUndo();
				string[] split = modified.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
				mList.items.Clear();
				foreach (string s in split) mList.items.Add(s);

				if (string.IsNullOrEmpty(mList.selection) || !mList.items.Contains(mList.selection))
				{
					mList.selection = mList.items.Count > 0 ? mList.items[0] : "";
				}
			}

			string sel = NGUIEditorTools.DrawList("Selection", mList.items.ToArray(), mList.selection);

			if (mList.selection != sel)
			{
				RegisterUndo();
				mList.selection = sel;
			}

			UISysFontPopupList.Position pos = (UISysFontPopupList.Position)EditorGUILayout.EnumPopup("Position", mList.position);

			if (mList.position != pos)
			{
				RegisterUndo();
				mList.position = pos;
			}

			float ts = EditorGUILayout.FloatField("Text Scale", mList.textScale);
			Color tc = EditorGUILayout.ColorField("Text Color", mList.textColor);
			Color bc = EditorGUILayout.ColorField("Background", mList.backgroundColor);
			Color hc = EditorGUILayout.ColorField("Highlight", mList.highlightColor);

			GUILayout.BeginHorizontal();
			bool isLocalized = EditorGUILayout.Toggle("Localized", mList.isLocalized, GUILayout.Width(100f));
			bool isAnimated = EditorGUILayout.Toggle("Animated", mList.isAnimated);
			GUILayout.EndHorizontal();

			if (mList.textScale != ts ||
				mList.textColor != tc ||
				mList.highlightColor != hc ||
				mList.backgroundColor != bc ||
				mList.isLocalized != isLocalized ||
				mList.isAnimated != isAnimated)
			{
				RegisterUndo();
				mList.textScale = ts;
				mList.textColor = tc;
				mList.backgroundColor = bc;
				mList.highlightColor = hc;
				mList.isLocalized = isLocalized;
				mList.isAnimated = isAnimated;
			}

			NGUIEditorTools.DrawSeparator();

			GUILayout.BeginHorizontal();
			GUILayout.Space(6f);
			GUILayout.Label("Padding", GUILayout.Width(76f));
			GUILayout.BeginVertical();
			GUILayout.Space(-12f);
			Vector2 padding = EditorGUILayout.Vector2Field("", mList.padding);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			if (mList.padding != padding)
			{
				RegisterUndo();
				mList.padding = padding;
			}

			EditorGUIUtility.LookLikeControls(100f);

			GameObject go = EditorGUILayout.ObjectField("Event Receiver", mList.eventReceiver,
				typeof(GameObject), true) as GameObject;

			string fn = EditorGUILayout.TextField("Function Name", mList.functionName);

			if (mList.eventReceiver != go || mList.functionName != fn)
			{
				RegisterUndo();
				mList.eventReceiver = go;
				mList.functionName = fn;
			}
		}
	}
}
