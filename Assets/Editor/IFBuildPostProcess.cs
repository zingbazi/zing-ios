// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Xml;
 
public static class IFBuildPostProcess
{
	[PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
//		if(target == BuildTarget.iPhone) {
//			string infoPlistPath = path + "/Info.plist";
//			FileInfo infoPlist = new FileInfo(path);
//			using(StreamReader reader = infoPlist.OpenText()) {
//				string plist = reader.ReadToEnd();
//				if(plist.IndexOf("UIAppFonts") < 0) {
//					Debug.Log("Need to add UIAppFonts!");
//				} else {
//					Debug.Log("UIAppFonts already exists!");
//				}
//			}
//		}
	}
}