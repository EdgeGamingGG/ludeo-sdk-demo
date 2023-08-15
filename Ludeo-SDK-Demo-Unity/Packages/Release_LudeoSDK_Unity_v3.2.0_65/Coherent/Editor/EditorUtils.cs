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

using System.IO;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace cohtml
{

public static class EditorUtils
{
	public static string StringifyPlatform(BuildTarget platform)
	{
		switch (platform)
		{
		case BuildTarget.Android:
			return "Android";
		case BuildTarget.iOS:
			return "iOS";
		case BuildTarget.StandaloneWindows:
			return "Windows_x86";
		case BuildTarget.StandaloneWindows64:
			return "Windows_x64";
#if UNITY_2017_3_OR_NEWER
		case BuildTarget.StandaloneOSX:
			return "MacOSX";
#else
		case BuildTarget.StandaloneOSXIntel:
			return "MacOSX_x86";
		case BuildTarget.StandaloneOSXIntel64:
			return "MacOSX_x64";
		case BuildTarget.StandaloneOSXUniversal:
			return "MacOSX_Universal";
#endif
		default:
			throw new System.NotImplementedException(platform.ToString());
		}
	}

	public static string StringifyBackend(GraphicsDeviceType backend)
	{
		switch (backend)
		{
		case GraphicsDeviceType.Direct3D11:
			return "Dx11";
		case GraphicsDeviceType.OpenGLCore:
			return "OpenGL";
		case GraphicsDeviceType.OpenGLES2:
			return "GLES2";
		case GraphicsDeviceType.OpenGLES3:
			return "GLES3";
		case GraphicsDeviceType.Metal:
			return "Metal";
		default:
			throw new System.NotImplementedException(backend.ToString());
		}
	}

	public static string GetAppExtention(BuildTarget platform)
	{
		switch (platform)
		{
		case BuildTarget.Android:
			return ".apk";
		case BuildTarget.iOS:
			return "";
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			return ".exe";
#if UNITY_2017_3_OR_NEWER
		case BuildTarget.StandaloneOSX:
#else
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
#endif
			return ".app";
		default:
			throw new System.NotImplementedException(platform.ToString());
		}
	}

	public static string FindBrowserExecutablePath()
	{
	#if UNITY_EDITOR_WIN
		string[] possibleRegistryLocations =
		{
			"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe",
			"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Google Chrome\\InstallLocation"
		};
		foreach (var registryKey in possibleRegistryLocations)
		{
			#if UNITY_2018_1_OR_NEWER
			string chromePath = RegistryUtil.GetRegistryStringValue(registryKey, null, null, RegistryView.Default);
			#else
			string chromePath = (string)Microsoft.Win32.Registry.GetValue(registryKey, null, null);
			#endif

			if (!System.String.IsNullOrEmpty(chromePath))
			{
				return chromePath;
			}
		}
		return null;

	#elif UNITY_EDITOR_OSX
		return "Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
	#else
		return null;
	#endif
	}

	public static void CopyFolder(string sourceFolder, string destinationFolder, string[] FileExtensionsForExclude = null)
	{
		if (!Directory.Exists(destinationFolder))
		{
			Directory.CreateDirectory(destinationFolder);
		}

		foreach (string file in Directory.GetFiles(sourceFolder))
		{
			string fileName = Path.GetFileName(file);
			bool any = false;
			if (FileExtensionsForExclude != null)
			{
				foreach (string extension in FileExtensionsForExclude)
				{
					if (fileName.Contains(extension))
					{
						any = true;
						break;
					}
				}
			}

			if (any)
			{
				continue;
			}

			string destination = Path.Combine(destinationFolder, fileName);
			File.Copy(file, destination);
		}

		foreach (string folder in Directory.GetDirectories(sourceFolder))
		{
			string name = Path.GetFileName(folder);
			string destination = Path.Combine(destinationFolder, name);
			CopyFolder(folder, destination, FileExtensionsForExclude);
		}
	}

	public static void DeleteDirectoryRecursively(string directory)
	{
		if (Directory.Exists(directory))
		{
			Directory.Delete(directory, true);
		}
	}
	}
}
