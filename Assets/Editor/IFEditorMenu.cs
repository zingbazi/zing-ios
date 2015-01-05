// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using SharpUnit;
using System.Reflection;
using System.Text;
using System.IO;

static public class IFEditorMenu
{
	[MenuItem("IFES/Run Unit Tests %u")]
	static public void RunUnitTests()
	{
		Debug.Log("Running Unit Tests");
		IFDatabase.databaseResourceName = "test_database";
		IFDatabase.recreateDatabase = true;
        TestSuite suite = new TestSuite();
        suite.AddAll(new ModelTests());
		suite.AddAll(new RuleEngineTests());
        TestResult res = suite.Run(null);
        Unity3D_TestReporter reporter = new Unity3D_TestReporter();
        reporter.LogResults(res);
		IFDatabase.FreeResources();
		IFDatabase.databaseResourceName = "database";
		IFDatabase.recreateDatabase = false;
		NGUITools.DestroyImmediate(IFGameManager.LoadableAssets.gameObject);
	}
	
	[MenuItem("IFES/Toggle App Home Screen %#h")]
	public static void ToggleAppHomeScreen()
	{
		if(Application.isPlaying) {
			IFHomeScreenController controller = IFGameManager.LoadableAssets.cachedHomeScreenController;
			if(controller != null) {
				controller.gameObject.SetActive(!controller.gameObject.activeSelf);	
			}
		}
	}
	
	[MenuItem("IFES/Clear PlayerPrefs")]
	public static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}
	
	[MenuItem("IFES/Dump PlayerPrefs")]
	public static void DumpPlayerPrefs()
	{
		IFUtils.DumpPlayerPrefs();
	}
	
	[MenuItem("IFES/Change Atlas References/iOS")]
	public static void ChangeAtalsReferencesToiOS()
	{
		ChangeAtlasAndFontReferencesToWidth("640");
	}
	
	[MenuItem("IFES/Change Atlas References/Android")]
	public static void ChangeAtalsReferencesToAndroid()
	{
		ChangeAtlasAndFontReferencesToWidth("768");
	}
	
	static void ChangeAtlasAndFontReferencesToWidth(string widthString)
	{
		string mainAtlasPath = null;
		string replacementAtlasPath = null;

		string mainGamesNumbersPath = null;
		string replacementGamesNumbersPath = null;

		string mainRoundNumbersPath = null;
		string replacementRoundNumbersPath = null;

		string mainScoreNumbersPath = null;
		string replacementScoreNumbersPath = null;

		foreach(string path in AssetDatabase.GetAllAssetPaths()) {
			if(path.EndsWith("Production UI.prefab")) mainAtlasPath = path;
			if(path.EndsWith("Production UI "+widthString+".prefab")) replacementAtlasPath = path;

			if(path.EndsWith("My Games Numbers.prefab")) mainGamesNumbersPath = path;
			if(path.EndsWith("My Games Numbers "+widthString+".prefab")) replacementGamesNumbersPath = path;

			if(path.EndsWith("Round Numbers.prefab")) mainRoundNumbersPath = path;
			if(path.EndsWith("Round Numbers "+widthString+".prefab")) replacementRoundNumbersPath = path;

			if(path.EndsWith("Score Numbers.prefab")) mainScoreNumbersPath = path;
			if(path.EndsWith("Score Numbers "+widthString+".prefab")) replacementScoreNumbersPath = path;

		}
		GameObject atlasPrefab = AssetDatabase.LoadAssetAtPath(mainAtlasPath, typeof(GameObject)) as GameObject;
		GameObject replacementPrefab = AssetDatabase.LoadAssetAtPath(replacementAtlasPath, typeof(GameObject)) as GameObject;
		atlasPrefab.GetComponent<UIAtlas>().replacement = replacementPrefab.GetComponent<UIAtlas>();
		
		GameObject gamesNumbers = AssetDatabase.LoadAssetAtPath(mainGamesNumbersPath, typeof(GameObject)) as GameObject;
		GameObject replacementGamesNumbers = AssetDatabase.LoadAssetAtPath(replacementGamesNumbersPath, typeof(GameObject)) as GameObject;
		gamesNumbers.GetComponent<UIFont>().replacement = replacementGamesNumbers.GetComponent<UIFont>();

		GameObject roundNumbers = AssetDatabase.LoadAssetAtPath(mainRoundNumbersPath, typeof(GameObject)) as GameObject;
		GameObject replacementRoundNumbers = AssetDatabase.LoadAssetAtPath(replacementRoundNumbersPath, typeof(GameObject)) as GameObject;
		roundNumbers.GetComponent<UIFont>().replacement = replacementRoundNumbers.GetComponent<UIFont>();

		GameObject scoreNumbers = AssetDatabase.LoadAssetAtPath(mainScoreNumbersPath, typeof(GameObject)) as GameObject;
		GameObject replacementScoreNumbers = AssetDatabase.LoadAssetAtPath(replacementScoreNumbersPath, typeof(GameObject)) as GameObject;
		scoreNumbers.GetComponent<UIFont>().replacement = replacementScoreNumbers.GetComponent<UIFont>();
	}
	
	[MenuItem("IFES/Set Proper SysFont Font in Scene")]
	public static void SetProperSysFontFontInScene()
	{
		UIRoot theRoot = GameObject.FindObjectOfType(typeof(UIRoot)) as UIRoot;
		UISysFontLabel[] labels = theRoot.GetComponentsInChildren<UISysFontLabel>(true);
		foreach(UISysFontLabel label in labels) {
			if(label.AppleFontName.Contains("Bold")) {
				Debug.Log("Using bold for "+label.gameObject.name+": \""+label.Text+"\"");
				label.AppleFontName = "BNazaninBold";
				label.AndroidFontName ="BNazaninBold.ttf";				
				label.IsBold = true;
			} else {
				label.AppleFontName = "BNazanin";
				label.AndroidFontName ="BNazanin.ttf";
			}
			label.MakePixelPerfect();
		}
	}
	
	[MenuItem("IFES/Create Localization Strings")]
	public static void CreateLocalizationStrings()
	{
		IFLoadableAssets loadable = (IFLoadableAssets)GameObject.FindSceneObjectsOfType(typeof(IFLoadableAssets))[0];
		
		TextAsset englishStringsAsset = Resources.Load("English", typeof(TextAsset)) as TextAsset;
		ByteReader reader = new ByteReader(englishStringsAsset);
		Dictionary<string, string> localizable = reader.ReadDictionary();
		
		StringBuilder sb = new StringBuilder();
		
		Type type = typeof(IFLoadableAssets);
			
		foreach(FieldInfo info in type.GetFields(BindingFlags.Instance | BindingFlags.Public)) {
			if(info.FieldType.Equals(typeof(GameObject)) && info.Name.Contains("Prefab")) {
				GameObject prefab = info.GetValue(loadable) as GameObject;
				if(prefab != null) {
					GameObject po = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
					if(po != null) {
						foreach(IFLocalize loc in po.GetComponentsInChildren<IFLocalize>(true)) {
							if(loc.key.Equals(string.Empty)) {
								Debug.Log(loc.gameObject.name + " in " + prefab.name + " was empty");	
							} else {
								string val;
								if(!localizable.TryGetValue(loc.key, out val)) val = loc.key;
								sb.AppendLine(loc.key+" = "+val);
							}
						}
						foreach(UILocalize loc in po.GetComponentsInChildren<UILocalize>(true)) {
							if(loc.key.Equals(string.Empty)) {
								Debug.Log(loc.gameObject.name + " in " + prefab.name + " was empty");	
							} else {
								string val;
								if(!localizable.TryGetValue(loc.key, out val)) val = loc.key;
								sb.AppendLine(loc.key+" = "+val);
							}
						}
						GameObject.DestroyImmediate(po);
						GameObject.DestroyImmediate(Localization.instance.gameObject);
					} else {
						Debug.LogError("Problem instantiating prefab: "+info.Name);
					}
				} else {
					Debug.Log(info.Name + " is null, ignoring.");
				}
			}
		}
		
		string englishFilePath = Application.dataPath + "/../" + AssetDatabase.GetAssetPath(englishStringsAsset);
		File.WriteAllText(englishFilePath, sb.ToString());
        AssetDatabase.Refresh();
	}
	
	[MenuItem("IFES/Add Localization To Labels")]
	public static void AddLocalizationToLabels()
	{
		IFLoadableAssets loadable = (IFLoadableAssets)GameObject.FindSceneObjectsOfType(typeof(IFLoadableAssets))[0];
		
		Type type = typeof(IFLoadableAssets);
			
		foreach(FieldInfo info in type.GetFields(BindingFlags.Instance | BindingFlags.Public)) {
			if(info.FieldType.Equals(typeof(GameObject)) && info.Name.Contains("Prefab")) {
				GameObject prefab = info.GetValue(loadable) as GameObject;
				GameObject po = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				
				foreach(UISysFontLabel sysFontLabel in po.GetComponentsInChildren<UISysFontLabel>(true)) {
					Debug.Log("UISysFontLabel("+sysFontLabel.gameObject.name+") = "+sysFontLabel.Text);
					IFLocalize localize = sysFontLabel.GetComponent<IFLocalize>();
					if(localize == null) {
						localize = sysFontLabel.gameObject.AddComponent<IFLocalize>();
						localize.key = sysFontLabel.Text;
						Debug.Log("Localization added");
					}
				}
				
				foreach(UILabel label in po.GetComponentsInChildren<UILabel>(true)) {
					IFLocalize localize = label.GetComponent<IFLocalize>();
					if(localize == null) {
						localize = label.gameObject.AddComponent<IFLocalize>();
						localize.key = label.text;
					}
				}
				
				PrefabUtility.ReplacePrefab(po, prefab);
				GameObject.DestroyImmediate(po);
			}
		}
		
        AssetDatabase.Refresh();
	}
}
