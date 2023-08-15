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

#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1|| UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
#define COHERENT_UNITY_PRE_5
#endif

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace cohtml
{
[InitializeOnLoad]
public static class ExposeProperties
{
	static Texture m_Texture;
	static Texture m_LogoTexture;
	static Texture m_SupportTexture;
	static Texture m_DocsTexture;

	static GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

	static ExposeProperties()
	{
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowListElementOnGUI;
	}

	public static void Expose(List<Foldout> foldouts)
	{
		EditorGUILayout.BeginVertical(emptyOptions);
		foreach (Foldout fold in foldouts)
		{
			if (fold == null)
			{
				continue;
			}

			bool hasPropertiesToShow = false;
			foreach (PropertyField field in fold.Fields)
			{
				if (!Application.isPlaying || !field.IsStatic)
				{
					hasPropertiesToShow = true;
					break;
				}
			}

			if (!hasPropertiesToShow)
			{
				continue;
			}

			fold.Show = EditorGUILayout.Foldout(fold.Show, new GUIContent(fold.Name, fold.Tooltip));

			if (fold.Show)
			{
				foreach (PropertyField field in fold.Fields)
				{
					if (Application.isPlaying && field.IsStatic)
					{
						continue;
					}

					var realType = field.RealType;

					EditorGUILayout.BeginHorizontal(emptyOptions);
					GUIContent content = new GUIContent(field.Name);
					if (field.Tooltip.Length > 0)
					{
						content.tooltip = field.Tooltip;
					}

					switch (field.Type)
					{
						case SerializedPropertyType.Integer:
							field.SetValue(EditorGUILayout.IntField(content, (int)field.GetValue(), emptyOptions));
							break;

						case SerializedPropertyType.Float:
							field.SetValue(EditorGUILayout.FloatField(content, (float)field.GetValue(), emptyOptions));
							break;

						case SerializedPropertyType.Boolean:
							field.SetValue(EditorGUILayout.Toggle(content, (bool)field.GetValue(), emptyOptions));
							break;

						case SerializedPropertyType.String:
							field.SetValue(EditorGUILayout.TextField(content, (string)field.GetValue(), emptyOptions));
							break;

						case SerializedPropertyType.Vector2:
							field.SetValue(EditorGUILayout.Vector2Field(field.Name, (Vector2)field.GetValue(), emptyOptions));
							break;

						case SerializedPropertyType.Vector3:
							field.SetValue(EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(), emptyOptions));
							break;

						case SerializedPropertyType.Enum:
							field.SetValue(EditorGUILayout.EnumPopup(content, (Enum)field.GetValue(), emptyOptions));
							break;
						case SerializedPropertyType.ObjectReference:
							field.SetValue(EditorGUILayout.ObjectField(content.text, (UnityEngine.Object)field.GetValue(), realType, true, emptyOptions));
							break;

						default:

							break;
					}

					EditorGUILayout.EndHorizontal();
				}
			}
		}

		EditorGUILayout.EndVertical();
	}

	public static void AddCohtmlLayer()
	{
		if (!m_LogoTexture)
		{
			m_LogoTexture = Resources.Load<Texture2D>("Gizmos/Coherent_Inspector");
		}

		if (!m_SupportTexture)
		{
			m_SupportTexture = Resources.Load<Texture2D>("Gizmos/Coherent_Support");
		}

		if (!m_DocsTexture)
		{
			m_DocsTexture = Resources.Load<Texture2D>("Gizmos/Coherent_Docs");
		}

		EditorGUILayout.BeginVertical(GUILayout.Height(32));
		EditorGUILayout.BeginHorizontal(emptyOptions);
		var labelStyle = new GUIStyle();
		labelStyle.fixedHeight = 32;
		labelStyle.fixedWidth = 163;
		if (m_LogoTexture)
		{
			EditorGUILayout.LabelField(new GUIContent("", m_LogoTexture), labelStyle, emptyOptions);
		}

		if (m_DocsTexture && GUILayout.Button(m_DocsTexture))
		{
			Application.OpenURL("https://coherent-labs.com/Documentation/unity-gameface/");
		}

		if (m_SupportTexture && GUILayout.Button(m_SupportTexture))
		{
			Application.OpenURL("https://developers.coherent-labs.com");
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}

	public static void GetProperties(object obj, ref List<Foldout> foldouts)
	{
		if (foldouts == null)
		{
			foldouts = new List<Foldout>(new Foldout[(int)ExposePropertyInfo.FoldoutType.Count]);
		}

		PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo info in properties)
		{
			if (!(info.CanRead && info.CanWrite))
			{
				continue;
			}

			object[] attributes = info.GetCustomAttributes(true);

			bool isExposed = false;
			object infoAttribute = null;

			foreach (object attribute in attributes)
			{
				var attrType = attribute.GetType();
				if (attrType == typeof(ExposePropertyAttribute) ||
				    (attrType == typeof(ExposePropertyStandaloneAttribute)
				     && IsStandaloneTarget()) ||
				    (attrType == typeof(ExposePropertyMobileAttribute)
				     && IsMobileTarget()))
				{
					infoAttribute = attribute;
					isExposed = true;
					break;
				}
			}

			if (!isExposed)
			{
				continue;
			}

			if (PropertyField.GetPropertyType(info, out var type))
			{
				AddPropertyField(obj, foldouts, info, type, infoAttribute);
			}
		}
	}

	public static void GetFields(object obj, ref List<Foldout> foldouts)
	{
		if (foldouts == null)
		{
			foldouts = new List<Foldout>(new Foldout[(int)ExposePropertyInfo.FoldoutType.Count]);
		}

		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		foreach (FieldInfo info in fields)
		{
			object[] attributes = info.GetCustomAttributes(true);

			bool isExposed = false;
			object infoAttribute = null;

			foreach (object attribute in attributes)
			{
				if (attribute.GetType() == typeof(ExposeFieldAttribute))
				{
					infoAttribute = attribute;
					isExposed = true;
					break;
				}
			}

			if (!isExposed)
			{
				continue;
			}

			if (PropertyField.GetFieldType(info, out var type))
			{
				AddPropertyField(obj, foldouts, info, type, infoAttribute);
			}
		}
	}

	private static void AddPropertyField(object obj, List<Foldout> foldouts, MemberInfo info, SerializedPropertyType type, object infoAttribute)
	{
		PropertyField field = new PropertyField(obj, info, type, infoAttribute);

		var category = ExposePropertyInfo.FoldoutType.General;
		if (infoAttribute is ExposePropertyInfo attr)
		{
			category = attr.Category;
		}

		if (foldouts[(int)category] == null)
		{
			foldouts[(int)category] = new Foldout(category);
		}

		foldouts[(int)category].AddField(field);
	}


	static bool IsStandaloneTarget()
	{
		var target = EditorUserBuildSettings.activeBuildTarget.ToString();
		return target.StartsWith("Standalone");
	}

	static bool IsMobileTarget()
	{
		var target = EditorUserBuildSettings.activeBuildTarget;
		#if COHERENT_UNITY_PRE_5
		return (target == BuildTarget.Android || target == BuildTarget.iPhone);
		#else
		return (target == BuildTarget.Android || target == BuildTarget.iOS);
		#endif
	}

	static void HierarchyWindowListElementOnGUI(int instanceID, Rect selectionRect)
	{
		var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
		if (go)
		{
			//TODO: pass types as arguments
			var view = go.GetComponent(typeof(CohtmlView));
			var sys = go.GetComponent(typeof(CohtmlUISystem));
			if (view || sys)
			{
				if (!m_Texture)
				{
					m_Texture = Resources.Load<Texture2D>("Gizmos/Coherent_Icon");
				}

				if (m_Texture)
				{
					var newRect = selectionRect;
					newRect.x = selectionRect.xMax - selectionRect.height;
					newRect.width = selectionRect.height;
					newRect.height = selectionRect.height;
					GUI.DrawTexture(newRect, m_Texture);
				}
			}
		}
	}
}

public class PropertyField
{
	object m_Instance;
	object m_InfoAttribute;
	MemberInfo m_Info;
	SerializedPropertyType m_Type;
	MethodInfo m_Getter;
	MethodInfo m_Setter;

	public PropertyField(object instance, MemberInfo info, SerializedPropertyType type, object infoAttribute)
	{
		m_Instance = instance;
		m_Info = info;
		m_Type = type;
		m_InfoAttribute = infoAttribute;

		if (m_Info is PropertyInfo propertyInfo)
		{
			m_Getter = propertyInfo.GetGetMethod();
			m_Setter = propertyInfo.GetSetMethod();
		}
	}

	public Type RealType
	{
		get
		{
			if (m_Info is PropertyInfo propertyInfo)
			{
				return propertyInfo.PropertyType;
			}

			if (m_Info is FieldInfo fieldInfo)
			{
				return fieldInfo.FieldType;
			}

			throw new TypeLoadException("Unknown type.");
		}
	}

	public SerializedPropertyType Type
	{
		get { return m_Type; }
	}

	public string Name
	{
		get
		{
			string name;

			if (m_InfoAttribute is ExposePropertyInfo ia && ia.PrettyName != null && ia.PrettyName.Length > 0)
			{
				name = ia.PrettyName;
			}
			else
			{
				name = ObjectNames.NicifyVariableName(m_Info.Name);
			}

			return name;
		}
	}

	public string Tooltip
	{
		get
		{
			string tip = "";

			if (m_InfoAttribute is ExposePropertyInfo ia && ia.Tooltip != null)
			{
				tip = ia.Tooltip;
			}

			return tip;
		}
	}

	public bool IsStatic
	{
		get
		{
			if (m_InfoAttribute is ExposePropertyInfo ia)
			{
				return ia.IsStatic;
			}

			return false;
		}
	}

	public object GetValue()
	{
		if (m_Info is PropertyInfo)
		{
			return m_Getter.Invoke(m_Instance, null);
		}

		if (m_Info is FieldInfo a)
		{
			return a.GetValue(m_Instance);
		}

		throw new TypeLoadException("Unknown type");
	}

	public void SetValue(object value)
	{
#pragma warning disable 618
		if (!Equal(value))
		{
			Undo.RecordObject((UnityEngine.Object)m_Instance, this.Name);

			if (m_Info is PropertyInfo)
			{
				m_Setter.Invoke(m_Instance, new object[] {value});
			}
			else if (m_Info is FieldInfo a)
			{
				a.SetValue(m_Instance, value);
			}

			EditorUtility.SetDirty((UnityEngine.Object)m_Instance);
		}
#pragma warning restore 618
	}

	public static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType)
	{
		propertyType = SerializedPropertyType.Generic;

		Type type = info.PropertyType;

		return GetType(ref propertyType, type);
	}

	public static bool GetFieldType(FieldInfo info, out SerializedPropertyType propertyType)
	{
		propertyType = SerializedPropertyType.Generic;

		Type type = info.FieldType;

		return GetType(ref propertyType, type);
	}

	private static bool GetType(ref SerializedPropertyType propertyType, Type type)
	{
		if (type == typeof(int))
		{
			propertyType = SerializedPropertyType.Integer;
			return true;
		}

		if (type == typeof(float))
		{
			propertyType = SerializedPropertyType.Float;
			return true;
		}

		if (type == typeof(bool))
		{
			propertyType = SerializedPropertyType.Boolean;
			return true;
		}

		if (type == typeof(string))
		{
			propertyType = SerializedPropertyType.String;
			return true;
		}

		if (type == typeof(Vector2))
		{
			propertyType = SerializedPropertyType.Vector2;
			return true;
		}

		if (type == typeof(Vector3))
		{
			propertyType = SerializedPropertyType.Vector3;
			return true;
		}

		if (type.IsEnum)
		{
			propertyType = SerializedPropertyType.Enum;
			return true;
		}

		if (type.IsSubclassOf(typeof(UnityEngine.Object)))
		{
			propertyType = SerializedPropertyType.ObjectReference;
			return true;
		}

		return false;
	}

	bool Equal(object other)
	{
		switch (m_Type)
		{
			case SerializedPropertyType.Integer:
				return (int)GetValue() == (int)other;

			case SerializedPropertyType.Float:
				return (float)GetValue() == (float)other;

			case SerializedPropertyType.Boolean:
				return (bool)GetValue() == (bool)other;

			case SerializedPropertyType.String:
				return (string)GetValue() == (string)other;

			case SerializedPropertyType.Vector2:
				return (Vector2)GetValue() == (Vector2)other;

			case SerializedPropertyType.Vector3:
				return (Vector3)GetValue() == (Vector3)other;

			case SerializedPropertyType.Enum:
				return (Enum)GetValue() == (Enum)other;

			case SerializedPropertyType.ObjectReference:
				return (UnityEngine.Object)GetValue() == (UnityEngine.Object)other;

			default:

				break;
		}

		return false;
	}
}

public class Foldout
{
	public Foldout(ExposePropertyInfo.FoldoutType type)
	{
		m_Fields = new List<PropertyField>();

		Type = type;

		switch (Type)
		{
			case ExposePropertyInfo.FoldoutType.General:
				Name = "General";
				Tooltip = "Shows the most common UI properties";
				Show = true;
				break;
			case ExposePropertyInfo.FoldoutType.Rendering:
				Name = "Rendering";
				Tooltip = "Shows rendering-related properties";
				break;
			case ExposePropertyInfo.FoldoutType.AdvancedRendering:
				Name = "Advanced rendering";
				Tooltip = "Shows advanced rendering properties";
				break;
			case ExposePropertyInfo.FoldoutType.Input:
				Name = "Input";
				Tooltip = "Shows UI input-related properties";
				break;
			case ExposePropertyInfo.FoldoutType.Scripting:
				Name = "Scripting";
				Tooltip = "Shows UI scripting-related properties";
				break;
		}
	}

	public string Name { get; private set; }

	public string Tooltip { get; private set; }

	public ExposePropertyInfo.FoldoutType Type { get; private set; }

	public bool Show
	{
		get { return m_Show; }
		set { m_Show = value; }
	}

	public List<PropertyField> Fields
	{
		get { return m_Fields; }
	}

	public void AddField(PropertyField f)
	{
		m_Fields.Add(f);
	}

	List<PropertyField> m_Fields;
	bool m_Show;
}
}
