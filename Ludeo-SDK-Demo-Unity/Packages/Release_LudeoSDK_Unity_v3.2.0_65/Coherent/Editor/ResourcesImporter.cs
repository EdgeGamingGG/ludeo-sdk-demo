/*
This file is part of Cohtml, Gameface and Prysm - modern user interface technologies.

Copyright (c) 2012-2023 Coherent Labs AD and/or its licensors. All
rights reserved in all media.

The coded instructions, statements, computer programs, and/or related
material (collectively the "Data") in these files contain confidential
and unpublished information proprietary Coherent Labs and/or its
licensors, which is protected by United States of America federal
copyright law and by international treaties.

This software or source code is supplied under the terms of a license
agreement and nondisclosure agreement with Coherent Labs AD and may
not be copied, disclosed, or exploited except in accordance with the
terms of that agreement. The Data may not be disclosed or distributed to
third parties, in whole or in part, without the prior written consent of
Coherent Labs AD.

COHERENT LABS MAKES NO REPRESENTATION ABOUT THE SUITABILITY OF THIS
SOURCE CODE FOR ANY PURPOSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT
HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY, NONINFRINGEMENT, AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER, ITS AFFILIATES,
PARENT COMPANIES, LICENSORS, SUPPLIERS, OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
ANY WAY OUT OF THE USE OR PERFORMANCE OF THIS SOFTWARE OR SOURCE CODE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace cohtml
{
public static class ResourcesImporter
{
	internal struct SceneData
	{
		public readonly string Name;
		public readonly string ScenePath;
		private string m_HtmlPath;

		public SceneData(string name, string parentDir = null)
		{
			Name = name;
			ScenePath = $"{parentDir ?? name}/{name}.unity";
			m_HtmlPath = null;
		}

		public string HtmlPath
		{
			get
			{
				if (string.IsNullOrEmpty(m_HtmlPath))
				{
					m_HtmlPath = $"{UIResourcesDir}/{Name}/{PascalToKebabCase(Name)}.html";
				}

				return m_HtmlPath;
			}
		}

		private string PascalToKebabCase(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			return Regex.Replace(
					value,
					"(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
					"-$1",
					RegexOptions.Compiled)
				.Trim()
				.ToLower();
		}
	}

	public const string UIResourcesDir = "UIResources";
	public const string CohtmlDir = "Cohtml";

	public const string StreamingAssetsDir = "StreamingAssets";
	public const string AssetsDir = "Assets";

	public const string HelloDir = "Hello";
	public const string MobaName = "Moba";
	public const string MainMenuName = "MainMenu";

	// TODO: Get the sample names from the package.json "displayName" field
	private static readonly string[] s_SampleNames =
	{
		"HelloSamples",
		"MadRabbits",
		MobaName,
		MainMenuName,
		"LiveViews",
		"Strategy",
		"Websockets",
		"VideoPlayback",
		"InputPropagation",
		"FrontendSamples"
	};

	private const string MetaExtension = ".meta";

	private static Dictionary<string, SceneData> s_CohtmlScenes;

	static ResourcesImporter()
	{
		s_CohtmlScenes = new Dictionary<string, SceneData>();
		for (int i = 0; i < s_SampleNames.Length; i++)
		{
			s_CohtmlScenes.Add(s_SampleNames[i], new SceneData(s_SampleNames[i]));
		}
	}

	[InitializeOnLoadMethod]
	public static void Run()
	{
		EditorSceneManager.sceneOpened += OnSceneOpened;
	}

	private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
	{
		if (scene.path == null)
		{
			return;
		}

		foreach (SceneData sceneData in s_CohtmlScenes.Values)
		{
			if (scene.path.Contains(sceneData.Name))
			{
				ImportSampleResources(scene.path, sceneData.ScenePath, sceneData.HtmlPath);

				if (sceneData.Name == MobaName)
				{
					IncludeSampleToBuildSettings(scene.path.Replace(sceneData.ScenePath, s_CohtmlScenes[MainMenuName].ScenePath));
				}
				else if (sceneData.Name == MainMenuName)
				{
					IncludeSampleToBuildSettings(scene.path.Replace(sceneData.ScenePath, s_CohtmlScenes[MobaName].ScenePath));
				}
			}
		}
	}

	private static void ImportSampleResources(string sampleScenePath, string name, string htmlPath)
	{
		ShowProgressBar($"import {name} UI resources.", 0.1f);
		string samplePath = Path.GetDirectoryName(sampleScenePath);
		if (ImportSample(samplePath, name) && AttachHtmlPathToCohtmlView(htmlPath))
		{
			DeleteResourceDirectory(samplePath);
			IncludeSampleToBuildSettings(sampleScenePath);
			LogHandler.Log($"Sample \"{htmlPath}\" imported in \"{Utils.CombinePaths(AssetsDir, StreamingAssetsDir, CohtmlDir, UIResourcesDir)}\".");
		}

		EditorUtility.ClearProgressBar();
	}

	private static void DeleteResourceDirectory(string samplePath)
	{
		string targetDirectory = Path.GetFullPath(Path.Combine(samplePath, UIResourcesDir));
		Directory.Delete(targetDirectory, true);
		File.Delete($"{targetDirectory}{MetaExtension}");
		AssetDatabase.Refresh();
	}

	public static void RemoveSamples()
	{
		string path = Utils.CombinePaths(AssetsDir, "Samples", CohtmlDir);
		ShowProgressBar("Remove Samples", 0.1f);
		EditorUtils.DeleteDirectoryRecursively(path);
		if (File.Exists(path + MetaExtension))
		{
			File.Delete(path + MetaExtension);
		}

		string uiResourcesPath = Utils.CombinePaths(
			AssetsDir,
			StreamingAssetsDir,
			CohtmlDir,
			UIResourcesDir);

		ShowProgressBar("Remove Samples", 0.5f);
		foreach (string scene in s_CohtmlScenes.Keys)
		{
			path = Path.Combine(uiResourcesPath, scene);
			EditorUtils.DeleteDirectoryRecursively(path);
			File.Delete(path + MetaExtension);
		}

		ShowProgressBar("Remove Samples", 0.8f);
		RemoveSampleFromBuildSettings();
		ShowProgressBar("Remove Samples", 0.9f);
		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}

	private static void RemoveSampleFromBuildSettings()
	{
		EditorBuildSettingsScene[] scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes).ToArray();
		EditorBuildSettings.scenes = scenes
			.Where(scene => !s_CohtmlScenes.Values
				.Any(data => scene.path.Contains(data.ScenePath)))
			.ToArray();
	}

	private static void IncludeSampleToBuildSettings(string sampleScenePath)
	{
		List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

		if (!scenes.Exists(x => x.path == sampleScenePath))
		{
			scenes.Add(new EditorBuildSettingsScene(sampleScenePath, true));
		}

		EditorBuildSettings.scenes = scenes.ToArray();
	}

	private static bool ImportSample(string sampleScenePath, string scene)
	{
		string targetDirectory = Path.GetFullPath(Path.Combine(sampleScenePath, UIResourcesDir));
		string destinationDirectory = Utils.CombinePaths(Application.streamingAssetsPath,
			CohtmlDir,
			UIResourcesDir,
			scene.Split('/')[0]);

		if (Directory.Exists(destinationDirectory) || !Directory.Exists(targetDirectory))
		{
			return false;
		}

		EditorUtils.CopyFolder(targetDirectory, destinationDirectory, new[] { MetaExtension });

		AssetDatabase.Refresh();

		return true;
	}

	private static bool AttachHtmlPathToCohtmlView(string htmlPath)
	{
		if (htmlPath.Contains("propagation"))
		{
			return true;
		}

		CohtmlView firstView = Object.FindObjectOfType<CohtmlView>();

		if (firstView != null)
		{
			if (!htmlPath.Contains("propagation"))
			{
				firstView.Page = DefaultResourceHandler.CouiProtocol + htmlPath;
			}

			return true;
		}

		return false;
	}

	public static void ImportHelloTemplate()
	{
		ShowProgressBar("Import hello template", 0.1f);
		ImportSample(Utils.CombinePaths(Utils.CohtmlUpmPath, "Samples~/", HelloDir), HelloDir);
		EditorUtility.ClearProgressBar();
	}

	public static void ImportFrontendSamples()
	{
		string target = Path.GetFullPath(Utils.CombinePaths(Utils.CohtmlUpmPath, "Samples~", "uiresources"));
		string destination = Utils.CombinePaths(CohtmlUISystem.StreamingAssetsPath, CohtmlDir, UIResourcesDir, "FrontendSamples");
		if (Directory.Exists(destination))
		{
			LogHandler.Log($"\"{Utils.CombinePaths(AssetsDir,  StreamingAssetsDir, CohtmlDir, UIResourcesDir, "FrontendSamples")}\" already exists.");
			return;
		}

		string title = "Import FrontendSamples";
		ShowProgressBar(title, 0.1f);
		EditorUtils.CopyFolder(target, destination);
		ShowProgressBar(title, 0.75f);
		AssetDatabase.Refresh();
		ShowProgressBar(title, 1f);
		LogHandler.Log($"\"FrontendSamples\" imported in \"{Utils.CombinePaths(AssetsDir, StreamingAssetsDir, CohtmlDir, UIResourcesDir)}\".");
		EditorUtility.ClearProgressBar();
	}

	private static void ShowProgressBar(string title, float progress, string info = "")
	{
		EditorUtility.DisplayProgressBar($"Cohtml {title}", info, progress);
	}
}
}
