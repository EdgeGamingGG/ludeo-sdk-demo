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

using UnityEngine;
#if COHERENT_RENDERING_PIPELINES
using UnityEngine.Rendering;
#endif

using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using cohtml.Net;
using renoir;

namespace cohtml
{
[AddComponentMenu("Cohtml/Cohtml View")]
public class CohtmlView : MonoBehaviour
{
	public const int DefaultWidth = 1280;
	public const int DefaultHeight = 720;
	#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
	static CohtmlView()
	{
		Library.SetDependenciesPath();
	}
	#endif

	private void EnsurePostEffectRenderer(bool enabled)
	{
		PostEffectRenderer drawComponent = GetComponent<PostEffectRenderer>();
		if (drawComponent != null)
		{
			drawComponent.enabled = enabled;
		}
		else if (enabled)
		{
			gameObject.AddComponent<PostEffectRenderer>();
		}
	}

	/// <summary>
	/// Creates the ViewListener instance for the view. Change to allow usage of custom ViewListener
	/// </summary>
	public static Func<ViewListener> ViewListenerFactoryFunc = () => { return new ViewListener(); };

	/// <summary>
	/// Creates the TextInputHandler instance for the view. Change to allow usage of custom TextInputHandler
	/// </summary>
	public static Func<TextInputHandler> TextInputHandlerFactoryFunc = () => { return new TextInputHandler(); };

	uint m_ViewId;

	[HideInInspector]
	[SerializeField]
	private CohtmlUISystem m_UISystem;

	/// <summary>
	/// Gets or sets the UI System used by this view
	/// </summary>
	/// <value>
	/// The UI System component
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "UI System",
		Tooltip = "UI System to be used by this view",
		IsStatic = true)]
	public CohtmlUISystem CohtmlUISystem
	{
		get
		{
			if (Application.isPlaying && m_UISystem == null)
			{
				m_UISystem = CohtmlUISystem.GetDefaultUISystem();
			}

			return m_UISystem;
		}
		set
		{
			if (View != null)
			{
				throw new ApplicationException("UI System can't be changed" +
				                               " if the view has already been created");
			}

			m_UISystem = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	string m_Page = "coui://UIResources/Hello/hello.html";

	/// <summary>
	/// Gets or sets the URL of the view
	/// </summary>
	/// <value>
	/// The loaded URL of view
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "URL",
		Tooltip = "Indicates the URL that will be initially loaded",
		IsStatic = false)]
	public string Page
	{
		get { return m_Page; }
		set
		{
			if (m_Page == value || value == null)
			{
				return;
			}

			m_Page = value;

			if (View != null)
			{
				View.LoadURL(m_Page);
				LogHandler.Log($"\"{name}\" page changed to {m_Page}.");
			}
		}
	}

	[HideInInspector]
	[SerializeField]
	int m_Width = DefaultWidth;

	/// <summary>
	/// Gets or sets the width of the view.
	/// </summary>
	/// <value>
	/// The width.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Width",
		Tooltip = "Indicates the width of the View",
		IsStatic = false)]
	public int Width
	{
		get { return m_Width; }
		set { Resize(value, m_Height); }
	}

	[HideInInspector]
	[SerializeField]
	int m_Height = DefaultHeight;

	/// <summary>
	/// Gets or sets the height of the view.
	/// </summary>
	/// <value>
	/// The height.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Height",
		Tooltip = "Indicates the height of the View",
		IsStatic = false)]
	public int Height
	{
		get { return m_Height; }
		set { Resize(m_Width, value); }
	}

	[HideInInspector]
	[SerializeField]
	bool m_ComplexSelectorsEnabled = false;

	[HideInInspector]
	[SerializeField]
	bool m_UISurfacePartitioning = false;

	/// <summary>
	/// Gets or sets a value indicating whether this view supports complex CSS selectors.
	/// </summary>
	/// <value>
	/// <c>true</c> if view supports complex CSS selectors; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Complex CSS selectors",
		Tooltip = "Enables complex CSS selectors for this View",
		IsStatic = true)]
	public bool EnableComplexSelectors
	{
		get { return m_ComplexSelectorsEnabled; }
		set
		{
			if (m_ComplexSelectorsEnabled == value)
			{
				return;
			}

			if (View != null)
			{
				throw new ApplicationException("Complex CSS selectors can't be enabled/disabled if the view has already been created");
			}

			m_ComplexSelectorsEnabled = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this view is in UI Surface Partitioning Mode
	/// </summary>
	/// <value>
	/// <c>true</c> if view is in UI Surface Partitioning Mode; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
	PrettyName = "UI Surface Partitioning",
		Tooltip = "The View will be in UI Surface Partitioning Mode",
		IsStatic = true)]
	public bool EnableUISurfacePartitioning
	{
		get { return m_UISurfacePartitioning; }
		set
		{
			if (m_UISurfacePartitioning == value)
			{
				return;
			}

			m_UISurfacePartitioning = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	bool m_IsTransparent = true;

	/// <summary>
	/// Gets or sets a value indicating whether this view supports transparency.
	/// </summary>
	/// <value>
	/// <c>true</c> if view supports transparency; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Rendering,
		PrettyName = "Is transparent",
		Tooltip = "Events triggered in the UI will fire Unity messages",
		IsStatic = true)]
	public bool IsTransparent
	{
		get { return m_IsTransparent; }
		set
		{
			if (m_IsTransparent == value)
			{
				return;
			}

			if (View != null)
			{
				throw new ApplicationException("Transparency can't be changed if the view has already been created");
			}

			m_IsTransparent = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	bool m_PixelPerfect = true;

	/// <summary>
	/// If checked, the view will use the camera's width and height
	/// </summary>
	/// <value>
	/// <c>true</c> if we want to use camera's width and height; otherwise <c>false</c>.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Rendering,
		PrettyName = "Pixel Perfect",
		Tooltip = "The View will be automatically resized to always match the size of the camera",
		IsStatic = true)]
	public bool PixelPerfect
	{
		get { return m_PixelPerfect; }
		set { m_PixelPerfect = value; }
	}

	[HideInInspector]
	[SerializeField]
	bool m_DrawAsPostEffect;

	/// <summary>
	/// Gets or sets a value indicating whether this view is drawn after post effects.
	/// </summary>
	/// <value>
	/// <c>AfterPostEffects</c> if the view is drawn after post effects; otherwise, <c>false</c>.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Rendering,
		PrettyName = "Draw as post-effect",
		Tooltip = "Enable when using post effects (camera Views only). The option is automatically enabled when \"Enable Backdrop Filter\" is activated.",
		IsStatic = false)]
	public bool DrawAsPostEffect
	{
		get { return m_DrawAsPostEffect || m_EnableBackdropFilter; }
		set
		{
			if (m_DrawAsPostEffect == value)
			{
				return;
			}

			EnsurePostEffectRenderer(value);
			m_DrawAsPostEffect = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	bool m_EnableBackdropFilter = false;

	/// <summary>
	/// If checked, the view elements with the backdrop filter will be able to filter content from the scene.
	/// </summary>
	/// <value>
	/// <c>true</c><c>false</c>.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Rendering,
		PrettyName = "Enable Backdrop Filter",
		Tooltip = "Elements with backdrop-filter will be able to filter content from the scene. This option automatically enables \"Draw as post effect\".",
		IsStatic = true)]
	public bool EnableBackdropFilter
	{
		get { return m_EnableBackdropFilter; }
		set
		{
			m_EnableBackdropFilter = value;
			EnsurePostEffectRenderer(DrawAsPostEffect);
		}
	}

	[HideInInspector]
	[SerializeField]
	bool m_WideTextures = false;

	/// <summary>
	/// If checked, the view will use 16 bit textures for all intermediate render targets
	/// </summary>
	/// <value>
	/// <c>true</c> if we want to use 16 bit textures for render targets; otherwise <c>false</c>.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Rendering,
		PrettyName = "Wide Color Textures",
		Tooltip = "Internally the view will use 16 bit textures as intermediate render targets",
		IsStatic = true)]
	public bool WideColorTextures
	{
		get { return m_WideTextures; }
		set { m_WideTextures = value; }
	}

	[HideInInspector]
	[SerializeField]
	bool m_ApplyGammaCorrection = true;

	/// <summary>
	/// Apply a Gamma Correction to Cohtml shader material in Linear Color Space.
	/// </summary>
	/// <value>
	/// Value by default is false. True will apply gamma correction property color multiplier.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Rendering,
	PrettyName = "Apply Gamma Correction",
	Tooltip = "Enable the Cohtml shader material gamma correction in Linear Color Space.",
	IsStatic = true)]
	public bool ApplyGammaCorrection
		{
		get => m_ApplyGammaCorrection;
		set => m_ApplyGammaCorrection = value;
	}

	public bool IsOnScreen { get; private set; }

	[HideInInspector]
	[SerializeField]
	private bool raycastTarget = true;

	/// <summary>
	/// Determines whether this view receives input.
	/// This property is ignored when AutoFocus is enabled.
	/// AutoFocus will be disabled if you set this property.
	/// All automatic processing and reading of this property is done in the
	/// `LateUpdate()` / `OnGUI()` callbacks in Unity, letting you do all your logic
	/// for View focus in `Update()`.
	/// </summary>
	/// <value>
	/// <c>true</c> if this view receives input; otherwise, <c>false</c>.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Input,
		PrettyName = "Raycast Target",
		Tooltip = "Determines whether this view will receive input.",
		IsStatic = false)]
	public bool RaycastTarget
	{
		get => isActiveAndEnabled && raycastTarget;
		set => raycastTarget = value;
	}

	[HideInInspector]
	[SerializeField]
	bool m_EnableBindingAttribute = true;

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="CohtmlView"/> enables usage of the CoherentMethod attribute
	/// in components in the host GameObject.
	/// When true, all the components in the host GameObject are inspected for the CoherentMethod attribute (in the Start() function)
	/// and the decorated methods are automatically bound when the ReadyForBindings event is received.
	/// </summary>
	/// <value>
	/// <c>true</c> if usage of the CoherentMethod is enabled; otherwise, <c>false</c>.
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.Scripting,
		PrettyName = "Enable [CoherentMethodAttribute]",
		Tooltip = "Allows automatic binding of methods to the UI",
		IsStatic = true)]
	public bool EnableBindingAttribute
	{
		get { return m_EnableBindingAttribute; }
		set { m_EnableBindingAttribute = value; }
	}

	/// <summary>
	/// Gets the camera the view is rendering on or null if the view is rendered on a surface.
	/// </summary>
	/// <value>The camera used by the view or null.</value>
	public Camera RenderingCamera { get; private set; }

	/// <summary>
	/// Gets the underlying View instance.
	/// </summary>
	/// <value>
	/// The underlying View instance.
	/// </value>
	public View View { get; private set; }

	ViewListener m_Listener;

	/// <summary>
	/// Gets the view listener.
	/// </summary>
	/// <value>The view listener.</value>
	public ViewListener Listener
	{
		get
		{
			if (m_Listener == null)
			{
				if (ViewListenerFactoryFunc != null)
				{
					m_Listener = ViewListenerFactoryFunc();
				}

				if (m_Listener == null)
				{
					LogHandler.LogWarning("CohtmlView: Unable to create view" +
					                      "listener using factory function! " +
					                      "Falling back to default listener.");
					m_Listener = new ViewListener();
				}
			}

			return m_Listener;
		}
	}

	private TextInputHandler m_TextInputHandler;

	/// <summary>
	/// Gets the text input handler.
	/// </summary>
	/// <value>The Text input handler.</value>
	public TextInputHandler TextInputHandler
	{
		get
		{
			if (m_TextInputHandler == null)
			{
				if (TextInputHandlerFactoryFunc != null)
				{
					m_TextInputHandler = TextInputHandlerFactoryFunc();
				}

				if (m_TextInputHandler == null)
				{
					LogHandler.LogWarning("CohtmlView: Unable to create text input handler " +
					                      "using factory function! " +
					                      "Falling back to default text input handler.");
					m_TextInputHandler = new TextInputHandler();
				}
			}

			return m_TextInputHandler;
		}
	}

	RenderTexture m_UserBackgroundTemp;
	IntPtr m_LastSetViewTexturePtr;
	IntPtr m_UserBackgroundTexturePtr;

	public RenderTexture ViewTexture { get; private set; }

	RenderTexture m_DepthTexture;
	IntPtr m_LastSetDepthTexturePtr;
	Material m_RenderMaterial;
	public Material RenderMaterial => m_RenderMaterial;
	List<MethodBindingInfo> m_CoherentMethods;
	bool m_HasDrawn;
	private bool m_ShouldRedraw = false;

	private AudioSubscriber m_AudioSubscriber;

	[HideInInspector]
	[SerializeField]
	private AudioSource m_AudioSource;

	/// <summary>
	/// Gets or sets the AudioSource for the CohtmlView.
	/// </summary>
	/// <value>
	/// The AudioSource component
	/// </value>
	[ExposeProperty(
		Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Audio Source",
		Tooltip = "The AudioSource for the the Coherent View",
		IsStatic = true)]
	public AudioSource AudioSource
	{
		get
		{
			if (m_AudioSource == null && Application.isPlaying)
			{
				m_AudioSource = GetComponent<AudioSource>();

				if (m_AudioSource == null)
				{
					m_AudioSource = gameObject.AddComponent<AudioSource>();
				}
			}

			return m_AudioSource;
		}
		set { m_AudioSource = value; }
	}

	/// <summary>
	/// Resize the view to the specified width and height.
	/// </summary>
	/// <param name='width'>
	/// New width for the view.
	/// </param>
	/// <param name='height'>
	/// New height for the view.
	/// </param>
	public void Resize(int width, int height)
	{
		width = Mathf.Max(1, width);
		height = Mathf.Max(1, height);

		if (width == m_Width && height == m_Height)
		{
			return;
		}

		m_Width = width;
		m_Height = height;

		if (View != null)
		{
			View.Resize((uint)m_Width, (uint)m_Height);
			if (!m_UISurfacePartitioning)
			{
				RecreateRenderTarget();
			}
			else
			{
				cohtml.Net.UnityPlugin.Instance().SetRenderTextureData(System.IntPtr.Zero, System.IntPtr.Zero, (uint)Width, (uint)Height, 1, 0);
				cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.SetRenderTarget, m_UISystem.Id, m_ViewId));
			}
		}
	}

	/// <summary>
	/// Returns the camera dimensions of the current view.
	/// </summary>
	public bool GetCamSize(out int width, out int height)
	{
		if (RenderingCamera != null)
		{
			width = RenderingCamera.pixelWidth;
			height = RenderingCamera.pixelHeight;

			return true;
		}

		width = -1;
		height = -1;

		return false;
	}

	/// <summary>
	/// Returns the ratio between the view size and camera size.
	/// </summary>
	public Vector2 ViewToCamSizeRatio()
	{
		if (RenderingCamera != null)
		{
			return new Vector2(m_Width / RenderingCamera.pixelWidth,
				m_Height / RenderingCamera.pixelHeight);
		}

		return Vector2.one;
	}

	public void UpdateUserBackgroundIfNeeded(RenderTexture cameraTarget)
	{
		if (cameraTarget != null)
		{
			IntPtr currentCameraTexturePtr = cameraTarget.GetNativeTexturePtr();

			if (m_UserBackgroundTexturePtr != currentCameraTexturePtr)
			{
				m_UserBackgroundTexturePtr = currentCameraTexturePtr;

				if (LibraryParamsManager.ShouldUseCSharpBackend)
				{
					System.Runtime.InteropServices.GCHandle gch1 = System.Runtime.InteropServices.GCHandle.Alloc(cameraTarget);
					currentCameraTexturePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(gch1);
				}

				renoir.PixelFormat userBackgroundFormat = BackendUtilities.UnityToRenoirPixelFormat(cameraTarget.graphicsFormat);
#if COHERENT_RENDERING_PIPELINES
				uint flipY = 0;
#else
				uint flipY = 1;
#endif

				cohtml.Net.UnityPlugin.Instance().SetUserBackground(m_UISystem.Id, m_ViewId, currentCameraTexturePtr,
					(uint)cameraTarget.width, (uint)cameraTarget.height, (int)userBackgroundFormat, flipY);
			}
		}
	}

	protected virtual void Start()
	{
		PostEffectRenderer drawComponent = GetComponent<PostEffectRenderer>();
		DrawAsPostEffect = drawComponent != null && drawComponent.enabled;

		if (m_UISystem == null)
		{
			m_UISystem = CohtmlUISystem.GetDefaultUISystem();
		}

		m_UISystem.AddView(this);

		if (EnableBindingAttribute)
		{
			m_CoherentMethods = MethodHelper.GetMethodsInGameObject(gameObject);
		}
		else
		{
			m_CoherentMethods = new List<MethodBindingInfo>();
		}

		if (RenderingCamera == null)
		{
			RenderingCamera = GetComponent<Camera>();
		}

		IsOnScreen = RenderingCamera;
		if (!IsOnScreen || (m_UISurfacePartitioning && SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan))
		{
			m_UISurfacePartitioning = false;
		}

		AddMeshToWorldView();
		CreateView();
		#if COHERENT_RENDERING_PIPELINES
		m_UISystem.RefreshHDRPCallbacks();
		#endif
	}

	protected virtual void Update()
	{
		if (View != null)
		{
			if (m_ShouldRedraw)
			{
				m_ShouldRedraw = false;
				View.ContinuousRepaint(m_ShouldRedraw);
			}

			if (!m_UISurfacePartitioning && (!ViewTexture.IsCreated() || !m_DepthTexture.IsCreated()))
			{
				RecreateRenderTarget();
			}

			if (RenderingCamera != null && m_PixelPerfect)
			{
				int width;
				int height;

				if (GetCamSize(out width, out height))
				{
					Resize(width, height);
				}
			}

			View.Advance(m_UISystem.Id, Time.unscaledTime * 1000.0);

			m_HasDrawn = false;
		}
	}

	protected virtual void OnPreRender()
	{
		Draw();
	}

	protected virtual void OnWillRenderObject()
	{
		Draw();
	}

	protected virtual void OnPostRender()
	{
		RenderTexture cameraTarget = RenderingCamera.activeTexture;
		UpdateUserBackgroundIfNeeded(cameraTarget);

		// at this point the rendering of the scene is finished and we can draw the UI and access the camera's scene texture
		Draw();

		if (!m_DrawAsPostEffect && m_UISurfacePartitioning)
		{
			Library.UnityCompositor.PaintCompositionsForView(m_ViewId, m_RenderMaterial);
		}

		if (!m_DrawAsPostEffect && ViewTexture)
		{
			GL.PushMatrix();
			GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
			Graphics.DrawTexture(new Rect(0f, 0, Screen.width, Screen.height),
				ViewTexture,
				new Rect(0f, 0f, 1f, 1f),
				0, 0, 0, 0,
				UnityEngine.Color.white,
				m_RenderMaterial);
			GL.PopMatrix();
		}
	}

	protected virtual void OnDestroy()
	{
		if (Library.IsLibraryInitialized)
		{
			Library.UnityCompositor.DeregisterView(m_ViewId);
		}

		if (m_RenderMaterial != null)
		{
			Destroy(m_RenderMaterial);
		}

		if (ViewTexture != null)
		{
			ViewTexture.Release();
			Destroy(ViewTexture);
		}

		if (m_DepthTexture != null)
		{
			m_DepthTexture.Release();
			Destroy(m_DepthTexture);
		}

		if (m_UISystem != null)
		{
			m_UISystem.RemoveView(this);
		}

		if (View != null)
		{
			if (LibraryParamsManager.ShouldUseCSharpBackend)
			{
				cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.DestroyViewRenderer, m_UISystem.Id, m_ViewId));
			}
			else
			{
				//Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
				GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.DestroyViewRenderer, m_UISystem.Id, m_ViewId));
#pragma warning restore 618
			}

			View.UnloadDocument();
			View.Destroy(m_UISystem.Id);
		}

		m_AudioSubscriber?.Dispose();

		m_TextInputHandler = null;
		m_Listener = null;
	}

	#if COHERENT_RENDERING_PIPELINES
	private CommandBuffer m_HdrpBuffer;
	private CommandBuffer m_BlitBuffer;
	private bool m_RenderingEventsRegistered;

	protected virtual void OnEnable()
	{
		RenderingCamera = GetComponent<Camera>();
		if (RenderingCamera != null)
		{
			m_HdrpBuffer = new CommandBuffer
			{
				name = "cohtml"
			};

			m_BlitBuffer = new CommandBuffer
			{
				name = "cohtml[user bg blit]"
			};
		}

		if (m_UISystem != null)
		{
			m_UISystem.RefreshHDRPCallbacks();
		}
	}

	protected virtual void OnDisable()
	{
		if (m_UISystem != null)
		{
			m_UISystem.RefreshHDRPCallbacks();
		}

		if (RenderingCamera != null)
		{
			m_HdrpBuffer.Release();
			m_BlitBuffer.Release();
		}
	}

	internal void RegisterRenderingEvents(bool register)
	{
		if (register)
		{
			if (!m_RenderingEventsRegistered && RenderingCamera != null)
			{
				m_RenderingEventsRegistered = true;
				// it seems that endCameraRendering delegate does not do the trick
				// and the cohtml commands are still rendered after the scene; hence
				// we do the whle painting on "endFrameRendering" -- this is always after
				// the camera has finished rendering
				RenderPipelineManager.endFrameRendering += OnEndFrame;
			}
		}
		else
		{
			m_RenderingEventsRegistered = false;
			RenderPipelineManager.endFrameRendering -= OnEndFrame;
		}
	}

	void OnEndFrame(ScriptableRenderContext ctx, Camera[] cameras)
	{
		if (!ViewTexture && !m_UISurfacePartitioning)
		{
			return;
		}

		if (m_EnableBackdropFilter)
		{
			if (!m_UserBackgroundTemp || m_UserBackgroundTemp.width != m_Width
				||  m_UserBackgroundTemp.height != m_Height)
			{
				m_UserBackgroundTemp = new RenderTexture(m_Width, m_Height, 0,
						m_WideTextures ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
				m_UserBackgroundTemp.Create();
			}

			m_BlitBuffer.Blit(BuiltinRenderTextureType.CameraTarget, m_UserBackgroundTemp);
			Graphics.ExecuteCommandBuffer(m_BlitBuffer);
			m_BlitBuffer.Clear();
			UpdateUserBackgroundIfNeeded(m_UserBackgroundTemp);
		}

		Draw();

		Library.ExecuteCSharpBackendCommandBuffer();

		for (var i = 0; i < cameras.Length; ++i)
		{
			if (cameras[i] == RenderingCamera)
			{
				RenderHDRP();
				ctx.Submit();
				return;
			}
		}
	}

	void RenderHDRP()
	{
		if (!m_UISurfacePartitioning)
		{
			m_HdrpBuffer.Blit(ViewTexture, BuiltinRenderTextureType.CurrentActive, m_RenderMaterial);
			Graphics.ExecuteCommandBuffer(m_HdrpBuffer);
			m_HdrpBuffer.Clear();
		}
		else
		{
			Library.UnityCompositor.PaintCompositionsForView(m_ViewId, m_RenderMaterial);
		}

	}

	#endif

	void BindCoherentMethods()
	{
		foreach (var item in m_CoherentMethods)
		{
			if (item.IsEvent)
			{
				View.RegisterForEvent(item.ScriptEventName, item.BoundFunction);
			}
			else
			{
				View.BindCall(item.ScriptEventName, item.BoundFunction);
			}
		}
	}

	void CreateView()
	{
		if (!m_UISystem.IsReady)
		{
			m_UISystem.CreateNativeUISystem(m_UISystem.Settings);
		}

		if (string.IsNullOrEmpty(Page))
		{
			throw new ApplicationException("The Page of a view must not be null or empty.");
		}

		var viewSettings = new ViewSettings
		{
			Width = (uint)m_Width,
			Height = (uint)m_Height,
			Listener = Listener,
			TextInputHandler = TextInputHandler,
			EnableComplexCSSSelectorsStyling = m_ComplexSelectorsEnabled,
			EnableUISurfacePartitioning = m_UISurfacePartitioning
		};

		if (LibraryParamsManager.ShouldUseCSharpBackend || m_UISurfacePartitioning)
		{
			viewSettings.ExecuteCommandProcessingWithLayout = true;
		}

		View = m_UISystem.CreateView(viewSettings);
		if (View == null)
		{
			enabled = false;
			return;
		}

		m_ViewId = View.GetId();
		LogHandler.Log("View created with URL: " + m_Page);

		m_AudioSubscriber = new AudioSubscriber(this);
		m_AudioSubscriber.Subscribe();

		Library.UnityCompositor.RegisterView(m_ViewId);

		View.LoadURL(m_Page);

		if (m_RenderMaterial == null)
		{
			CreateRenderMaterial();
		}

		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.CreateViewRenderer, m_UISystem.Id, m_ViewId));
		}
		else
		{
			//Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
			GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.CreateViewRenderer, m_UISystem.Id, m_ViewId));
#pragma warning restore 618
		}

		if (m_UISurfacePartitioning)
		{
			cohtml.Net.UnityPlugin.Instance().SetRenderTextureData(System.IntPtr.Zero, System.IntPtr.Zero, (uint)Width, (uint)Height, 1, 0);
			cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.SetRenderTarget, m_UISystem.Id, m_ViewId));
		}
		else
		{
			RecreateRenderTarget();
		}
	}

	private void CreateRenderMaterial()
	{
		Shader shader = GetShader();

		if (shader == null)
		{
			LogHandler.LogError(name + ": No shader found");
			return;
		}

		SetRenderMaterial(shader);
		AttachMaterialToCamera();
	}

	void AttachMaterialToCamera()
	{
		if (RenderingCamera == null)
		{
			Renderer r = GetComponent<Renderer>();

			if (r != null)
			{
				r.material = m_RenderMaterial;
			}
			else
			{
				LogHandler.LogError($"{name} has missing renderer!");
			}
		}
	}

	void SetRenderMaterial(Shader shader)
	{
		m_RenderMaterial = new Material(shader);

		if (QualitySettings.activeColorSpace == ColorSpace.Linear && m_ApplyGammaCorrection)
		{
			m_RenderMaterial.SetFloat("_ColorMultiplier", 2.2f);
		}
		
		m_RenderMaterial.name = "CoherentRenderMaterial" + View.GetId();
		if (!m_UISurfacePartitioning)
		{
			m_RenderMaterial.SetTexture("_MainTex", ViewTexture);
		}
	}

	Shader GetShader()
	{
		Shader shader;
		#if COHERENT_RENDERING_PIPELINES
		shader = Shader.Find(RenderingCamera != null
								? "Coherent/ViewShader"
								: IsTransparent
									? "Shader Graphs/CohtmlRPLitDiffuseTransparent"
									: "Shader Graphs/CohtmlRPLitDiffuse");
		#else
		shader = Shader.Find(RenderingCamera != null
			? "Coherent/ViewShader"
			: IsTransparent
				? "Coherent/TransparentDiffuse"
				: "Coherent/Diffuse");
		#endif

		return shader;
	}

	/// <summary>
	/// Add a Quad mesh renderer to world CohtmlView if not exist.
	/// </summary>
	public void AddMeshToWorldView()
	{
		if (!IsOnScreen && GetComponent<MeshFilter>() == null)
		{
			this.gameObject.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
			this.gameObject.AddComponent<MeshRenderer>();
			this.gameObject.AddComponent<MeshCollider>();
			this.transform.localScale = new Vector3(16f, 9f, 1f);
			LogHandler.LogWarning($"Create Quad renderer in {this.name} object. The in-world {this} Mesh is mandatory to render properly!");
		}
	}

	void SetRenderTarget(IntPtr viewTexturePtr, IntPtr depthTexturePtr)
	{
		m_LastSetViewTexturePtr = viewTexturePtr;
		m_LastSetDepthTexturePtr = depthTexturePtr;

		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			System.Runtime.InteropServices.GCHandle gch1 = System.Runtime.InteropServices.GCHandle.Alloc(ViewTexture);
			System.Runtime.InteropServices.GCHandle gch2 = System.Runtime.InteropServices.GCHandle.Alloc(m_DepthTexture);

			viewTexturePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(gch1);
			depthTexturePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(gch2);
		}

		cohtml.Net.UnityPlugin.Instance().SetRenderTextureData(
			viewTexturePtr,
			depthTexturePtr,
			(uint)ViewTexture.width,
			(uint)ViewTexture.height,
			(uint)ViewTexture.antiAliasing,
			(uint)(m_WideTextures ? 1 : 0));

		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.SetRenderTarget, m_UISystem.Id, m_ViewId));
		}
		else
		{
			//Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
			GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.SetRenderTarget, m_UISystem.Id, m_ViewId));
#pragma warning restore 618
		}

		m_ShouldRedraw = true;
		View.ContinuousRepaint(m_ShouldRedraw);
	}

	void RecreateRenderTarget()
	{
		RecreateViewTexture(Width, Height);
		SetRenderTarget(ViewTexture.GetNativeTexturePtr(), m_DepthTexture.GetNativeTexturePtr());
	}

	void RecreateViewTexture(int width, int height)
	{
		if (ViewTexture != null)
		{
			ViewTexture.Release();
			ViewTexture = null;
		}

		ViewTexture = new RenderTexture(width,
			height,
			0,
			m_WideTextures ?
				UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat :
				UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
		ViewTexture.name = "CoherentRenderTexture" + View.GetId();
		ViewTexture.Create();

		if (m_DepthTexture != null)
		{
			m_DepthTexture.Release();
			m_DepthTexture = null;
		}

		m_DepthTexture = new RenderTexture(width, height, 24,
			RenderTextureFormat.Depth,
			RenderTextureReadWrite.Default);
		m_DepthTexture.name = "CoherentDepthTexture" + View.GetId();
		m_DepthTexture.Create();

		RenderTexture current = RenderTexture.active;
		RenderTexture.active = ViewTexture;
		GL.Clear(true, true, UnityEngine.Color.clear);
		RenderTexture.active = m_DepthTexture;
		GL.Clear(true, true, UnityEngine.Color.clear);
		RenderTexture.active = current;

		if (m_RenderMaterial != null)
		{
			m_RenderMaterial.SetTexture("_MainTex", ViewTexture);
		}
	}

	void Draw()
	{
		if (m_HasDrawn || View == null || !enabled)
		{
			return;
		}

		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			cohtml.Net.UnityPlugin.Instance().UnityRenderEvent(RenderEvents.MakeEvent(RenderEventType.DrawView, m_UISystem.Id, m_ViewId));
		}
		else
		{
			//Disable warning for using deprecated overload of IssuePluginEvent
#pragma warning disable 618
			GL.IssuePluginEvent(RenderEvents.MakeEvent(RenderEventType.DrawView, m_UISystem.Id, m_ViewId));
#pragma warning restore 618
		}

		m_HasDrawn = true;
	}

	public uint GetId()
	{
		return View.GetId();
	}


		protected virtual void OnApplicationQuit()
		{
			Debug.Log("Cohtml calls ApplicationQuit");
			Listener.OnApplicationQuit();
		}
	}
}
