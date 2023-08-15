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
using UnityEditor;
using UnityEngine;

public class InputConfirmWindow : EditorWindow
{
	[Serializable]
	public class WindowContent
	{
		public WindowContent()
		{
			CancelButtonText = "Cancel";
			ConfirmButtonText = "Confirm";
			InputFieldDescription = "Input Field Description";
			InputFieldValue = 1;
			InputFieldRange = new Vector2(0, int.MaxValue);
			InputFieldValidationMessage = string.Format("Number must be between {0} & {1}", InputFieldRange.x, InputFieldRange.y);
			CheckBox = true;
			CheckBoxValue = true;
			CheckBoxDescription = "Checkbox description";
			Description = "Cohtml Description";
			Title = "Cohtml Title";
		}

		public string Title { get; set; }

		public string Description { get; set; }

		public string InputFieldDescription { get; set; }

		public int InputFieldValue { get; set; }

		public Vector2 InputFieldRange { get; set; }

		public bool CheckBox { get; set; }

		public string CheckBoxDescription { get; set; }

		public bool CheckBoxValue { get; set; }

		public string InputFieldValidationMessage { get; set; }

		public string ConfirmButtonText { get; set; }

		public string CancelButtonText { get; set; }
	}

	public class ConfirmWindowResult
	{
		public bool Checkbox { get; set; }

		public int InputFieldValue { get; set; }
	}

	public static WindowContent m_content;
	private static InputConfirmWindow m_ConfirmWindow;
	private static Vector2 m_spawnPosition;

	public static Action<ConfirmWindowResult> OnConfirmButtonPressed { get; set; }

	public static Action<ConfirmWindowResult> OnCancelButtonPressed { get; set; }

	public static InputConfirmWindow ConfirmWindow
	{
		get
		{
			if (m_ConfirmWindow == null)
			{
				m_ConfirmWindow = GetWindow<InputConfirmWindow>();
			}

			return m_ConfirmWindow;
		}
	}

	public static Vector2 SpawnPosition
	{
		get
		{
			if (m_spawnPosition == Vector2.zero)
			{
				m_spawnPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			}

			return m_spawnPosition;
		}
	}

	public static void Popup(WindowContent content)
	{
		m_content = content;
		ConfirmWindow.Show();
	}

	void OnGUI()
	{
		GUI.color = Color.white;
		ConfirmWindow.position = new Rect(SpawnPosition.x, SpawnPosition.y, 400, 200);
		EditorGUILayout.LabelField(m_content.Description, EditorStyles.wordWrappedLabel);

		if (!string.IsNullOrEmpty(m_content.InputFieldDescription))
		{
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.Label(m_content.InputFieldDescription, GUILayout.MaxWidth(350), GUILayout.MaxHeight(100));
			m_content.InputFieldValue = EditorGUILayout.IntField(m_content.InputFieldValue, GUILayout.MaxWidth(70));
			GUILayout.EndHorizontal();

			if (m_content.InputFieldValue < m_content.InputFieldRange.x || m_content.InputFieldValue > m_content.InputFieldRange.y)
			{
				GUI.color = Color.red;
				GUILayout.Label(m_content.InputFieldValidationMessage, GUILayout.MaxWidth(350), GUILayout.MaxHeight(300));
				GUI.color = Color.white;
			}
		}

		if (m_content.CheckBox)
		{
			m_content.CheckBoxValue = GUILayout.Toggle(m_content.CheckBoxValue, m_content.CheckBoxDescription);
		}

		GUILayout.FlexibleSpace();
		if (GUILayout.Button(m_content.ConfirmButtonText))
		{
			if (OnConfirmButtonPressed != null)
			{
				ConfirmWindowResult result = new ConfirmWindowResult
				{
					Checkbox = m_content.CheckBoxValue,
					InputFieldValue = m_content.InputFieldValue
				};

				OnConfirmButtonPressed.Invoke(result);
			}

			ConfirmWindow.Close();
		}

		if (GUILayout.Button(m_content.CancelButtonText))
		{
			if (OnCancelButtonPressed != null)
			{
				ConfirmWindowResult result = new ConfirmWindowResult
				{
					Checkbox = m_content.CheckBoxValue,
					InputFieldValue = m_content.InputFieldValue
				};

				OnCancelButtonPressed.Invoke(result);
			}

			ConfirmWindow.Close();
		}
	}
}