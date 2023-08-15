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

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace cohtml
{
[InitializeOnLoad]
public static class KnownIssuesWindow
{
	[Serializable]
	public class Versions
	{
		public string[] versions;
	}

	public const string ContextMenuKnownIssuesWindow = "Gameface/Known Issues";
	private const string VersionPattern = @"(?'version'\d+\.\d+\.\d+)";
	private const string LastOpenedEditorVersionSessionKey = "Cohtml_KnownIssues_LastOpenedEditorVersion";
	private const string VersionsJson = "Editor/KnownIssues/EditorVersions.json";
	private static string unityVersion = Application.unityVersion;

	static KnownIssuesWindow()
	{
		EditorApplication.delayCall += DisplayIfVersionChanged;
	}

	private static void DisplayIfVersionChanged()
	{
		string lastOpenedEditorVersion = PlayerPrefs.GetString(LastOpenedEditorVersionSessionKey, null);
		if (lastOpenedEditorVersion == null || lastOpenedEditorVersion != unityVersion)
		{
			DisplayContent();
			lastOpenedEditorVersion = unityVersion;
			PlayerPrefs.SetString(LastOpenedEditorVersionSessionKey, lastOpenedEditorVersion);
		}
	}

	internal static void DisplayContent()
	{
		string cleanedEditorVersion = Regex.Match(unityVersion, VersionPattern).Groups["version"].Value;
		string[] titles = JsonUtility.FromJson<Versions>(
				File.ReadAllText(Path.GetFullPath(Utils.CombinePaths(Utils.CohtmlUpmPath, VersionsJson)))
			)
			.versions;

		foreach (string title in titles)
		{
			string[] versions = title.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			if (versions.Any(version => cleanedEditorVersion.Contains(version)))
			{
				InfoWindow.Popup("Cohtml",
					$"You are using Unity3D editor version {unityVersion} with known issues.{Environment.NewLine}" +
					$"To see that window again press Context menu \"{ContextMenuKnownIssuesWindow}\".",
					"Visit Known Issues",
					() => Application.OpenURL($"https://docs.coherent-labs.com/unity-gameface/integration/known_issues_unity/#{MakeAnchor(title)}"));
				break;
			}
		}
	}

	private static string MakeAnchor(string title)
	{
		return title.Replace(",", "-")
			.Replace(" ", string.Empty)
			.Replace(".", string.Empty);
	}
}
}
