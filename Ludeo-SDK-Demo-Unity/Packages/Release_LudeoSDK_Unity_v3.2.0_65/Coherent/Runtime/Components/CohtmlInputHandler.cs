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
using System.Collections.Generic;
using cohtml.Net;
using UnityEngine;

namespace cohtml.InputSystem
{
public class CohtmlInputHandler : MonoBehaviour
{
	[Tooltip("Enable Cohtml integration to send mouse events automatically. Cannot be changed at runtime.")]
	public bool EnableMouse = true;

	[Tooltip("Enable Cohtml integration to send keyboard events automatically. Cannot be changed at runtime.")]
	public bool EnableKeyboard = true;

	[Tooltip("Enable Cohtml integration to send events from one or multiple gamepads automatically. Cannot be changed at runtime.")]
	public bool EnableGamepad = true;

	[Tooltip("Enable Cohtml integration to send touch events automatically. Cannot be changed at runtime.")]
	public bool EnableTouch = true;

	[Tooltip("CSS classes that mark a tag as ignored for the Cohtml input event system.\n" +
	         "If the tag is а parent, it will ignore its children.")]
	public string[] IgnoredInputCSSClasses = { "hidden" };

	[Tooltip("Determinate how many pixels will scroll with one tick of the mouse.\n" +
	         "Negative number will reverse the direction of scrolling. Can be changed at runtime.")]
	public int ScrollPixels = 40;

	[Tooltip("The event system automatically manages focus between views.\n" +
	         "The focused in-world CohtmlView will be below the mouse pointer or on-screen.\n" +
	         "The first focused by default is an on-screen CohtmlView component, attached to the main camera if exists.")]
	public bool EnableAutoFocus = true;

	[Tooltip("Enable the input handler to focus views on pointer move event.\n" +
	         "This property is ignored when disabling \"EnableAutoFocus\".\n" +
	         "Can be changed at runtime.")]
	public bool EnableFocusOnPointerMove = false;

	[Tooltip("The camera used to handle input events through raycast.\n" +
	         "By default this is the Main Camera.\n" +
	         "Use the static property RaycastCamera to change it at runtime.")]
	[SerializeField]
	private Camera m_RaycastCamera;

	private static CohtmlInputHandler s_Instance;
	private List<CohtmlView> m_GatheredEventViews;
	private static InputBase s_Input;

	public static CohtmlInputHandler Instance => GetOrInitInstance();

	public static InputBase Input
	{
		get
		{
			GetOrInitInstance();
			return s_Input;
		}
	}

	public bool InputEnabled => EnableMouse || EnableKeyboard || EnableGamepad || EnableTouch;

	/// <summary>
	/// The camera used for handling input events with raycast.
	/// By default is the Main Camera.
	/// </summary>
	public static Camera RaycastCamera
	{
		get => s_Instance.GetRaycastCamera();
		set => s_Instance.m_RaycastCamera = value;
	}

	/// <summary>
	/// Represent the status of a current input event, which was handled by some of the gathered Views of the input event propagation flow.
	/// Will be True when some CohtmlView handled the event.
	/// Will be False at the start of the input event propagation flow.
	/// Will be False after event propagation flow finishes and no view handles that event.
	/// </summary>
	private static bool s_IsInputTargetFound;

	public static Action<CohtmlView> OnFocusView { get; set; }
	public static Action<CohtmlUISystem> OnFocusSystem { get; set; }
	public static Action<InputEventWrapper> OnMouseEventTargetNotFound { get; set; }
	public static Action<TouchEventCollection> OnTouchEventTargetNotFound { get; set; }
	public static Action<InputEventWrapper> OnKeyEventTargetNotFound { get; set; }
	public static Action<char> OnCharEventTargetNotFound { get; set; }
	public static Action<GamepadBase.GamepadMap> OnGamepadEventTargetNotFound { get; set; }

	public static CohtmlUISystem FocusedSystem { get; private set; }

	public static CohtmlView FocusedView { get; private set; }

	protected virtual void Awake()
	{
		if (s_Instance != null)
		{
			return;
		}

		s_Instance = this;
	}

	protected virtual void Start()
	{
		if (EnableAutoFocus)
		{
			GatherOnScreenViews();
			if (m_GatheredEventViews != null && m_GatheredEventViews.Count > 0)
			{
				FocusView(m_GatheredEventViews[0]);
			}
			else
			{
				CohtmlView view = FindObjectOfType<CohtmlView>();
				if (view)
				{
					FocusView(view);
				}
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (InputEnabled)
		{
			s_Input = InputBase.Initialize();
			m_GatheredEventViews = new List<CohtmlView>();

			if (EnableMouse)
			{
				Input.OnMouseEvent += SendEventToViews;
			}

			if (EnableKeyboard)
			{
				Input.OnKeyEvent += SendKeyToFocusedView;
				Input.OnCharEvent += SendCharToFocusedView;
			}

			if (EnableTouch)
			{
				Input.OnTouchEvent += SendEventToViews;
			}
		}
		GetRaycastCamera();
	}

	protected virtual void Reset()
	{
		GetRaycastCamera();
	}

	protected virtual void OnDestroy()
	{
		if (Input != null)
		{
			Input.Dispose();
		}
		s_Instance = null;
	}

	#if !ENABLE_INPUT_SYSTEM
	private InputManager m_InputManager;

	protected virtual void Update()
	{
		if (m_InputManager == null)
		{
			m_InputManager = (InputManager)Input;
		}

		m_InputManager.ProcessEvent(Event.current);
	}

	protected virtual void OnGUI()
	{
		if (m_InputManager == null)
		{
			m_InputManager = (InputManager)Input;
		}

		m_InputManager.ProcessEvent(Event.current);
	}
	#endif

	public void FocusView(CohtmlView view, PointerEvent.EventType type = PointerEvent.EventType.Up)
	{
		if (view != null &&
		    FocusedView != view &&
		    EnableAutoFocus &&
		    (type != PointerEvent.EventType.Move || EnableFocusOnPointerMove))
		{
			FocusedView = view;
			OnFocusView?.Invoke(FocusedView);
			if (FocusedSystem != view.CohtmlUISystem)
			{
				FocusedSystem = view.CohtmlUISystem;
				OnFocusSystem?.Invoke(FocusedSystem);
			}
		}
	}

	public static CohtmlInputHandler GetOrInitInstance()
	{
		if (s_Instance == null)
		{
			CohtmlUISystem.GetDefaultUISystem();
			if (Application.isPlaying && CohtmlUISystem.DefaultUISystem != null)
			{
				s_Instance = CohtmlUISystem.DefaultUISystem.gameObject.AddMissingComponent<CohtmlInputHandler>();
				LogHandler.Log("Cohtml Input Handler component added to default Cohtml UISystеm.");
			}
			else
			{
				s_Instance = new GameObject("Cohtml Input Handler").AddComponent<CohtmlInputHandler>();
			}

			s_Instance.Awake();
		}

		return s_Instance;
	}

	public Camera GetRaycastCamera()
	{
		if (!m_RaycastCamera)
		{
			m_RaycastCamera = Camera.main;
		}

		return m_RaycastCamera;
	}

	public void SendEventToViews(PointerEvent eventData, InputEventWrapper unityEvent)
	{
		m_GatheredEventViews.Clear();
		GatherOnScreenViews();
		for (int i = 0; i < m_GatheredEventViews.Count; i++)
		{
			eventData.AddView(m_GatheredEventViews[i], eventData.InvertY);
		}

		CohtmlView targetViewFound = PropagateEventToGatheredViews(eventData);

		if (targetViewFound)
		{
			FocusView(targetViewFound, eventData.PointerEventType);
			return;
		}

		m_GatheredEventViews.Clear();
		GatherWorldViewsSorted(ref eventData);
		targetViewFound = PropagateEventToGatheredViews(eventData);

		if (targetViewFound)
		{
			FocusView(targetViewFound, eventData.PointerEventType);
		}
		else
		{
			OnMouseEventTargetNotFound?.Invoke(unityEvent);
		}
	}

	public void SendEventToViews(TouchEventCollection touches)
	{
		m_GatheredEventViews.Clear();
		GatherOnScreenViews();
		for (int i = 0; i < m_GatheredEventViews.Count; i++)
		{
			for (uint j = 0; j < touches.Capacity; j++)
			{
				if (touches[j].IsActive)
				{
					touches[j].AddView(m_GatheredEventViews[i], touches[j].InvertY);
				}
			}
		}

		CohtmlView targetView = PropagateEventToGatheredViews(touches);
		if (targetView && touches.ActiveTouches.Length > 0)
		{
			FocusView(targetView, touches.ActiveTouches[0].PointerEventType);
			return;
		}

		m_GatheredEventViews.Clear();
		for (uint i = 0; i < touches.Capacity; i++)
		{
			if (touches[i].IsActive)
			{
				PointerEvent touchEvent = touches[i];
				GatherWorldViewsSorted(ref touchEvent);
				touches[i] = (TouchEvent)touchEvent;
			}
		}

		targetView = PropagateEventToGatheredViews(touches);
		if (targetView && touches.ActiveTouches.Length > 0)
		{
			FocusView(targetView, touches.ActiveTouches[0].PointerEventType);
		}
		else
		{
			OnTouchEventTargetNotFound?.Invoke(touches);
		}
	}

	public void SendKeyToFocusedView(KeyEvent eventData, InputEventWrapper unityEvent)
	{
		SendEventToFocusedView(eventData);

		if (!s_IsInputTargetFound)
		{
			OnKeyEventTargetNotFound?.Invoke(unityEvent);
		}
	}

	public void SendCharToFocusedView(KeyEvent eventData, char character)
	{
		SendEventToFocusedView(eventData);

		if (!s_IsInputTargetFound)
		{
			OnCharEventTargetNotFound?.Invoke(character);
		}
	}

	public void SendEventToFocusedView(KeyEvent eventData)
	{
		if (FocusedView == null)
		{
			return;
		}

		m_GatheredEventViews.Clear();
		m_GatheredEventViews.Add(FocusedView);
		PropagateEventToGatheredViews(eventData);
	}

	private void GatherOnScreenViews()
	{
		CohtmlView[] views = Array.Empty<CohtmlView>();
		if (RaycastCamera != null)
		{
			views = RaycastCamera.GetComponents<CohtmlView>();
			Array.Reverse(views);
		}

		for (int i = 0; i < views.Length; i++)
		{
			if (views[i] != null && views[i].RaycastTarget)
			{
				AddViewIfNotExist(views[i]);
			}
		}
	}

	public void GatherWorldViewsSorted(ref PointerEvent pointerEvent)
	{
		CohtmlView view;
		RaycastHit[] hits = Raycast(pointerEvent.Position);
		Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));
		for (int i = 0; i < hits.Length; i++)
		{
			view = hits[i].collider.GetComponent<CohtmlView>();
			if (!view || !view.RaycastTarget)
			{
				continue;
			}

			pointerEvent.AddView(view,
				new Vector2Int(
					(int)(hits[i].textureCoord.x * view.Width),
					(int)((1 - hits[i].textureCoord.y) * view.Height)
				)
			);

			AddViewIfNotExist(view);
		}
	}

	private void AddViewIfNotExist(CohtmlView view)
	{
		if (m_GatheredEventViews != null && m_GatheredEventViews.IndexOf(view) == -1)
		{
			m_GatheredEventViews.Add(view);
		}
	}

	private RaycastHit[] Raycast(Vector2 pointer)
	{
		if (m_RaycastCamera != null)
		{
			return Physics.RaycastAll(m_RaycastCamera.ScreenPointToRay(pointer));
		}

		return Array.Empty<RaycastHit>();
	}

	private CohtmlView PropagateEventToGatheredViews(IViewSendable eventData)
	{
		s_IsInputTargetFound = false;
		for (int i = 0; i < m_GatheredEventViews.Count; i++)
		{
			eventData.Send(m_GatheredEventViews[i]);
			if (s_IsInputTargetFound)
			{
				return m_GatheredEventViews[i];
			}
		}

		return null;
	}

	public static Actions HandleInputEvent(INodeProxy node, PhaseType phase)
	{
		HTMLTag tag = node.GetTag();
		if (s_IsInputTargetFound || tag == HTMLTag.UNKNOWN || tag == HTMLTag.HTML)
		{
			return Actions.ContinueHandling;
		}

		for (uint i = 0; i < node.GetClassesCount(); i++)
		{
			string nodeClass = node.GetClass(i);
			if (Array.Exists(Instance.IgnoredInputCSSClasses, c => c == nodeClass))
			{
				return Actions.InterruptHandling;
			}
		}

		if (phase == PhaseType.AT_TARGET && tag != HTMLTag.BODY)
		{
			s_IsInputTargetFound = true;
		}

		return Actions.ContinueHandling;
	}
}
}
