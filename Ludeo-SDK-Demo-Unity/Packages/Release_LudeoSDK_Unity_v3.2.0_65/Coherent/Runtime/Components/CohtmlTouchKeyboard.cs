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

using UnityEngine;
using UnityEngine.Events;

namespace cohtml
{
public class CohtmlTouchKeyboard : VirtualKeyboard
{
	public class Options
	{
		public TouchScreenKeyboardType KeyboardType;
		public bool AutoCorrection;
		public bool Multiline;
		public bool Secure;
		public bool Alert;
		public string Placeholder;
		public int MaxLength;
	}

	[SerializeField]
	[Tooltip("Max characters that can be typed in the input field. The default is 1000 characters.")]
	private int m_DefaultMaxLength = 1000;

	[SerializeField]
	private UnityEvent OnKeyboardOptionsSet;

	TouchScreenKeyboard m_Keyboard;

	public Options KeyboardOptions { get; set; }

	public override void Start()
	{
		if (!TouchScreenKeyboard.isSupported)
		{
			enabled = false;
		}
		else
		{
			base.Start();
		}
	}

	void Update()
	{
		if (m_Keyboard != null &&
		    m_Keyboard.status == TouchScreenKeyboard.Status.Done
		)
		{
			RefreshText(m_Keyboard.text);
		}
	}

	protected override void OnSetKeyboardOptionsCallback()
	{
		OnKeyboardOptionsSet.Invoke();
		if (KeyboardOptions == null)
		{
			SetDefaultOptions();
		}
	}

	protected override void OnFocusCallback(string content)
	{
		m_Keyboard = TouchScreenKeyboard.Open(
			content,
			KeyboardOptions.KeyboardType,
			KeyboardOptions.AutoCorrection,
			KeyboardOptions.Multiline,
			KeyboardOptions.Secure,
			KeyboardOptions.Alert,
			KeyboardOptions.Placeholder,
			KeyboardOptions.MaxLength
		);
	}

	protected override string OnBlurCallback()
	{
		string text = m_Keyboard.text;
		m_Keyboard = null;

		return text;
	}

	private void SetDefaultOptions()
	{
		KeyboardOptions = new Options
		{
			KeyboardType = GetTextLayoutType(ViewComponent.TextInputHandler.GetAttributeValue<string>("type")),
			AutoCorrection = false,
			Multiline = false,
			Secure = false,
			Alert = false,
			Placeholder = ViewComponent.TextInputHandler.GetAttributeValue<string>("placeholder"),
			MaxLength = ViewComponent.TextInputHandler.GetAttributeValue<int>("maxLength")
		};

		if (KeyboardOptions.MaxLength == 0)
		{
			KeyboardOptions.MaxLength = m_DefaultMaxLength;
		}
	}

	private TouchScreenKeyboardType GetTextLayoutType(string type)
	{
		switch (type)
		{
			case "number":
				return TouchScreenKeyboardType.NumberPad;
			case "email":
				return TouchScreenKeyboardType.EmailAddress;
			case "tel":
				return TouchScreenKeyboardType.PhonePad;
			case "url":
				return TouchScreenKeyboardType.URL;
			case "search":
				return TouchScreenKeyboardType.Search;
			default:
				return TouchScreenKeyboardType.Default;
		}
	}
}
}
