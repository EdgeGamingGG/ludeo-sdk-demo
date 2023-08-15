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

using System.Collections.Generic;
using UnityEngine;

namespace cohtml.InputSystem
{
public abstract class PointerEvent : InputEvent
{
	public enum EventType
	{
		Up,
		Down,
		Move
	}

	private readonly Dictionary<CohtmlView, Vector2Int> m_ViewPointerPositionMap = new Dictionary<CohtmlView, Vector2Int>();

	public Vector2Int Position { get; set; }

	public Vector2Int InvertY => new Vector2Int(Position.x, Screen.height - Position.y);

	public int X
	{
		get => Position.x;
		set => Position = new Vector2Int(value, Y);
	}

	public int Y
	{
		get => Position.y;
		set => Position = new Vector2Int(X, value);
	}

	public abstract EventType PointerEventType { get; }

	public void AddView(CohtmlView view, Vector2Int position)
	{
		if (m_ViewPointerPositionMap.ContainsKey(view))
		{
			m_ViewPointerPositionMap[view] = position;
		}
		else
		{
			m_ViewPointerPositionMap.Add(view, position);
		}
	}

	public Vector2Int GetViewPosition(CohtmlView view)
	{
		if (m_ViewPointerPositionMap.ContainsKey(view))
		{
			return m_ViewPointerPositionMap[view];
		}

		return Vector2Int.zero;
	}

	public void RemoveView(CohtmlView view)
	{
		m_ViewPointerPositionMap.Remove(view);
	}

	public void ClearViews()
	{
		m_ViewPointerPositionMap.Clear();
	}
}
}
