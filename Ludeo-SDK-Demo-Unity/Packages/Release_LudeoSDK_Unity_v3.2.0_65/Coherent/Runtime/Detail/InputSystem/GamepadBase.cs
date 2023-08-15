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

namespace cohtml.InputSystem
{
/// <summary>
/// Base class for COHTML gamepad storing data for native code.
/// </summary>
public abstract class GamepadBase : IDisposable
{
	[Serializable]
	public struct GamepadMap
	{
		public GamepadMap(uint id, string name, int buttonsLength, int axesLength) : this()
		{
			Id = id;
			Name = name;
			Buttons = new float[buttonsLength];
			Axes = new float[axesLength];
			RegisteredInSystems = new HashSet<CohtmlUISystem>();
		}

		public uint Id;
		public string Name;

		public float[] Buttons;
		public float[] Axes;

		public HashSet<CohtmlUISystem> RegisteredInSystems;
	}

	public const int DefaultButtonsCount = 18;
	public const int DefaultAxesCount = 4;

	protected Dictionary<uint, GamepadMap> m_Gamepads;

	protected int m_GamepadButtonsCount;
	protected int m_GamepadAxesCount;

	protected GamepadBase()
	{
		m_GamepadButtonsCount = DefaultButtonsCount;
		m_GamepadAxesCount = DefaultAxesCount;
	}

	public Dictionary<uint, GamepadMap> Gamepads => m_Gamepads;

	/// <summary>
	/// Register a new gamepad in the CohtmlPlugin.
	/// </summary>
	protected virtual void RegisterGamepads()
	{
		if (CohtmlInputHandler.FocusedSystem == null || !CohtmlInputHandler.FocusedSystem.IsReady)
		{
			return;
		}

		foreach (GamepadMap gamepad in Gamepads.Values)
		{
			if (!gamepad.RegisteredInSystems.Contains(CohtmlInputHandler.FocusedSystem))
			{
				CohtmlInputHandler.FocusedSystem.SystemNative.RegisterGamepad(gamepad.Id,
					gamepad.Name,
					(uint)gamepad.Axes.Length,
					(uint)gamepad.Buttons.Length
				);
			}

			gamepad.RegisteredInSystems.Add(CohtmlInputHandler.FocusedSystem);
		}
	}

	/// <summary>
	/// Update the gamepad controller state.
	/// </summary>
	public void UpdateGamepadStateNative(GamepadMap gamepad)
	{
		if (CohtmlInputHandler.FocusedSystem == null)
		{
			CohtmlInputHandler.OnGamepadEventTargetNotFound?.Invoke(gamepad);
			return;
		}

		RegisterGamepads();
		CohtmlInputHandler.FocusedSystem.SystemNative.UpdateGamepadState(gamepad.Id, gamepad.Axes, gamepad.Buttons);
	}

	/// <summary>
	/// Unregister gamepad from the Cohtml plugin
	/// </summary>
	/// <param name="id">Controller ID</param>
	public virtual void UnregisterNative(uint id)
	{
		foreach (CohtmlUISystem system in Gamepads[id].RegisteredInSystems)
		{
			if (system != null && system.IsReady)
			{
				system.SystemNative.UnregisterGamepad(id);
			}
		}

		LogHandler.Log(string.Format("Gamepad ID:{0} unregistered.", id));
	}

	public virtual void Dispose()
	{
	}
}
}
