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

#if !ENABLE_INPUT_SYSTEM
using UnityEngine;
using cohtml.Net;
using TouchPhase = UnityEngine.TouchPhase;
using System.Collections.Generic;

#if UNITY_PS4
using cohtml.InputSystem.PS4;
#endif

namespace cohtml.InputSystem
{
public class InputManager : InputBase
{
	private static bool s_IsShiftPressed;
	private static DelayTimer s_ShiftRepeatTimer = new DelayTimer(KeyRepeatDelay, KeyRepeatRate);
	private readonly GamepadInputManager m_Gamepad;
	private Vector2 m_lastPointerPosition;
	private static bool s_IsLongPressed;
	private MouseEvent m_MouseEventCache;

	private readonly Dictionary<char, int> m_AsciiToSystemCodes = new Dictionary<char, int>
	{
		{ '\n', 0x0D }
	};


	public InputManager()
	{
		m_MouseEventCache = new MouseEvent();
		m_UnityEvent = new InputEventWrapper(new Event());
		if (CohtmlInputHandler.Instance.EnableGamepad)
		{
			#if UNITY_PS4
			m_Gamepad = new GamepadInputManagerPS4();
			#else
			m_Gamepad = new GamepadInputManager();
			#endif

			Gamepad = m_Gamepad;
		}
	}

	public void ProcessEvent(Event inputEvent)
	{
		if (!CohtmlInputHandler.Instance.InputEnabled)
		{
			return;
		}

		ProcessMissingMouseButtons();
		GenerateMissingKeyEvent();
		ProcessTouchEvent();

		if (inputEvent == null)
		{
			return;
		}

		m_UnityEvent = new InputEventWrapper(inputEvent);
		ProcessMouseMovement();

		switch (m_UnityEvent.Event.type)
		{
			case EventType.MouseDown:
			case EventType.MouseUp:
			case EventType.ScrollWheel:
				OnMouseEvent?.Invoke(ProcessMouseEvent(), m_UnityEvent);
				break;
			case EventType.KeyDown:
			case EventType.KeyUp:
				OnKeyEvent?.Invoke(ProcessKeyEvent(), m_UnityEvent);
				break;
		}

		if (CohtmlInputHandler.Instance.EnableGamepad)
		{
			m_Gamepad.Update();
		}
	}

	private void ProcessMouseMovement()
	{
		if (!CohtmlInputHandler.Instance.EnableMouse)
		{
			return;
		}

		if (m_lastPointerPosition != m_UnityEvent.Event.mousePosition)
		{
			m_UnityEvent.Event.type = EventType.MouseMove;
			m_lastPointerPosition = m_UnityEvent.Event.mousePosition;
			OnMouseEvent?.Invoke(ProcessMouseEvent(), m_UnityEvent);
		}
	}

	public void ProcessMissingMouseButtons()
	{
		if (!CohtmlInputHandler.Instance.EnableMouse)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Mouse3))
		{
			UpdateMouseEvent(3, EventType.MouseDown);
		}
		else if (Input.GetKeyUp(KeyCode.Mouse3))
		{
			UpdateMouseEvent(3, EventType.MouseUp);
		}

		if (Input.GetKeyDown(KeyCode.Mouse4))
		{
			UpdateMouseEvent(4, EventType.MouseDown);
		}
		else if (Input.GetKeyUp(KeyCode.Mouse4))
		{
			UpdateMouseEvent(4, EventType.MouseUp);
		}

		void UpdateMouseEvent(int mouseButton, EventType eventType)
		{
			m_UnityEvent.Event.button = mouseButton;
			m_UnityEvent.Event.type = eventType;
			m_UnityEvent.Event.mousePosition = m_lastPointerPosition;

			OnMouseEvent?.Invoke(ProcessMouseEvent(), m_UnityEvent);
		}
	}


	public override MouseEvent ProcessMouseEvent()
	{
		if (!CohtmlInputHandler.Instance.EnableMouse)
		{
			return null;
		}

		m_MouseEventCache.Position = new Vector2Int((int)m_UnityEvent.Event.mousePosition.x, Screen.height - (int)m_UnityEvent.Event.mousePosition.y);
		switch (m_UnityEvent.Event.type)
		{
			case EventType.MouseMove:
				m_MouseEventCache.Type = MouseEventData.EventType.MouseMove;
				break;
			case EventType.MouseDown:
				m_MouseEventCache.Type = MouseEventData.EventType.MouseDown;
				break;
			case EventType.MouseUp:
				m_MouseEventCache.Type = MouseEventData.EventType.MouseUp;
				break;
			case EventType.ScrollWheel:
				m_MouseEventCache.Type = MouseEventData.EventType.MouseWheel;
				Vector2 scroll = SetScrollData(m_UnityEvent.Event.delta);
				m_MouseEventCache.WheelX = scroll.x;
				m_MouseEventCache.WheelY = scroll.y;
				break;
		}

		m_MouseEventCache.Modifiers = GetKeyboardModifiers();
		m_MouseEventCache.MouseModifiers = GetMouseModifiers();
		SetCurrentPressedButton(m_MouseEventCache);
		return m_MouseEventCache;
	}

	void SetCurrentPressedButton(IMouseEventData mouseData)
	{
		if (m_UnityEvent.Event.type == EventType.MouseDown || m_UnityEvent.Event.type == EventType.MouseUp)
		{
			switch (m_UnityEvent.Event.button)
			{
				case 0:
					mouseData.Button = MouseEventData.MouseButton.ButtonLeft;
					break;
				case 1:
					mouseData.Button = MouseEventData.MouseButton.ButtonRight;
					break;
				case 2:
					mouseData.Button = MouseEventData.MouseButton.ButtonMiddle;
					break;
				case 3:
					mouseData.Button = MouseEventData.MouseButton.ButtonBack;
					break;
				case 4:
					mouseData.Button = MouseEventData.MouseButton.ButtonForward;
					break;
			}
		}
	}

	protected override EventMouseModifiersState GetMouseModifiers()
	{
		return new EventMouseModifiersState
		{
			IsLeftButtonDown = Input.GetMouseButton(0),
			IsMiddleButtonDown = Input.GetMouseButton(2),
			IsRightButtonDown = Input.GetMouseButton(1),
			IsBackButtonDown = Input.GetMouseButton(3),
			IsForwardButtonDown = Input.GetMouseButton(4)
		};
	}

	public override KeyEvent ProcessKeyEvent()
	{
		KeyEvent sendData = new KeyEvent();
		switch (m_UnityEvent.Event.type)
		{
			case EventType.KeyDown:
				sendData.Type = KeyEventData.EventType.KeyDown;
				break;
			case EventType.KeyUp:
				sendData.Type = KeyEventData.EventType.KeyUp;
				break;
		}

		SetKeyCode(ref sendData, m_UnityEvent.Event.character, (int)m_UnityEvent.Event.keyCode);
		sendData.IsAutoRepeat = IsAutoRepeat(sendData);

		UpdateKeyboardModifiers(m_UnityEvent.Event.control,
			m_UnityEvent.Event.alt,
			m_UnityEvent.Event.shift,
			m_UnityEvent.Event.numeric,
			m_UnityEvent.Event.capsLock,
			m_UnityEvent.Event.command,
			m_UnityEvent.Event.keyCode == KeyCode.AltGr);
		sendData.Modifiers = GetKeyboardModifiers();
		sendData.Location = GetKeyLocation((int)m_UnityEvent.Event.keyCode);

		return sendData;
	}

	public void GenerateMissingKeyEvent()
	{
		if (!CohtmlInputHandler.Instance.EnableKeyboard)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
		{
			s_IsShiftPressed = true;
			UpdateShiftEvent(KeyEventData.EventType.KeyDown, false);
		}
		else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
		{
			s_IsShiftPressed = false;
			s_ShiftRepeatTimer.Reset();
			UpdateShiftEvent(KeyEventData.EventType.KeyUp, false);
		}

		if (s_IsShiftPressed)
		{
			s_ShiftRepeatTimer.Update();
			if (s_ShiftRepeatTimer.Phase == DelayTimer.PhaseType.InRate)
			{
				UpdateShiftEvent(KeyEventData.EventType.KeyDown, true);
			}
		}
	}

	private void UpdateShiftEvent(KeyEventData.EventType eventType, bool isRepeat)
	{
		KeyEvent KeyData = new KeyEvent
		{
			Type = eventType,
			KeyCode = s_KeyCodeMapping[(int)KeyCode.LeftShift],
			Modifiers = GetKeyboardModifiers(),
			IsAutoRepeat = isRepeat,
		};

		KeyData.Location = GetKeyLocation((int)m_UnityEvent.Event.keyCode);
		OnKeyEvent?.Invoke(KeyData, m_UnityEvent);
	}

	public void ProcessTouchEvent()
	{
		if (!CohtmlInputHandler.Instance.EnableTouch || !Input.touchSupported)
		{
			return;
		}

		Touch currentTouch;
		int limitTouches = Mathf.Clamp(Input.touchCount, 0, s_TouchDataCached.Capacity);
		for (uint i = 0; i < limitTouches; i++)
		{
			currentTouch = Input.GetTouch((int)i);
			s_TouchDataCached[i].ClearViews();
			s_TouchDataCached[i].Id = (uint)currentTouch.fingerId;
			s_TouchDataCached[i].Position = Vector2Int.RoundToInt(currentTouch.position);
			s_TouchDataCached[i].Modifiers = GetKeyboardModifiers();

			switch (currentTouch.phase)
			{
				case TouchPhase.Began:
					s_TouchDataCached[i].Type = TouchEventData.EventType.TouchDown;
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					s_TouchDataCached[i].Type = TouchEventData.EventType.TouchMove;
					break;
				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					s_TouchDataCached[i].Type = TouchEventData.EventType.TouchUp;
					break;
			}
		}

		OnTouchEvent?.Invoke(s_TouchDataCached);
	}

	protected override EventModifiersState GetKeyboardModifiers()
	{
		return UpdateKeyboardModifiers(
			Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl),
			Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt),
			Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
			Input.GetKey(KeyCode.Numlock),
			Input.GetKey(KeyCode.CapsLock),
			Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand),
			Input.GetKey(KeyCode.AltGr));
	}

	public void SetKeyCode(ref KeyEvent eventData, char character, int keyCode)
	{
		if (character > (int)KeyCode.None)
		{
			eventData.Type = KeyEventData.EventType.Char;
			eventData.KeyCode = m_AsciiToSystemCodes.ContainsKey(character) ? m_AsciiToSystemCodes[character] : character;
		}
		else if (s_KeyCodeMapping.ContainsKey(keyCode))
		{
			eventData.KeyCode = s_KeyCodeMapping[keyCode];
		}
	}

	public bool IsAutoRepeat(KeyEvent eventData)
	{
		bool isRepeat = s_IsLongPressed;

		if (eventData.Type == KeyEventData.EventType.Char)
		{
			return false;
		}

		switch (eventData.Type)
		{
			case KeyEventData.EventType.KeyDown:
				s_IsLongPressed = true;

				break;
			case KeyEventData.EventType.KeyUp:
				s_IsLongPressed = false;
				isRepeat = s_IsLongPressed;

				break;
		}

		return isRepeat;
	}

	protected override bool IsNumPadKey(int keyCode)
	{
		return (int)KeyCode.Keypad0 <= keyCode && keyCode <= (int)KeyCode.KeypadEquals;
	}

	protected override void MapUnityCodesWithSystemCodes()
	{
		base.MapUnityCodesWithSystemCodes();
		s_KeyCodeMapping[(int)KeyCode.None] = 0;
		s_KeyCodeMapping[(int)KeyCode.Backspace] = 0x08;
		s_KeyCodeMapping[(int)KeyCode.Tab] = 0x09;
		s_KeyCodeMapping[(int)KeyCode.Clear] = 0x0C;
		s_KeyCodeMapping[(int)KeyCode.Return] = 0x0D;
		s_KeyCodeMapping[(int)KeyCode.Pause] = 0x13;
		s_KeyCodeMapping[(int)KeyCode.Escape] = 0x1B;
		s_KeyCodeMapping[(int)KeyCode.Space] = 0x20;
		s_KeyCodeMapping[(int)KeyCode.Exclaim] = 0x31;
		s_KeyCodeMapping[(int)KeyCode.DoubleQuote] = 0xDE;
		s_KeyCodeMapping[(int)KeyCode.Hash] = 0x33;
		s_KeyCodeMapping[(int)KeyCode.Dollar] = 0x34;
		s_KeyCodeMapping[(int)KeyCode.Ampersand] = 0x37;
		s_KeyCodeMapping[(int)KeyCode.Quote] = 0xDE;
		s_KeyCodeMapping[(int)KeyCode.LeftParen] = 0x39;
		s_KeyCodeMapping[(int)KeyCode.RightParen] = 0x30;
		s_KeyCodeMapping[(int)KeyCode.Asterisk] = 0x38;
		s_KeyCodeMapping[(int)KeyCode.Plus] = 0xBB;
		s_KeyCodeMapping[(int)KeyCode.Comma] = 0xBC;
		s_KeyCodeMapping[(int)KeyCode.Minus] = 0xBD;
		s_KeyCodeMapping[(int)KeyCode.Period] = 0xBE;
		s_KeyCodeMapping[(int)KeyCode.Slash] = 0xBF;
		s_KeyCodeMapping[(int)KeyCode.Alpha0] = 0x30;
		s_KeyCodeMapping[(int)KeyCode.Alpha1] = 0x31;
		s_KeyCodeMapping[(int)KeyCode.Alpha2] = 0x32;
		s_KeyCodeMapping[(int)KeyCode.Alpha3] = 0x33;
		s_KeyCodeMapping[(int)KeyCode.Alpha4] = 0x34;
		s_KeyCodeMapping[(int)KeyCode.Alpha5] = 0x35;
		s_KeyCodeMapping[(int)KeyCode.Alpha6] = 0x36;
		s_KeyCodeMapping[(int)KeyCode.Alpha7] = 0x37;
		s_KeyCodeMapping[(int)KeyCode.Alpha8] = 0x38;
		s_KeyCodeMapping[(int)KeyCode.Alpha9] = 0x39;
		s_KeyCodeMapping[(int)KeyCode.Colon] = 0xBA;
		s_KeyCodeMapping[(int)KeyCode.Semicolon] = 0xBA;
		s_KeyCodeMapping[(int)KeyCode.Less] = 0xBC;
		s_KeyCodeMapping[(int)KeyCode.Equals] = 0xBB;
		s_KeyCodeMapping[(int)KeyCode.Greater] = 0xBE;
		s_KeyCodeMapping[(int)KeyCode.Question] = 0xBF;
		s_KeyCodeMapping[(int)KeyCode.At] = 0x32;
		s_KeyCodeMapping[(int)KeyCode.LeftBracket] = 0xDB;
		s_KeyCodeMapping[(int)KeyCode.Backslash] = 0xDC;
		s_KeyCodeMapping[(int)KeyCode.RightBracket] = 0xDD;
		s_KeyCodeMapping[(int)KeyCode.Caret] = 0x36;
		s_KeyCodeMapping[(int)KeyCode.Underscore] = 0xBD;
		s_KeyCodeMapping[(int)KeyCode.BackQuote] = 0xC0;
		s_KeyCodeMapping[(int)KeyCode.A] = 65;
		s_KeyCodeMapping[(int)KeyCode.B] = 66;
		s_KeyCodeMapping[(int)KeyCode.C] = 67;
		s_KeyCodeMapping[(int)KeyCode.D] = 68;
		s_KeyCodeMapping[(int)KeyCode.E] = 69;
		s_KeyCodeMapping[(int)KeyCode.F] = 70;
		s_KeyCodeMapping[(int)KeyCode.G] = 71;
		s_KeyCodeMapping[(int)KeyCode.H] = 72;
		s_KeyCodeMapping[(int)KeyCode.I] = 73;
		s_KeyCodeMapping[(int)KeyCode.J] = 74;
		s_KeyCodeMapping[(int)KeyCode.K] = 75;
		s_KeyCodeMapping[(int)KeyCode.L] = 76;
		s_KeyCodeMapping[(int)KeyCode.M] = 77;
		s_KeyCodeMapping[(int)KeyCode.N] = 78;
		s_KeyCodeMapping[(int)KeyCode.O] = 79;
		s_KeyCodeMapping[(int)KeyCode.P] = 80;
		s_KeyCodeMapping[(int)KeyCode.Q] = 81;
		s_KeyCodeMapping[(int)KeyCode.R] = 82;
		s_KeyCodeMapping[(int)KeyCode.S] = 83;
		s_KeyCodeMapping[(int)KeyCode.T] = 84;
		s_KeyCodeMapping[(int)KeyCode.U] = 85;
		s_KeyCodeMapping[(int)KeyCode.V] = 86;
		s_KeyCodeMapping[(int)KeyCode.W] = 87;
		s_KeyCodeMapping[(int)KeyCode.X] = 88;
		s_KeyCodeMapping[(int)KeyCode.Y] = 89;
		s_KeyCodeMapping[(int)KeyCode.Z] = 90;
		s_KeyCodeMapping[(int)KeyCode.Delete] = 0x2E;
		s_KeyCodeMapping[(int)KeyCode.Keypad0] = 0x60;
		s_KeyCodeMapping[(int)KeyCode.Keypad1] = 0x61;
		s_KeyCodeMapping[(int)KeyCode.Keypad2] = 0x62;
		s_KeyCodeMapping[(int)KeyCode.Keypad3] = 0x63;
		s_KeyCodeMapping[(int)KeyCode.Keypad4] = 0x64;
		s_KeyCodeMapping[(int)KeyCode.Keypad5] = 0x65;
		s_KeyCodeMapping[(int)KeyCode.Keypad6] = 0x66;
		s_KeyCodeMapping[(int)KeyCode.Keypad7] = 0x67;
		s_KeyCodeMapping[(int)KeyCode.Keypad8] = 0x68;
		s_KeyCodeMapping[(int)KeyCode.Keypad9] = 0x69;
		s_KeyCodeMapping[(int)KeyCode.KeypadPeriod] = 0x6E;
		s_KeyCodeMapping[(int)KeyCode.KeypadDivide] = 0x6F;
		s_KeyCodeMapping[(int)KeyCode.KeypadMultiply] = 0x6A;
		s_KeyCodeMapping[(int)KeyCode.KeypadMinus] = 0x6D;
		s_KeyCodeMapping[(int)KeyCode.KeypadPlus] = 0x6B;
		s_KeyCodeMapping[(int)KeyCode.KeypadEnter] = 0x0D;
		s_KeyCodeMapping[(int)KeyCode.KeypadEquals] = 0;
		s_KeyCodeMapping[(int)KeyCode.UpArrow] = 0x26;
		s_KeyCodeMapping[(int)KeyCode.DownArrow] = 0x28;
		s_KeyCodeMapping[(int)KeyCode.RightArrow] = 0x27;
		s_KeyCodeMapping[(int)KeyCode.LeftArrow] = 0x25;
		s_KeyCodeMapping[(int)KeyCode.Insert] = 0x2D;
		s_KeyCodeMapping[(int)KeyCode.Home] = 0x24;
		s_KeyCodeMapping[(int)KeyCode.End] = 0x23;
		s_KeyCodeMapping[(int)KeyCode.PageUp] = 0x21;
		s_KeyCodeMapping[(int)KeyCode.PageDown] = 0x22;
		s_KeyCodeMapping[(int)KeyCode.F1] = 0x70;
		s_KeyCodeMapping[(int)KeyCode.F2] = 0x71;
		s_KeyCodeMapping[(int)KeyCode.F3] = 0x72;
		s_KeyCodeMapping[(int)KeyCode.F4] = 0x73;
		s_KeyCodeMapping[(int)KeyCode.F5] = 0x74;
		s_KeyCodeMapping[(int)KeyCode.F6] = 0x75;
		s_KeyCodeMapping[(int)KeyCode.F7] = 0x76;
		s_KeyCodeMapping[(int)KeyCode.F8] = 0x77;
		s_KeyCodeMapping[(int)KeyCode.F9] = 0x78;
		s_KeyCodeMapping[(int)KeyCode.F10] = 0x79;
		s_KeyCodeMapping[(int)KeyCode.F11] = 0x7A;
		s_KeyCodeMapping[(int)KeyCode.F12] = 0x7B;
		s_KeyCodeMapping[(int)KeyCode.F13] = 0x7C;
		s_KeyCodeMapping[(int)KeyCode.F14] = 0x7D;
		s_KeyCodeMapping[(int)KeyCode.F15] = 0x7E;
		s_KeyCodeMapping[(int)KeyCode.Numlock] = 0x90;
		s_KeyCodeMapping[(int)KeyCode.CapsLock] = 0x14;
		s_KeyCodeMapping[(int)KeyCode.ScrollLock] = 0x91;
		s_KeyCodeMapping[(int)KeyCode.RightShift] = 0x10;
		s_KeyCodeMapping[(int)KeyCode.LeftShift] = 0x10;
		s_KeyCodeMapping[(int)KeyCode.RightControl] = 0x11;
		s_KeyCodeMapping[(int)KeyCode.LeftControl] = 0x11;
		s_KeyCodeMapping[(int)KeyCode.RightAlt] = 0x12;
		s_KeyCodeMapping[(int)KeyCode.LeftAlt] = 0x12;
		s_KeyCodeMapping[(int)KeyCode.RightApple] = 0x5C;
		s_KeyCodeMapping[(int)KeyCode.LeftApple] = 0x5B;
		s_KeyCodeMapping[(int)KeyCode.RightWindows] = 0x5C;
		s_KeyCodeMapping[(int)KeyCode.LeftWindows] = 0x5B;
		s_KeyCodeMapping[(int)KeyCode.AltGr] = 0x12;
		s_KeyCodeMapping[(int)KeyCode.Help] = 0x2F;
		s_KeyCodeMapping[(int)KeyCode.Print] = 0x2A;
		s_KeyCodeMapping[(int)KeyCode.SysReq] = 0x2C;
		s_KeyCodeMapping[(int)KeyCode.Break] = 0x13;
		s_KeyCodeMapping[(int)KeyCode.Menu] = 0x5D;

		s_RightKeysMap = new[]
		{
			(int)KeyCode.RightShift,
			(int)KeyCode.RightControl,
			(int)KeyCode.RightAlt,
			(int)KeyCode.RightApple,
			(int)KeyCode.RightWindows
		};

		s_LeftKeysMap = new[]
		{
			(int)KeyCode.LeftShift,
			(int)KeyCode.LeftControl,
			(int)KeyCode.LeftAlt,
			(int)KeyCode.LeftApple,
			(int)KeyCode.LeftWindows
		};
	}
}
}
#endif
