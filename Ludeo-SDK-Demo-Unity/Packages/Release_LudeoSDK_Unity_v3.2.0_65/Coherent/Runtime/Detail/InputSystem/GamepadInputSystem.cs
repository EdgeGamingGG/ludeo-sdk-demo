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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace cohtml.InputSystem
{
/// <summary>
/// Input system gamepad interpretation for COHTML handle input.
/// Works only when Input System is applied in Unity project settings and
/// Input System is installed from Package Manager.
/// </summary>
public class GamepadInputSystem : GamepadBase
{
	static InputAction s_ButtonAction;
	static InputAction s_DPadAction;
	static InputAction s_StickMoveAction;

	protected static uint s_DeviceIdCache;
	protected static Dictionary<string, int> s_ButtonsMap;
	protected static Dictionary<string, int> s_AxesMap;

	public GamepadInputSystem()
	{
		Subscribe();
	}

	public virtual void Subscribe()
	{
		m_Gamepads = new Dictionary<uint, GamepadMap>();

		MapGamepadToJs();
		s_ButtonAction = new InputAction("Cohtml_ProcessGamepadButton", InputActionType.PassThrough, "<gamepad>/<button>");
		s_ButtonAction.performed += callbackContext => OnButtonAction(callbackContext.control as ButtonControl);
		s_ButtonAction.Enable();

		s_DPadAction = new InputAction("Cohtml_ProcessGamepadDPad", InputActionType.PassThrough, "<gamepad>/<dpad>");
		s_DPadAction.performed += callbackContext => OnDpadAction(callbackContext.control as DpadControl);
		s_DPadAction.Enable();

		s_StickMoveAction = new InputAction("Cohtml_ProcessGamepadStick", InputActionType.PassThrough, "<gamepad>/<stick>");
		s_StickMoveAction.performed += callbackContext => OnStickMoveAction(callbackContext.control as StickControl);
		s_StickMoveAction.Enable();

		UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
	}

	private void OnDeviceChange(InputDevice device, InputDeviceChange deviceChange)
	{
		if (!device.displayName.Contains("Controller"))
		{
			return;
		}

		switch (deviceChange)
		{
			case InputDeviceChange.Removed:
			case InputDeviceChange.Disconnected:
				RemoveGamepad((uint)device.deviceId);
				break;
			case InputDeviceChange.Added:
				AddGamepad((uint)device.deviceId, device.name);
				break;
		}
	}

	private void OnButtonAction(ButtonControl control)
	{
		s_DeviceIdCache = (uint)control.device.deviceId;
		AddGamepad(s_DeviceIdCache, control.device.name);

		SetButton(control, Gamepads[s_DeviceIdCache]);

		UpdateGamepadStateNative(Gamepads[s_DeviceIdCache]);
	}

	private void OnDpadAction(DpadControl control)
	{
		s_DeviceIdCache = (uint)control.device.deviceId;
		AddGamepad(s_DeviceIdCache, control.device.name);

		SetButton(control.up, Gamepads[s_DeviceIdCache]);
		SetButton(control.down, Gamepads[s_DeviceIdCache]);
		SetButton(control.left, Gamepads[s_DeviceIdCache]);
		SetButton(control.right, Gamepads[s_DeviceIdCache]);

		UpdateGamepadStateNative(Gamepads[s_DeviceIdCache]);
	}

	private void OnStickMoveAction(StickControl control)
	{
		s_DeviceIdCache = (uint)control.device.deviceId;
		AddGamepad(s_DeviceIdCache, control.device.name);
		SetAxis(control, Gamepads[s_DeviceIdCache]);
		UpdateGamepadStateNative(Gamepads[s_DeviceIdCache]);
	}

	protected void AddGamepad(uint id, string name)
	{
		if (!Gamepads.ContainsKey(id))
		{
			Gamepads.Add(id, new GamepadMap(id, name, m_GamepadButtonsCount, m_GamepadAxesCount));
			RegisterGamepads();
			LogHandler.Log(string.Format($"Gamepad ID:({id}) registered."));
		}
	}

	protected void RemoveGamepad(uint id)
	{
		if (Gamepads.ContainsKey(id))
		{
			UnregisterNative(id);
			Gamepads.Remove(id);
		}
	}

	protected void SetButton(ButtonControl control, GamepadMap currentGamepad)
	{
		if (s_ButtonsMap.ContainsKey(control.name))
		{
			currentGamepad.Buttons[s_ButtonsMap[control.name]] = control.ReadValue();
		}
	}

	protected void SetAxis(StickControl control, GamepadMap currentGamepad)
	{
		Vector2 axis = control.ReadValue();
		var index = s_AxesMap[control.name];

		currentGamepad.Axes[index] = axis.x;
		currentGamepad.Axes[index + 1] = axis.y;
	}

	protected virtual void MapGamepadToJs()
	{
		s_ButtonsMap = new Dictionary<string, int>
		{
			{ "buttonSouth", 0 },
			{ "buttonEast", 1 },
			{ "buttonWest", 2 },
			{ "buttonNorth", 3 },
			{ "leftShoulder", 4 },
			{ "rightShoulder", 5 },
			{ "leftTrigger", 6 },
			{ "rightTrigger", 7 },
			{ "select", 8 },
			{ "start", 9 },
			{ "leftStickPress", 10 },
			{ "rightStickPress", 11 },
			{ "up", 12 },
			{ "down", 13 },
			{ "left", 14 },
			{ "right", 15 },
			{ "systemButton", 16 },
			{ "touchpadButton", 17 }
		};

		// Hold start index of the vector for sticks. X = index, Y = index + 1.
		s_AxesMap = new Dictionary<string, int>
		{
			{ "leftStick", 0 }, // LeftStick X = 0, LeftStick Y = 1 
			{ "rightStick", 2 }, // RightStick X = 2, RightStick Y = 3
		};
	}

	public override void Dispose()
	{
		s_DPadAction.Disable();
		s_ButtonAction.Disable();
		s_StickMoveAction.Disable();
		s_DPadAction.Dispose();
		s_ButtonAction.Dispose();
		s_StickMoveAction.Dispose();
	}
}
}
#endif
