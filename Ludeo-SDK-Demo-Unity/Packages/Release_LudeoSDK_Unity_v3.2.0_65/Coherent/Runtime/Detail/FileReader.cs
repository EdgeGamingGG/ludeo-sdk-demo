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
using System.IO;
using cohtml.Net;

namespace cohtml
{
public class FileReader : UnityFileSystemReader
{
	private const string FontsStr = "fonts";
	private const string PreloadedFontsStr = DefaultResourceHandler.CouiProtocol + DefaultResourceHandler.PreloadedHost + "/" + FontsStr;
	static FileReader m_Instance;
	static List<TextAsset> m_Fonts;
	static string m_ResourcesPath;

	public static FileReader Reader
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new FileReader();
				m_ResourcesPath = Path.Combine(CohtmlUISystem.StreamingAssetsPath, DefaultResourceHandler.UIResourcesPath);

			}

			return m_Instance;
		}
	}

	public static void RegisterPreloadedFonts(IUISystem system)
	{
		m_Fonts = new List<TextAsset>(Resources.LoadAll<TextAsset>(FontsStr));
		for (int i = 0; i < m_Fonts.Count; i++)
		{
			system.RegisterFont(string.Format("{0}/{1}.ttf", PreloadedFontsStr, m_Fonts[i].name));
		}
	}

	public override UnitySyncStreamReader OpenFile(string path)
	{
		if(path.Contains(PreloadedFontsStr))
		{
			for(int i = 0; i < m_Fonts.Count; i++)
			{
				if(path.Contains(m_Fonts[i].name))
				{
					byte[] font = m_Fonts[i].bytes;
					return new StreamReader(font);
				}
			}

			return null;
		}

		path = NormalizePath(path);

		if (File.Exists(path))
		{
			return new StreamReader(path);
		}

		return null;
	}

	string NormalizePath(string path)
	{
		if(path.StartsWith(DefaultResourceHandler.CouiProtocol, StringComparison.CurrentCulture))
		{
			path = Path.Combine(m_ResourcesPath, path.Substring(DefaultResourceHandler.CouiProtocol.Length));
		}

		return Path.GetFullPath(path);
	}
}
}
