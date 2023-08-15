using System;
using System.IO;
using cohtml.Net;
using UnityEngine;

namespace cohtml
{
public static class Library
{
	private static ILibrary s_Library;

	private static TaskScheduler s_TaskScheduler;

	#if UNITY_2019_3_OR_NEWER
	static UnityBackend s_Backend = null;

	public static UnityBackend RenderingBackend => s_Backend;
	#endif

	public static UnityPluginListener UnityPluginListener { get; set; }

	public static bool ShouldUseCSharpBackend => LibraryParamsManager.ShouldUseCSharpBackend;

	public static ILibraryParams LibraryParams => LibraryParamsManager.LibraryParams;

	public static bool IsLibraryInitialized => s_Library != null;

	public static Compositor UnityCompositor { get; set; }

	#if (UNITY_XBOXONE || UNITY_GAMECORE) && !UNITY_EDITOR
	// We are manually loading libraries on Xbox One and Xbox Series X, because they are not loading automatically.
	static IntPtr s_RenoirModuleHandle = IntPtr.Zero;
	static IntPtr s_HttpServer = IntPtr.Zero;
	static IntPtr s_MediaDecoders = IntPtr.Zero;
	#endif

	public static void LoadModuleDependencies()
	{
		#if UNITY_XBOXONE && !UNITY_EDITOR
		LoadModule(ref s_RenoirModuleHandle, "RenoirCore.XB1.dll");
		LoadModule(ref s_HttpServer, "HttpServer.Durango.dll");
		LoadModule(ref s_MediaDecoders, "MediaDecoders.Durango.dll");
		#elif UNITY_GAMECORE_XBOXONE && !UNITY_EDITOR
		LoadModule(ref s_RenoirModuleHandle, "RenoirCore.XboxOne.dll");
		LoadModule(ref s_HttpServer, "HttpServer.XboxOne.dll");
		LoadModule(ref s_MediaDecoders, "MediaDecoders.XboxOne.dll");
		#elif UNITY_GAMECORE_SCARLETT && !UNITY_EDITOR
		LoadModule(ref s_RenoirModuleHandle, "RenoirCore.Scarlett.dll");
		LoadModule(ref s_HttpServer, "HttpServer.Scarlett.dll");
		LoadModule(ref s_MediaDecoders, "MediaDecoders.Scarlett.dll");
		#endif
	}

	private static void LoadModule(ref IntPtr module, string moduleName)
	{
		if (module == IntPtr.Zero)
		{
			string dllPath = Utils.CombinePaths(CohtmlUISystem.DataPath, "Plugins", moduleName);

			// The handle isn't released because we expect to use it for the whole lifetime of the app
			module = PlatformExtensions.LoadModule(dllPath);
			if (module == IntPtr.Zero)
			{
				LogHandler.LogError(string.Format("Unable to load {0}", moduleName));
			}
		}
	}

	public static void RestoreProcessPath()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		string pluginsPath = GetPluginsFolderPath();
		string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

		if (currentPath.Contains(pluginsPath))
		{
			Environment.SetEnvironmentVariable("PATH", currentPath.Replace(pluginsPath + Path.PathSeparator, ""), EnvironmentVariableTarget.Process);
		}
		#endif
	}

	public static void SetDependenciesPath()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		string pluginsPath = GetPluginsFolderPath();
		string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

		if (!currentPath.Contains(pluginsPath))
		{
			Environment.SetEnvironmentVariable("PATH", pluginsPath + Path.PathSeparator + currentPath, EnvironmentVariableTarget.Process);
		}
		#endif
	}

	static string GetPluginsFolderPath()
	{
		string relativePluginsFolder = "Plugins";

		#if UNITY_EDITOR

		string currentArch = IntPtr.Size == 4 ? "x86" : "x86_64";
		relativePluginsFolder = Path.Combine(relativePluginsFolder, currentArch);

		string[] pluginsPaths = { Path.GetFullPath(Utils.CohtmlUpmPath), CohtmlUISystem.DataPath };
		for (int i = 0; i < pluginsPaths.Length; i++)
		{
			pluginsPaths[i] = Path.Combine(pluginsPaths[i], relativePluginsFolder);

			if (Directory.Exists(pluginsPaths[i]))
			{
				return pluginsPaths[i];
			}
		}

		return null;

		#else
		string pluginsFolder = CohtmlUISystem.StreamingAssetsPath;
		pluginsFolder = Utils.CombinePaths(pluginsFolder, "..", relativePluginsFolder);
		pluginsFolder = Path.GetFullPath(pluginsFolder);
		return pluginsFolder;
		#endif
	}

	public static IUISystem CreateUISystem(ISystemSettings settings)
	{
		CreateLibrary();

		if (!IsLibraryInitialized)
		{
			LogHandler.LogError("The Library isn't initialized!");
			return null;
		}

		SetDependenciesPath();

		IUISystem system = UnityPluginListener.OnInitializeSystem?.Invoke(settings);

		if (system == null)
		{
			system = s_Library.CreateSystem(settings);
		}

		RestoreProcessPath();

		return system;
	}

	public static void ExecuteCSharpBackendCommandBuffer()
	{
		if (!LibraryParamsManager.ShouldUseCSharpBackend)
		{
			return;
		}

		#if UNITY_2019_3_OR_NEWER
		RenderingBackend.ExecuteBuffers();
		#else
		LogHandler.LogWarning("C# rendering backend require Unity 2019.3 or newer!");
		#endif
	}

	public static void Dispose()
	{
		if (IsLibraryInitialized)
		{
			s_TaskScheduler.Dispose();
			s_Library.Dispose();
			s_Library = null;
		}

		FileReader.Reader.Dispose();

		#if (UNITY_XBOXONE || UNITY_GAMECORE) && !UNITY_EDITOR
		s_RenoirModuleHandle = IntPtr.Zero;
		s_HttpServer = IntPtr.Zero;
		s_MediaDecoders = IntPtr.Zero;
		#endif
	}

	public static void ExecuteWork(WorkType type)
	{
		s_Library.ExecuteWork(type, WorkExecutionMode.WEM_UntilQueueEmpty);
	}

	public static void ScheduleWork(WorkType type)
	{
		s_TaskScheduler.ScheduleTask(type);
	}

	public static void CreateLibrary()
	{
		if (IsLibraryInitialized)
		{
			return;
		}

		if (UnityPluginListener == null)
		{
			UnityPluginListener = new UnityPluginListener();
		}

		CheckLicense();

		s_Library = UnityPluginListener.OnInitializeLibrary?.Invoke();
		if (!IsLibraryInitialized)
		{
			s_Library = cohtml.Net.Library.Initialize(License.COHTML_LICENSE_KEY, LibraryParamsManager.GetCompositeLibraryParams(), UnityPluginListener, SystemInfo.deviceModel);
		}

		if (!IsLibraryInitialized)
		{
			throw new NullReferenceException("Library is not initialized.");
		}

		s_TaskScheduler = new TaskScheduler();
		s_TaskScheduler.Start();

		#if UNITY_2019_3_OR_NEWER
		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			s_Backend = new UnityBackend();
			s_Backend.Initialize();
			LogHandler.Log("C# rendering backend enabled!");
		}
		#endif

		UnityCompositor = new Compositor();
		UnityCompositor.SendManagedFuncitonsToNative();

		UnityPlugin.Instance().SetLibraryMultithreadAwareness(LibraryParamsManager.ShouldUseCSharpBackend);
		UnityPlugin.Instance().SetShouldUseCSharpBackend(LibraryParamsManager.ShouldUseCSharpBackend);
	}

	private static void CheckLicense()
	{
		bool haveLicense = !string.IsNullOrEmpty(License.COHTML_LICENSE_KEY);
		if (UnityPluginListener.OnHaveLicenseCheck != null)
		{
			haveLicense = UnityPluginListener.OnHaveLicenseCheck.Invoke();
		}

		if (!haveLicense)
		{
			throw new ApplicationException("You must supply a license key in order to start the library! " +
			                               "Follow the instructions in the manual for editing the " +
			                               "License.cs file.");
		}
	}

	public static void MergeSystemDebugSettings(ref CohtmlSystemSettings systemSettings)
	{
		LibraryParamsManager.GetJsonParamsOrDefault();
		systemSettings.m_EnableDebugger = LibraryParamsManager.EnableDebugger;
		systemSettings.m_EnableDebuggerInBuild = LibraryParamsManager.EnableDebuggerInBuild;
		systemSettings.DebuggerPort = LibraryParamsManager.DebuggerPort;
	}
}
}
