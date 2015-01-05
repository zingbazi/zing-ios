using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Threading;

public class TestFlightXCodeSchemas
{
	List<string> 	targets = new List<string>();
	List<string> 	configs = new List<string>();
	List<string> 	schemes = new List<string>();
	int				defaultConfig = 0;
	
	public string[] Targets { get { return targets.ToArray(); } }
	public string[] Configs { get { return configs.ToArray(); } }
	public string[] Schemes { get { return schemes.ToArray(); } }
	public string DefaultConfig { get { return defaultConfig >= 0 && defaultConfig < configs.Count ? configs[defaultConfig]:"";} }
		
	/// <summary>
	/// protected constructor, use enumerate to create this type
	/// </summary>
	protected TestFlightXCodeSchemas() {}
	
	/// <summary>
	/// Enumerate the schemas for an xcode project
	/// </summary>
	/// <param name="projectPath">
	/// A <see cref="System.String"/> that contains the path to folder containing the xcode project (not the project itself)
	/// </param>
	/// <returns>
	/// A <see cref="TestFlightXCodeSchemas"/> containing the targets, configs and schemes belonging to that project. Or null on any error
	/// </returns>
	public static TestFlightXCodeSchemas Enumerate(TestFlightPreferences preferences, bool buildProvisions=false)
	{
		var projectPath = preferences.teamPrefs.buildPath;
		TestFlightXCodeSchemas output = new TestFlightXCodeSchemas();
		try 
		{
			output = CreateAndReadSchemaList(projectPath);
			if(buildProvisions && (output == null || output.Schemes.Length == 0))
			{
				BuildAndProcessProjectIfMissing(preferences);
				output = CreateAndReadSchemaList (projectPath);
			}
		}
		catch(System.Exception e) 
		{
			Debug.LogWarning("Autopilot: Unable to detect schema settings, reason:\n"+e);
		}
		
		return output;
	}
	
	public static TestFlightXCodeSchemas Enumerate(string pathToBuiltProject, bool launchProject=false)
	{	
		TestFlightXCodeSchemas output = new TestFlightXCodeSchemas();
		try 
		{
			if(launchProject)
			{
				PostProcessProject(pathToBuiltProject);
			}
			output = CreateAndReadSchemaList(pathToBuiltProject);
		}
		catch(System.Exception e) 
		{
			Debug.LogWarning("Autopilot: Unable to detect schema settings, reason:\n"+e);
		}
		
		return output;
	}
		
	static TestFlightXCodeSchemas CreateAndReadSchemaList (string projectPath)
	{
		TestFlightXCodeSchemas output = new TestFlightXCodeSchemas();
		
		var cmd = "-c \"xcodebuild -list > schema.lst\"";
		System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo("bash", cmd);
		pi.WorkingDirectory = projectPath;
		//pi.RedirectStandardOutput = true;
		//pi.RedirectStandardError = true;
		//pi.UseShellExecute = false;
		System.Diagnostics.Process p = System.Diagnostics.Process.Start(pi);
		if(p == null)
			return null;
		
		p.WaitForExit(2000);		
		if(!p.HasExited)
			return null;
		
		if(p.ExitCode != 0)
			return null;
		
		var lstPath = Path.Combine(projectPath,"schema.lst");
		var fi = new FileInfo(lstPath);
		if(!fi.Exists)
			return null;
		
		var tr = new StreamReader(fi.OpenRead());
		
		string strOutput = tr.ReadToEnd() + "\n";
		tr.Close();
		fi.Delete();
		
		// build configs 
		output.targets = ExtractList("Targets", strOutput);
		output.configs = ExtractList("Build Configurations", strOutput);
		output.schemes = ExtractList("Schemes", strOutput);
		
		output.defaultConfig = output.configs.IndexOf(FindDefaultConfig(strOutput));
		
		return output;
	}

	public static void BuildAndProcessProjectIfMissing (TestFlightPreferences preferences)
	{
		if(!TestFlightBuildPipeline.HasPro())
			return;
		
		var projectPath = preferences.teamPrefs.buildPath;
		if(TestFlightBuildPipeline.BuildPlayerIOS(preferences, false))
		{
			PostProcessProject (projectPath);
		}
	}

	static void PostProcessProject(string projectPath)
	{
		System.Diagnostics.Process.Start("open", Path.Combine(projectPath, "Unity-iPhone.xcodeproj"));
		Thread.Sleep(200);
		
		System.Diagnostics.Process.Start("osascript", "-e 'quit application \"Xcode\"'");
		/*
		var file = new StreamWriter(File.OpenWrite("closexcode.applescript"));
		file.WriteLine("quit app \"'Xcode'\"");
		file.Close();
		System.Diagnostics.Process.Start("open", "closexcode.applescript");
		*/
	}
	
	static private List<string> ExtractList(string header, string input)
	{
		List<string> output = new List<string>();
		string pattern = @"(?:"+header+@"\:\n)(?:\s+(\S+)\n)+";
				
		Match m = Regex.Match(input, pattern);
		for(int i=1; i<m.Groups.Count; ++i)
		{
			foreach(Capture c in m.Groups[i].Captures)
				output.Add(c.Value);
		}
		
		output.RemoveAll((obj) => obj.Contains("iPhone-simulator"));
		return output;
	}
	
	static private string FindDefaultConfig(string input)
	{
		Match m = Regex.Match(input, "If no build configuration is specified and -scheme is not passed then \"([^\"]+)\" is used.");
		if(!m.Success || m.Groups.Count <= 1)
			return "";
		else 
			return m.Groups[1].Captures[0].Value;
	}
}