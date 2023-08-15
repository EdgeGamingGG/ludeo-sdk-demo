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
using cohtml.Net;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Rendering;
#endif

namespace cohtml
{
public static class LibraryParamsManager
{
	public const string CohtmlLibraryParamsKey = "Configuration/LibraryParams";
	private static ExtendedLibraryParams s_ExtendedLibraryParamsCached;
	private static bool s_IsDirty = true;
	private static bool s_isExtendedLibraryParamsLoaded;

	public static Func<ExtendedLibraryParams, ExtendedLibraryParams> OnCustomizeExtendedParams { get; set; }

	public static bool EnableMemoryTracking => GetJsonParamsOrDefault().EnableMemoryTracking;

	public static bool ShouldUseCSharpBackend => GetJsonParamsOrDefault().UseCSharpRenderingBackend;

	public static ILibraryParams LibraryParams => GetJsonParamsOrDefault().Params;

	public static bool EnableDebugger => GetJsonParamsOrDefault().EnableDebugger;

	public static bool EnableDebuggerInBuild => GetJsonParamsOrDefault().EnableDebuggerInBuild;

	public static int DebuggerPort => GetJsonParamsOrDefault().DebuggerPort;

	public static ILibraryParams GetCompositeLibraryParams()
	{
		s_ExtendedLibraryParamsCached = GetJsonParamsOrDefault();
		CustomizeExtendedParams(ref s_ExtendedLibraryParamsCached);
		return s_ExtendedLibraryParamsCached.Params;
	}

	public static void CustomizeExtendedParams(ref ExtendedLibraryParams extendedLibraryParams)
	{
		if (OnCustomizeExtendedParams != null)
		{
			extendedLibraryParams = OnCustomizeExtendedParams.Invoke(extendedLibraryParams);
		}
	}

	public static ExtendedLibraryParams GetJsonParamsOrDefault()
	{
		if (!s_IsDirty && s_isExtendedLibraryParamsLoaded)
		{
			return s_ExtendedLibraryParamsCached;
		}

		s_isExtendedLibraryParamsLoaded = true;
		s_IsDirty = false;
		TextAsset textAsset = GetJson();
		if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
		{
			s_ExtendedLibraryParamsCached = JsonUtility.FromJson<ExtendedLibraryParams>(textAsset.text);
		}
		else
		{
			s_ExtendedLibraryParamsCached = new ExtendedLibraryParams();
		}

		return s_ExtendedLibraryParamsCached;
	}

	public static TextAsset GetJson()
	{
		return Resources.Load<TextAsset>(CohtmlLibraryParamsKey);
	}

	#if UNITY_EDITOR
	public static void SaveJson(ExtendedLibraryParams extendedLibraryParams)
	{
		s_IsDirty = true;
		string jsonData = JsonUtility.ToJson(extendedLibraryParams, true);
		File.WriteAllText(Path.GetFullPath(Utils.CombinePaths(Utils.CohtmlUpmPath, "Resources", CohtmlLibraryParamsKey) + ".json"), jsonData);
		AssetDatabase.Refresh();
	}
	#endif
}
}
