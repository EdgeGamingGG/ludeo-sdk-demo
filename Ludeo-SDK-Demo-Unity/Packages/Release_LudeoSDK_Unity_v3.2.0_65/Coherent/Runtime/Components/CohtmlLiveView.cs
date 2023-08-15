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
using UnityEngine;

namespace cohtml
{
/// <summary>
/// Component that needs to be attached to a camera and creates a Coherent UI Live View
/// </summary>
public class CohtmlLiveView : MonoBehaviour
{
	[ExposeField(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Target System",
		Tooltip = "The System that will store the Live View's texture. " +
		          "When is empty will get the Default System.",
		IsStatic = true)]
	[SerializeField] CohtmlUISystem m_TargetSystem;
	string m_Uri;

	[HideInInspector] [SerializeField] string m_LiveViewName = "NewLiveView";
	[HideInInspector] [SerializeField] int m_Width = 512;
	[HideInInspector] [SerializeField] int m_Height = 512;
	[HideInInspector] [SerializeField] Camera m_TargetCamera;
	[HideInInspector] [SerializeField] RenderTexture m_TargetTexture;

	/// <summary>
	/// Gets or sets the Name of the live view
	/// </summary>
	/// <value>
	/// The Name of the view
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Name",
		Tooltip = "Indicates the Name of the Live View available in the page",
		IsStatic = false)]
	public string LiveViewName
	{
		get { return m_LiveViewName; }
		set { m_LiveViewName = value; }
	}

	/// <summary>
	/// Gets or sets the Width of the live view
	/// </summary>
	/// <value>
	/// The width of the view
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Width",
		Tooltip = "Indicates the Width that the Live View will have",
		IsStatic = true)]
	public int Width
	{
		get { return m_Width; }
		set
		{
			if (!Application.isPlaying)
			{
				m_Width = Mathf.Clamp(value, 1, 16384);
			}
		}
	}

	/// <summary>
	/// Gets or sets the Height of the live view
	/// </summary>
	/// <value>
	/// The height of the view
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Height",
		Tooltip = "Indicates the Height that the Live View will have",
		IsStatic = true)]
	public int Height
	{
		get { return m_Height; }
		set
		{
			if (!Application.isPlaying)
			{
				m_Height = Mathf.Clamp(value, 1, 16384);
			}
		}
	}

	/// <summary>
	/// Gets or sets the System store the live view
	/// </summary>
	/// <value>
	/// The System component
	/// </value>
	public CohtmlUISystem System
	{
		get
		{
			if (!m_TargetSystem)
			{
				m_TargetSystem = CohtmlUISystem.GetDefaultUISystem();
			}

			return m_TargetSystem;
		}
		set { m_TargetSystem = value; }
	}

	/// <summary>
	/// Gets or sets the Camera source for the live view
	/// </summary>
	/// <value>
	/// The Camera component
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Target Camera",
		Tooltip = "The Camera that will render the Live View",
		IsStatic = false)]
	public Camera TargetCamera
	{
		get { return m_TargetCamera; }
		set
		{
			if (value != null && value.targetTexture != null)
			{
				m_TargetCamera.targetTexture = null;
			}

			m_TargetCamera = value;
		}
	}

	/// <summary>
	/// Gets or sets the source texture for the live view
	/// </summary>
	/// <value>
	/// The Texture object
	/// </value>
	[ExposeProperty(Category = ExposePropertyInfo.FoldoutType.General,
		PrettyName = "Target Texture",
		Tooltip = "A texture used as source for the Live View",
		IsStatic = true)]
	public RenderTexture TargetTexture
	{
		get { return m_TargetTexture; }
		set { m_TargetTexture = value; }
	}

	public uint ImageHandle { get; set; }

	private RenderTexture NewRenderTexture
	{
		get
		{
			return new RenderTexture(m_Width,
				m_Height,
				16,
				RenderTextureFormat.ARGB32,
				RenderTextureReadWrite.Default);
		}
	}

	protected virtual void Awake()
	{
		if (m_TargetCamera == null)
		{
			Camera cameraComponent = GetComponent<Camera>();

			if (cameraComponent != null && cameraComponent != Camera.main)
			{
				m_TargetCamera = cameraComponent;
			}
		}
	}

	protected virtual void Start()
	{
		m_Uri = new Uri(DefaultResourceHandler.CouiProtocol + m_LiveViewName).AbsoluteUri;
		System.UserImagesManager.AddLiveView(m_Uri, this);
	}

	protected virtual void Update()
	{
		if (m_TargetTexture == null || !m_TargetTexture.IsCreated())
		{
			RecreateRenderTexture();
		}

		if (m_TargetCamera != null && m_TargetCamera.targetTexture != m_TargetTexture)
		{
			AttachCameraTargetTexture();
		}
	}

	protected virtual void OnEnable()
	{
		if (m_TargetTexture == null && m_TargetCamera == null)
		{
			LogHandler.LogError("No target defined for live view " + m_LiveViewName + ".");
			enabled = false;
		}
	}

	protected virtual void OnDestroy()
	{
		m_TargetSystem.UserImagesManager.RemoveLiveView(m_Uri);
		ReleaseTexture();
	}

	void RecreateRenderTexture()
	{
		if (!m_TargetTexture)
		{
			m_TargetTexture = NewRenderTexture;
		}

		m_TargetTexture.Create();

		#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN
		RenderTexture current = RenderTexture.active;
		RenderTexture.active = m_TargetTexture;
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = current;
		#endif
	}

	void AttachCameraTargetTexture()
	{
		if (m_TargetCamera != null && m_TargetTexture != null)
		{
			m_TargetCamera.targetTexture = m_TargetTexture;
			m_TargetCamera.enabled = true;
		}
	}

	void ReleaseTexture()
	{
		if (m_TargetCamera != null)
		{
			m_TargetCamera.targetTexture = null;
			m_TargetCamera.enabled = false;
		}

		if (m_TargetTexture)
		{
			m_TargetTexture.Release();
		}
	}
}
}