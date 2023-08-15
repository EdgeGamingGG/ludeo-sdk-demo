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
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using UnityEngine;

namespace cohtml
{
[AttributeUsage(AttributeTargets.Method)]
public class CoherentMethodAttribute : Attribute
{
	public string ScriptEventName { get; set; }

	public bool IsEvent { get; set; }

	public CoherentMethodAttribute(string scriptEventName)
		: this(scriptEventName, false)
	{
	}

	public CoherentMethodAttribute(string scriptEventName, bool isEvent)
	{
		ScriptEventName = scriptEventName;
		IsEvent = isEvent;
	}
}

public struct MethodBindingInfo
{
	public string ScriptEventName { get; set; }

	public MethodInfo Method { get; set; }

	public Delegate BoundFunction { get; set; }

	public bool IsEvent { get; set; }
}

public static class MethodHelper
{
	static Dictionary<Type, List<MethodBindingInfo>> s_MethodsCache;

	static MethodHelper()
	{
		s_MethodsCache = new Dictionary<Type, List<MethodBindingInfo>>();
	}

	static MethodBindingInfo BindMethod(MethodBindingInfo method, Component component)
	{
		return (method.Method != null)
			? new MethodBindingInfo() {
					ScriptEventName = method.ScriptEventName,
					BoundFunction = ToDelegate(method.Method, component),
					IsEvent = method.IsEvent
				}
			: method;
	}

	static List<MethodBindingInfo> BindMethods(List<MethodBindingInfo> methods, Component component)
	{
		return methods.Select((x) => BindMethod(x, component)).ToList();
	}

	static List<MethodBindingInfo> GetMethodsInComponent(Component component)
	{
		List<MethodBindingInfo> coherentMethods = new List<MethodBindingInfo>();

		Type type = component.GetType();

		List<MethodBindingInfo> cachedMethods;
		if (s_MethodsCache.TryGetValue(type, out cachedMethods))
		{
			return BindMethods(cachedMethods, component);
		}

		// Iterate methods of each type
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		foreach (MethodInfo methodInfo in type.GetMethods(bindingFlags))
		{
			// Iterate custom attributes
			var attributes = methodInfo.GetCustomAttributes(typeof(CoherentMethodAttribute), true);
			foreach (object customAttribute in attributes)
			{
				CoherentMethodAttribute coherentMethodAttribute = (customAttribute as CoherentMethodAttribute);

				if (methodInfo.IsStatic)
				{
					coherentMethods.Add(new MethodBindingInfo(){
						ScriptEventName = coherentMethodAttribute.ScriptEventName,
						BoundFunction = ToDelegate(methodInfo, null),
						IsEvent = coherentMethodAttribute.IsEvent
					});
				}
				else
				{
					coherentMethods.Add(new MethodBindingInfo(){
						ScriptEventName = coherentMethodAttribute.ScriptEventName,
						Method = methodInfo,
						IsEvent = coherentMethodAttribute.IsEvent
					});
				}
			}
		}

		s_MethodsCache.Add(type, coherentMethods);

		return BindMethods(coherentMethods, component);
	}

	public static List<MethodBindingInfo> GetMethodsInGameObject(GameObject gameObject)
	{
		List<MethodBindingInfo> coherentMethods = new List<MethodBindingInfo>();

		Component[] components = gameObject.GetComponents(typeof(MonoBehaviour));

		foreach (var item in components)
		{
			MonoBehaviour monoBehaviour = item as MonoBehaviour;
			if (monoBehaviour == null || !monoBehaviour.enabled)
			{
				continue;
			}
			coherentMethods.AddRange(GetMethodsInComponent(item));
		}

		return coherentMethods;
	}

	/// <summary>
	/// Builds a Delegate instance from the supplied MethodInfo object and a target to invoke against.
	/// </summary>
	public static Delegate ToDelegate(MethodInfo methodInfo, object target)
	{
		if (methodInfo == null)
		{
			throw new ArgumentNullException("Cannot create a delegate instance from null MethodInfo!");
		}

		Type delegateType;

		var typeArgs = methodInfo.GetParameters()
			.Select(p => p.ParameterType)
			.ToList();

		if (methodInfo.ReturnType == typeof(void))
		{
			delegateType = Expression.GetActionType(typeArgs.ToArray());
		}
		else
		{
			typeArgs.Add(methodInfo.ReturnType);
			delegateType = Expression.GetFuncType(typeArgs.ToArray());
		}

		var result = (target == null)
			? Delegate.CreateDelegate(delegateType, methodInfo)
			: Delegate.CreateDelegate(delegateType, target, methodInfo);

		return result;
	}
}
}
