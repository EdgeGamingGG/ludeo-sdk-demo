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

#if ENABLE_INPUT_SYSTEM
using cohtml.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

#if UNITY_PS4
using cohtml.InputSystem.PS4;
#endif

namespace cohtml.InputSystem
{
public class InputSystem : InputBase
{
	const string DeltaString = "delta";
	const string ScrollString = "scroll";
	const string PressString = "press";

	const short SkipCallsNumber = 10;
	static InputAction m_MouseAction;
	static InputAction m_KeyboardAction;
	static InputAction m_MouseVectorAction;
	private static MouseEvent s_MouseEventCached;
	private static EventMouseModifiersState s_MouseModifiers;

	private short m_NumberOfCalls;

	public InputSystem()
	{
		SubscribeToMouseEvents();
		SubscribeToKeyEvents();
		SubscribeToTouchEvents();

		s_MouseEventCached = new MouseEvent();

		if (CohtmlInputHandler.Instance.EnableGamepad)
		{
			#if UNITY_PS4
			Gamepad = new GamepadInputSystemPS4();
			#else
			Gamepad = new GamepadInputSystem();
			#endif
		}
	}

	public bool KeyboardConnected => Keyboard.current != null;

	private void SubscribeToMouseEvents()
	{
		if (!CohtmlInputHandler.Instance.EnableMouse)
		{
			return;
		}

		m_MouseAction = new InputAction("Cohtml_ProcessMouse", InputActionType.PassThrough, "<mouse>/<button>");
		m_MouseAction.performed += context =>
		{
			if (context.control.name == PressString)
			{
				return;
			}

			m_UnityEvent = new InputEventWrapper(context);
			OnMouseEvent?.Invoke(ProcessMouseEvent(), m_UnityEvent);
		};
		m_MouseAction.Enable();

		m_MouseVectorAction = new InputAction("Cohtml_ProcessMouseVectors", InputActionType.PassThrough);
		m_MouseVectorAction.AddBinding("<mouse>/delta");
		m_MouseVectorAction.AddBinding("<mouse>/scroll");
		m_MouseVectorAction.performed += context =>
		{
			if (context.control.name == DeltaString && WaitCall() ||
			    context.control.name != ScrollString && context.control.name != DeltaString)
			{
				return;
			}

			m_UnityEvent = new InputEventWrapper(context);
			OnMouseEvent?.Invoke(ProcessMouseVectors(), m_UnityEvent);
		};

		m_MouseVectorAction.Enable();
	}

	private void SubscribeToKeyEvents()
	{
		if (!CohtmlInputHandler.Instance.EnableKeyboard || !KeyboardConnected)
		{
			return;
		}

		m_KeyboardAction = new InputAction(name: "Cohtml_ProcessKeyboard", InputActionType.PassThrough, "<Keyboard>/<key>");
		m_KeyboardAction.performed += context =>
		{
			m_UnityEvent = new InputEventWrapper(context);
			OnKeyEvent?.Invoke(ProcessKeyEvent(), m_UnityEvent);
		};

		m_KeyboardAction.Enable();

		if (Application.isPlaying)
		{
			Keyboard.current.onTextInput += SendCharKey;
		}
	}

	private void SubscribeToTouchEvents()
	{
		if (!CohtmlInputHandler.Instance.EnableTouch)
		{
			return;
		}

		EnhancedTouchSupport.Enable();
		Touch.onFingerDown += SetTouch;
		Touch.onFingerMove += SetTouch;
		Touch.onFingerUp += SetTouch;
	}

	public override MouseEvent ProcessMouseEvent()
	{
		ButtonControl buttonControl = m_UnityEvent.Event.control as ButtonControl;
		s_MouseEventCached = new MouseEvent
		{
			Type = buttonControl.isPressed
				? MouseEventData.EventType.MouseDown
				: MouseEventData.EventType.MouseUp,
			Button = SetCurrentPressedButton(buttonControl.name),
			Modifiers = GetKeyboardModifiers(),
			MouseModifiers = GetMouseModifiers()
		};

		s_MouseEventCached.Position = Vector2Int.RoundToInt(((Pointer)m_UnityEvent.Event.control.device).position.ReadValue());
		return s_MouseEventCached;
	}

	public MouseEvent ProcessMouseVectors()
	{
		if (m_UnityEvent.Event.control.name == ScrollString)
		{
			Vector2 scroll = SetScrollData(-Mouse.current.scroll.ReadValue());
			s_MouseEventCached.WheelX = scroll.x;
			s_MouseEventCached.WheelY = scroll.y;
			s_MouseEventCached.Type = MouseEventData.EventType.MouseWheel;
		}
		else if (m_UnityEvent.Event.control.name == DeltaString)
		{
			s_MouseEventCached.Type = MouseEventData.EventType.MouseMove;
		}

		s_MouseEventCached.Position = Vector2Int.RoundToInt(((Pointer)m_UnityEvent.Event.control.device).position.ReadValue());

		return s_MouseEventCached;
	}

	public override KeyEvent ProcessKeyEvent()
	{
		KeyControl keyControl = m_UnityEvent.Event.control as KeyControl;
		KeyEvent sendData = new KeyEvent
		{
			Modifiers = GetKeyboardModifiers(),
			KeyCode = SetKeyCode((int)keyControl.keyCode),
			IsAutoRepeat = false,
			Location = GetKeyLocation((int)keyControl.keyCode),
			Type = keyControl.isPressed ? KeyEventData.EventType.KeyDown : KeyEventData.EventType.KeyUp,
		};

		return sendData;
	}

	private void SendCharKey(char character)
	{
		if (!Application.isPlaying && KeyboardConnected)
		{
			Keyboard.current.onTextInput -= SendCharKey;
		}

		KeyEvent keyCharData = new KeyEvent
		{
			KeyCode = character,
			Type = KeyEventData.EventType.Char,
			Modifiers = GetKeyboardModifiers(),
			Location = KeyEventData.EventLocation.Standard,
			IsAutoRepeat = true
		};

		m_UnityEvent = new InputEventWrapper();
		OnCharEvent?.Invoke(keyCharData, character);
	}

	private void SetTouch(Finger finger)
	{
		if (!Application.isPlaying)
		{
			return;
		}

		s_TouchDataCached.UpdateEventTypes();
		TouchEvent touchEvent = s_TouchDataCached[(uint)finger.index];
		touchEvent.ClearViews();
		touchEvent.Modifiers = GetKeyboardModifiers();
		touchEvent.Position = Vector2Int.RoundToInt(finger.screenPosition);
		switch (finger.currentTouch.phase)
		{
			case TouchPhase.Began:
				touchEvent.Type = TouchEventData.EventType.TouchDown;
				break;
			case TouchPhase.Moved:
			case TouchPhase.Stationary:
				touchEvent.Type = TouchEventData.EventType.TouchMove;
				break;
			default:
				touchEvent.Type = TouchEventData.EventType.TouchUp;
				break;
		}

		OnTouchEvent?.Invoke(s_TouchDataCached);
	}

	// Prevent too many calls from Mouse movement
	private bool WaitCall()
	{
		if (m_NumberOfCalls <= SkipCallsNumber)
		{
			++m_NumberOfCalls;

			return true;
		}

		m_NumberOfCalls = 0;

		return false;
	}

	private MouseEventData.MouseButton SetCurrentPressedButton(string buttonName)
	{
		switch (buttonName)
		{
			case "leftButton":
				return MouseEventData.MouseButton.ButtonLeft;
			case "middleButton":
				return MouseEventData.MouseButton.ButtonMiddle;
			case "rightButton":
				return MouseEventData.MouseButton.ButtonRight;
			case "forwardButton":
				return MouseEventData.MouseButton.ButtonForward;
			case "backButton":
				return MouseEventData.MouseButton.ButtonBack;
			default:
				return MouseEventData.MouseButton.ButtonNone;
		}
	}

	protected override EventMouseModifiersState GetMouseModifiers()
	{
		s_MouseModifiers = new EventMouseModifiersState
		{
			IsLeftButtonDown = m_UnityEvent.Event.control.device["leftButton"].IsPressed(),
			IsMiddleButtonDown = m_UnityEvent.Event.control.device["middleButton"].IsPressed(),
			IsRightButtonDown = m_UnityEvent.Event.control.device["rightButton"].IsPressed(),
			IsBackButtonDown = m_UnityEvent.Event.control.device["backButton"].IsPressed(),
			IsForwardButtonDown = m_UnityEvent.Event.control.device["forwardButton"].IsPressed(),
		};

		return s_MouseModifiers;
	}

	protected override EventModifiersState GetKeyboardModifiers()
	{
		if (!KeyboardConnected)
		{
			return UpdateKeyboardModifiers(false, false, false, false, false, false, false);
		}

		return UpdateKeyboardModifiers(
			Keyboard.current.ctrlKey.isPressed,
			Keyboard.current.altKey.isPressed,
			Keyboard.current.shiftKey.isPressed,
			Keyboard.current.numLockKey.isPressed,
			Keyboard.current.capsLockKey.isPressed,
			(Keyboard.current.leftMetaKey.isPressed || Keyboard.current.rightMetaKey.isPressed),
			Keyboard.current.rightAltKey.isPressed);
	}

	public int SetKeyCode(int keyCode)
	{
		if (s_KeyCodeMapping.ContainsKey(keyCode))
		{
			return s_KeyCodeMapping[keyCode];
		}

		return -1;
	}

	protected override bool IsNumPadKey(int key)
	{
		return (int)Key.NumpadEnter <= key && key <= (int)Key.Numpad9;
	}

	protected override void MapUnityCodesWithSystemCodes()
	{
		base.MapUnityCodesWithSystemCodes();
		s_KeyCodeMapping[(int)Key.None] = 0;
		s_KeyCodeMapping[(int)Key.Backspace] = 0x08;
		s_KeyCodeMapping[(int)Key.Tab] = 0x09;
		s_KeyCodeMapping[(int)Key.Enter] = 0x0D;
		s_KeyCodeMapping[(int)Key.Pause] = 0x13;
		s_KeyCodeMapping[(int)Key.Escape] = 0x1B;
		s_KeyCodeMapping[(int)Key.Space] = 0x20;
		s_KeyCodeMapping[(int)Key.Quote] = 0xDE;
		s_KeyCodeMapping[(int)Key.Comma] = 0xBC;
		s_KeyCodeMapping[(int)Key.Minus] = 0xBD;
		s_KeyCodeMapping[(int)Key.Period] = 0xBE;
		s_KeyCodeMapping[(int)Key.Slash] = 0xBF;
		s_KeyCodeMapping[(int)Key.Digit0] = 0x30;
		s_KeyCodeMapping[(int)Key.Digit1] = 0x31;
		s_KeyCodeMapping[(int)Key.Digit2] = 0x32;
		s_KeyCodeMapping[(int)Key.Digit3] = 0x33;
		s_KeyCodeMapping[(int)Key.Digit4] = 0x34;
		s_KeyCodeMapping[(int)Key.Digit5] = 0x35;
		s_KeyCodeMapping[(int)Key.Digit6] = 0x36;
		s_KeyCodeMapping[(int)Key.Digit7] = 0x37;
		s_KeyCodeMapping[(int)Key.Digit8] = 0x38;
		s_KeyCodeMapping[(int)Key.Digit9] = 0x39;
		s_KeyCodeMapping[(int)Key.Semicolon] = 0xBA;
		s_KeyCodeMapping[(int)Key.Equals] = 0xBB;
		s_KeyCodeMapping[(int)Key.LeftBracket] = 0xDB;
		s_KeyCodeMapping[(int)Key.Backslash] = 0xDC;
		s_KeyCodeMapping[(int)Key.RightBracket] = 0xDD;
		s_KeyCodeMapping[(int)Key.A] = 65;
		s_KeyCodeMapping[(int)Key.B] = 66;
		s_KeyCodeMapping[(int)Key.C] = 67;
		s_KeyCodeMapping[(int)Key.D] = 68;
		s_KeyCodeMapping[(int)Key.E] = 69;
		s_KeyCodeMapping[(int)Key.F] = 70;
		s_KeyCodeMapping[(int)Key.G] = 71;
		s_KeyCodeMapping[(int)Key.H] = 72;
		s_KeyCodeMapping[(int)Key.I] = 73;
		s_KeyCodeMapping[(int)Key.J] = 74;
		s_KeyCodeMapping[(int)Key.K] = 75;
		s_KeyCodeMapping[(int)Key.L] = 76;
		s_KeyCodeMapping[(int)Key.M] = 77;
		s_KeyCodeMapping[(int)Key.N] = 78;
		s_KeyCodeMapping[(int)Key.O] = 79;
		s_KeyCodeMapping[(int)Key.P] = 80;
		s_KeyCodeMapping[(int)Key.Q] = 81;
		s_KeyCodeMapping[(int)Key.R] = 82;
		s_KeyCodeMapping[(int)Key.S] = 83;
		s_KeyCodeMapping[(int)Key.T] = 84;
		s_KeyCodeMapping[(int)Key.U] = 85;
		s_KeyCodeMapping[(int)Key.V] = 86;
		s_KeyCodeMapping[(int)Key.W] = 87;
		s_KeyCodeMapping[(int)Key.X] = 88;
		s_KeyCodeMapping[(int)Key.Y] = 89;
		s_KeyCodeMapping[(int)Key.Z] = 90;
		s_KeyCodeMapping[(int)Key.Delete] = 0x2E;
		s_KeyCodeMapping[(int)Key.Numpad0] = 0x60;
		s_KeyCodeMapping[(int)Key.Numpad1] = 0x61;
		s_KeyCodeMapping[(int)Key.Numpad2] = 0x62;
		s_KeyCodeMapping[(int)Key.Numpad3] = 0x63;
		s_KeyCodeMapping[(int)Key.Numpad4] = 0x64;
		s_KeyCodeMapping[(int)Key.Numpad5] = 0x65;
		s_KeyCodeMapping[(int)Key.Numpad6] = 0x66;
		s_KeyCodeMapping[(int)Key.Numpad7] = 0x67;
		s_KeyCodeMapping[(int)Key.Numpad8] = 0x68;
		s_KeyCodeMapping[(int)Key.Numpad9] = 0x69;
		s_KeyCodeMapping[(int)Key.NumpadPeriod] = 0x6E;
		s_KeyCodeMapping[(int)Key.NumpadDivide] = 0x6F;
		s_KeyCodeMapping[(int)Key.NumpadMultiply] = 0x6A;
		s_KeyCodeMapping[(int)Key.NumpadMinus] = 0x6D;
		s_KeyCodeMapping[(int)Key.NumpadPlus] = 0x6B;
		s_KeyCodeMapping[(int)Key.NumpadEnter] = 0x0D;
		s_KeyCodeMapping[(int)Key.NumpadEquals] = 0;
		s_KeyCodeMapping[(int)Key.UpArrow] = 0x26;
		s_KeyCodeMapping[(int)Key.DownArrow] = 0x28;
		s_KeyCodeMapping[(int)Key.RightArrow] = 0x27;
		s_KeyCodeMapping[(int)Key.LeftArrow] = 0x25;
		s_KeyCodeMapping[(int)Key.Insert] = 0x2D;
		s_KeyCodeMapping[(int)Key.Home] = 0x24;
		s_KeyCodeMapping[(int)Key.End] = 0x23;
		s_KeyCodeMapping[(int)Key.PageUp] = 0x21;
		s_KeyCodeMapping[(int)Key.PageDown] = 0x22;
		s_KeyCodeMapping[(int)Key.F1] = 0x70;
		s_KeyCodeMapping[(int)Key.F2] = 0x71;
		s_KeyCodeMapping[(int)Key.F3] = 0x72;
		s_KeyCodeMapping[(int)Key.F4] = 0x73;
		s_KeyCodeMapping[(int)Key.F5] = 0x74;
		s_KeyCodeMapping[(int)Key.F6] = 0x75;
		s_KeyCodeMapping[(int)Key.F7] = 0x76;
		s_KeyCodeMapping[(int)Key.F8] = 0x77;
		s_KeyCodeMapping[(int)Key.F9] = 0x78;
		s_KeyCodeMapping[(int)Key.F10] = 0x79;
		s_KeyCodeMapping[(int)Key.F11] = 0x7A;
		s_KeyCodeMapping[(int)Key.F12] = 0x7B;
		s_KeyCodeMapping[(int)Key.NumLock] = 0x90;
		s_KeyCodeMapping[(int)Key.CapsLock] = 0x14;
		s_KeyCodeMapping[(int)Key.ScrollLock] = 0x91;
		s_KeyCodeMapping[(int)Key.RightShift] = 0x10;
		s_KeyCodeMapping[(int)Key.LeftShift] = 0x10;
		s_KeyCodeMapping[(int)Key.RightCtrl] = 0x11;
		s_KeyCodeMapping[(int)Key.LeftCtrl] = 0x11;
		s_KeyCodeMapping[(int)Key.RightAlt] = 0x12;
		s_KeyCodeMapping[(int)Key.LeftAlt] = 0x12;
		s_KeyCodeMapping[(int)Key.RightApple] = 0x5C;
		s_KeyCodeMapping[(int)Key.LeftApple] = 0x5B;
		s_KeyCodeMapping[(int)Key.RightWindows] = 0x5C;
		s_KeyCodeMapping[(int)Key.LeftWindows] = 0x5B;
		s_KeyCodeMapping[(int)Key.AltGr] = 0x12;
		s_KeyCodeMapping[(int)Key.PrintScreen] = 0x2A;
		s_KeyCodeMapping[(int)Key.ContextMenu] = 0x5D;

		s_RightKeysMap = new[]
		{
			(int)Key.RightShift,
			(int)Key.RightCtrl,
			(int)Key.RightAlt,
			(int)Key.RightApple,
			(int)Key.RightWindows
		};

		s_LeftKeysMap = new[]
		{
			(int)Key.LeftShift,
			(int)Key.LeftCtrl,
			(int)Key.LeftAlt,
			(int)Key.LeftApple,
			(int)Key.LeftWindows
		};
	}

	public override void Dispose()
	{
		m_MouseAction.Disable();
		m_KeyboardAction.Disable();
		m_MouseVectorAction.Disable();

		m_MouseAction.Dispose();
		m_KeyboardAction.Dispose();
		m_MouseVectorAction.Dispose();

		if (KeyboardConnected)
		{
			Keyboard.current.onTextInput -= SendCharKey;
		}

		Touch.onFingerDown -= SetTouch;
		Touch.onFingerMove -= SetTouch;
		Touch.onFingerUp -= SetTouch;
		EnhancedTouchSupport.Disable();
	}
}
}
#endif
