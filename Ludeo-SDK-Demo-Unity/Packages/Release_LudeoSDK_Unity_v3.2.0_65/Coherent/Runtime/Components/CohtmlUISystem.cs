using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cohtml.InputSystem;
using cohtml.Net;
#if UNITY_EDITOR
using UnityEditor.Compilation;
#endif

namespace cohtml
{
[AddComponentMenu("Cohtml/Cohtml UI System")]
[DisallowMultipleComponent]
public class CohtmlUISystem : MonoBehaviour
{
	public const string CohtmlDefaultUISystemName = "CohtmlDefaultUISystem";

	public static string DataPath => Application.dataPath;

	public static string StreamingAssetsPath => Application.streamingAssetsPath;

	public static CohtmlUISystem DefaultUISystem { get; private set; }

	public IUISystem SystemNative { get; private set; }

	[SerializeField]
	private CohtmlSystemSettings m_Settings;

	private List<CohtmlView> m_Views = new List<CohtmlView>();

	/// <summary>
	///  Id of registered UISystem reference in COHTML native library.
	/// </summary>
	public uint Id { get; private set; }

	public UserImagesManager UserImagesManager { get; set; }

	/// <summary>
	/// Gets the resource handler.
	/// </summary>
	/// <value>The resource handler.</value>
	public IResourceHandler ResourceHandler => Settings.ResourceHandler;

	/// <summary>
	/// Gets the localization manager.
	/// </summary>
	/// <value>The localization manager.</value>
	public ILocalizationManager LocalizationManager => Settings.LocalizationManager;

	/// <summary>
	/// Determines whether this instance is ready.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is ready; otherwise, <c>false</c>.
	/// </returns>
	public bool IsReady => SystemNative != null;

	/// <summary>
	/// Returns a List with the view components that belong to the UI system.
	/// </summary>
	/// <value>List with CohtmlView components.</value>
	public ReadOnlyCollection<CohtmlView> UIViews => m_Views.AsReadOnly();

	public CohtmlSystemSettings Settings
	{
		get
		{
			if (m_Settings == null)
			{
				m_Settings = GetComponent<CohtmlSystemSettings>();
				if (m_Settings == null)
				{
					m_Settings = gameObject.AddComponent<CohtmlSystemSettings>();
				}
			}

			return m_Settings;
		}
	}

	internal void AddView(CohtmlView view)
	{
		m_Views.Add(view);
	}

	internal bool RemoveView(CohtmlView view)
	{
		return m_Views.Remove(view);
	}

	public View CreateView(ViewSettings settings)
	{
		if (SystemNative == null)
		{
			return null;
		}

		return SystemNative.CreateView(settings, Id);
	}

	protected virtual void Awake()
	{
		Library.SetDependenciesPath();

		#if UNITY_EDITOR
		CompilationPipeline.compilationStarted += ShutDownPluginOnCompilationStarted;
		#endif

		Library.LoadModuleDependencies();

		// Enable Memory tracking before first UnityPlugin initialization in Player mode.
		// Make sure we get the client's choice before first cohtml.Net.UnityPlugin.Instance() call.
		cohtml.Net.UnityPlugin.EnableMemoryTracking(LibraryParamsManager.EnableMemoryTracking);
		cohtml.Net.UnityPlugin.SetUnityVersion(Application.unityVersion);
		UserImagesManager = new UserImagesManager(this);
		useGUILayout = false;

		GetDefaultUISystem();
	}

	protected virtual void Start()
	{
		if (!IsReady)
		{
			CreateNativeUISystem(Settings);
		}
	}

	protected virtual void Update()
	{
		if (SystemNative == null)
		{
			return;
		}

		SystemNative.Advance(Time.unscaledTime * 1000.0);

		UserImagesManager.UpdateLiveViews();

		// OnGUI is called on second frame so we need to synchronize
		// BeginFrame and EndFrame manually, otherwise BeginFrame will be called once more than EndFrame
		// and this will lead to breaking the state of resources
		// This is checked only on Windows platforms. Need further investigation. 
		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_XBOXONE
		if (Time.frameCount > 1)
		{
			#endif
			if (DefaultUISystem != null && DefaultUISystem == this && !LibraryParamsManager.ShouldUseCSharpBackend)
			{
				// Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
				GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.BackendBeginFrame, Id));
#pragma warning restore 618
			}

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_XBOXONE
		}
		#endif

		cohtml.Net.UnityPlugin.Instance().ProcessResources(Id);
	}

	protected virtual void OnRenderObject()
	{
		Library.ExecuteCSharpBackendCommandBuffer();
	}

	protected virtual void OnGUI()
	{
		if (Event.current.type == EventType.Repaint &&
		    (DefaultUISystem != null && DefaultUISystem == this) &&
		    !LibraryParamsManager.ShouldUseCSharpBackend)
		{
#pragma warning disable 618
			GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.BackendEndFrame, Id));
#pragma warning restore 618
		}
	}

	protected virtual void OnDestroy()
	{
		if (DefaultUISystem == this)
		{
			Library.Dispose();
		}

		if (UserImagesManager != null)
		{
			UserImagesManager.Dispose();
		}

		// Make sure to destroy the views first and the system after that.
		for (int i = 0; i < m_Views.Count; i++)
		{
			Destroy(m_Views[i]);
		}

		Destroy(Settings);

		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.DestroySystemRenderer, Id));
		}
		else
		{
			//Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
			GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.DestroySystemRenderer, Id));
#pragma warning restore 618
		}

		if (SystemNative != null)
		{
			SystemNative.Destroy();
			SystemNative = null;
		}
	}

	protected virtual void OnApplicationQuit()
	{
		cohtml.Net.UnityPlugin.Instance().OnApplicationQuit();
		Library.Dispose();

		DefaultUISystem = null;
	}

	private void ShutDownPluginOnCompilationStarted(object context)
	{
		if (Application.isPlaying)
		{
			LogHandler.LogWarning($"{context} detected. Shutting down.");
			ShutDown();
		}
	}

	public void CreateNativeUISystem(CohtmlSystemSettings systemSettings)
	{
		if (IsReady)
		{
			return;
		}

		if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
		{
			throw new ApplicationException("ARGB32 render texture format is not supported by the graphics device. " +
			                               "Cohtml can't function properly without such render texture.");
		}

		if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			throw new ApplicationException("Depth render texture format is not supported by the graphics device. " +
			                               "Cohtml can't function properly without such render texture.");
		}

		if (this == DefaultUISystem)
		{
			Library.MergeSystemDebugSettings(ref systemSettings);
		}

		SystemNative = Library.CreateUISystem(systemSettings.NativeSettings);
		if (SystemNative == null)
		{
			throw new ApplicationException("Creating a UI System failed!");
		}

		Id = SystemNative.GetId();
		LogHandler.Log($"{name} Id: {Id} created.");

		if (Settings.ResourceHandler is ISystemStorable)
		{
			((ISystemStorable)Settings.ResourceHandler).System = this;
		}

		FileReader.RegisterPreloadedFonts(SystemNative);

		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.CreateSystemRenderer, Id));
		}
		else
		{
			//Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
			GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.CreateSystemRenderer, Id));
#pragma warning restore 618
		}

		CohtmlInputHandler.GetOrInitInstance();

		UserImagesManager.SubscribePreloadedTextureReleased();
	}

	public static CohtmlUISystem GetDefaultUISystem()
	{
		if (DefaultUISystem == null)
		{
			GameObject systemObject = GameObject.Find(CohtmlDefaultUISystemName);
			if (systemObject != null)
			{
				DefaultUISystem = systemObject.GetComponent<CohtmlUISystem>();
			}

			if (DefaultUISystem == null)
			{
				DefaultUISystem = new GameObject(CohtmlDefaultUISystemName, typeof(CohtmlUISystem)).GetComponent<CohtmlUISystem>();
			}

			DontDestroyOnLoad(DefaultUISystem);
		}

		return DefaultUISystem;
	}

	private void ShutDown()
	{
		#if UNITY_EDITOR
		DestroyImmediate(this);
		#else
		Destroy(this);
		#endif
		if (DefaultUISystem == this)
		{
			cohtml.Net.UnityPlugin.Instance().DestroySystems();
			cohtml.Net.UnityPlugin.Instance().OnApplicationQuit();
			DefaultUISystem = null;
		}
	}

	#if COHERENT_RENDERING_PIPELINES
	internal void RefreshHDRPCallbacks()
	{
		m_Views.Sort((a, b) =>
		{
			if (a == null || b == null || a.RenderingCamera == null || b.RenderingCamera == null)
			{
				return 0;
			}

			return a.RenderingCamera.depth.CompareTo(b.RenderingCamera.depth);
		});

		foreach (CohtmlView view in m_Views)
		{
			view.RegisterRenderingEvents(false);
			if (view.isActiveAndEnabled)
			{
				view.RegisterRenderingEvents(true);
			}
		}
	}
	#endif
}
}
