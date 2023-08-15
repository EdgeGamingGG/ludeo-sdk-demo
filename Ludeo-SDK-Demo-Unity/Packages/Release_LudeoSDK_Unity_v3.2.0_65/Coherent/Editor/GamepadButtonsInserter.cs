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
using System;
using UnityEditor;
using UnityEngine;

namespace cohtml
{
public class GamepadButtonsInserter
{
	private const string AddGamepadControlsAsked = "Cohtml_Add_GamepadControlsAsked";
	private static int m_numberOfGamepads = 1;

	private abstract class Control
	{
		public enum Type
		{
			Button = 0,
			Axis = 2
		};

		public string m_Name;
		public string m_PositiveButtonName;

		public float m_Gravity = 1f;
		public float m_DeadZone = 0.2f;
		public float m_Sensitivity;

		public Type m_Type;

		public int m_Axis;
		public int m_GamepadNumber;
	}

	private class Axis : Control
	{
		public Axis(string name, float sensitivity, int axis, int gamepadNumber)
		{
			this.m_Name = name;
			this.m_Sensitivity = sensitivity;
			this.m_Type = Type.Axis;
			this.m_Axis = axis;
			this.m_GamepadNumber = gamepadNumber;
		}
	}

	private class Button : Control
	{
		public Button(string name, string positiveButtonName, int gamepadNumber)
		{
			m_Name = name;
			m_PositiveButtonName = positiveButtonName;
			m_Type = Type.Button;
			m_GamepadNumber = gamepadNumber;
		}
	}

	public static void PopulateGamepadControls()
	{
		AddGamepadControls(true);
	}

	public static void AddGamepadControls(bool force = false)
	{
		// Check if not populated already or asked already. Force to open the window when is called from menu
		if (!force && (AxisDefined("Cohtml_Gamepad0_LeftStick_X") ||
		               Convert.ToBoolean(PlayerPrefs.GetInt(AddGamepadControlsAsked))))
		{
			return;
		}

		// Subscribe to prompt window confirm button.
		InputConfirmWindow.OnConfirmButtonPressed = result =>
		{
			m_numberOfGamepads = result.InputFieldValue;

			PopulateGamepadMap();
			PlayerPrefs.SetInt(AddGamepadControlsAsked, Convert.ToInt32(result.Checkbox));
		};

		// Subscribe to prompt window cancel button.
		InputConfirmWindow.OnCancelButtonPressed = result => { PlayerPrefs.SetInt(AddGamepadControlsAsked, Convert.ToInt32(result.Checkbox)); };

		// Open input prompt window. 
		Vector2 inputFieldRange = new Vector2(1, 8);
		string descriptionInPlayMode = Application.isPlaying
			? " You are in play mode. When you exit play mode the gamepad controls will disappear. "
			: string.Empty;
		string manualAdditionDescription = "You can add gamepad controls manually via the  \"Gameface/Populate gamepad controls\" menu.";
		InputConfirmWindow.Popup(new InputConfirmWindow.WindowContent
		{
			Title = "Configure Cohtml gamepad input",
			Description = "Do you want to populate your project with generic gamepad controls? " +
			              "You can view the new configuration at \n \"Project Settings/InputManager\"?" +
			              descriptionInPlayMode +
			              manualAdditionDescription,
			InputFieldDescription = "How many gamepad controls want to support? ",
			InputFieldValue = m_numberOfGamepads,
			InputFieldRange = inputFieldRange,
			InputFieldValidationMessage = string.Format("Number must between {0} & {1}", inputFieldRange.x, inputFieldRange.y),
			CheckBox = true,
			CheckBoxValue = false,
			CheckBoxDescription = "Don't ask me again",
			ConfirmButtonText = "Populate",
			CancelButtonText = "Cancel"
		});
	}

	private static void SubscribeToPreStartEvent()
	{
		#if UNITY_2017_2_OR_NEWER
		EditorApplication.playModeStateChanged += playModeState =>
		{
			if (playModeState == PlayModeStateChange.EnteredPlayMode)
			{
				AddGamepadControls();
			}
		};
		#else
		EditorApplication.playmodeStateChanged += () =>
		{
			if (EditorApplication.isPlaying)
			{
				AddGamepadControls();
			}
		};
		#endif
	}

	private static void PopulateGamepadMap()
	{
		for (int i = 0; i < m_numberOfGamepads; i++)
		{
			int gamepadId = i + 1; // Gamepad map start from 1 in positive button settings.
			AddControl(new Axis("Cohtml_Gamepad" + i + "_LeftStick_X", 1f, 0, gamepadId));
			AddControl(new Axis("Cohtml_Gamepad" + i + "_LeftStick_Y", -1f, 1, gamepadId));

			AddControl(new Axis("Cohtml_Gamepad" + i + "_RightStick_X", 1f, 3, gamepadId));
			AddControl(new Axis("Cohtml_Gamepad" + i + "_RightStick_Y", -1f, 4, gamepadId));

			AddControl(new Axis("Cohtml_Gamepad" + i + "_LeftTrigger", 1f, 8, gamepadId));
			AddControl(new Axis("Cohtml_Gamepad" + i + "_RightTrigger", 1f, 9, gamepadId));

			AddControl(new Axis("Cohtml_Gamepad" + i + "_DPad_X", 1f, 5, gamepadId));
			AddControl(new Axis("Cohtml_Gamepad" + i + "_DPad_Y", 1f, 6, gamepadId));

			AddControl(new Button("Cohtml_Gamepad" + i + "_LeftShoulder", "joystick " + gamepadId + " button 4", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_RightShoulder", "joystick " + gamepadId + " button 5", gamepadId));

			AddControl(new Button("Cohtml_Gamepad" + i + "_LeftStickPress", "joystick " + gamepadId + " button 8", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_RightStickPress", "joystick " + gamepadId + " button 9", gamepadId));

			AddControl(new Button("Cohtml_Gamepad" + i + "_North", "joystick " + gamepadId + " button 3", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_South", "joystick " + gamepadId + " button 0", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_East", "joystick " + gamepadId + " button 1", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_West", "joystick " + gamepadId + " button 2", gamepadId));

			AddControl(new Button("Cohtml_Gamepad" + i + "_Select", "joystick " + gamepadId + " button 6", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_Start", "joystick " + gamepadId + " button 7", gamepadId));

			AddControl(new Button("Cohtml_Gamepad" + i + "_SystemButton", "joystick " + gamepadId + " button 12", gamepadId));
			AddControl(new Button("Cohtml_Gamepad" + i + "_TouchpadButton", "joystick " + gamepadId + " button 13", gamepadId));
		}

		LogHandler.Log("Generic gamepad controls were automatically added in ProjectSettings/InputManager file.");
	}

	private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
	{
		SerializedProperty child = parent.Copy();
		child.Next(true);
		do
		{
			if (child.name == name) return child;
		} while (child.Next(false));

		return null;
	}

	private static bool AxisDefined(string axisName)
	{
		SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
		SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

		axesProperty.Next(true);
		axesProperty.Next(true);
		while (axesProperty.Next(false))
		{
			SerializedProperty axis = axesProperty.Copy();

			if (axis.Next(true) && axis.stringValue == axisName)
			{
				return true;
			}
		}

		return false;
	}

	private static void AddControl(Control control)
	{
		if (AxisDefined(control.m_Name))
		{
			return;
		}

		SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
		SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

		axesProperty.arraySize++;
		serializedObject.ApplyModifiedProperties();

		SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);

		GetChildProperty(axisProperty, "m_Name").stringValue = control.m_Name;
		GetChildProperty(axisProperty, "positiveButton").stringValue = control.m_PositiveButtonName;
		GetChildProperty(axisProperty, "gravity").floatValue = control.m_Gravity;
		GetChildProperty(axisProperty, "dead").floatValue = control.m_DeadZone;
		GetChildProperty(axisProperty, "sensitivity").floatValue = control.m_Sensitivity;
		GetChildProperty(axisProperty, "type").intValue = (int)control.m_Type;
		GetChildProperty(axisProperty, "axis").intValue = control.m_Axis;
		GetChildProperty(axisProperty, "joyNum").intValue = control.m_GamepadNumber;

		serializedObject.ApplyModifiedProperties();
	}
}
}
#endif
