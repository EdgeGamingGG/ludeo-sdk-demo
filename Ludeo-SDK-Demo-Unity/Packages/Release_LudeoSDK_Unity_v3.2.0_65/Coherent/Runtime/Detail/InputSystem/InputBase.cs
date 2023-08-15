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
using System.Collections.Generic;
using UnityEngine;

namespace cohtml.InputSystem
{
public abstract class InputBase : IDisposable
{
	protected const float KeyRepeatDelay = 0.5f; // 500ms
	protected const float KeyRepeatRate = 0.033f; // 33ms
	protected const int SystemRightKey = 0x5C;
	protected const int SystemLeftKey = 0x5B;
	protected static Dictionary<int, int> s_KeyCodeMapping;
	protected static int[] s_RightKeysMap;
	protected static int[] s_LeftKeysMap;
	protected static TouchEventCollection s_TouchDataCached;

	private GestureManager m_GestureManager;
	private GestureListener m_GestureListener;
	private PanRecognizer m_PanRecognizer;
	private FlingRecognizer m_FlingRecognizer;
	private TapRecognizer m_TapRecognizer;

	protected InputEventWrapper m_UnityEvent;

	public Action<MouseEvent, InputEventWrapper> OnMouseEvent { get; set; }
	public Action<KeyEvent, InputEventWrapper> OnKeyEvent { get; set; }
	public Action<KeyEvent, char> OnCharEvent { get; set; }
	public Action<TouchEventCollection> OnTouchEvent { get; set; }

	protected InputBase()
	{
		if (CohtmlInputHandler.Instance.EnableTouch)
		{
			InitializeGestures();
			s_TouchDataCached = new TouchEventCollection();
		}

		MapUnityCodesWithSystemCodes();
	}

	public GamepadBase Gamepad { get; set; }

	public abstract MouseEvent ProcessMouseEvent();

	public abstract KeyEvent ProcessKeyEvent();

	// Unify raw scroll values given from Unity3D Input on every platform.
	// -1 is for negative and +1 is for positive scroll direction which we multiply by CohtmlSystem's scroll pixel property
	protected Vector2 SetScrollData(Vector2 scroll)
	{
		scroll.x = Mathf.Clamp(scroll.x, -1, 1) * CohtmlInputHandler.Instance.ScrollPixels;
		scroll.y = Mathf.Clamp(scroll.y, -1, 1) * CohtmlInputHandler.Instance.ScrollPixels;

		return scroll;
	}

	private void InitializeGestures()
	{
		m_GestureManager = new GestureManager();
		m_GestureListener = new GestureListener(this);

		m_PanRecognizer = new PanRecognizer(m_GestureManager, 1);
		m_PanRecognizer.SetGestureListener(m_GestureListener);
		m_GestureManager.AddRecognizer(m_PanRecognizer);

		m_FlingRecognizer = new FlingRecognizer(m_GestureManager, 1);
		m_FlingRecognizer.SetGestureListener(m_GestureListener);
		m_GestureManager.AddRecognizer(m_FlingRecognizer);

		m_TapRecognizer = new TapRecognizer(m_GestureManager, 1);
		m_TapRecognizer.SetGestureListener(m_GestureListener);
		m_GestureManager.AddRecognizer(m_TapRecognizer);
	}

	protected virtual void MapUnityCodesWithSystemCodes()
	{
		s_KeyCodeMapping = new Dictionary<int, int>();
	}

	public virtual KeyEventData.EventLocation GetKeyLocation(int keyCode)
	{
		if (IsNumPadKey(keyCode))
		{
			return KeyEventData.EventLocation.Numpad;
		}

		if (Array.Exists(s_LeftKeysMap, k => k == keyCode))
		{
			return KeyEventData.EventLocation.Left;
		}

		if (Array.Exists(s_RightKeysMap, k => k == keyCode))
		{
			return KeyEventData.EventLocation.Right;
		}

		return KeyEventData.EventLocation.Standard;
	}

	protected abstract bool IsNumPadKey(int keyCode);

	public virtual EventModifiersState UpdateKeyboardModifiers(bool isCtrlDown,
		bool isAltDown,
		bool isShiftDown,
		bool isNumLockDown,
		bool isCapsLockDown,
		bool isMetaDown,
		bool isAltGrDown)
	{
		return new EventModifiersState
		{
			IsCtrlDown = isCtrlDown,
			IsAltDown = isAltDown,
			IsShiftDown = isShiftDown,
			IsMetaDown = isMetaDown,
			IsNumLockOn = isNumLockDown,
			IsAltGraphDown = isAltGrDown,
			IsCapsOn = isCapsLockDown
		};
	}

	protected abstract EventMouseModifiersState GetMouseModifiers();

	public static InputBase Initialize()
	{
		#if ENABLE_INPUT_SYSTEM
		return new InputSystem();
		#else
		return new InputManager();
		#endif
	}

	protected abstract EventModifiersState GetKeyboardModifiers();

	public void UpdateTouches(TouchEventData[] touchEventData, uint touchesCount)
	{
		m_GestureManager.UpdateTouches(touchEventData, touchesCount);
	}

	public virtual void Dispose()
	{
	}
}
}
