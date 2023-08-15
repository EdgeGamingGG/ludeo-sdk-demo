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

using cohtml.Net;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cohtml
{
[Serializable]
[DisallowMultipleComponent]
public class CohtmlSystemSettings : MonoBehaviour
{
	public const int DefaultDebuggerPort = 9444;
	public bool m_EnableDebugger = true;
	public bool m_EnableDebuggerInBuild = false;

	[SerializeField] private int m_DebuggerPort = DefaultDebuggerPort;

	[SerializeField] private UnityEvent m_OnResourceHandlerAssign = new UnityEvent();
	[SerializeField] private UnityEvent m_OnLocalizationManagerAssign = new UnityEvent();
	[SerializeField] private UnityEvent m_OnTextTransformationManagerAssign = new UnityEvent();

	private IResourceHandler m_ResourceHandler;
	private ILocalizationManager m_LocalizationManager;
	private ITextTransformationManager m_TextTransformationManager;

	public ISystemSettings NativeSettings
	{
		get
		{
			ChangePortWhenXbox();

			ISystemSettings settings = new SystemSettings
			{
				DebuggerPort = m_DebuggerPort,
			};

			if (!m_EnableDebugger || m_DebuggerPort <= 0)
			{
				settings.EnableDebugger = false;
			}
			else if (m_EnableDebuggerInBuild)
			{
				settings.EnableDebugger = true;
			}
			else if (Debug.isDebugBuild || Application.isEditor)
			{
				settings.EnableDebugger = true;
			}

			if (!Application.runInBackground &&
			    settings.EnableDebugger &&
			    DebuggerPort != -1)
			{
				Application.runInBackground = true;
				LogHandler.Log(string.Format("Cohtml DevTools debugger forced to run in the background at port {0}.", m_DebuggerPort));
			}

			settings.ResourceHandler = ResourceHandler;
			settings.LocalizationManagerInstance = LocalizationManager;
			settings.TextTransformationManager = TextTransformationManager;

			return settings;
		}
	}

	private void ChangePortWhenXbox()
	{
		#if UNITY_XBOXONE
		if (m_DebuggerPort != 4601)
		{
			m_DebuggerPort = 4601;

			if (m_EnableDebugger && m_EnableDebuggerInBuild && Debug.isDebugBuild)
			{
				Debug.LogWarning("XboxOne Cohtml DevTools debugger port is occupied from Unity3D. " +
				                 "To use Cohtml Devtools on Xbox One build disable \"Script Debugging\" in build settings.");
			}
			else
			{
				Debug.LogWarning("XboxOne Cohtml DevTools debugger port is changed to 4601.");
			}
		}
		#endif
	}

	public int DebuggerPort
	{
		get { return m_DebuggerPort; }
		set { m_DebuggerPort = Mathf.Clamp(value, 1, ushort.MaxValue); }
	}

	public IDictionary<string, List<string>> HostLocationsMap
	{
		get { return ResourceHandler is ILocationsSearchable ? ((ILocationsSearchable)ResourceHandler).HostLocationsMap : null; }
		set
		{
			if (ResourceHandler is ILocationsSearchable)
			{
				((ILocationsSearchable)ResourceHandler).HostLocationsMap = value;
			}
		}
	}

	/// <summary>
	/// Resource handler for the instantiated system.
	/// Trying to get custom resource handler, assigned from user.
	/// If not assigned from user will get default implementation of COHTML resource handler.
	/// </summary>
	public IResourceHandler ResourceHandler
	{
		get
		{
			if (m_ResourceHandler == null)
			{
				m_OnResourceHandlerAssign.Invoke();
				if (m_ResourceHandler == null)
				{
					LogHandler.Log("System: Unable to find resource handler. " +
					               "Falling back to default resource handler.");
					m_ResourceHandler = new DefaultResourceHandler();
				}
			}

			return m_ResourceHandler;
		}
		set { m_ResourceHandler = value; }
	}

	/// <summary>
	/// Localization manager for the instantiated system.
	/// First trying to get custom localization manager, assigned from user.
	/// If not assigned from user will assign default COHTML localization manager implementation.
	/// </summary>
	public ILocalizationManager LocalizationManager
	{
		get
		{
			if (m_LocalizationManager == null)
			{
				m_OnLocalizationManagerAssign.Invoke();
				if (m_LocalizationManager == null)
				{
					LogHandler.Log("System: Unable to find localization manager. " +
					               "Falling back to default localization manager.");
					m_LocalizationManager = new ILocalizationManager();
				}
			}

			return m_LocalizationManager;
		}
		set { m_LocalizationManager = value; }
	}

	/// <summary>
	/// Text transformation manager for the instantiated system.
	/// First trying to get custom text transformation manager, assigned from user.
	/// If not assigned from user will assign default COHTML text transformation manager implementation.
	/// </summary>
	public ITextTransformationManager TextTransformationManager
	{
		get
		{
			if (m_TextTransformationManager == null)
			{
				m_OnTextTransformationManagerAssign.Invoke();
				if (m_TextTransformationManager == null)
				{
					LogHandler.Log("System: Unable to find text transformation manager. " +
					               "Falling back to default text transformation manager.");
					m_TextTransformationManager = new ITextTransformationManager();
				}
			}

			return m_TextTransformationManager;
		}
		set { m_TextTransformationManager = value; }
	}

	protected virtual void OnDestroy()
	{
		if (m_ResourceHandler != null)
		{
			m_ResourceHandler.Dispose();
			m_ResourceHandler = null;
		}

		if (m_LocalizationManager != null)
		{
			m_LocalizationManager.Dispose();
			m_LocalizationManager = null;
		}

		if (m_TextTransformationManager != null)
		{
			m_TextTransformationManager.Dispose();
			m_TextTransformationManager = null;
		}
	}
}
}