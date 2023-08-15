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

// This file is auto-generated. Do not edit.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cohtml
{
public partial class CustomEventsEditor
{
	SerializedProperty m_CustomEventNamesProperty;
	SerializedProperty m_CustomEventsProperty;
	SerializedProperty m_CustomStringEventNamesProperty;
	SerializedProperty m_CustomStringEventsProperty;

	public void OnEnableGenerated()
	{
		(target as CustomEvents).m_CreatedEventTypes =
			new List<CustomEventType>(new CustomEventType[] {
				new CustomEventType(CustomEventValueType.Null,
									CustomEventValueType.Null,
									CustomEventValueType.Null,
									CustomEventValueType.Null),
				new CustomEventType(CustomEventValueType.String,
									CustomEventValueType.Null,
									CustomEventValueType.Null,
									CustomEventValueType.Null),
		});

		m_CustomEventNamesProperty = serializedObject.FindProperty("m_CustomEventNames");
		m_CustomEventsProperty = serializedObject.FindProperty("m_CustomEvents");
		m_CustomStringEventNamesProperty = serializedObject.FindProperty("m_CustomStringEventNames");
		m_CustomStringEventsProperty = serializedObject.FindProperty("m_CustomStringEvents");
	}

	public void OnInspectorGUIGenerated()
	{
		serializedObject.Update();

		for (int i = 0; i < m_CustomEventNamesProperty.arraySize; i++)
		{
			SerializedProperty eventName = m_CustomEventNamesProperty.GetArrayElementAtIndex(i);

			EditorGUILayout.PropertyField(eventName, m_EventNameContent);
			EditorGUILayout.PropertyField(m_CustomEventsProperty.GetArrayElementAtIndex(i),
										  new GUIContent(eventName.stringValue));

			Rect lastRect = GUILayoutUtility.GetLastRect();
			Rect rect = new Rect(lastRect.xMax - m_ContentSize.x, lastRect.y + 1f, m_ContentSize.x, m_ContentSize.y);

			if (GUI.Button(rect, m_IconToolbarMinus, GUIStyle.none))
			{
				m_CustomEventsProperty.DeleteArrayElementAtIndex(i);
				m_CustomEventNamesProperty.DeleteArrayElementAtIndex(i);
				i--;
			}
		}
		for (int i = 0; i < m_CustomStringEventNamesProperty.arraySize; i++)
		{
			SerializedProperty eventName = m_CustomStringEventNamesProperty.GetArrayElementAtIndex(i);

			EditorGUILayout.PropertyField(eventName, m_EventNameContent);
			EditorGUILayout.PropertyField(m_CustomStringEventsProperty.GetArrayElementAtIndex(i),
										  new GUIContent(eventName.stringValue));

			Rect lastRect = GUILayoutUtility.GetLastRect();
			Rect rect = new Rect(lastRect.xMax - m_ContentSize.x, lastRect.y + 1f, m_ContentSize.x, m_ContentSize.y);

			if (GUI.Button(rect, m_IconToolbarMinus, GUIStyle.none))
			{
				m_CustomStringEventsProperty.DeleteArrayElementAtIndex(i);
				m_CustomStringEventNamesProperty.DeleteArrayElementAtIndex(i);
				i--;
			}
		}

		serializedObject.ApplyModifiedProperties();
	}

	void OnAddNewSelected(object index)
	{
		serializedObject.Update();
		switch ((int)index)
		{
		case 0:
			m_CustomEventNamesProperty.arraySize++;
			m_CustomEventsProperty.arraySize++;
			break;
		case 1:
			m_CustomStringEventNamesProperty.arraySize++;
			m_CustomStringEventsProperty.arraySize++;
			break;
		}
		serializedObject.ApplyModifiedProperties();
	}
}
}

