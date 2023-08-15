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
using cohtml.Net;

namespace cohtml
{
public class TextInputHandler : ITextInputHandler
{
	private IInputProxy m_Proxy;
	private Net.Range m_RangeCached;

	public string InputText
	{
		get
		{
			if (m_Proxy != null)
			{
				return m_Proxy.GetText(0) ?? string.Empty;
			}

			return string.Empty;
		}

		set
		{
			if (value == null)
			{
				value = string.Empty;
			}

			m_RangeCached = new Net.Range {Start = 0, End = (uint)(InputText?.Length ?? 0)};
			byte[] bytes = Utils.EncodeString(value);
			m_Proxy.SetText(value, (uint)bytes.Length, m_RangeCached);
		}
	}

	public Action<string> OnFocusCallback { get; set; }

	public Func<string> OnBlurCallback { get; set; }

	public override void OnFocus(IInputProxy proxy)
	{
		m_Proxy = proxy;
		base.OnFocus(m_Proxy);
		if (OnFocusCallback != null)
		{
			OnFocusCallback.Invoke(InputText);
		}
	}

	public override void OnBlur(IInputProxy proxy)
	{
		base.OnBlur(proxy);
		m_Proxy = proxy;
		if (OnBlurCallback != null)
		{
			InputText = OnBlurCallback.Invoke();
		}
	}

	public T GetAttributeValue<T>(string attribute)
	{
		T value = default;
		if (m_Proxy != null)
		{
			try
			{
				value = (T)Convert.ChangeType(m_Proxy.GetAttribute(attribute), typeof(T));
			}
			catch
			{
				value = default;
			}
		}

		return value;
	}
}
}
