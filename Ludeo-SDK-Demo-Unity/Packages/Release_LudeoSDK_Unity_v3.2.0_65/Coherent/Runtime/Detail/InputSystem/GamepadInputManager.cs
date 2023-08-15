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
using UnityEngine;

namespace cohtml.InputSystem
{
/// <summary>
///  Gamepad input implementation using InputManager
/// Works only when Input settings is on Input manager option.
/// </summary>
public class GamepadInputManager : GamepadBase
{
	protected static Dictionary<ushort, string> s_ButtonsMap;
	protected static Dictionary<ushort, string> s_AxesMap;
	protected static DelayTimer s_CheckRoutineDelayTimer;
	private readonly string[] m_EmptyArray = Array.Empty<string>();
	private bool m_ListenInputStopped;
	private string[] m_UnityGamepads;

	public GamepadInputManager()
	{
		m_Gamepads = new Dictionary<uint, GamepadMap>();
		s_CheckRoutineDelayTimer = new DelayTimer(1f, 0f);

		MapGamepadToJs();
	}

	public void Update()
	{
		if (m_ListenInputStopped)
		{
			return;
		}

		CheckConnectedGamepads();

		if (Gamepads.Count > 0)
		{
			foreach (GamepadMap gamepad in Gamepads.Values)
			{
				UpdateGamepad(gamepad);
				UpdateGamepadStateNative(gamepad);
			}
		}
	}

	private void CheckConnectedGamepads()
	{
		s_CheckRoutineDelayTimer.Update();
		if (s_CheckRoutineDelayTimer.Phase != DelayTimer.PhaseType.InRate)
		{
			return;
		}

		s_CheckRoutineDelayTimer.Reset();
		m_UnityGamepads = Input.GetJoystickNames();
		m_UnityGamepads = Array.FindAll(m_UnityGamepads, x => !string.IsNullOrWhiteSpace(x));

		if (m_UnityGamepads.Length == Gamepads.Count ||
		    !CohtmlInputHandler.FocusedSystem.IsReady)
		{
			return;
		}

		RemoveDisconectedGamepads();

		AddNewConnectedGamepads();
	}

	private void AddNewConnectedGamepads()
	{
		for (uint i = 0; i < m_UnityGamepads.Length; i++)
		{
			AddGamepad(i, m_UnityGamepads[i]);
		}
	}

	private void RemoveDisconectedGamepads()
	{
		List<uint> gamepadsForRemoval = new List<uint>();
		foreach (GamepadMap gamepad in Gamepads.Values)
		{
			if (!Array.Exists(m_UnityGamepads, x => x == gamepad.Name))
			{
				gamepadsForRemoval.Add(gamepad.Id);
			}
		}

		for (int i = 0; i < gamepadsForRemoval.Count; i++)
		{
			RemoveGamepad(gamepadsForRemoval[i]);
		}
	}

	protected void AddGamepad(uint id, string name)
	{
		if (!Gamepads.ContainsKey(id))
		{
			Gamepads.Add(id, new GamepadMap(id, name, m_GamepadButtonsCount, m_GamepadAxesCount));
			RegisterGamepads();
		}
	}

	protected void RemoveGamepad(uint id)
	{
		if (Gamepads.ContainsKey(id))
		{
			Gamepads.Remove(id);
			UnregisterNative(id);
		}
	}

	protected virtual void UpdateGamepad(GamepadMap gamepad)
	{
		try
		{
			foreach (var button in s_ButtonsMap)
			{
				gamepad.Buttons[button.Key] = Input.GetAxisRaw(string.Format(button.Value, gamepad.Id));
			}

			float dPadValue = Input.GetAxisRaw(string.Format(s_ButtonsMap[12], gamepad.Id));
			gamepad.Buttons[12] = dPadValue > 0 ? 1 : 0; // up
			gamepad.Buttons[13] = dPadValue < 0 ? 1 : 0; // down

			dPadValue = Input.GetAxisRaw(string.Format(s_ButtonsMap[14], gamepad.Id));
			gamepad.Buttons[14] = dPadValue < 0 ? 1 : 0; // left
			gamepad.Buttons[15] = dPadValue > 0 ? 1 : 0; // right

			foreach (var axis in s_AxesMap)
			{
				gamepad.Axes[axis.Key] = Input.GetAxisRaw(string.Format(axis.Value, gamepad.Id, "X"));
				gamepad.Axes[axis.Key + 1] = -Input.GetAxisRaw(string.Format(axis.Value, gamepad.Id, "Y"));
			}
		}
		catch (ArgumentException)
		{
			// Ignore ArgumentException when client reject to populate gamepad controllers.
			// Stop listening for controllers.
			m_ListenInputStopped = true;
		}
	}

	protected virtual void MapGamepadToJs()
	{
		s_ButtonsMap = new Dictionary<ushort, string>
		{
			{ 0, "Cohtml_Gamepad{0}_South" },
			{ 1, "Cohtml_Gamepad{0}_East" },
			{ 2, "Cohtml_Gamepad{0}_West" },
			{ 3, "Cohtml_Gamepad{0}_North" },
			{ 4, "Cohtml_Gamepad{0}_LeftShoulder" },
			{ 5, "Cohtml_Gamepad{0}_RightShoulder" },
			{ 6, "Cohtml_Gamepad{0}_LeftTrigger" },
			{ 7, "Cohtml_Gamepad{0}_RightTrigger" },
			{ 8, "Cohtml_Gamepad{0}_Select" },
			{ 9, "Cohtml_Gamepad{0}_Start" },
			{ 10, "Cohtml_Gamepad{0}_LeftStickPress" },
			{ 11, "Cohtml_Gamepad{0}_RightStickPress" },
			{ 12, "Cohtml_Gamepad{0}_DPad_Y" },
			{ 14, "Cohtml_Gamepad{0}_DPad_X" },
			{ 16, "Cohtml_Gamepad{0}_SystemButton" },
			{ 17, "Cohtml_Gamepad{0}_TouchpadButton" }
		};

		// Hold start index of the vector for sticks. X = index, Y = index + 1.
		s_AxesMap = new Dictionary<ushort, string>
		{
			{ 0, "Cohtml_Gamepad{0}_LeftStick_{1}" }, // LeftStick X = 0, LeftStick Y = 1 
			{ 2, "Cohtml_Gamepad{0}_RightStick_{1}" } // RightStick X = 2, RightStick Y = 3
		};
	}
}
}
