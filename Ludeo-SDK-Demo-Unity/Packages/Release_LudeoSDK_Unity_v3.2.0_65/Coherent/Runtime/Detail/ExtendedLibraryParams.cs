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
using cohtml.Net;
using UnityEngine;

namespace cohtml
{
/// <summary>
/// Represent the parameters required to create a Cohtml Library.
/// The properties can be set from the context menu by opening the Gameface/Configure Library window.
/// They can also be changed through a script when subscribing to 'LibraryParamsManager.OnCustomizeExtendedParams'.
/// </summary>
[Serializable]
public class ExtendedLibraryParams
{
	public const int DefaultTextAtlasDimension = 1024;

	[SerializeField]
	private Severity loggingSeverity = Severity.Error;

	[SerializeField]
	private string defaultStyleFontFamily;

	[SerializeField]
	private float pathTessellationThresholdRatio;

	[SerializeField]
	private bool allowMultipleRenderingThreads;

	[SerializeField]
	private Vector2Int textAtlasDimensions;

	public bool EnableMemoryTracking;

	// UI System DevTools Debugger properties
	public bool EnableDebugger;
	public int DebuggerPort;
	public bool EnableDebuggerInBuild;

	[SerializeField]
	private bool useCSharpRenderingBackend;

	private ILibraryParams libraryParams;

	/// <summary>
	/// Initialize new Extended Params object with default values.
	/// </summary>
	public ExtendedLibraryParams()
	{
		libraryParams = new LibraryParams
		{
			LogHandler = LogHandler.Instance,
			FileSystemReader = FileReader.Reader,
			ResourceThreadsCountHint = TaskScheduler.ThreadCount,
			#if UNITY_SWITCH
			WritableDirectory = "",
			#else
			WritableDirectory = Application.persistentDataPath,
			#endif
			RenoirLibraryParams = new RenderingLibraryParams()
		};

		LoggingSeverity = Severity.Error;
		DefaultStyleFontFamily = "Droid Sans";
		PathTessellationThresholdRatio = 2.5f;
		AllowMultipleRenderingThreads = false;
		TextAtlasDimensions = new Vector2Int(DefaultTextAtlasDimension, DefaultTextAtlasDimension);

		UseCSharpRenderingBackend = false;
		EnableDebugger = true;
		DebuggerPort = CohtmlSystemSettings.DefaultDebuggerPort;
		EnableDebuggerInBuild = false;
		EnableMemoryTracking = false;
	}

	/// <summary>
	/// Sync the library params with internal member values.
	/// </summary>
	public ILibraryParams Params
	{
		get
		{
			LoggingSeverity = loggingSeverity;
			DefaultStyleFontFamily = defaultStyleFontFamily;
			PathTessellationThresholdRatio = pathTessellationThresholdRatio;
			AllowMultipleRenderingThreads = allowMultipleRenderingThreads;
			TextAtlasDimensions = textAtlasDimensions;
			return libraryParams;
		}
		set
		{
			// Prevent to lose important references
			Net.ILogHandler logHandler = libraryParams.LogHandler;
			IFileSystemReader fileSystemReader = libraryParams.FileSystemReader;
			libraryParams = value;
			libraryParams.LogHandler = logHandler;
			libraryParams.FileSystemReader = fileSystemReader;
		}
	}

	public bool UseCSharpRenderingBackend
	{
		get
		{
			#if UNITY_SWITCH
				return true;
			#elif UNITY_ANDROID
				if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan)
				{
					return useCSharpRenderingBackend;
				}
				return false;
			#else
			return useCSharpRenderingBackend;
			#endif
		}

		set => useCSharpRenderingBackend = value;
	}

	public Severity LoggingSeverity
	{
		get => loggingSeverity;
		set
		{
			loggingSeverity = value;
			libraryParams.LoggingSeverity = value;
		}
	}

	public Vector2Int TextAtlasDimensions
	{
		get => textAtlasDimensions;
		set
		{
			textAtlasDimensions = value;
			libraryParams.RenoirLibraryParams.TextAtlasWidth = (uint)value.x;
			libraryParams.RenoirLibraryParams.TextAtlasHeight = (uint)value.y;
		}
	}

	public string DefaultStyleFontFamily
	{
		get => defaultStyleFontFamily;
		set
		{
			defaultStyleFontFamily = value;
			libraryParams.DefaultStyleFontFamily = value;
		}
	}

	public float PathTessellationThresholdRatio
	{
		get => pathTessellationThresholdRatio;
		set
		{
			pathTessellationThresholdRatio = value;
			libraryParams.RenoirLibraryParams.PathTessellationThresholdRatio = value;
		}
	}

	public bool AllowMultipleRenderingThreads
	{
		get => allowMultipleRenderingThreads;
		set
		{
			allowMultipleRenderingThreads = value;
			libraryParams.RenoirLibraryParams.AllowMultipleRenderingThreads = value;
		}
	}
}
}
